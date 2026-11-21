using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Modelo;
using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Application.Logistica;

public sealed class ServicioDespachos(IRepositorio repo, ITransacciones tx, Autorizador auth, SesionActual sesion,
    IEnumerable<IFabricaEntrega> familias, IReloj reloj)
{
    public async Task Despachar(Guid id)
    {
        await auth.Exigir(Rol.BODEGA);
        await tx.Ejecutar(async () => {
            var pedido = await repo.Pedido(id); var familia = familias.Single(f => f.Tipo == pedido.Entrega);
            pedido.Transicionar(EstadoPedido.DESPACHADO, sesion.Nombre, reloj.Ahora);
            repo.Agregar(new Despacho(id, familia.CrearEtiqueta().Crear(id), reloj.Ahora, reloj.Ahora.AddDays(familia.CrearPlazo().Dias)));
            repo.Agregar(new EventoOutbox(id, "DespachoRegistrado")); return true;
        });
    }
    public async Task Entregar(Guid id)
    {
        await auth.Exigir(Rol.BODEGA); await tx.Ejecutar(async () => {
            (await repo.Pedido(id)).Transicionar(EstadoPedido.ENTREGADO, sesion.Nombre, reloj.Ahora); return true;
        });
    }
}

// Template Method: el esqueleto es comun; cada motivo decide plazo y condicion.
public abstract class ProcesarDevolucion(IRepositorio repo, ITransacciones tx, Autorizador auth, IReloj reloj, IReembolsosSimulados reembolsos)
{
    protected abstract MotivoDevolucion Motivo { get; }
    protected abstract void Inspeccionar(TimeSpan antiguedad, bool sinUso);
    public async Task<Devolucion> Ejecutar(Guid id, bool sinUso)
    {
        await auth.Exigir(Rol.VENTAS);
        return await tx.Ejecutar(async () => {
            var pedido = await repo.Pedido(id);
            if (pedido.Estado != EstadoPedido.ENTREGADO || pedido.EntregadoEn is null) throw new InvalidOperationException("Se requiere pedido entregado.");
            if (await repo.DevolucionPorPedido(id) is not null) throw new InvalidOperationException("Pedido ya devuelto.");
            var edad = reloj.Ahora - pedido.EntregadoEn.Value;
            if (edad < TimeSpan.Zero) throw new InvalidOperationException("Fecha de entrega futura.");
            Inspeccionar(edad, sinUso);
            var importe = pedido.Subtotal - pedido.Descuento;
            var referencia = await reembolsos.ReembolsarAsync(id, importe);
            var devolucion = new Devolucion(id, Motivo, importe, referencia, reloj.Ahora);
            var productos = await repo.Productos();
            foreach (var grupo in pedido.Detalles.SelectMany(d => d.Componentes).GroupBy(c => c.Sku))
            {
                var producto = productos.Single(p => p.Sku == grupo.Key); var cantidad = grupo.Sum(c => c.Cantidad);
                producto.Mover(cantidad); repo.Agregar(new MovimientoInventario(producto.Id, cantidad, "DEVOLUCION", id.ToString()));
            }
            repo.Agregar(devolucion); repo.Agregar(new EventoOutbox(id, "DevolucionAceptada")); return devolucion;
        });
    }
}
public sealed class DevolucionDefecto(IRepositorio r, ITransacciones t, Autorizador a, IReloj reloj, IReembolsosSimulados reembolsos) : ProcesarDevolucion(r, t, a, reloj, reembolsos)
{
    protected override MotivoDevolucion Motivo => MotivoDevolucion.DEFECTO;
    protected override void Inspeccionar(TimeSpan antiguedad, bool sinUso)
    { if (antiguedad > TimeSpan.FromDays(30)) throw new InvalidOperationException("Defecto fuera de 30 dias."); }
}
public sealed class DevolucionArrepentimiento(IRepositorio r, ITransacciones t, Autorizador a, IReloj reloj, IReembolsosSimulados reembolsos) : ProcesarDevolucion(r, t, a, reloj, reembolsos)
{
    protected override MotivoDevolucion Motivo => MotivoDevolucion.ARREPENTIMIENTO;
    protected override void Inspeccionar(TimeSpan antiguedad, bool sinUso)
    { if (!sinUso || antiguedad > TimeSpan.FromDays(7)) throw new InvalidOperationException("Arrepentimiento requiere producto sin uso y hasta 7 dias."); }
}
public sealed class ServicioInventario(IRepositorio repo, ITransacciones tx, Autorizador auth)
{
    public async Task Mover(Guid producto, int cantidad, string motivo, string referencia)
    {
        await auth.Exigir(Rol.BODEGA); ArgumentException.ThrowIfNullOrWhiteSpace(motivo); ArgumentException.ThrowIfNullOrWhiteSpace(referencia);
        await tx.Ejecutar(async () => { (await repo.Productos()).Single(p => p.Id == producto).Mover(cantidad);
            repo.Agregar(new MovimientoInventario(producto, cantidad, motivo, referencia)); return true; });
    }
}
