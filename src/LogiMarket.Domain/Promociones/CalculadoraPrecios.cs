namespace LogiMarket.Domain.Promociones;

public sealed record ContextoPromocion(bool EsVip, int UnidadesFisicas);
public sealed record Totales(decimal Subtotal, decimal Descuento, decimal Tarifa, decimal Total);

// Strategy: la calculadora depende del contrato y admite nuevas politicas.
public interface IPoliticaDescuento
{
    int Prioridad { get; }
    decimal Tasa { get; }
    bool Aplica(ContextoPromocion contexto);
}

public sealed class Mayorista15 : IPoliticaDescuento
{
    public int Prioridad => 20;
    public decimal Tasa => 0.15m;
    public bool Aplica(ContextoPromocion contexto) => contexto.UnidadesFisicas >= 10;
}

public sealed class Vip10 : IPoliticaDescuento
{
    public int Prioridad => 10;
    public decimal Tasa => 0.10m;
    public bool Aplica(ContextoPromocion contexto) => contexto.EsVip;
}

public sealed class SinDescuento : IPoliticaDescuento
{
    public int Prioridad => 0;
    public decimal Tasa => 0;
    public bool Aplica(ContextoPromocion contexto) => true;
}

public sealed class CalculadoraPrecios(IEnumerable<IPoliticaDescuento> politicas)
{
    public Totales Calcular(decimal subtotal, decimal tarifa, ContextoPromocion contexto)
    {
        if (subtotal < 0 || tarifa < 0 || contexto.UnidadesFisicas <= 0)
            throw new ArgumentException("Importes o cantidades invalidos.");
        var politica = politicas.OrderByDescending(p => p.Prioridad).First(p => p.Aplica(contexto));
        var descuento = decimal.Round(subtotal * politica.Tasa, 2, MidpointRounding.AwayFromZero);
        return new(subtotal, descuento, tarifa, subtotal - descuento + tarifa);
    }
}
