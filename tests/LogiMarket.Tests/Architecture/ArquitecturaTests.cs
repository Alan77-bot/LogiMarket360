using LogiMarket.Application.Catalogo;
using LogiMarket.Domain.Modelo;
using LogiMarket.Infrastructure.Persistence;

namespace LogiMarket.Tests.Architecture;

public class ArquitecturaTests
{
    [Fact]
    public void P38_DependenciasInternasSinFrameworksDePersistenciaNiUi()
    {
        var dominio = typeof(Producto).Assembly.GetReferencedAssemblies().Select(a => a.Name!).ToArray();
        Assert.DoesNotContain(dominio, n => n.StartsWith("LogiMarket.") || n.Contains("EntityFramework") || n.Contains("Windows.Forms"));
        var aplicacion = typeof(ServicioCatalogo).Assembly.GetReferencedAssemblies().Select(a => a.Name!).ToArray();
        Assert.DoesNotContain(aplicacion, n => n.Contains("Infrastructure") || n.Contains("EntityFramework") || n.Contains("Windows.Forms"));
        Assert.Contains("LogiMarket.Domain", aplicacion); Assert.Contains("LogiMarket.Application", typeof(LogiMarketDbContext).Assembly.GetReferencedAssemblies().Select(a => a.Name));
    }
    [Fact]
    public void P21_MilFilasCompartenIdentidadConComparacionEquivalente()
    {
        var fabrica = new CatalogoVisualFlyweightFactory();
        var antes = GC.GetAllocatedBytesForCurrentThread();
        var sin = Enumerable.Range(0, 1000).Select(i => new Fila(i, new("P01", "Mouse USB", "Accesorios", "Catalogo de demostracion"), 1, 20)).ToArray();
        var bytesSin = GC.GetAllocatedBytesForCurrentThread() - antes;
        antes = GC.GetAllocatedBytesForCurrentThread();
        var con = Enumerable.Range(0, 1000).Select(i => new Fila(i, fabrica.Obtener("P01", "Mouse USB"), 1, 20)).ToArray();
        var bytesCon = GC.GetAllocatedBytesForCurrentThread() - antes;
        Assert.Same(con[0].Metadatos, con[999].Metadatos); Assert.NotSame(sin[0].Metadatos, sin[999].Metadatos); Assert.Single(con.Select(f => f.Metadatos).Distinct(ReferenceEqualityComparer.Instance));
        Assert.Equal(1, fabrica.Instancias); Assert.True(bytesCon < bytesSin); GC.KeepAlive(sin);
    }
    private sealed record Fila(int Indice, MetadatosSku Metadatos, int Cantidad, decimal Precio);
}
