using System.Globalization;
using LogiMarket.Domain.Modelo;
using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Application.Comprobantes;

public interface ISalidaDocumento { Task Emitir(string nombre, string contenido); }
// Bridge: el comprobante y el medio de salida evolucionan por separado.
public abstract class Comprobante(ISalidaDocumento salida)
{
    protected static string Dinero(decimal importe) => importe.ToString("F2", CultureInfo.InvariantCulture);
    public Task Exportar() => salida.Emitir(Nombre, Contenido());
    protected abstract string Nombre { get; }
    public abstract string Contenido();
}
public sealed class ComprobanteVenta(Pedido pedido, ISalidaDocumento salida) : Comprobante(salida)
{
    protected override string Nombre => $"venta-{pedido.Id:N}";
    public override string Contenido() => $"LOGIMARKET 360 - COMPROBANTE INTERNO\nPedido: {pedido.Id}\nCliente: {pedido.ClienteNombre}\n" +
        string.Join("\n", pedido.Detalles.Select(d => $"{d.Sku} x{d.Cantidad}")) +
        $"\nSubtotal: USD {Dinero(pedido.Subtotal)}\nDescuento: USD {Dinero(pedido.Descuento)}\nEnvio: USD {Dinero(pedido.Tarifa)}\nTotal: USD {Dinero(pedido.Total)}\nDocumento de simulacion sin validez tributaria.";
}
public sealed class ComprobanteDevolucion(Devolucion devolucion, ISalidaDocumento salida) : Comprobante(salida)
{
    protected override string Nombre => $"devolucion-{devolucion.Id:N}";
    public override string Contenido() => $"LOGIMARKET 360 - DEVOLUCION TOTAL\nPedido: {devolucion.PedidoId}\nMotivo: {devolucion.Motivo}\nReembolso simulado: USD {Dinero(devolucion.Reembolso)}\nReferencia: {devolucion.Referencia}\nNo incluye tarifa de envio.";
}
