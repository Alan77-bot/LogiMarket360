using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Catalogo;
using LogiMarket.Application.Commands;
using LogiMarket.Application.Eventos;
using LogiMarket.Application.Logistica;
using LogiMarket.Application.Pedidos;
using LogiMarket.Domain.Modelo;
using LogiMarket.Infrastructure.Persistence;
using LogiMarket.Infrastructure.DependencyInjection;
using LogiMarket.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Tests.Acceptance;

public class FlujosTests
{
    [Fact]
    public async Task P01_MigracionesYSeedIdempotente()
    {
        await using var e = new Escenario(); await e.Iniciar(); await e.Usar(s => s.GetRequiredService<Inicializador>().Ejecutar());
        await e.Usar(async s => { var db = s.GetRequiredService<LogiMarketDbContext>(); Assert.Equal(5, await db.Productos.CountAsync()); Assert.Equal(2, await db.Kits.CountAsync()); Assert.Equal(4, await db.Usuarios.CountAsync()); Assert.NotEmpty(await db.Database.GetAppliedMigrationsAsync()); Assert.Empty(await db.Database.GetPendingMigrationsAsync()); });
    }
    [Theory]
    [InlineData("Ana", TipoEntrega.ESTANDAR, "KIT_TELETRABAJO", 1, "", 0, 59, "P01", 7)]
    [InlineData("Luis", TipoEntrega.EXPRES, "P04", 2, "P05", 2, 102, "P04", 2)]
    [InlineData("Carmen", TipoEntrega.ESTANDAR, "P03", 10, "", 0, 90, "P03", 10)]
    public async Task P02_P04_CasosOficiales(string cliente, TipoEntrega entrega, string sku, int cantidad, string otro, int otraCantidad, decimal total, string stockSku, int stock)
    {
        await using var e = new Escenario(); await e.Iniciar(); var lineas = new List<LineaEntrada> { new(sku, cantidad) }; if (otro != "") lineas.Add(new(otro, otraCantidad));
        var id = await e.Borrador(cliente, entrega, lineas.ToArray()); await e.Usar(s => s.GetRequiredService<ServicioPedidos>().Validar(id));
        await e.Usar(async s => await s.GetRequiredService<ConfirmacionPedidoFacade>().Confirmar(id, "oficial"));
        Assert.Equal(total, (await e.Pedido(id)).Total); Assert.Equal(EstadoPedido.PAGADO, (await e.Pedido(id)).Estado);
        await e.Usar(async s => { var repo = s.GetRequiredService<IRepositorio>(); Assert.Equal(stock, (await repo.Productos()).Single(p => p.Sku == stockSku).Stock); Assert.NotNull(await repo.PagoPorPedido(id));
            if (cliente == "Ana") { Assert.Equal(4, (await repo.Productos()).Single(p => p.Sku == "P02").Stock); Assert.Equal(19, (await repo.Productos()).Single(p => p.Sku == "P03").Stock); }
            if (cliente == "Luis") Assert.Equal(28, (await repo.Productos()).Single(p => p.Sku == "P05").Stock);
            if (cliente == "Carmen") Assert.Equal(15, (await repo.Pedido(id)).Descuento);
        });
    }
    [Fact]
    public async Task P05_KitPorSeisSinEfectos()
    {
        await using var e = new Escenario(); await e.Iniciar(); var id = await e.Borrador("Ana", TipoEntrega.ESTANDAR, new LineaEntrada("KIT_TELETRABAJO", 6));
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => e.Usar(s => s.GetRequiredService<ServicioPedidos>().Validar(id))); Assert.Contains("P02", error.Message);
        await SinEfectos(e, id);
    }
    [Fact]
    public async Task P06_RechazoNoDescuentaStock()
    {
        await using var e = new Escenario(); await e.Iniciar(); var id = await e.Borrador(); await e.Usar(s => s.GetRequiredService<ServicioPedidos>().Validar(id));
        e.Proveedor.GetRequiredService<OpcionesDemostracion>().RechazarCobro = true;
        await Assert.ThrowsAsync<InvalidOperationException>(() => e.Usar(async s => await s.GetRequiredService<ConfirmacionPedidoFacade>().Confirmar(id, "rechazo")));
        await SinEfectos(e, id); Assert.Equal(EstadoPedido.VALIDADO, (await e.Pedido(id)).Estado);
        await e.Usar(async s => { var logs = await s.GetRequiredService<IConsultasOperativas>().Auditorias(); Assert.Contains(logs, a => a.Operacion == "COBRO" && a.Resultado.StartsWith("ERROR")); });
    }
    private static async Task SinEfectos(Escenario e, Guid id) => await e.Usar(async s => {
        var repo = s.GetRequiredService<IRepositorio>(); Assert.Equal(8, (await repo.Productos()).Single(p => p.Sku == "P01").Stock);
        Assert.Null(await repo.PagoPorPedido(id)); Assert.Empty(await s.GetRequiredService<IConsultasOperativas>().Eventos()); Assert.Empty(await s.GetRequiredService<IConsultasOperativas>().Movimientos()); });
    [Fact]
    public async Task P07_ConfirmacionIdempotente()
    {
        await using var e = new Escenario(); await e.Iniciar(); var id = await e.Pagar();
        var pago = await e.Usar(s => s.GetRequiredService<ConfirmacionPedidoFacade>().Confirmar(id, id.ToString()));
        var repetido = await e.Usar(s => s.GetRequiredService<ConfirmacionPedidoFacade>().Confirmar(id, id.ToString())); Assert.Equal(pago.Id, repetido.Id);
        await e.Usar(async s => { var db = s.GetRequiredService<LogiMarketDbContext>(); Assert.Equal(1, await db.Set<Pago>().CountAsync()); Assert.Equal(3, await db.Set<MovimientoInventario>().CountAsync()); });
    }
    [Fact]
    public async Task P08_PagadoNoAdmiteCancelarNiEditar()
    {
        await using var e = new Escenario(); await e.Iniciar(); var id = await e.Pagar(); var previo = await e.Pedido(id);
        await Assert.ThrowsAsync<InvalidOperationException>(() => e.Usar(s => s.GetRequiredService<ServicioPedidos>().Cancelar(id)));
        await Assert.ThrowsAsync<InvalidOperationException>(() => e.Usar(s => s.GetRequiredService<ServicioPedidos>().Guardar(new(previo.ClienteId, previo.Entrega, [new("P01", 1)]), id)));
        var posterior = await e.Pedido(id); Assert.Equal(previo.Total, posterior.Total); Assert.Equal(previo.Historial.Count, posterior.Historial.Count);
    }
    [Fact]
    public async Task P09_DespachoEntregaYRechazoSinPago()
    {
        await using var e = new Escenario(); await e.Iniciar(); var borrador = await e.Borrador(); var id = await e.Pagar(); await e.Entrar("bodega");
        await Assert.ThrowsAsync<InvalidOperationException>(() => e.Usar(s => s.GetRequiredService<ServicioDespachos>().Despachar(borrador)));
        await e.Usar(s => s.GetRequiredService<ServicioDespachos>().Despachar(id)); await e.Usar(s => s.GetRequiredService<ServicioDespachos>().Entregar(id));
        Assert.Equal(EstadoPedido.ENTREGADO, (await e.Pedido(id)).Estado); Assert.Equal(EstadoPedido.BORRADOR, (await e.Pedido(borrador)).Estado);
    }
    [Fact]
    public async Task P10_PersistenciaConOtroProveedorDi()
    {
        await using var e = new Escenario(); await e.Iniciar(); var id = await e.Pagar();
        using var otro = new ServiceCollection().AgregarLogiMarket(e.Ruta).BuildServiceProvider(); using var scope = otro.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IRepositorio>(); Assert.Equal(EstadoPedido.PAGADO, (await repo.Pedido(id)).Estado); Assert.NotNull(await repo.PagoPorPedido(id));
        Assert.Equal(7, (await repo.Productos()).Single(p => p.Sku == "P01").Stock); Assert.NotEmpty(await scope.ServiceProvider.GetRequiredService<IConsultasOperativas>().Auditorias());
    }
    [Theory]
    [InlineData("consulta")]
    [InlineData("bodega")]
    public async Task P22_ProxyDetieneAntesDeCobro(string usuario)
    {
        await using var e = new Escenario(); await e.Iniciar(); await e.Entrar(usuario);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => e.Usar(s => s.GetRequiredService<IPasarelaPago>().CobrarAsync(10, "prohibido")));
        await e.Usar(async s => Assert.DoesNotContain(await s.GetRequiredService<IConsultasOperativas>().Auditorias(), a => a.Operacion == "COBRO"));
    }
    [Fact]
    public async Task P24_ComandoInvocadoRegistraResultado()
    {
        await using var e = new Escenario(); await e.Iniciar(); var id = await e.Borrador(); await e.Usar(s => s.GetRequiredService<ServicioPedidos>().Validar(id));
        await e.Usar(s => s.GetRequiredService<InvocadorComandos>().Ejecutar(new ConfirmarPedidoCommand(s.GetRequiredService<ConfirmacionPedidoFacade>(), id, "command")));
        await e.Usar(async s => Assert.Contains(await s.GetRequiredService<IConsultasOperativas>().Auditorias(), a => a.Operacion == nameof(ConfirmarPedidoCommand) && a.Resultado.StartsWith("OK")));
    }
    [Theory]
    [InlineData("EMAIL")]
    [InlineData("SMS")]
    public async Task P11_P30_ObserverYFactoryPublicanUnaVez(string canal)
    {
        await using var e = new Escenario(); await e.Iniciar(); e.Proveedor.GetRequiredService<OpcionesDemostracion>().Canal = canal; await e.Pagar();
        await e.Usar(async s => Assert.Equal(1, await s.GetRequiredService<IProcesadorOutbox>().Procesar()));
        await e.Usar(async s => Assert.Equal(0, await s.GetRequiredService<IProcesadorOutbox>().Procesar()));
        Assert.Equal(1, e.Proveedor.GetRequiredService<MetricasSesion>().Pagos);
        await e.Usar(async s => { var consultas = s.GetRequiredService<IConsultasOperativas>(); Assert.Equal(canal, Assert.Single(await consultas.Avisos()).Canal); Assert.Single((await consultas.Auditorias()).Where(a => a.Operacion == "PedidoPagado")); });
    }
    [Fact]
    public async Task P35_DevolucionAnaUnicaPor54()
    {
        await using var e = new Escenario(); await e.Iniciar(); var id = await e.Pagar();
        await e.Usar(s => s.GetRequiredService<ServicioDespachos>().Despachar(id)); await e.Usar(s => s.GetRequiredService<ServicioDespachos>().Entregar(id));
        e.Reloj.Ahora = e.Reloj.Ahora.AddDays(20);
        var devolucion = await e.Usar(s => s.GetRequiredService<DevolucionDefecto>().Ejecutar(id, false)); Assert.Equal(54, devolucion.Reembolso);
        await Assert.ThrowsAsync<InvalidOperationException>(() => e.Usar(s => s.GetRequiredService<DevolucionDefecto>().Ejecutar(id, true)));
        await e.Usar(async s => { Assert.Single(await s.GetRequiredService<IConsultasOperativas>().Devoluciones()); Assert.Equal(8, (await s.GetRequiredService<IRepositorio>().Productos()).Single(p => p.Sku == "P01").Stock); });
    }
    [Fact]
    public async Task P33_ArrepentimientoDiaDiezNoAlteraStock()
    {
        await using var e = new Escenario(); await e.Iniciar(); var id = await e.Pagar(); await e.Usar(s => s.GetRequiredService<ServicioDespachos>().Despachar(id)); await e.Usar(s => s.GetRequiredService<ServicioDespachos>().Entregar(id)); e.Reloj.Ahora = e.Reloj.Ahora.AddDays(10);
        await Assert.ThrowsAsync<InvalidOperationException>(() => e.Usar(s => s.GetRequiredService<DevolucionArrepentimiento>().Ejecutar(id, true)));
        await e.Usar(async s => { Assert.Empty(await s.GetRequiredService<IConsultasOperativas>().Devoluciones()); Assert.Equal(7, (await s.GetRequiredService<IRepositorio>().Productos()).Single(p => p.Sku == "P01").Stock); });
    }
    [Fact]
    public async Task P36_DosConfirmacionesConcurrentesSinSobreventa()
    {
        await using var e = new Escenario(); await e.Iniciar();
        await e.Usar(async s => { var repo = s.GetRequiredService<IRepositorio>(); (await repo.Productos()).Single(p => p.Sku == "P01").Mover(-7); await repo.Guardar(); });
        var a = await e.Borrador("Ana", TipoEntrega.ESTANDAR, new LineaEntrada("P01", 1)); var b = await e.Borrador("Luis", TipoEntrega.ESTANDAR, new LineaEntrada("P01", 1));
        await e.Usar(s => s.GetRequiredService<ServicioPedidos>().Validar(a)); await e.Usar(s => s.GetRequiredService<ServicioPedidos>().Validar(b));
        async Task<bool> Cobrar(Guid id) { try { await e.Usar(s => s.GetRequiredService<ConfirmacionPedidoFacade>().Confirmar(id, id.ToString())); return true; } catch (InvalidOperationException) { return false; } }
        var resultados = await Task.WhenAll(Task.Run(() => Cobrar(a)), Task.Run(() => Cobrar(b))); Assert.Single(resultados.Where(r => r));
        await e.Usar(async s => { Assert.Equal(0, (await s.GetRequiredService<IRepositorio>().Productos()).Single(p => p.Sku == "P01").Stock); Assert.Equal(1, await s.GetRequiredService<LogiMarketDbContext>().Set<Pago>().CountAsync()); });
    }
    [Fact]
    public async Task P37_RecuperacionOutboxConNuevoProcesador()
    {
        await using var e = new Escenario(); await e.Iniciar(); await e.Pagar(); var opciones = e.Proveedor.GetRequiredService<OpcionesDemostracion>(); opciones.FallarNotificacion = true;
        await e.Usar(async s => Assert.Equal(0, await s.GetRequiredService<IProcesadorOutbox>().Procesar()));
        await e.Usar(async s => { var evento = Assert.Single(await s.GetRequiredService<IConsultasOperativas>().Eventos()); Assert.Null(evento.ProcesadoEn); Assert.Equal(1, evento.Intentos); });
        opciones.FallarNotificacion = false; await e.Usar(async s => Assert.Equal(1, await s.GetRequiredService<IProcesadorOutbox>().Procesar()));
        await e.Usar(async s => { Assert.Single(await s.GetRequiredService<IConsultasOperativas>().Avisos()); Assert.Equal(1, await s.GetRequiredService<LogiMarketDbContext>().Set<Pago>().CountAsync()); });
    }
    [Fact]
    public async Task InstantaneaNoCambiaConCatalogo()
    {
        await using var e = new Escenario(); await e.Iniciar(); var id = await e.Pagar();
        await e.Usar(async s => { var p = (await s.GetRequiredService<IRepositorio>().Productos()).Single(p => p.Sku == "P01"); await s.GetRequiredService<ServicioCatalogo>().GuardarProducto(p.Id, p.Sku, p.Nombre, 99, p.Peso, 0); });
        Assert.Equal(59, (await e.Pedido(id)).Total); Assert.Equal(20, (await e.Pedido(id)).Detalles.SelectMany(d => d.Componentes).Single(c => c.Sku == "P01").Precio);
    }
}

