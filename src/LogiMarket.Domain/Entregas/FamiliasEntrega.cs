using LogiMarket.Domain.Modelo;

namespace LogiMarket.Domain.Entregas;

public interface ITarifaEntrega { decimal Importe { get; } }
public interface IPlazoEntrega { int Dias { get; } }
public interface IEtiquetaEntrega { string Crear(Guid pedido); }
// Abstract Factory: una familia produce tarifa, plazo y etiqueta compatibles.
public interface IFabricaEntrega
{
    TipoEntrega Tipo { get; }
    ITarifaEntrega CrearTarifa();
    IPlazoEntrega CrearPlazo();
    IEtiquetaEntrega CrearEtiqueta();
}
internal sealed record Tarifa(decimal Importe) : ITarifaEntrega;
internal sealed record Plazo(int Dias) : IPlazoEntrega;
internal sealed record Etiqueta(string Prefijo) : IEtiquetaEntrega
{ public string Crear(Guid pedido) => $"{Prefijo}-{pedido:N}"; }
public sealed class EntregaEstandarFactory : IFabricaEntrega
{
    public TipoEntrega Tipo => TipoEntrega.ESTANDAR;
    public ITarifaEntrega CrearTarifa() => new Tarifa(5m);
    public IPlazoEntrega CrearPlazo() => new Plazo(3);
    public IEtiquetaEntrega CrearEtiqueta() => new Etiqueta("STD");
}
public sealed class EntregaExpresFactory : IFabricaEntrega
{
    public TipoEntrega Tipo => TipoEntrega.EXPRES;
    public ITarifaEntrega CrearTarifa() => new Tarifa(12m);
    public IPlazoEntrega CrearPlazo() => new Plazo(1);
    public IEtiquetaEntrega CrearEtiqueta() => new Etiqueta("EXP");
}
