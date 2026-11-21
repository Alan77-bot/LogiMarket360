namespace LogiMarket.Domain.Modelo;

public enum Rol { ADMIN, VENTAS, BODEGA, CONSULTA }
public enum CategoriaCliente { NORMAL, VIP }
public enum EstadoPedido { BORRADOR, VALIDADO, PAGADO, DESPACHADO, ENTREGADO, CANCELADO }
public enum TipoEntrega { ESTANDAR, EXPRES }
public enum MotivoDevolucion { DEFECTO, ARREPENTIMIENTO }

public sealed class Producto
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Sku { get; private set; } = "";
    public string Nombre { get; private set; } = "";
    public decimal Precio { get; private set; }
    public decimal Peso { get; private set; }
    public int Stock { get; private set; }
    public Guid Version { get; private set; } = Guid.NewGuid();
    private Producto() { }
    public Producto(string sku, string nombre, decimal precio, decimal peso, int stock)
    { Editar(sku, nombre, precio, peso); if (stock < 0) throw new ArgumentException("Stock invalido."); Stock = stock; }
    public void Editar(string sku, string nombre, decimal precio, decimal peso)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku); ArgumentException.ThrowIfNullOrWhiteSpace(nombre);
        if (precio <= 0 || decimal.Round(precio, 2) != precio || peso < 0) throw new ArgumentException("Precio positivo con dos decimales y peso no negativo requeridos.");
        Sku = sku.Trim().ToUpperInvariant(); Nombre = nombre.Trim(); Precio = precio; Peso = peso; Version = Guid.NewGuid();
    }
    public void Mover(int cantidad)
    {
        if (cantidad == 0 || (long)Stock + cantidad < 0 || (long)Stock + cantidad > int.MaxValue) throw new InvalidOperationException($"Stock insuficiente o movimiento invalido: {Sku}.");
        Stock = checked(Stock + cantidad); Version = Guid.NewGuid();
    }
}

public sealed class Cliente
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nombre { get; private set; } = "";
    public CategoriaCliente Categoria { get; private set; }
    public bool Activo { get; private set; } = true;
    private Cliente() { }
    public Cliente(string nombre, CategoriaCliente categoria) => Editar(nombre, categoria, true);
    public void Editar(string nombre, CategoriaCliente categoria, bool activo)
    { ArgumentException.ThrowIfNullOrWhiteSpace(nombre); Nombre = nombre.Trim(); Categoria = categoria; Activo = activo; }
}

public sealed class Usuario
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nombre { get; private set; } = "";
    public string Hash { get; private set; } = "";
    public Rol Rol { get; private set; }
    private Usuario() { }
    public Usuario(string nombre, string hash, Rol rol) { Nombre = nombre; Hash = hash; Rol = rol; }
}

public sealed class Kit
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Sku { get; private set; } = "";
    public string Nombre { get; private set; } = "";
    private List<KitComponente> componentes = [];
    public IReadOnlyList<KitComponente> Componentes => componentes.AsReadOnly();
    private Kit() { }
    public Kit(string sku, string nombre) { Renombrar(sku, nombre); }
    public void Renombrar(string sku, string nombre)
    { ArgumentException.ThrowIfNullOrWhiteSpace(sku); ArgumentException.ThrowIfNullOrWhiteSpace(nombre); Sku = sku.Trim().ToUpperInvariant(); Nombre = nombre.Trim(); }
    public void Reemplazar(IEnumerable<KitComponente> componentes)
    { var nuevos = componentes.ToList(); if (nuevos.Count == 0) throw new ArgumentException("El kit necesita componentes."); this.componentes = nuevos; }
}

public sealed class KitComponente
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid KitId { get; private set; }
    public Guid? ProductoId { get; private set; }
    public Guid? KitHijoId { get; private set; }
    public int Cantidad { get; private set; }
    private KitComponente() { }
    public KitComponente(Guid kitId, Guid? productoId, Guid? kitHijoId, int cantidad)
    { if ((productoId is null) == (kitHijoId is null) || cantidad <= 0) throw new ArgumentException("Componente invalido."); KitId = kitId; ProductoId = productoId; KitHijoId = kitHijoId; Cantidad = cantidad; }
}
