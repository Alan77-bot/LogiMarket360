using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Catalogo;
using LogiMarket.Application.Clientes;
using LogiMarket.Application.Commands;
using LogiMarket.Application.Eventos;
using LogiMarket.Application.Logistica;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Promociones;
using LogiMarket.Infrastructure.Notifications;
using LogiMarket.Infrastructure.Outbox;
using LogiMarket.Infrastructure.Payments;
using LogiMarket.Infrastructure.Persistence;
using LogiMarket.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Infrastructure.DependencyInjection;

public static class RegistroServicios
{
    public static IServiceCollection AgregarLogiMarket(this IServiceCollection servicios, string ruta)
    {
        servicios.AddDbContextFactory<LogiMarketDbContext>(o => o.UseSqlite($"Data Source={ruta};Default Timeout=30"));
        servicios.AddScoped<RepositorioEf>();
        servicios.AddScoped<IRepositorio>(s => s.GetRequiredService<RepositorioEf>());
        servicios.AddScoped<ITransacciones>(s => s.GetRequiredService<RepositorioEf>());
        servicios.AddScoped<IConsultasOperativas>(s => s.GetRequiredService<RepositorioEf>());
        servicios.AddScoped<IRegistroRecepciones>(s => s.GetRequiredService<RepositorioEf>());
        servicios.AddScoped<IBitacora, BitacoraEf>(); servicios.AddSingleton<IHashContrasena, HashContrasena>(); servicios.AddSingleton<IReloj, RelojSistema>();
        servicios.AddSingleton<SesionActual>(); servicios.AddSingleton<OpcionesDemostracion>(); servicios.AddSingleton<MetricasSesion>();
        servicios.AddScoped<Autorizador>(); servicios.AddScoped<Autenticacion>(); servicios.AddScoped<Inicializador>();
        servicios.AddSingleton<IPoliticaDescuento, Mayorista15>(); servicios.AddSingleton<IPoliticaDescuento, Vip10>(); servicios.AddSingleton<IPoliticaDescuento, SinDescuento>();
        servicios.AddSingleton<CalculadoraPrecios>(); servicios.AddSingleton<IFabricaEntrega, EntregaEstandarFactory>(); servicios.AddSingleton<IFabricaEntrega, EntregaExpresFactory>();
        servicios.AddSingleton<PasarelaLegadaSimulada>(); servicios.AddSingleton<PasarelaLegadaAdapter>(); servicios.AddSingleton<PasarelaAlternativaSimulada>(); servicios.AddSingleton<SelectorPasarela>();
        servicios.AddSingleton<IReembolsosSimulados, ReembolsosSimulados>();
        servicios.AddScoped<IPasarelaPago>(s => new PagoAutorizadoProxy(s.GetRequiredService<Autorizador>(), new AuditoriaPagoDecorator(s.GetRequiredService<SelectorPasarela>(), s.GetRequiredService<IBitacora>(), s.GetRequiredService<SesionActual>())));
        servicios.AddScoped<ServicioCatalogo>(); servicios.AddScoped<ServicioClientes>(); servicios.AddScoped<ServicioPedidos>(); servicios.AddScoped<ConfirmacionPedidoFacade>();
        servicios.AddScoped<ServicioDespachos>(); servicios.AddScoped<ServicioInventario>(); servicios.AddScoped<DevolucionDefecto>(); servicios.AddScoped<DevolucionArrepentimiento>(); servicios.AddScoped<InvocadorComandos>();
        servicios.AddScoped<CreadorEmail>(); servicios.AddScoped<CreadorSms>();
        servicios.AddScoped<ISuscriptorEvento, NotificacionObserver>(); servicios.AddScoped<ISuscriptorEvento, AuditoriaObserver>(); servicios.AddScoped<ISuscriptorEvento, MetricasObserver>();
        servicios.AddScoped<PublicadorEventos>(); servicios.AddScoped<IProcesadorOutbox, ProcesadorOutbox>();
        return servicios;
    }
}
