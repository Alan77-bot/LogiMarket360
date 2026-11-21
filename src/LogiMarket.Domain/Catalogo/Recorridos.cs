using System.Collections;

namespace LogiMarket.Domain.Catalogo;

// Iterator: pila propia, orden estable y cantidades propagadas sin exponer listas.
public sealed class IteradorFisico : IEnumerator<ComponenteFisico>
{
    private readonly Stack<(IElementoCatalogo Elemento, int Cantidad)> pendientes = new();
    public ComponenteFisico Current { get; private set; } = null!;
    object IEnumerator.Current => Current;
    public IteradorFisico(IElementoCatalogo raiz, int cantidad = 1)
    { if (cantidad <= 0) throw new ArgumentOutOfRangeException(nameof(cantidad)); pendientes.Push((raiz, cantidad)); }
    public bool MoveNext()
    {
        while (pendientes.TryPop(out var item))
        {
            if (item.Elemento is ProductoSimple producto)
            { Current = new(producto.Sku, item.Cantidad, producto.Precio, producto.Peso); return true; }
            var kit = (KitCompuesto)item.Elemento;
            foreach (var hijo in kit.Componentes.Reverse()) pendientes.Push((hijo.Elemento, checked(hijo.Cantidad * item.Cantidad)));
        }
        return false;
    }
    public void Reset() => throw new NotSupportedException("Cree otro iterador.");
    public void Dispose() => pendientes.Clear();
}
public interface IVisitanteCatalogo
{
    void Visitar(ProductoSimple producto, int cantidad);
    void Visitar(KitCompuesto kit, int cantidad);
}
public sealed class PesoVisitor : IVisitanteCatalogo
{
    public decimal Peso { get; private set; }
    public void Visitar(ProductoSimple producto, int cantidad) => Peso += producto.Peso * cantidad;
    public void Visitar(KitCompuesto kit, int cantidad)
    { foreach (var hijo in kit.Componentes) hijo.Elemento.Aceptar(this, checked(cantidad * hijo.Cantidad)); }
}
public sealed class DesgloseVisitor : IVisitanteCatalogo
{
    private readonly SortedDictionary<string, int> cantidades = new(StringComparer.Ordinal);
    public IReadOnlyDictionary<string, int> Cantidades => cantidades;
    public void Visitar(ProductoSimple producto, int cantidad) => cantidades[producto.Sku] = cantidades.GetValueOrDefault(producto.Sku) + cantidad;
    public void Visitar(KitCompuesto kit, int cantidad)
    { foreach (var hijo in kit.Componentes) hijo.Elemento.Aceptar(this, checked(cantidad * hijo.Cantidad)); }
}
