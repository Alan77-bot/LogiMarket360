namespace LogiMarket.Domain.Catalogo;

// Composite: productos y kits comparten el mismo contrato.
public interface IElementoCatalogo
{
    string Sku { get; }
    decimal Precio { get; }
    IEnumerable<ComponenteFisico> Expandir(int cantidad = 1);
    void Aceptar(IVisitanteCatalogo visitante, int cantidad = 1);
}

public sealed record ComponenteFisico(string Sku, int Cantidad, decimal Precio, decimal Peso);

public sealed class ProductoSimple : IElementoCatalogo
{
    public void Aceptar(IVisitanteCatalogo visitante, int cantidad = 1) => visitante.Visitar(this, cantidad);
    public string Sku { get; }
    public string Nombre { get; }
    public decimal Precio { get; }
    public decimal Peso { get; }

    public ProductoSimple(string sku, string nombre, decimal precio, decimal peso)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        if (precio <= 0 || peso < 0) throw new ArgumentException("Precio o peso invalido.");
        Sku = sku; Nombre = nombre; Precio = precio; Peso = peso;
    }

    public IEnumerable<ComponenteFisico> Expandir(int cantidad = 1)
    {
        if (cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(cantidad));
        yield return new(Sku, cantidad, Precio, Peso);
    }
}

public sealed class KitCompuesto : IElementoCatalogo
{
    public IReadOnlyList<(IElementoCatalogo Elemento, int Cantidad)> Componentes => componentes.AsReadOnly();
    public void Aceptar(IVisitanteCatalogo visitante, int cantidad = 1) => visitante.Visitar(this, cantidad);
    private readonly List<(IElementoCatalogo Elemento, int Cantidad)> componentes = [];
    public string Sku { get; }
    public decimal Precio => componentes.Sum(c => c.Elemento.Precio * c.Cantidad);

    public KitCompuesto(string sku)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        Sku = sku;
    }

    public void Agregar(IElementoCatalogo elemento, int cantidad = 1)
    {
        ArgumentNullException.ThrowIfNull(elemento);
        if (cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(cantidad));
        // Se comprueba el arbol antes de modificar la composicion.
        if (Contiene(elemento, Sku)) throw new InvalidOperationException("El kit produciria un ciclo.");
        componentes.Add((elemento, cantidad));
    }

    private static bool Contiene(IElementoCatalogo elemento, string sku) =>
        elemento.Sku == sku || elemento is KitCompuesto kit && kit.componentes.Any(c => Contiene(c.Elemento, sku));

    public IEnumerable<ComponenteFisico> Expandir(int cantidad = 1)
    {
        if (cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(cantidad));
        using var iterador = new IteradorFisico(this, cantidad);
        while (iterador.MoveNext()) yield return iterador.Current;
    }
}
