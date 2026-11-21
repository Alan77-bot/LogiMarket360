using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Catalogo;
using LogiMarket.Application.Clientes;
using LogiMarket.Application.Pedidos;
using LogiMarket.Domain.Modelo;
using LogiMarket.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Tests.Acceptance;

public class IntegridadTests
{
    private sealed class PasarelaConFallo : IPasarelaPago
    { public Task<ResultadoCobro> CobrarAsync(decimal importe, string clave) => throw new IOException("Colaborador indisponible."); }
    [Fact]
    public async Task P20_FalloColaboradorNoDejaPersistenciaParcial()
    {
        await using var e = new Escenario(s => s.AddScoped<IPasarelaPago, PasarelaConFallo>()); await e.Iniciar();
        var id = await e.Borrador(); await e.Usar(s => s.GetRequiredService<ServicioPedidos>().Validar(id));
        await Assert.ThrowsAsync<IOException>(() => e.Usar(s => s.GetRequiredService<ConfirmacionPedidoFacade>().Confirmar(id, "fallo")));
        await e.Usar(async s => { var r = s.GetRequiredService<IRepositorio>(); Assert.Null(await r.PagoPorPedido(id)); Assert.Equal(8, (await r.Productos()).Single(p => p.Sku == "P01").Stock); Assert.Empty(await s.GetRequiredService<IConsultasOperativas>().Eventos()); Assert.Equal(EstadoPedido.VALIDADO, (await r.Pedido(id)).Estado); });
    }
    [Fact]
    public async Task EdicionConservaIdentidadYReemplazaLineas()
    {
        await using var e = new Escenario(); await e.Iniciar(); var id = await e.Borrador(); var p = await e.Pedido(id);
        var mismo = await e.Usar(s => s.GetRequiredService<ServicioPedidos>().Guardar(new(p.ClienteId, TipoEntrega.EXPRES, [new("P04", 2)]), id));
        Assert.Equal(id, mismo); var editado = await e.Pedido(id); Assert.Single(editado.Detalles); Assert.Equal("P04", editado.Detalles[0].Sku); Assert.Equal(84, editado.Total);
    }
    [Fact]
    public async Task ClienteInactivoBloqueaValidacion()
    {
        await using var e = new Escenario(); await e.Iniciar(); var id = await e.Borrador(); var p = await e.Pedido(id);
        await e.Usar(s => s.GetRequiredService<ServicioClientes>().Guardar(p.ClienteId, "Ana", CategoriaCliente.VIP, false));
        await Assert.ThrowsAsync<InvalidOperationException>(() => e.Usar(s => s.GetRequiredService<ServicioPedidos>().Validar(id)));
    }
    [Fact]
    public async Task KitCircularPersistidoSeRevierte()
    {
        await using var e = new Escenario(); await e.Iniciar();
        var kits = await e.Usar(s => s.GetRequiredService<IRepositorio>().Kits()); var oficina = kits.Single(k => k.Sku == "KIT_OFICINA"); var tele = kits.Single(k => k.Sku == "KIT_TELETRABAJO");
        await Assert.ThrowsAsync<InvalidOperationException>(() => e.Usar(s => s.GetRequiredService<ServicioCatalogo>().GuardarKit(oficina.Id, oficina.Sku, oficina.Nombre, [new(tele.Id, true, 1)])));
        Assert.Equal(2, (await e.Usar(s => s.GetRequiredService<IRepositorio>().Kits())).Single(k => k.Id == oficina.Id).Componentes.Count);
    }
    [Fact]
    public async Task ProductoUsadoNoSeEliminaNiCambiaSku()
    {
        await using var e = new Escenario(); await e.Iniciar(); await e.Pagar(); var producto = (await e.Usar(s => s.GetRequiredService<IRepositorio>().Productos())).Single(p => p.Sku == "P01");
        await Assert.ThrowsAsync<InvalidOperationException>(() => e.Usar(s => s.GetRequiredService<ServicioCatalogo>().Eliminar(producto.Id, false)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => e.Usar(s => s.GetRequiredService<ServicioCatalogo>().GuardarProducto(producto.Id, "OTRO", producto.Nombre, producto.Precio, producto.Peso, 0)));
    }
    [Fact]
    public async Task InstantaneaConservaArbolAnidadoYClon()
    {
        await using var e = new Escenario(); await e.Iniciar(); var id = await e.Borrador(); var p = await e.Pedido(id);
        Assert.Contains("KIT_OFICINA", p.Detalles[0].ComposicionJson); var copia = p.ClonarBorrador(); copia.CambiarCantidad(0, 2);
        Assert.Equal(1, p.Detalles[0].Cantidad); Assert.Equal(2, copia.Detalles[0].Cantidad); Assert.Equal(p.Detalles[0].ComposicionJson, copia.Detalles[0].ComposicionJson);
    }
}
