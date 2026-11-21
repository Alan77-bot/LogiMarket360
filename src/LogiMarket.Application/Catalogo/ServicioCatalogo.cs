using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Modelo;

namespace LogiMarket.Application.Catalogo;

public sealed record FilaCatalogo(Guid Id, string Sku, string Nombre, decimal Precio, decimal Peso, int Stock, bool EsKit);
public sealed record ComponenteEntrada(Guid Id, bool EsKit, int Cantidad);
public sealed class CatalogoCompuesto
{
    public static IReadOnlyDictionary<string, IElementoCatalogo> Construir(List<Producto> productos, List<Kit> kits)
    {
        var resultado = productos.ToDictionary(p => p.Sku, p => (IElementoCatalogo)new ProductoSimple(p.Sku, p.Nombre, p.Precio, p.Peso));
        var visitando = new HashSet<Guid>();
        IElementoCatalogo ConstruirKit(Kit kit)
        {
            if (resultado.TryGetValue(kit.Sku, out var listo)) return listo;
            if (!visitando.Add(kit.Id)) throw new InvalidOperationException("Referencia circular en kit.");
            var compuesto = new KitCompuesto(kit.Sku);
            foreach (var c in kit.Componentes.OrderBy(c => c.ProductoId is Guid productoId ? productos.Single(p => p.Id == productoId).Sku : kits.Single(k => k.Id == c.KitHijoId).Sku, StringComparer.Ordinal))
                compuesto.Agregar(c.ProductoId is Guid id ? resultado[productos.Single(p => p.Id == id).Sku] : ConstruirKit(kits.Single(k => k.Id == c.KitHijoId)), c.Cantidad);
            if (compuesto.Componentes.Count == 0) throw new InvalidOperationException("Kit sin componentes.");
            visitando.Remove(kit.Id); resultado.Add(kit.Sku, compuesto); return compuesto;
        }
        foreach (var kit in kits) ConstruirKit(kit);
        return resultado;
    }
}
public sealed class ServicioCatalogo(IRepositorio repo, ITransacciones tx, Autorizador auth)
{
    public async Task<List<FilaCatalogo>> Consultar(string filtro = "")
    {
        await auth.Lectura(); var productos = await repo.Productos(); var kits = await repo.Kits();
        var arbol = CatalogoCompuesto.Construir(productos, kits);
        var filas = productos.Select(p => new FilaCatalogo(p.Id, p.Sku, p.Nombre, p.Precio, p.Peso, p.Stock, false)).ToList();
        foreach (var k in kits)
        {
            var elemento = arbol[k.Sku]; var peso = new PesoVisitor(); elemento.Aceptar(peso);
            var desglose = new DesgloseVisitor(); elemento.Aceptar(desglose);
            var stock = desglose.Cantidades.Min(c => productos.Single(p => p.Sku == c.Key).Stock / c.Value);
            filas.Add(new(k.Id, k.Sku, k.Nombre, elemento.Precio, peso.Peso, stock, true));
        }
        return filas.Where(f => $"{f.Sku} {f.Nombre}".Contains(filtro, StringComparison.OrdinalIgnoreCase)).OrderBy(f => f.Sku).ToList();
    }
    public async Task GuardarProducto(Guid? id, string sku, string nombre, decimal precio, decimal peso, int stockInicial)
    {
        await auth.Exigir();
        await tx.Ejecutar(async () => {
            var productos = await repo.Productos(); var kits = await repo.Kits(); sku = sku.Trim().ToUpperInvariant();
            if (productos.Any(p => p.Sku == sku && p.Id != id) || kits.Any(k => k.Sku == sku)) throw new ArgumentException("SKU duplicado.");
            if (id is null) repo.Agregar(new Producto(sku, nombre, precio, peso, stockInicial));
            else
            {
                var producto = productos.Single(p => p.Id == id);
                if (producto.Sku != sku && (await repo.Pedidos()).Any(p => p.Detalles.SelectMany(d => d.Componentes).Any(c => c.Sku == producto.Sku)))
                    throw new InvalidOperationException("No se puede cambiar el SKU utilizado en un pedido.");
                producto.Editar(sku, nombre, precio, peso);
            }
            return true;
        });
    }
    public async Task GuardarKit(Guid? id, string sku, string nombre, IReadOnlyList<ComponenteEntrada> componentes)
    {
        await auth.Exigir();
        await tx.Ejecutar(async () => {
            var productos = await repo.Productos(); var kits = await repo.Kits(); sku = sku.Trim().ToUpperInvariant();
            if (productos.Any(p => p.Sku == sku) || kits.Any(k => k.Sku == sku && k.Id != id)) throw new ArgumentException("SKU duplicado.");
            var kit = id is null ? new Kit(sku, nombre) : kits.Single(k => k.Id == id);
            kit.Renombrar(sku, nombre); kit.Reemplazar(componentes.Select(c => new KitComponente(kit.Id, c.EsKit ? null : c.Id, c.EsKit ? c.Id : null, c.Cantidad)));
            if (id is null) { kits.Add(kit); repo.Agregar(kit); }
            CatalogoCompuesto.Construir(productos, kits); return true;
        });
    }
    public async Task Eliminar(Guid id, bool esKit)
    {
        await auth.Exigir(); await tx.Ejecutar(async () => {
            if (esKit) repo.Eliminar((await repo.Kits()).Single(k => k.Id == id));
            else
            {
                var producto = (await repo.Productos()).Single(p => p.Id == id);
                if ((await repo.Pedidos()).Any(p => p.Detalles.SelectMany(d => d.Componentes).Any(c => c.Sku == producto.Sku))) throw new InvalidOperationException("Producto utilizado en pedidos.");
                repo.Eliminar(producto);
            }
            return true;
        });
    }
}
