using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Comprobantes;
using LogiMarket.Application.Eventos;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Reportes;
using LogiMarket.Domain.Catalogo;
using LogiMarket.Domain.Entregas;
using LogiMarket.Domain.Modelo;
using LogiMarket.Domain.Pedidos;
using LogiMarket.Domain.Promociones;
using LogiMarket.Infrastructure.Documents;
using LogiMarket.Infrastructure.Payments;
using LogiMarket.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.Tests.Patterns;

public class PatronesTests
{
    private static Pedido Crear(int cantidad = 1)
    {
        var pedido = new PedidoBuilder().Para(new Cliente("Ana", CategoriaCliente.VIP)).ConEntrega(TipoEntrega.ESTANDAR).Agregar(new ProductoSimple("P01", "Mouse", 20, .2m), cantidad).Construir();
        pedido.Valorar(new([new Mayorista15(), new Vip10(), new SinDescuento()]), 5);
        return pedido;
    }
    [Fact]
    public void P12_FamiliasCompatibles()
    {
        IFabricaEntrega estandar = new EntregaEstandarFactory(), expres = new EntregaExpresFactory();
        Assert.Equal(5, estandar.CrearTarifa().Importe); Assert.Equal(3, estandar.CrearPlazo().Dias); Assert.StartsWith("STD", estandar.CrearEtiqueta().Crear(Guid.NewGuid()));
        Assert.Equal(12, expres.CrearTarifa().Importe); Assert.Equal(1, expres.CrearPlazo().Dias); Assert.StartsWith("EXP", expres.CrearEtiqueta().Crear(Guid.NewGuid()));
    }
    [Fact]
    public void P13_BuilderRechazaIncompleto()
    { Assert.Throws<InvalidOperationException>(() => new PedidoBuilder().Construir()); Assert.Throws<ArgumentException>(() => new PedidoBuilder().Para(new Cliente("Ana", CategoriaCliente.VIP)).Construir()); Assert.Single(Crear().Detalles); }
    [Fact]
    public void P14_PrototypeProfundo()
    {
        var original = Crear(); var copia = original.ClonarBorrador();
        Assert.NotEqual(original.Id, copia.Id); Assert.NotSame(original.Detalles, copia.Detalles); Assert.NotSame(original.Detalles[0].Componentes, copia.Detalles[0].Componentes);
        copia.CambiarCantidad(0, 3); Assert.Equal(1, original.Detalles[0].Cantidad); Assert.Equal(3, copia.Detalles[0].Cantidad); Assert.Equal(EstadoPedido.BORRADOR, copia.Estado);
    }
    [Fact]
    public async Task P15_SingletonIdentidadYConcurrencia()
    {
        await using var e = new Escenario(); using var a = e.Proveedor.CreateScope(); using var b = e.Proveedor.CreateScope();
        var metricas = a.ServiceProvider.GetRequiredService<MetricasSesion>(); Assert.Same(metricas, b.ServiceProvider.GetRequiredService<MetricasSesion>());
        var evento = new EventoOutbox(Guid.NewGuid(), "PedidoPagado"); Parallel.For(0, 1000, _ => metricas.Registrar(evento)); Assert.Equal(1, metricas.Pagos);
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task P16_P38_LspProveedores(bool alternativo)
    {
        var opciones = new OpcionesDemostracion(); IPasarelaPago proveedor = alternativo ? new PasarelaAlternativaSimulada(opciones) : new PasarelaLegadaAdapter(new(), opciones);
        var resultado = await proveedor.CobrarAsync(59, "clave"); var repetido = await proveedor.CobrarAsync(59, "clave"); Assert.True(resultado.Aceptado); Assert.Equal(resultado.Referencia, repetido.Referencia);
        await Assert.ThrowsAsync<ArgumentException>(async () => await proveedor.CobrarAsync(0, "cero"));
        opciones.RechazarCobro = true; Assert.False((await proveedor.CobrarAsync(59, "fallo")).Aceptado);
    }
    private sealed class SalidaCaptura : ISalidaDocumento
    { public string Texto { get; private set; } = ""; public Task Emitir(string nombre, string contenido) { Texto = contenido; return Task.CompletedTask; } }
    [Fact]
    public async Task P17_BridgeCuatroCombinacionesContenidoYArchivo()
    {
        var directorio = Path.Combine(Path.GetTempPath(), "lm-txt-" + Guid.NewGuid().ToString("N"));
        try
        {
            var captura = new SalidaCaptura(); var archivo = new SalidaArchivoTxt(directorio); var p = Crear();
            p.Valorar(new([new Mayorista15(), new Vip10(), new SinDescuento()]), 5);
            var d = new Devolucion(p.Id, MotivoDevolucion.DEFECTO, p.Subtotal - p.Descuento, "REF", DateTimeOffset.UtcNow);
            await new ComprobanteVenta(p, captura).Exportar(); Assert.Contains("Total: USD 23.00", captura.Texto);
            await new ComprobanteVenta(p, archivo).Exportar(); await new ComprobanteDevolucion(d, captura).Exportar(); Assert.Contains("18.00", captura.Texto);
            await new ComprobanteDevolucion(d, archivo).Exportar(); Assert.Equal(2, Directory.GetFiles(directorio).Length); Assert.All(Directory.GetFiles(directorio), f => Assert.Contains("LOGIMARKET", File.ReadAllText(f)));
        }
        finally { if (Directory.Exists(directorio)) foreach (var f in Directory.GetFiles(directorio)) File.Delete(f); if (Directory.Exists(directorio)) Directory.Delete(directorio); }
    }
    private sealed class BitacoraCaptura : IBitacora
    { public List<string> Secuencia { get; } = []; public Task Registrar(string usuario, string operacion, string resultado, string correlacion) { Secuencia.Add(resultado); return Task.CompletedTask; } }
    private sealed class PasarelaCaptura(List<string> secuencia, bool acepta) : IPasarelaPago
    { public Task<ResultadoCobro> CobrarAsync(decimal importe, string clave) { secuencia.Add("PASARELA"); return Task.FromResult(new ResultadoCobro(acepta, "REF", "Rechazo")); } }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task P19_DecoradorRodeaEjecucion(bool acepta)
    {
        var bitacora = new BitacoraCaptura(); var decorador = new AuditoriaPagoDecorator(new PasarelaCaptura(bitacora.Secuencia, acepta), bitacora, new());
        Assert.Equal(acepta, (await decorador.CobrarAsync(10, "clave")).Aceptado); Assert.Equal("INICIO", bitacora.Secuencia[0]); Assert.Equal("PASARELA", bitacora.Secuencia[1]); Assert.StartsWith(acepta ? "FIN" : "ERROR", bitacora.Secuencia[2]);
    }
    [Fact]
    public void P23_CadenaPrimeroClienteLuegoStock()
    {
        var p = Crear(); var resultado = CadenaConfirmacion.Validar(new(p, null, []), false); Assert.Equal("CLIENTE", resultado.Codigo);
        resultado = CadenaConfirmacion.Validar(new(p, new Cliente("Ana", CategoriaCliente.VIP), []), false); Assert.Equal("STOCK", resultado.Codigo);
    }
    [Fact]
    public void P25_InterpreterSeleccionaPagado()
    {
        var p = Crear(3); p.Valorar(new([new SinDescuento()]), 5); p.Transicionar(EstadoPedido.VALIDADO, "test", DateTimeOffset.UtcNow); p.Transicionar(EstadoPedido.PAGADO, "test", DateTimeOffset.UtcNow);
        Assert.True(new InterpreterFiltro().Analizar("ESTADO = PAGADO AND TOTAL >= 50").Evaluar(p)); Assert.False(new InterpreterFiltro().Analizar("ESTADO = BORRADOR AND TOTAL >= 50").Evaluar(p));
    }
    [Fact]
    public void P26_InterpreterPrecedenciaYErrores()
    {
        var p = Crear(); var parser = new InterpreterFiltro();
        Assert.True(parser.Analizar("CLIENTE = Ana OR TOTAL >= 100 AND ESTADO = PAGADO").Evaluar(p));
        Assert.False(parser.Analizar("(CLIENTE = Ana OR TOTAL >= 100) AND ESTADO = PAGADO").Evaluar(p));
        Assert.Throws<FormatException>(() => parser.Analizar("TOTAL > 10")); Assert.Throws<FormatException>(() => parser.Analizar("(ESTADO = PAGADO"));
    }
    [Fact]
    public void P27_P34_IteratorYVisitorKits()
    {
        var oficina = new KitCompuesto("KIT_OFICINA"); oficina.Agregar(new ProductoSimple("P01", "Mouse", 20, .2m)); oficina.Agregar(new ProductoSimple("P02", "Teclado", 30, .7m));
        var tele = new KitCompuesto("KIT_TELETRABAJO"); tele.Agregar(oficina); tele.Agregar(new ProductoSimple("P03", "Memoria", 10, .1m));
        using var iterador = new IteradorFisico(tele); var skus = new List<string>(); while (iterador.MoveNext()) skus.Add(iterador.Current.Sku); Assert.Equal(new[] { "P01", "P02", "P03" }, skus); Assert.False(iterador.MoveNext());
        var peso = new PesoVisitor(); tele.Aceptar(peso); Assert.Equal(1m, peso.Peso); var desglose = new DesgloseVisitor(); tele.Aceptar(desglose); Assert.Equal(3, desglose.Cantidades.Count);
    }
    [Fact]
    public void P29_MementoTresCambiosDosDeshacerUnRehacer()
    {
        var historial = new HistorialBorrador(); historial.Capturar(Crear(1)); historial.Capturar(Crear(2)); historial.Capturar(Crear(3));
        Assert.Equal(2, historial.Deshacer().Detalles[0].Cantidad); Assert.Equal(1, historial.Deshacer().Detalles[0].Cantidad); Assert.Equal(2, historial.Rehacer().Detalles[0].Cantidad);
        historial.Bloquear(); Assert.Throws<InvalidOperationException>(() => historial.Deshacer());
    }
    [Fact]
    public void MementoPermiteDeshacerEliminarLaUltimaLinea()
    {
        var historial = new HistorialBorrador(); historial.Capturar(Crear()); historial.CapturarVacio();
        Assert.Single(historial.DeshacerCompleto()!.Detalles); Assert.Null(historial.RehacerCompleto());
    }
    [Fact]
    public void P31_StateRechazaRetrocesoYRepeticion()
    {
        var p = Crear(); Assert.Throws<InvalidOperationException>(() => p.Transicionar(EstadoPedido.DESPACHADO, "x", DateTimeOffset.UtcNow));
        foreach (var estado in new[] { EstadoPedido.VALIDADO, EstadoPedido.PAGADO, EstadoPedido.DESPACHADO, EstadoPedido.ENTREGADO }) p.Transicionar(estado, "test", DateTimeOffset.UtcNow);
        Assert.Throws<InvalidOperationException>(() => p.Transicionar(EstadoPedido.CANCELADO, "x", DateTimeOffset.UtcNow)); Assert.Throws<InvalidOperationException>(() => p.Transicionar(EstadoPedido.ENTREGADO, "x", DateTimeOffset.UtcNow));
    }
    private sealed class PromocionNueva : IPoliticaDescuento
    { public int Prioridad => 100; public decimal Tasa => .20m; public bool Aplica(ContextoPromocion contexto) => true; }
    [Fact]
    public void P32_ExtenderStrategySinCambiarCalculadora()
    { var calculadora = new CalculadoraPrecios([new SinDescuento(), new PromocionNueva()]); Assert.Equal(20, calculadora.Calcular(100, 5, new(false, 1)).Descuento); }
    [Fact]
    public void RedondeoComercialAwayFromZero()
    { var c = new CalculadoraPrecios([new Vip10()]); Assert.Equal(0.01m, c.Calcular(.05m, 5, new(true, 1)).Descuento); }
    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    [InlineData(1, -1)]
    public void ProductoRechazaValoresInvalidos(decimal precio, decimal peso)
    { Assert.Throws<ArgumentException>(() => new ProductoSimple("P01", "Mouse", precio, peso)); }
}
