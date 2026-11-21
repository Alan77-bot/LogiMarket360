using LogiMarket.Domain.Modelo;
using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Application.Abstractions;

public interface IRepositorio
{
    Task<List<Producto>> Productos();
    Task<List<Kit>> Kits();
    Task<List<Cliente>> Clientes();
    Task<List<Pedido>> Pedidos();
    Task<Pedido> Pedido(Guid id);
    Task<Usuario?> Usuario(string nombre);
    Task<Pago?> PagoPorClave(string clave);
    Task<Pago?> PagoPorPedido(Guid pedido);
    Task<Devolucion?> DevolucionPorPedido(Guid pedido);
    void Agregar<T>(T entidad) where T : class;
    void Eliminar<T>(T entidad) where T : class;
    Task Guardar();
}
public interface ITransacciones
{
    Task<T> Ejecutar<T>(Func<Task<T>> operacion);
}
public interface IConsultasOperativas
{
    Task<List<MovimientoInventario>> Movimientos();
    Task<List<Auditoria>> Auditorias();
    Task<List<EventoOutbox>> Eventos();
    Task<List<Despacho>> Despachos();
    Task<List<Devolucion>> Devoluciones();
    Task<List<Aviso>> Avisos();
}
public interface IBitacora
{ Task Registrar(string usuario, string operacion, string resultado, string correlacion); }
public interface IHashContrasena
{ string Crear(string clave); bool Verificar(string clave, string hash); }
public interface IReloj { DateTimeOffset Ahora { get; } }
public interface IPasarelaPago
{ Task<ResultadoCobro> CobrarAsync(decimal importe, string clave); }
public interface IReembolsosSimulados
{ Task<string> ReembolsarAsync(Guid pedido, decimal importe); }
public sealed record ResultadoCobro(bool Aceptado, string Referencia, string Mensaje);
public interface IProcesadorOutbox { Task<int> Procesar(); }
public interface ISuscriptorEvento
{ string Nombre { get; } Task Recibir(EventoOutbox evento); }
public interface IRegistroRecepciones
{ Task<bool> Recibido(Guid evento, string suscriptor); void Marcar(Guid evento, string suscriptor); }

public sealed class OpcionesDemostracion
{
    public bool RechazarCobro { get; set; }
    public bool FallarNotificacion { get; set; }
    public bool ProveedorAlternativo { get; set; }
    public string Canal { get; set; } = "EMAIL";
}
