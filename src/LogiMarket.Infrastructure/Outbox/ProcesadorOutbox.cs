using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Eventos;
using LogiMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Outbox;

public sealed class ProcesadorOutbox(LogiMarketDbContext db, PublicadorEventos publicador) : IProcesadorOutbox
{
    public async Task<int> Procesar()
    {
        var cantidad = 0;
        // Un bloqueo de escritura evita que dos procesadores publiquen a la vez.
        var repo = new RepositorioEf(db);
        await repo.Ejecutar(async () => {
            var eventos = await db.Set<LogiMarket.Domain.Modelo.EventoOutbox>().Where(e => e.ProcesadoEn == null).ToListAsync();
            foreach (var evento in eventos.OrderBy(e => e.Fecha))
            {
                try { await publicador.Publicar(evento); evento.Procesado(); cantidad++; }
                catch (Exception ex) { evento.Fallido(ex.Message); }
                await db.SaveChangesAsync();
            }
            return true;
        });
        return cantidad;
    }
}
