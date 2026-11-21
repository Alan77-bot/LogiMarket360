using System.Security.Cryptography;
using LogiMarket.Application.Abstractions;
using LogiMarket.Domain.Modelo;
using LogiMarket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LogiMarket.Infrastructure.Security;

public sealed class HashContrasena : IHashContrasena
{
    private const int Iteraciones = 210000;
    public string Crear(string clave)
    {
        var sal = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(clave, sal, Iteraciones, HashAlgorithmName.SHA256, 32);
        return $"{Iteraciones}:{Convert.ToBase64String(sal)}:{Convert.ToBase64String(hash)}";
    }
    public bool Verificar(string clave, string hash)
    {
        try { var partes = hash.Split(':'); var esperado = Convert.FromBase64String(partes[2]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(clave, Convert.FromBase64String(partes[1]), int.Parse(partes[0]), HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(actual, esperado); }
        catch (Exception ex) when (ex is FormatException or IndexOutOfRangeException or ArgumentException) { return false; }
    }
}
public sealed class RelojSistema : IReloj { public DateTimeOffset Ahora => DateTimeOffset.UtcNow; }
public sealed class BitacoraEf(IDbContextFactory<LogiMarketDbContext> fabrica, LogiMarketDbContext actual) : IBitacora
{
    public async Task Registrar(string usuario, string operacion, string resultado, string correlacion)
    {
        if (actual.Database.CurrentTransaction is not null)
        { actual.Add(new Auditoria(usuario, operacion, resultado, correlacion)); return; }
        // Fuera de la transaccion se guardan tambien intentos denegados.
        await using var db = await fabrica.CreateDbContextAsync();
        db.Add(new Auditoria(usuario, operacion, resultado, correlacion)); await db.SaveChangesAsync();
    }
}
