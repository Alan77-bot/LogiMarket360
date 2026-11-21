using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Comprobantes;
using LogiMarket.Application.Eventos;
using LogiMarket.Application.Seguridad;
using LogiMarket.Infrastructure.DependencyInjection;
using LogiMarket.Infrastructure.Documents;
using LogiMarket.Infrastructure.Persistence;
using LogiMarket.WinForms.Shared;
using LogiMarket.WinForms.Shell;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        // Composition root: las implementaciones concretas se eligen solo aqui.
        var ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LogiMarket360");
        Directory.CreateDirectory(ruta);
        var servicios = new ServiceCollection().AgregarLogiMarket(Path.Combine(ruta, "logimarket.db"));
        servicios.AddSingleton<ISalidaDocumento>(new SalidaArchivoTxt(Path.Combine(ruta, "comprobantes")));
        using var proveedor = servicios.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        try
        {
            using (var scope = proveedor.CreateScope()) scope.ServiceProvider.GetRequiredService<Inicializador>().Ejecutar().GetAwaiter().GetResult();
            var operaciones = new OperacionesUi(proveedor); var sesion = proveedor.GetRequiredService<SesionActual>();
            while (true)
            {
                using var acceso = new FormularioAcceso(operaciones); if (acceso.ShowDialog() != DialogResult.OK) break;
                using var principal = new FormularioPrincipal(operaciones, sesion, proveedor.GetRequiredService<MetricasSesion>(), proveedor.GetRequiredService<OpcionesDemostracion>(), proveedor.GetRequiredService<ISalidaDocumento>());
                System.Windows.Forms.Application.Run(principal); sesion.Cerrar();
            }
        }
        catch (Exception ex) { MessageBox.Show(ex.GetBaseException().Message, "Error de inicio", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
