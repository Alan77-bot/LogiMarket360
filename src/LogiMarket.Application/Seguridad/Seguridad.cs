using LogiMarket.Application.Abstractions;
using LogiMarket.Domain.Modelo;

namespace LogiMarket.Application.Seguridad;

public sealed class SesionActual
{
    public Guid? UsuarioId { get; private set; }
    public string Nombre { get; private set; } = "Anonimo";
    public Rol? Rol { get; private set; }
    public void Iniciar(Usuario usuario) { UsuarioId = usuario.Id; Nombre = usuario.Nombre; Rol = usuario.Rol; }
    public void Cerrar() { UsuarioId = null; Nombre = "Anonimo"; Rol = null; }
}
public sealed class Autorizador(SesionActual sesion, IBitacora bitacora)
{
    public async Task Exigir(params Rol[] roles)
    {
        if (sesion.Rol == Rol.ADMIN || sesion.Rol is not null && roles.Contains(sesion.Rol.Value)) return;
        await bitacora.Registrar(sesion.Nombre, "SEGURIDAD", "DENEGADO", Guid.NewGuid().ToString());
        throw new UnauthorizedAccessException("Su rol no permite esta operacion.");
    }
    public Task Lectura() => Exigir(Rol.VENTAS, Rol.BODEGA, Rol.CONSULTA);
}
public sealed class Autenticacion(IRepositorio repositorio, IHashContrasena hash, SesionActual sesion, IBitacora bitacora)
{
    public async Task Entrar(string nombre, string clave)
    {
        var usuario = await repositorio.Usuario(nombre.Trim().ToLowerInvariant());
        var valido = usuario is not null && hash.Verificar(clave, usuario.Hash);
        await bitacora.Registrar(nombre, "ACCESO", valido ? "ACEPTADO" : "RECHAZADO", Guid.NewGuid().ToString());
        if (!valido) throw new UnauthorizedAccessException("Credenciales incorrectas.");
        sesion.Iniciar(usuario!);
    }
}
// Proxy: autoriza antes de alcanzar al decorador o al proveedor.
public sealed class PagoAutorizadoProxy(Autorizador autorizador, IPasarelaPago interno) : IPasarelaPago
{
    public async Task<ResultadoCobro> CobrarAsync(decimal importe, string clave)
    { await autorizador.Exigir(Rol.VENTAS); return await interno.CobrarAsync(importe, clave); }
}
