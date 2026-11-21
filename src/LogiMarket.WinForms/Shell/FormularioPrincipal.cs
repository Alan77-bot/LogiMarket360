using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Comprobantes;
using LogiMarket.Application.Eventos;
using LogiMarket.Application.Seguridad;
using LogiMarket.WinForms.Features;
using LogiMarket.WinForms.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Shell;

public sealed class FormularioPrincipal : Form
{
    private readonly Panel contenido = new() { Dock = DockStyle.Fill, Padding = new Padding(12) };
    private readonly Label titulo = new() { Dock = DockStyle.Top, Height = 64, Padding = new Padding(18), Font = new Font("Segoe UI", 16, FontStyle.Bold) };
    private readonly VistaCarrito carrito;
    public FormularioPrincipal(OperacionesUi operaciones, SesionActual sesion, MetricasSesion metricas, OpcionesDemostracion opciones, ISalidaDocumento archivo)
    {
        Text = "LogiMarket 360"; Width = 1400; Height = 850; MinimumSize = new Size(1050, 650); StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 10); BackColor = Ui.Fondo; AutoScaleMode = AutoScaleMode.Dpi;
        var mediator = new FormularioPrincipalMediator(); var menu = new FlowLayoutPanel { Dock = DockStyle.Left, Width = 240, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, BackColor = Ui.Azul, Padding = new Padding(10) };
        menu.Controls.Add(new Label { Text = $"LOGIMARKET 360\n\n{sesion.Nombre}\nRol: {sesion.Rol}", ForeColor = Color.White, Height = 120, Width = 190, Font = new Font("Segoe UI", 11, FontStyle.Bold) });
        carrito = new(operaciones, mediator);
        var vistas = new (string Nombre, UserControl Vista)[] {
            ("Panel general", new VistaPanel(operaciones, mediator, metricas)), ("Catalogo y kits", new VistaCatalogo(operaciones, mediator)),
            ("Clientes", new VistaClientes(operaciones, mediator)), ("Carrito", carrito),
            ("Pedidos", new VistaPedidos(operaciones, mediator, "Pedidos")), ("Cobro", new VistaPedidos(operaciones, mediator, "Cobro")),
            ("Despacho y entrega", new VistaPedidos(operaciones, mediator, "Despacho")), ("Devoluciones", new VistaPedidos(operaciones, mediator, "Devoluciones")),
            ("Inventario", new VistaInventario(operaciones, mediator)), ("Reportes", new VistaReportes(operaciones, mediator, archivo)),
            ("Auditoria y demo", new VistaAuditoria(operaciones, mediator, opciones, sesion)) };
        foreach (var (nombre, vista) in vistas)
        { var boton = Ui.Boton(nombre, () => { contenido.Controls.Clear(); contenido.Controls.Add(vista); titulo.Text = nombre; return Task.CompletedTask; }); boton.Width = 190; menu.Controls.Add(boton); }
        menu.Controls.Add(Ui.Boton("Cerrar sesion", () => { Close(); return Task.CompletedTask; }));
        Controls.Add(contenido); Controls.Add(titulo); Controls.Add(menu); contenido.Controls.Add(vistas[0].Vista); titulo.Text = vistas[0].Nombre;
        Shown += async (_, _) => { try { await operaciones.Ejecutar(_ => Task.CompletedTask); await mediator.Actualizar(); } catch (Exception ex) {
            var directorio = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LogiMarket360");
            File.AppendAllText(Path.Combine(directorio, "ui-error.log"), DateTimeOffset.UtcNow + " " + ex + Environment.NewLine);
            MessageBox.Show(ex.Message + " " + ex.GetBaseException().Message);
        } };
        FormClosing += (_, e) => { if (carrito.TieneCambios && !Ui.Confirmar("Hay cambios sin guardar en el carrito. Cerrar y descartarlos?")) e.Cancel = true; };
    }
}

public sealed class FormularioAcceso : Form
{
    public FormularioAcceso(OperacionesUi operaciones)
    {
        Text = "Acceso - LogiMarket 360"; Width = 440; Height = 380; StartPosition = FormStartPosition.CenterScreen; Font = new Font("Segoe UI", 11); FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false;
        var panel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, Padding = new Padding(30), WrapContents = false };
        var usuario = new TextBox { Width = 340, PlaceholderText = "Usuario", AccessibleName = "Usuario" };
        var clave = new TextBox { Width = 340, PlaceholderText = "Contrasena", UseSystemPasswordChar = true, AccessibleName = "Contrasena" };
        var error = new Label { ForeColor = Color.DarkRed, Width = 340, Height = 55 };
        panel.Controls.Add(new Label { Text = "LOGIMARKET 360", AutoSize = true, Font = new Font("Segoe UI", 18, FontStyle.Bold) });
        panel.Controls.Add(new Label { Text = "Usuario", AutoSize = true }); panel.Controls.Add(usuario); panel.Controls.Add(new Label { Text = "Contrasena", AutoSize = true }); panel.Controls.Add(clave);
        var entrar = Ui.Boton("Iniciar sesion", async () => {
            try { await operaciones.Ejecutar(s => s.GetRequiredService<Autenticacion>().Entrar(usuario.Text, clave.Text)); DialogResult = DialogResult.OK; Close(); }
            catch (Exception ex) { error.Text = ex.GetBaseException().Message; clave.SelectAll(); clave.Focus(); }
        }); panel.Controls.Add(entrar); panel.Controls.Add(error); Controls.Add(panel); AcceptButton = entrar;
    }
}
