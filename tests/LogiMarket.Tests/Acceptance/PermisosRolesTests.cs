using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Catalogo;
using LogiMarket.Application.Clientes;
using LogiMarket.Application.Logistica;
using LogiMarket.Application.Pedidos;
using LogiMarket.Domain.Modelo;
using LogiMarket.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Tests.Acceptance;

public sealed class PermisosRolesTests
{
    [Theory]
    [InlineData("ventas")]
    [InlineData("bodega")]
    [InlineData("consulta")]
    public async Task OperacionesAjenasAlRolSeRechazanSinCambiarNegocio(string usuario)
    {
        await using var e = new Escenario(); await e.Iniciar();
        var pedido = await e.Borrador();
        var producto = await e.Usar(async s => (await s.GetRequiredService<IRepositorio>().Productos()).Single(p => p.Sku == "P01"));
        await e.Entrar(usuario);
        // Se llama al servicio directamente; ocultar botones no es la barrera de seguridad.
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => e.Usar(s => s.GetRequiredService<ServicioCatalogo>().GuardarProducto(null, "PROHIBIDO", "No crear", 1, 0, 1)));
        if (usuario != "ventas")
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => e.Usar(s => s.GetRequiredService<ServicioClientes>().Guardar(null, "No crear", CategoriaCliente.NORMAL, true)));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => e.Usar(s => s.GetRequiredService<ServicioPedidos>().Validar(pedido)));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => e.Usar(s => s.GetRequiredService<ServicioPedidos>().Cancelar(pedido)));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => e.Usar(async s => await s.GetRequiredService<ConfirmacionPedidoFacade>().Confirmar(pedido, "prohibido")));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => e.Usar(async s => await s.GetRequiredService<DevolucionDefecto>().Ejecutar(pedido, true)));
        }
        if (usuario != "bodega")
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => e.Usar(s => s.GetRequiredService<ServicioInventario>().Mover(producto.Id, 1, "NO", "prohibido")));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => e.Usar(s => s.GetRequiredService<ServicioDespachos>().Despachar(pedido)));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => e.Usar(s => s.GetRequiredService<ServicioDespachos>().Entregar(pedido)));
        }
        Assert.Equal(EstadoPedido.BORRADOR, (await e.Pedido(pedido)).Estado);
        await e.Usar(async s =>
        {
            var repo = s.GetRequiredService<IRepositorio>();
            Assert.Equal(5, (await repo.Productos()).Count);
            Assert.Equal(producto.Stock, (await repo.Productos()).Single(p => p.Id == producto.Id).Stock);
            Assert.Equal(3, (await repo.Clientes()).Count);
            Assert.Null(await repo.PagoPorClave("prohibido"));
            Assert.Null(await repo.DevolucionPorPedido(pedido));
        });
    }

    [Fact]
    public async Task VentaDespachoYConsultaFuncionanConSusPropiosRoles()
    {
        await using var e = new Escenario(); await e.Iniciar(); await e.Entrar("ventas");
        var id = await e.Pagar();
        await e.Entrar("bodega");
        await e.Usar(s => s.GetRequiredService<ServicioDespachos>().Despachar(id));
        await e.Usar(s => s.GetRequiredService<ServicioDespachos>().Entregar(id));
        await e.Entrar("consulta");
        var pedidos = await e.Usar(s => s.GetRequiredService<ServicioPedidos>().Consultar());
        Assert.Equal(EstadoPedido.ENTREGADO, pedidos.Single(p => p.Id == id).Estado);
        await e.Entrar("ventas");
        var devolucion = await e.Usar(s => s.GetRequiredService<DevolucionDefecto>().Ejecutar(id, true));
        Assert.Equal(54m, devolucion.Reembolso);
    }
}
