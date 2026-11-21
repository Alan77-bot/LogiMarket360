using System.Collections.Concurrent;
using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Seguridad;

namespace LogiMarket.Infrastructure.Payments;

// El legado solo acepta centavos; el adaptador conserva su firma incompatible.
public sealed class PasarelaLegadaSimulada
{
    private readonly ConcurrentDictionary<string, (long Importe, string Referencia)> cobros = new();
    public string CobrarCentavos(long centavos, string clave)
    {
        if (centavos <= 0) throw new ArgumentException("Importe invalido.");
        var cobro = cobros.GetOrAdd(clave, _ => (centavos, $"LEG-{Guid.NewGuid():N}"));
        if (cobro.Importe != centavos) throw new InvalidOperationException("Clave reutilizada con otro importe.");
        return cobro.Referencia;
    }
}
public sealed class PasarelaLegadaAdapter(PasarelaLegadaSimulada legado, OpcionesDemostracion opciones) : IPasarelaPago
{
    public Task<ResultadoCobro> CobrarAsync(decimal importe, string clave)
    {
        Validar(importe, clave);
        return Task.FromResult(opciones.RechazarCobro ? new ResultadoCobro(false, "", "Cobro rechazado por simulacion.") : new(true, legado.CobrarCentavos(checked((long)(importe * 100)), clave), "Aceptado"));
    }
    internal static void Validar(decimal importe, string clave)
    { ArgumentException.ThrowIfNullOrWhiteSpace(clave); if (importe <= 0 || decimal.Round(importe, 2) != importe) throw new ArgumentException("Importe invalido."); }
}
public sealed class PasarelaAlternativaSimulada(OpcionesDemostracion opciones) : IPasarelaPago
{
    private readonly ConcurrentDictionary<string, (decimal Importe, string Referencia)> cobros = new();
    public Task<ResultadoCobro> CobrarAsync(decimal importe, string clave)
    {
        PasarelaLegadaAdapter.Validar(importe, clave);
        if (opciones.RechazarCobro) return Task.FromResult(new ResultadoCobro(false, "", "Cobro rechazado por simulacion."));
        var cobro = cobros.GetOrAdd(clave, _ => (importe, $"ALT-{Guid.NewGuid():N}"));
        if (cobro.Importe != importe) throw new InvalidOperationException("Clave reutilizada con otro importe.");
        return Task.FromResult(new ResultadoCobro(true, cobro.Referencia, "Aceptado"));
    }
}
public sealed class SelectorPasarela(PasarelaLegadaAdapter legado, PasarelaAlternativaSimulada alternativo, OpcionesDemostracion opciones) : IPasarelaPago
{
    public Task<ResultadoCobro> CobrarAsync(decimal importe, string clave) =>
        (opciones.ProveedorAlternativo ? (IPasarelaPago)alternativo : legado).CobrarAsync(importe, clave);
}
// Decorator: agrega trazas sin cambiar el contrato del proveedor.
public sealed class AuditoriaPagoDecorator(IPasarelaPago interno, IBitacora bitacora, SesionActual sesion) : IPasarelaPago
{
    public async Task<ResultadoCobro> CobrarAsync(decimal importe, string clave)
    {
        await bitacora.Registrar(sesion.Nombre, "COBRO", "INICIO", clave);
        try {
            var resultado = await interno.CobrarAsync(importe, clave);
            await bitacora.Registrar(sesion.Nombre, "COBRO", resultado.Aceptado ? "FIN" : "ERROR: " + resultado.Mensaje, clave); return resultado;
        }
        catch (Exception ex) { await bitacora.Registrar(sesion.Nombre, "COBRO", "ERROR: " + ex.Message, clave); throw; }
    }
}
