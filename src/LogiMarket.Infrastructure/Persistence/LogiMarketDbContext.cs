using LogiMarket.Domain.Modelo;
using LogiMarket.Domain.Pedidos;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Persistence;

public sealed class LogiMarketDbContext(DbContextOptions<LogiMarketDbContext> opciones) : DbContext(opciones)
{
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Kit> Kits => Set<Kit>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<Producto>().HasIndex(x => x.Sku).IsUnique();
        m.Entity<Producto>().Property(x => x.Version).IsConcurrencyToken();
        m.Entity<Producto>().ToTable(t => { t.HasCheckConstraint("CK_Producto_Stock", "Stock >= 0"); t.HasCheckConstraint("CK_Producto_Precio", "CAST(Precio AS REAL) > 0"); });
        m.Entity<Cliente>().HasIndex(x => x.Nombre);
        m.Entity<Usuario>().HasIndex(x => x.Nombre).IsUnique();
        m.Entity<Kit>().HasIndex(x => x.Sku).IsUnique();
        m.Entity<Kit>().HasMany(x => x.Componentes).WithOne().HasForeignKey(x => x.KitId).OnDelete(DeleteBehavior.Cascade);
        m.Entity<KitComponente>().HasOne<Producto>().WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<KitComponente>().HasOne<Kit>().WithMany().HasForeignKey(x => x.KitHijoId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<KitComponente>().ToTable(t => t.HasCheckConstraint("CK_KitComponente_Destino", "Cantidad > 0 AND ((ProductoId IS NOT NULL AND KitHijoId IS NULL) OR (ProductoId IS NULL AND KitHijoId IS NOT NULL))"));
        m.Entity<Pedido>().HasOne<Cliente>().WithMany().HasForeignKey(x => x.ClienteId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Pedido>().Property(x => x.Version).IsConcurrencyToken();
        m.Entity<Pedido>().HasMany(x => x.Detalles).WithOne().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Cascade);
        m.Entity<Pedido>().HasMany(x => x.Historial).WithOne().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Cascade);
        m.Entity<PedidoDetalle>().HasMany(x => x.Componentes).WithOne().HasForeignKey(x => x.PedidoDetalleId).OnDelete(DeleteBehavior.Cascade);
        m.Entity<Pago>().HasIndex(x => x.PedidoId).IsUnique();
        m.Entity<Pago>().HasIndex(x => x.Clave).IsUnique();
        m.Entity<Pago>().HasIndex(x => x.Referencia).IsUnique();
        m.Entity<Pago>().HasOne<Pedido>().WithMany().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<MovimientoInventario>().HasOne<Producto>().WithMany().HasForeignKey(x => x.ProductoId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<MovimientoInventario>().HasIndex(x => new { x.ProductoId, x.Referencia, x.Motivo }).IsUnique();
        m.Entity<Despacho>().HasIndex(x => x.PedidoId).IsUnique();
        m.Entity<Despacho>().HasOne<Pedido>().WithMany().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<Devolucion>().HasIndex(x => x.PedidoId).IsUnique();
        m.Entity<Devolucion>().HasIndex(x => x.Referencia).IsUnique();
        m.Entity<Devolucion>().HasOne<Pedido>().WithMany().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<EventoOutbox>().HasOne<Pedido>().WithMany().HasForeignKey(x => x.PedidoId).OnDelete(DeleteBehavior.Restrict);
        m.Entity<RecepcionEvento>().HasKey(x => new { x.EventoId, x.Suscriptor });
        m.Entity<RecepcionEvento>().HasOne<EventoOutbox>().WithMany().HasForeignKey(x => x.EventoId);
        m.Entity<Aviso>().HasIndex(x => x.EventoId).IsUnique();
        m.Entity<Aviso>().HasOne<EventoOutbox>().WithMany().HasForeignKey(x => x.EventoId);
        m.Entity<Auditoria>().HasIndex(x => x.Correlacion);
        // Las identidades nacen en el dominio. EF debe insertar hijos nuevos,
        // aunque su Guid ya tenga valor al agregarlos a una coleccion rastreada.
        foreach (var entidad in m.Model.GetEntityTypes())
            if (entidad.FindProperty("Id") is { ClrType: var tipo } propiedad && tipo == typeof(Guid))
                propiedad.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
    }
}
