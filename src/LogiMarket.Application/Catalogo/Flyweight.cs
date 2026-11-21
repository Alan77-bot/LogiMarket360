namespace LogiMarket.Application.Catalogo;

// Flyweight: solo comparte datos inmutables; cantidad, precio y stock son externos.
public sealed record MetadatosSku(string Sku, string Nombre, string Categoria, string Descripcion);
public sealed class CatalogoVisualFlyweightFactory
{
    private readonly Dictionary<string, MetadatosSku> cache = [];
    public MetadatosSku Obtener(string sku, string nombre) => cache.TryGetValue(sku, out var valor) ? valor : cache[sku] = new(sku, nombre, "Accesorios", "Catalogo de demostracion");
    public int Instancias => cache.Count;
}
