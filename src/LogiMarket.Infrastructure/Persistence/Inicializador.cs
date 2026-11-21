using LogiMarket.Application.Abstractions;
using LogiMarket.Domain.Modelo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LogiMarket.Infrastructure.Persistence;

public sealed class Inicializador(LogiMarketDbContext db, IHashContrasena hash)
{
    public async Task Ejecutar()
    {
        await db.Database.MigrateAsync();
        var repo = new RepositorioEf(db);
        await repo.Ejecutar(async () => {
            // La semilla se instala una vez; no resucita datos borrados por el usuario.
            if (await db.Set<Auditoria>().AnyAsync(a => a.Operacion == "SEED_INICIAL")) return true;
            var datos = new[] { ("P01", "Mouse USB", 20m, 8, 0.20m), ("P02", "Teclado", 30m, 5, 0.70m),
                ("P03", "Memoria USB", 10m, 20, 0.10m), ("P04", "Webcam", 40m, 4, 0.35m), ("P05", "Alfombrilla", 5m, 30, 0.10m) };
            foreach (var d in datos) if (!await db.Productos.AnyAsync(p => p.Sku == d.Item1)) db.Add(new Producto(d.Item1, d.Item2, d.Item3, d.Item5, d.Item4));
            await db.SaveChangesAsync();
            var productos = await db.Productos.ToListAsync();
            var oficina = await db.Kits.SingleOrDefaultAsync(k => k.Sku == "KIT_OFICINA");
            if (oficina is null) { oficina = new Kit("KIT_OFICINA", "Kit oficina"); oficina.Reemplazar(new[] { "P01", "P02" }.Select(s => new KitComponente(oficina.Id, productos.Single(p => p.Sku == s).Id, null, 1))); db.Add(oficina); await db.SaveChangesAsync(); }
            if (!await db.Kits.AnyAsync(k => k.Sku == "KIT_TELETRABAJO"))
            { var tele = new Kit("KIT_TELETRABAJO", "Kit teletrabajo"); tele.Reemplazar([new(tele.Id, null, oficina.Id, 1), new(tele.Id, productos.Single(p => p.Sku == "P03").Id, null, 1)]); db.Add(tele); }
            foreach (var rol in Enum.GetValues<Rol>())
            { var nombre = rol.ToString().ToLowerInvariant(); if (!await db.Usuarios.AnyAsync(u => u.Nombre == nombre)) db.Add(new Usuario(nombre, hash.Crear("Demo123!"), rol)); }
            foreach (var (nombre, categoria) in new[] { ("Ana", CategoriaCliente.VIP), ("Luis", CategoriaCliente.NORMAL), ("Carmen", CategoriaCliente.NORMAL) })
                if (!await db.Clientes.AnyAsync(c => c.Nombre == nombre)) db.Add(new Cliente(nombre, categoria));
            db.Add(new Auditoria("SISTEMA", "SEED_INICIAL", "INSTALADO", "SEED-V1"));
            return true;
        });
    }
}

// Permite generar migraciones con dotnet ef sin iniciar la interfaz.
public sealed class FabricaDiseno : IDesignTimeDbContextFactory<LogiMarketDbContext>
{
    public LogiMarketDbContext CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<LogiMarketDbContext>().UseSqlite("Data Source=logimarket-design.db").Options);
}
