using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Catalogo;
using LogiMarket.Application.Comprobantes;
using LogiMarket.Application.Eventos;
using LogiMarket.Application.Pedidos;
using LogiMarket.Application.Reportes;
using LogiMarket.Application.Seguridad;
using LogiMarket.Domain.Pedidos;
using LogiMarket.WinForms.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Features;

public sealed class SalidaVistaPrevia(IWin32Window propietario) : ISalidaDocumento
{
    public Task Emitir(string nombre, string contenido)
    {
        using var vista = new Form { Text = nombre, Width = 760, Height = 600, StartPosition = FormStartPosition.CenterParent };
        vista.Controls.Add(new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Text = contenido.Replace("\n", Environment.NewLine), Font = new Font("Consolas", 11) });
        vista.Shown += (_, _) => ((TextBox)vista.Controls[0]).Select(0, 0);
        vista.ShowDialog(propietario); return Task.CompletedTask;
    }
}
public sealed class VistaReportes : UserControl
{
    private readonly OperacionesUi operaciones;
    private readonly ISalidaDocumento archivo;
    private readonly DataGridView grid = Ui.Grilla();
    private readonly TextBox filtro = new() { Width = 400, PlaceholderText = "CLIENTE = Ana OR TOTAL >= 100" };
    public VistaReportes(OperacionesUi operaciones, FormularioPrincipalMediator mediator, ISalidaDocumento archivo)
    {
        this.operaciones = operaciones; this.archivo = archivo; Dock = DockStyle.Fill;
        var barra = Ui.Barra(); barra.Controls.Add(filtro); barra.Controls.Add(Ui.Boton("Consultar", Refrescar));
        barra.Controls.Add(Ui.Boton("Venta vista previa", () => Exportar(false, false))); barra.Controls.Add(Ui.Boton("Venta TXT", () => Exportar(false, true)));
        barra.Controls.Add(Ui.Boton("Devolucion vista previa", () => Exportar(true, false))); barra.Controls.Add(Ui.Boton("Devolucion TXT", () => Exportar(true, true)));
        Controls.Add(grid); Controls.Add(barra); mediator.Registrar(Refrescar);
    }
    public async Task Refrescar()
    {
        var pedidos = await operaciones.Consultar(s => s.GetRequiredService<ServicioPedidos>().Consultar());
        Ui.Cargar(grid, string.IsNullOrWhiteSpace(filtro.Text) ? pedidos : pedidos.Where(new InterpreterFiltro().Analizar(filtro.Text).Evaluar));
        foreach (var n in new[] { "Detalles", "Historial" }) if (grid.Columns.Contains(n)) grid.Columns[n].Visible = false;
    }
    private async Task Exportar(bool devolucion, bool txt)
    {
        var pedido = grid.CurrentRow?.DataBoundItem as Pedido ?? throw new InvalidOperationException("Seleccione pedido.");
        var salida = txt ? archivo : new SalidaVistaPrevia(this);
        Comprobante documento;
        if (devolucion)
        { var registro = await operaciones.Consultar(s => s.GetRequiredService<IRepositorio>().DevolucionPorPedido(pedido.Id)); documento = new ComprobanteDevolucion(registro ?? throw new InvalidOperationException("Pedido sin devolucion."), salida); }
        else documento = new ComprobanteVenta(pedido, salida);
        await documento.Exportar(); if (txt) MessageBox.Show("Archivo TXT generado en la carpeta de comprobantes indicada en README.");
    }
}
public sealed class VistaPanel : UserControl
{
    private readonly Label resumen = new() { Dock = DockStyle.Top, Height = 260, Padding = new Padding(24), Font = new Font("Segoe UI", 17) };
    private readonly OperacionesUi operaciones;
    private readonly MetricasSesion metricas;
    public VistaPanel(OperacionesUi operaciones, FormularioPrincipalMediator mediator, MetricasSesion metricas)
    { this.operaciones = operaciones; this.metricas = metricas; Dock = DockStyle.Fill; Controls.Add(resumen); mediator.Registrar(Refrescar); }
    public async Task Refrescar()
    {
        var datos = await operaciones.Consultar(async s => (Pedidos: await s.GetRequiredService<ServicioPedidos>().Consultar(), Productos: await s.GetRequiredService<IRepositorio>().Productos()));
        resumen.Text = $"Operacion LogiMarket 360\n\nPedidos: {datos.Pedidos.Count}    Productos: {datos.Productos.Count}\n" +
            $"Alertas de stock (<= 3): {datos.Productos.Count(p => p.Stock <= 3)}\n" +
            $"Eventos de esta sesion: pagos {metricas.Pagos}, despachos {metricas.Despachos}, devoluciones {metricas.Devoluciones}";
    }
}
public sealed class VistaAuditoria : UserControl
{
    private readonly OperacionesUi operaciones;
    private readonly OpcionesDemostracion opciones;
    private readonly SesionActual sesion;
    private readonly DataGridView logs = Ui.Grilla();
    private readonly DataGridView eventos = Ui.Grilla();
    private readonly DataGridView avisos = Ui.Grilla();
    private readonly DataGridView flyweight = Ui.Grilla();
    public VistaAuditoria(OperacionesUi operaciones, FormularioPrincipalMediator mediator, OpcionesDemostracion opciones, SesionActual sesion)
    {
        this.operaciones = operaciones; this.opciones = opciones; this.sesion = sesion; Dock = DockStyle.Fill;
        var tabs = new TabControl { Dock = DockStyle.Fill };
        foreach (var (nombre, grilla) in new[] { ("Auditoria", logs), ("Outbox", eventos), ("Avisos simulados", avisos), ("Flyweight", flyweight) }) { var tab = new TabPage(nombre); tab.Controls.Add(grilla); tabs.TabPages.Add(tab); }
        var barra = Ui.Barra(); barra.Controls.Add(Ui.Boton("Opciones de demostracion", Configurar));
        barra.Controls.Add(Ui.Boton("Reintentar Outbox", async () => { await operaciones.Ejecutar(async s => { await s.GetRequiredService<Autorizador>().Exigir(); await s.GetRequiredService<IProcesadorOutbox>().Procesar(); }); await mediator.Actualizar(); }));
        barra.Controls.Add(Ui.Boton("Demostrar 1.000 filas", DemostrarFlyweight)); Controls.Add(tabs); Controls.Add(barra); mediator.Registrar(Refrescar);
    }
    public async Task Refrescar()
    {
        var datos = await operaciones.Consultar(async s => (Logs: await s.GetRequiredService<IConsultasOperativas>().Auditorias(), Eventos: await s.GetRequiredService<IConsultasOperativas>().Eventos(), Avisos: await s.GetRequiredService<IConsultasOperativas>().Avisos()));
        Ui.Cargar(logs, datos.Logs.OrderByDescending(l => l.Fecha)); Ui.Cargar(eventos, datos.Eventos); Ui.Cargar(avisos, datos.Avisos);
    }
    private async Task Configurar()
    {
        await operaciones.Consultar(async s => { await s.GetRequiredService<Autorizador>().Exigir(); return true; });
        using var d = new DialogoCampos("Demostracion local").Casilla("Proveedor alternativo", opciones.ProveedorAlternativo).Casilla("Rechazar cobro", opciones.RechazarCobro).Casilla("Fallar notificacion", opciones.FallarNotificacion).Lista("Canal", ["EMAIL", "SMS"], opciones.Canal);
        if (d.ShowDialog(this) != DialogResult.OK) return;
        opciones.ProveedorAlternativo = d.Marcado("Proveedor alternativo"); opciones.RechazarCobro = d.Marcado("Rechazar cobro"); opciones.FallarNotificacion = d.Marcado("Fallar notificacion"); opciones.Canal = d.Seleccion<string>("Canal");
        await operaciones.Ejecutar(s => s.GetRequiredService<IBitacora>().Registrar(sesion.Nombre, "DEMOSTRACION", $"Alternativo={opciones.ProveedorAlternativo}; Rechazo={opciones.RechazarCobro}; FalloOutbox={opciones.FallarNotificacion}", Guid.NewGuid().ToString()));
    }
    private Task DemostrarFlyweight()
    {
        var fabrica = new CatalogoVisualFlyweightFactory();
        // La medicion compara estructuras externas equivalentes en el mismo hilo.
        var antes = GC.GetAllocatedBytesForCurrentThread();
        var sin = Enumerable.Range(0, 1000).Select(i => new FilaVisual(i, new MetadatosSku("P01", "Mouse USB", "Accesorios", "Catalogo de demostracion"), 1, 20m)).ToArray();
        var bytesSin = GC.GetAllocatedBytesForCurrentThread() - antes; antes = GC.GetAllocatedBytesForCurrentThread();
        var con = Enumerable.Range(0, 1000).Select(i => new FilaVisual(i, fabrica.Obtener("P01", "Mouse USB"), 1, 20m)).ToArray();
        var bytesCon = GC.GetAllocatedBytesForCurrentThread() - antes; Ui.Cargar(flyweight, con); GC.KeepAlive(sin);
        if (flyweight.Parent is TabPage pagina && pagina.Parent is TabControl pestanas) pestanas.SelectedTab = pagina;
        MessageBox.Show($"Filas: 1000\nInstancias: {fabrica.Instancias}\nReferenceEquals: {ReferenceEquals(con[0].Metadatos, con[999].Metadatos)}\nSin compartir: {bytesSin} bytes asignados\nCompartido: {bytesCon} bytes asignados\nNo representa memoria total del proceso."); return Task.CompletedTask;
    }
    private sealed record FilaVisual(int Fila, MetadatosSku Metadatos, int Cantidad, decimal Precio);
}
