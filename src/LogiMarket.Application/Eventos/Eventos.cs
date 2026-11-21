using System.Collections.Concurrent;
using LogiMarket.Application.Abstractions;
using LogiMarket.Domain.Modelo;

namespace LogiMarket.Application.Eventos;

// Singleton por DI; los identificadores previenen contar dos veces el mismo evento.
public sealed class MetricasSesion
{
    private readonly ConcurrentDictionary<Guid, string> eventos = new();
    public void Registrar(EventoOutbox evento) => eventos.TryAdd(evento.Id, evento.Tipo);
    public int Pagos => eventos.Values.Count(t => t == "PedidoPagado");
    public int Despachos => eventos.Values.Count(t => t == "DespachoRegistrado");
    public int Devoluciones => eventos.Values.Count(t => t == "DevolucionAceptada");
}
// Observer: el publicador desconoce los concretos que atienden el evento.
public sealed class PublicadorEventos(IEnumerable<ISuscriptorEvento> suscriptores, IRegistroRecepciones recepciones, IRepositorio repo)
{
    public async Task Publicar(EventoOutbox evento)
    {
        foreach (var suscriptor in suscriptores)
        {
            if (await recepciones.Recibido(evento.Id, suscriptor.Nombre)) continue;
            await suscriptor.Recibir(evento); recepciones.Marcar(evento.Id, suscriptor.Nombre); await repo.Guardar();
        }
    }
}
public sealed class MetricasObserver(MetricasSesion metricas) : ISuscriptorEvento
{
    public string Nombre => "METRICAS";
    public Task Recibir(EventoOutbox evento) { metricas.Registrar(evento); return Task.CompletedTask; }
}
public sealed class AuditoriaObserver(IRepositorio repo) : ISuscriptorEvento
{
    public string Nombre => "AUDITORIA";
    public Task Recibir(EventoOutbox evento)
    { repo.Agregar(new Auditoria("OUTBOX", evento.Tipo, "PUBLICADO", evento.Id.ToString())); return Task.CompletedTask; }
}
