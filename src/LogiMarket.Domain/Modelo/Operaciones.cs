namespace LogiMarket.Domain.Modelo;

public sealed class Pago
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PedidoId { get; private set; }
    public string Clave { get; private set; } = "";
    public string Referencia { get; private set; } = "";
    public decimal Importe { get; private set; }
    public DateTimeOffset Fecha { get; private set; } = DateTimeOffset.UtcNow;
    private Pago() { }
    public Pago(Guid pedido, string clave, string referencia, decimal importe)
    { PedidoId = pedido; Clave = clave; Referencia = referencia; Importe = importe; }
}
public sealed class MovimientoInventario
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ProductoId { get; private set; }
    public int Cantidad { get; private set; }
    public string Motivo { get; private set; } = "";
    public string Referencia { get; private set; } = "";
    public DateTimeOffset Fecha { get; private set; } = DateTimeOffset.UtcNow;
    private MovimientoInventario() { }
    public MovimientoInventario(Guid producto, int cantidad, string motivo, string referencia)
    { ProductoId = producto; Cantidad = cantidad; Motivo = motivo; Referencia = referencia; }
}
public sealed class Despacho
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PedidoId { get; private set; }
    public string Guia { get; private set; } = "";
    public DateTimeOffset Fecha { get; private set; }
    public DateTimeOffset Estimada { get; private set; }
    private Despacho() { }
    public Despacho(Guid pedido, string guia, DateTimeOffset fecha, DateTimeOffset estimada)
    { PedidoId = pedido; Guia = guia; Fecha = fecha; Estimada = estimada; }
}
public sealed class Devolucion
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PedidoId { get; private set; }
    public MotivoDevolucion Motivo { get; private set; }
    public decimal Reembolso { get; private set; }
    public string Referencia { get; private set; } = "";
    public DateTimeOffset Fecha { get; private set; }
    private Devolucion() { }
    public Devolucion(Guid pedido, MotivoDevolucion motivo, decimal reembolso, string referencia, DateTimeOffset fecha)
    { PedidoId = pedido; Motivo = motivo; Reembolso = reembolso; Referencia = referencia; Fecha = fecha; }
}
public sealed class EventoOutbox
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid PedidoId { get; private set; }
    public string Tipo { get; private set; } = "";
    public DateTimeOffset Fecha { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcesadoEn { get; private set; }
    public int Intentos { get; private set; }
    public string? Error { get; private set; }
    private EventoOutbox() { }
    public EventoOutbox(Guid pedido, string tipo) { PedidoId = pedido; Tipo = tipo; }
    public void Procesado() { ProcesadoEn = DateTimeOffset.UtcNow; Error = null; }
    public void Fallido(string error) { Intentos++; Error = error; }
}
public sealed class Auditoria
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Usuario { get; private set; } = "";
    public string Operacion { get; private set; } = "";
    public string Resultado { get; private set; } = "";
    public string Correlacion { get; private set; } = "";
    public DateTimeOffset Fecha { get; private set; } = DateTimeOffset.UtcNow;
    private Auditoria() { }
    public Auditoria(string usuario, string operacion, string resultado, string correlacion)
    { Usuario = usuario; Operacion = operacion; Resultado = resultado; Correlacion = correlacion; }
}
public sealed class RecepcionEvento
{
    public Guid EventoId { get; private set; }
    public string Suscriptor { get; private set; } = "";
    private RecepcionEvento() { }
    public RecepcionEvento(Guid evento, string suscriptor) { EventoId = evento; Suscriptor = suscriptor; }
}
public sealed class Aviso
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid EventoId { get; private set; }
    public string Canal { get; private set; } = "";
    public string Mensaje { get; private set; } = "";
    private Aviso() { }
    public Aviso(Guid evento, string canal, string mensaje) { EventoId = evento; Canal = canal; Mensaje = mensaje; }
}
