using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Logistica;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Seguridad;

namespace LogiMarket.Application.Commands;

public interface IComando { string Entrada { get; } Task Ejecutar(); }
public sealed record ConfirmarPedidoCommand(ConfirmacionPedidoFacade Receptor, Guid Id, string Clave) : IComando
{ public string Entrada => $"{Id} {Clave}"; public async Task Ejecutar() => await Receptor.Confirmar(Id, Clave); }
public sealed record CancelarPedidoCommand(ServicioPedidos Receptor, Guid Id) : IComando
{ public string Entrada => Id.ToString(); public Task Ejecutar() => Receptor.Cancelar(Id); }
public sealed record DespacharPedidoCommand(ServicioDespachos Receptor, Guid Id) : IComando
{ public string Entrada => Id.ToString(); public Task Ejecutar() => Receptor.Despachar(Id); }
public sealed record ProcesarDevolucionCommand(ProcesarDevolucion Receptor, Guid Id, bool SinUso) : IComando
{ public string Entrada => $"{Id} sinUso={SinUso}"; public async Task Ejecutar() => await Receptor.Ejecutar(Id, SinUso); }
public sealed class InvocadorComandos(IBitacora bitacora, SesionActual sesion)
{
    public async Task Ejecutar(IComando comando)
    {
        var correlacion = Guid.NewGuid().ToString();
        try { await comando.Ejecutar(); await bitacora.Registrar(sesion.Nombre, comando.GetType().Name, "OK " + comando.Entrada, correlacion); }
        catch (Exception ex) { await bitacora.Registrar(sesion.Nombre, comando.GetType().Name, "ERROR " + comando.Entrada + " " + ex.Message, correlacion); throw; }
    }
}
