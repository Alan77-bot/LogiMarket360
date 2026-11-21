using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Catalogo;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Modelo;
using LogiMarket.Domain.Pedidos;
using LogiMarket.Domain.Promociones;

namespace LogiMarket.Application.Pedidos;

public sealed record LineaEntrada(string Sku, int Cantidad);
public sealed record SolicitudBorrador(Guid ClienteId, TipoEntrega Entrega, IReadOnlyList<LineaEntrada> Lineas);

public sealed class ServicioPedidos(IRepositorio repo, ITransacciones tx, Autorizador auth, SesionActual sesion,
    CalculadoraPrecios calculadora, IEnumerable<IFabricaEntrega> entregas, IReloj reloj)
{
    public async Task<Pedido> Preparar(SolicitudBorrador entrada)
    {
        await auth.Exigir(Rol.VENTAS);
        var cliente = (await repo.Clientes()).SingleOrDefault(c => c.Id == entrada.ClienteId) ?? throw new ArgumentException("Seleccione cliente.");
        var catalogo = CatalogoCompuesto.Construir(await repo.Productos(), await repo.Kits());
        var builder = new PedidoBuilder().Para(cliente).ConEntrega(entrada.Entrega);
        foreach (var linea in entrada.Lineas) builder.Agregar(catalogo.TryGetValue(linea.Sku, out var elemento) ? elemento : throw new ArgumentException("SKU inexistente."), linea.Cantidad);
        var pedido = builder.Construir(); pedido.Valorar(calculadora, entregas.Single(e => e.Tipo == entrada.Entrega).CrearTarifa().Importe); return pedido;
    }
    public async Task<Guid> Guardar(SolicitudBorrador entrada, Guid? reemplazar = null)
    {
        await auth.Exigir(Rol.VENTAS);
        return await tx.Ejecutar(async () => {
            var pedido = await Preparar(entrada);
            if (reemplazar is Guid id) { var anterior = await repo.Pedido(id); anterior.AplicarCambios(pedido); return anterior.Id; }
            repo.Agregar(pedido); return pedido.Id;
        });
    }
    public async Task<Guid> Clonar(Guid id)
    {
        await auth.Exigir(Rol.VENTAS);
        return await tx.Ejecutar(async () => { var copia = (await repo.Pedido(id)).ClonarBorrador(); repo.Agregar(copia); return copia.Id; });
    }
    public async Task Validar(Guid id)
    {
        await auth.Exigir(Rol.VENTAS);
        await tx.Ejecutar(async () => {
            var pedido = await repo.Pedido(id); pedido.ExigirEditable();
            // Se usan precios historicos de las lineas, no el catalogo actual.
            pedido.Valorar(calculadora, entregas.Single(e => e.Tipo == pedido.Entrega).CrearTarifa().Importe);
            var resultado = CadenaConfirmacion.Validar(new(pedido, (await repo.Clientes()).SingleOrDefault(c => c.Id == pedido.ClienteId), await repo.Productos()), false);
            if (!resultado.Valido) throw new InvalidOperationException(resultado.Mensaje);
            pedido.Transicionar(EstadoPedido.VALIDADO, sesion.Nombre, reloj.Ahora); return true;
        });
    }
    public async Task Cancelar(Guid id)
    {
        await auth.Exigir(Rol.VENTAS);
        await tx.Ejecutar(async () => { var pedido = await repo.Pedido(id); pedido.Transicionar(EstadoPedido.CANCELADO, sesion.Nombre, reloj.Ahora); return true; });
    }
    public async Task<List<Pedido>> Consultar() { await auth.Lectura(); return (await repo.Pedidos()).OrderByDescending(p => p.Fecha).ToList(); }
}

// Facade: coordina colaboradores; no conoce EF, SQL ni tasas de descuento.
public sealed class ConfirmacionPedidoFacade(IRepositorio repo, ITransacciones tx, Autorizador auth,
    IPasarelaPago pasarela, SesionActual sesion, IReloj reloj)
{
    public async Task<Pago> Confirmar(Guid id, string clave)
    {
        await auth.Exigir(Rol.VENTAS); ArgumentException.ThrowIfNullOrWhiteSpace(clave);
        return await tx.Ejecutar(async () => {
            var previo = await repo.PagoPorClave(clave);
            if (previo is not null)
            { if (previo.PedidoId != id) throw new InvalidOperationException("Clave usada para otro pedido."); return previo; }
            var pedido = await repo.Pedido(id);
            if (await repo.PagoPorPedido(id) is not null) throw new InvalidOperationException("El pedido ya tiene un pago.");
            var productos = await repo.Productos();
            var resultado = CadenaConfirmacion.Validar(new(pedido, (await repo.Clientes()).SingleOrDefault(c => c.Id == pedido.ClienteId), productos));
            if (!resultado.Valido) throw new InvalidOperationException(resultado.Mensaje);
            var cobro = await pasarela.CobrarAsync(pedido.Total, clave);
            if (!cobro.Aceptado) throw new InvalidOperationException(cobro.Mensaje);
            foreach (var grupo in pedido.Detalles.SelectMany(d => d.Componentes).GroupBy(c => c.Sku))
            {
                var producto = productos.Single(p => p.Sku == grupo.Key); var cantidad = grupo.Sum(c => c.Cantidad);
                producto.Mover(-cantidad); repo.Agregar(new MovimientoInventario(producto.Id, -cantidad, "VENTA", id.ToString()));
            }
            var pago = new Pago(id, clave, cobro.Referencia, pedido.Total); repo.Agregar(pago);
            pedido.Transicionar(EstadoPedido.PAGADO, sesion.Nombre, reloj.Ahora); repo.Agregar(new EventoOutbox(id, "PedidoPagado")); return pago;
        });
    }
}
