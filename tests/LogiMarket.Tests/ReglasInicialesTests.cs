using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Promociones;

namespace LogiMarket.Tests;

public class ReglasInicialesTests
{
    [Theory]
    [InlineData(true, 3, 60, 5, 59)]
    [InlineData(false, 4, 90, 12, 102)]
    [InlineData(false, 10, 100, 5, 90)]
    [InlineData(true, 10, 100, 5, 90)]
    public void PromocionesRespetanImportesYPrioridad(bool vip, int unidades, decimal subtotal, decimal tarifa, decimal esperado)
    {
        var calculadora = new CalculadoraPrecios([new Mayorista15(), new Vip10(), new SinDescuento()]);
        Assert.Equal(esperado, calculadora.Calcular(subtotal, tarifa, new(vip, unidades)).Total);
    }

    [Fact]
    public void KitAnidadoExpandeCantidadesYRechazaCicloIndirecto()
    {
        var oficina = new KitCompuesto("KIT_OFICINA");
        oficina.Agregar(new ProductoSimple("P01", "Mouse USB", 20, 0.20m));
        oficina.Agregar(new ProductoSimple("P02", "Teclado", 30, 0.70m));
        var teletrabajo = new KitCompuesto("KIT_TELETRABAJO");
        teletrabajo.Agregar(oficina);
        teletrabajo.Agregar(new ProductoSimple("P03", "Memoria USB", 10, 0.10m));
        Assert.Equal(50, oficina.Precio);
        Assert.Equal(60, teletrabajo.Precio);
        Assert.Equal(new[] { "P01", "P02", "P03" }, teletrabajo.Expandir().Select(c => c.Sku));
        Assert.All(teletrabajo.Expandir(6), c => Assert.Equal(6, c.Cantidad));
        Assert.Throws<InvalidOperationException>(() => oficina.Agregar(teletrabajo));
    }
}
