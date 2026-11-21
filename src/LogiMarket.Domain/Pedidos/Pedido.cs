using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Modelo;
using LogiMarket.Domain.Promociones;

namespace LogiMarket.Domain.Pedidos;

public sealed class Pedido
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ClienteId { get; private set; }
    public string ClienteNombre { get; private set; } = "";
    public bool ClienteVip { get; private set; }
    public TipoEntrega Entrega { get; private set; }
    public EstadoPedido Estado { get; private set; } = EstadoPedido.BORRADOR;
    public DateTimeOffset Fecha { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? EntregadoEn { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal Descuento { get; private set; }
    public decimal Tarifa { get; private set; }
    public decimal Total { get; private set; }
    public Guid Version { get; private set; } = Guid.NewGuid();
    private List<PedidoDetalle> detalles = [];
    private List<CambioEstado> historial = [];
    private bool necesitaValoracion;
    public IReadOnlyList<PedidoDetalle> Detalles => detalles.AsReadOnly();
    public IReadOnlyList<CambioEstado> Historial => historial.AsReadOnly();
    private Pedido() { }
    internal Pedido(Cliente cliente, TipoEntrega entrega, IEnumerable<PedidoDetalle> detalles)
    {
        ClienteId = cliente.Id; ClienteNombre = cliente.Nombre; ClienteVip = cliente.Categoria == CategoriaCliente.VIP;
        Entrega = entrega; this.detalles = detalles.Select(d => d.Copiar()).ToList();
        if (Detalles.Count == 0) throw new ArgumentException("Agregue al menos una linea.");
        necesitaValoracion = true;
        historial.Add(new(Id, Estado, "Creacion"));
    }
    public void Valorar(CalculadoraPrecios calculadora, decimal tarifa)
    {
        ExigirEditable(); var componentes = Detalles.SelectMany(d => d.Componentes).ToArray();
        var totales = calculadora.Calcular(componentes.Sum(c => c.Precio * c.Cantidad), tarifa, new(ClienteVip, componentes.Sum(c => c.Cantidad)));
        Subtotal = totales.Subtotal; Descuento = totales.Descuento; Tarifa = totales.Tarifa; Total = totales.Total;
        necesitaValoracion = false;
    }
    public void ExigirEditable()
    { if (Estado != EstadoPedido.BORRADOR) throw new InvalidOperationException("Solo se puede editar un borrador."); }
    public void Transicionar(EstadoPedido destino, string usuario, DateTimeOffset ahora)
    {
        // State: cada objeto de estado conoce sus siguientes estados validos.
        Estados.Obtener(Estado).Exigir(destino);
        // Una cantidad editada no puede validar importes calculados anteriormente.
        if (destino == EstadoPedido.VALIDADO && (necesitaValoracion || Total <= 0))
            throw new InvalidOperationException("Recalcule los importes antes de validar.");
        Estado = destino; Version = Guid.NewGuid();
        if (destino == EstadoPedido.ENTREGADO) EntregadoEn = ahora;
        historial.Add(new(Id, destino, usuario));
    }
    // Prototype: se copian las colecciones y se genera una identidad nueva.
    public Pedido ClonarBorrador()
    {
        ExigirEditable();
        var copia = new Pedido { ClienteId = ClienteId, ClienteNombre = ClienteNombre, ClienteVip = ClienteVip, Entrega = Entrega,
            Subtotal = Subtotal, Descuento = Descuento, Tarifa = Tarifa, Total = Total, necesitaValoracion = necesitaValoracion, detalles = Detalles.Select(d => d.Copiar()).ToList() };
        copia.historial.Add(new(copia.Id, EstadoPedido.BORRADOR, "Clonacion")); return copia;
    }
    public void AplicarCambios(Pedido nuevo)
    {
        ExigirEditable(); nuevo.ExigirEditable();
        ClienteId = nuevo.ClienteId; ClienteNombre = nuevo.ClienteNombre; ClienteVip = nuevo.ClienteVip; Entrega = nuevo.Entrega;
        detalles = nuevo.Detalles.Select(d => d.Copiar()).ToList();
        Subtotal = nuevo.Subtotal; Descuento = nuevo.Descuento; Tarifa = nuevo.Tarifa; Total = nuevo.Total; Version = Guid.NewGuid();
        necesitaValoracion = nuevo.necesitaValoracion;
    }
    public void CambiarCantidad(int indice, int cantidad)
    { ExigirEditable(); detalles[indice] = detalles[indice].ConCantidad(cantidad); necesitaValoracion = true; Version = Guid.NewGuid(); }
}

public sealed class PedidoDetalle
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PedidoId { get; private set; }
    public string Sku { get; private set; } = "";
    public int Cantidad { get; private set; }
    public string ComposicionJson { get; private set; } = "";
    private List<PedidoDetalleComponente> componentes = [];
    public IReadOnlyList<PedidoDetalleComponente> Componentes => componentes.AsReadOnly();
    private PedidoDetalle() { }
    public PedidoDetalle(IElementoCatalogo elemento, int cantidad)
    { Sku = elemento.Sku; Cantidad = cantidad; componentes = elemento.Expandir(cantidad).Select(c => new PedidoDetalleComponente(c)).ToList(); ComposicionJson = System.Text.Json.JsonSerializer.Serialize(InstantaneaElemento.Capturar(elemento)); }
    internal PedidoDetalle Copiar() => new() { Sku = Sku, Cantidad = Cantidad, ComposicionJson = ComposicionJson, componentes = Componentes.Select(c => c.Copiar()).ToList() };
    internal PedidoDetalle ConCantidad(int cantidad)
    {
        if (cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(cantidad));
        return new() { Sku = Sku, Cantidad = cantidad, ComposicionJson = ComposicionJson, componentes = Componentes.Select(c => new PedidoDetalleComponente(new(c.Sku, checked(c.Cantidad / Cantidad * cantidad), c.Precio, c.Peso))).ToList() };
    }
}

// La instantanea conserva tambien el arbol anidado, ademas del desglose relacional.
public sealed record InstantaneaElemento(string Sku, decimal Precio, decimal Peso, IReadOnlyList<InstantaneaHijo> Hijos)
{
    public static InstantaneaElemento Capturar(IElementoCatalogo elemento) => elemento is ProductoSimple producto
        ? new(producto.Sku, producto.Precio, producto.Peso, Array.Empty<InstantaneaHijo>())
        : new(elemento.Sku, elemento.Precio, 0, ((KitCompuesto)elemento).Componentes.Select(c => new InstantaneaHijo(c.Cantidad, Capturar(c.Elemento))).ToArray());
}
public sealed record InstantaneaHijo(int Cantidad, InstantaneaElemento Elemento);

public sealed class PedidoDetalleComponente
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PedidoDetalleId { get; private set; }
    public string Sku { get; private set; } = "";
    public int Cantidad { get; private set; }
    public decimal Precio { get; private set; }
    public decimal Peso { get; private set; }
    private PedidoDetalleComponente() { }
    public PedidoDetalleComponente(ComponenteFisico c) { Sku = c.Sku; Cantidad = c.Cantidad; Precio = c.Precio; Peso = c.Peso; }
    internal PedidoDetalleComponente Copiar() => new(new(Sku, Cantidad, Precio, Peso));
}

public sealed class CambioEstado
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PedidoId { get; private set; }
    public EstadoPedido Estado { get; private set; }
    public string Usuario { get; private set; } = "";
    public DateTimeOffset Fecha { get; private set; } = DateTimeOffset.UtcNow;
    private CambioEstado() { }
    public CambioEstado(Guid pedido, EstadoPedido estado, string usuario) { PedidoId = pedido; Estado = estado; Usuario = usuario; }
}

// Builder: no entrega agregados sin cliente y lineas completas.
public sealed class PedidoBuilder
{
    private Cliente? cliente;
    private TipoEntrega entrega;
    private readonly List<PedidoDetalle> detalles = [];
    public PedidoBuilder Para(Cliente valor) { cliente = valor; return this; }
    public PedidoBuilder ConEntrega(TipoEntrega valor) { entrega = valor; return this; }
    public PedidoBuilder Agregar(IElementoCatalogo elemento, int cantidad) { detalles.Add(new(elemento, cantidad)); return this; }
    public Pedido Construir() => new(cliente ?? throw new InvalidOperationException("Seleccione cliente."), entrega, detalles);
}

public abstract class EstadoBase(params EstadoPedido[] siguientes)
{
    public void Exigir(EstadoPedido destino)
    { if (!siguientes.Contains(destino)) throw new InvalidOperationException($"Transicion no permitida hacia {destino}."); }
}
public sealed class BorradorEstado() : EstadoBase(EstadoPedido.VALIDADO, EstadoPedido.CANCELADO);
public sealed class ValidadoEstado() : EstadoBase(EstadoPedido.PAGADO, EstadoPedido.CANCELADO);
public sealed class PagadoEstado() : EstadoBase(EstadoPedido.DESPACHADO);
public sealed class DespachadoEstado() : EstadoBase(EstadoPedido.ENTREGADO);
public sealed class EntregadoEstado() : EstadoBase;
public sealed class CanceladoEstado() : EstadoBase;
public static class Estados
{
    private static readonly IReadOnlyDictionary<EstadoPedido, EstadoBase> mapa = new Dictionary<EstadoPedido, EstadoBase>
    {
        [EstadoPedido.BORRADOR] = new BorradorEstado(), [EstadoPedido.VALIDADO] = new ValidadoEstado(),
        [EstadoPedido.PAGADO] = new PagadoEstado(), [EstadoPedido.DESPACHADO] = new DespachadoEstado(),
        [EstadoPedido.ENTREGADO] = new EntregadoEstado(), [EstadoPedido.CANCELADO] = new CanceladoEstado()
    };
    public static EstadoBase Obtener(EstadoPedido estado) => mapa[estado];
}
