using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Pedidos;
using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Modelo;
using LogiMarket.Domain.Pedidos;
using LogiMarket.Domain.Promociones;
using LogiMarket.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Tests.Acceptance;

public class ValoracionTests
{
    [Fact]
    public void CantidadEditadaExigeRevalorarAntesDeValidar()
    {
        var pedido = new PedidoBuilder().Para(new Cliente("Ana", CategoriaCliente.VIP))
            .Agregar(new ProductoSimple("P01", "Mouse", 20, .2m), 1).Construir();
        var calculadora = new CalculadoraPrecios([new Mayorista15(), new Vip10(), new SinDescuento()]);
        pedido.Valorar(calculadora, 5);
        pedido.CambiarCantidad(0, 3);
        Assert.Throws<InvalidOperationException>(() => pedido.Transicionar(EstadoPedido.VALIDADO, "test", DateTimeOffset.UtcNow));
        pedido.Valorar(calculadora, 5);
        pedido.Transicionar(EstadoPedido.VALIDADO, "test", DateTimeOffset.UtcNow);
        Assert.Equal(59, pedido.Total);
    }

    [Fact]
    public async Task ValidarRecalculaElBorradorDesdeSusInstantaneas()
    {
        await using var e = new Escenario();
        await e.Iniciar();
        var id = await e.Borrador();
        await e.Usar(async s => {
            var repo = s.GetRequiredService<IRepositorio>();
            (await repo.Pedido(id)).CambiarCantidad(0, 2);
            await repo.Guardar();
        });
        await e.Usar(s => s.GetRequiredService<ServicioPedidos>().Validar(id));
        Assert.Equal(113, (await e.Pedido(id)).Total);
    }
}
