using LogiMarket.Application.Abstractions;
using LogiMarket.Domain.Modelo;

namespace LogiMarket.Infrastructure.Notifications;

public interface INotificador { Task Enviar(EventoOutbox evento); }
// Factory Method: los creadores especializados producen el notificador concreto.
public abstract class CreadorNotificador
{
    public abstract INotificador Crear();
    public Task Notificar(EventoOutbox evento) => Crear().Enviar(evento);
}
public sealed class CreadorEmail(IRepositorio repo) : CreadorNotificador
{ public override INotificador Crear() => new NotificadorEmail(repo); }
public sealed class CreadorSms(IRepositorio repo) : CreadorNotificador
{ public override INotificador Crear() => new NotificadorSms(repo); }
public sealed class NotificadorEmail(IRepositorio repo) : INotificador
{ public Task Enviar(EventoOutbox evento) { repo.Agregar(new Aviso(evento.Id, "EMAIL", $"Correo simulado: {evento.Tipo}, pedido {evento.PedidoId}")); return Task.CompletedTask; } }
public sealed class NotificadorSms(IRepositorio repo) : INotificador
{ public Task Enviar(EventoOutbox evento) { repo.Agregar(new Aviso(evento.Id, "SMS", $"SMS simulado: {evento.Tipo}, pedido {evento.PedidoId}")); return Task.CompletedTask; } }
public sealed class NotificacionObserver(CreadorEmail email, CreadorSms sms, OpcionesDemostracion opciones) : ISuscriptorEvento
{
    public string Nombre => "NOTIFICACION";
    public Task Recibir(EventoOutbox evento)
    {
        if (opciones.FallarNotificacion) throw new InvalidOperationException("Fallo de notificacion simulado.");
        return (opciones.Canal == "SMS" ? (CreadorNotificador)sms : email).Notificar(evento);
    }
}
