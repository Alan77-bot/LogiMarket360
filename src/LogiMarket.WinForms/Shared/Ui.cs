using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Seguridad;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Shared;

// El ambito vive solo durante una operacion; no se comparte DbContext entre pantallas.
public sealed class OperacionesUi(IServiceProvider proveedor)
{
    public async Task<T> Consultar<T>(Func<IServiceProvider, Task<T>> accion)
    { using var ambito = proveedor.CreateScope(); await ambito.ServiceProvider.GetRequiredService<Autorizador>().Lectura(); return await accion(ambito.ServiceProvider); }
    public async Task Ejecutar(Func<IServiceProvider, Task> accion)
    {
        using var ambito = proveedor.CreateScope(); await accion(ambito.ServiceProvider);
        await ambito.ServiceProvider.GetRequiredService<IProcesadorOutbox>().Procesar();
    }
}
// Mediator: las vistas se registran y nunca llaman a otras vistas directamente.
public sealed class FormularioPrincipalMediator : LogiMarket.Application.Eventos.MediadorVistas
{
    public event Func<LogiMarket.Domain.Pedidos.Pedido, Task>? BorradorSolicitado;
    public Task EditarBorrador(LogiMarket.Domain.Pedidos.Pedido pedido) => BorradorSolicitado?.Invoke(pedido) ?? Task.CompletedTask;
}
public static class Ui
{
    public static readonly Color Fondo = Color.FromArgb(244, 247, 251);
    public static readonly Color Azul = Color.FromArgb(28, 68, 111);
    public static DataGridView Grilla()
    {
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false, BackgroundColor = Color.White,
        BorderStyle = BorderStyle.None, RowHeadersVisible = false, AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells };
        grid.CellFormatting += (_, e) => {
            if (e.Value is DateTimeOffset fecha)
            { e.Value = TimeZoneInfo.ConvertTime(fecha, TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time")).ToString("yyyy-MM-dd HH:mm"); e.FormattingApplied = true; }
        };
        return grid;
    }
    public static Button Boton(string texto, Func<Task> accion)
    {
        var boton = new Button { Text = texto, AutoSize = true, Height = 36, FlatStyle = FlatStyle.Flat, Padding = new Padding(8), BackColor = Color.White };
        boton.Click += async (_, _) => { boton.Enabled = false; try { await accion(); } catch (Exception ex) { MessageBox.Show(ex.GetBaseException().Message, "LogiMarket 360", MessageBoxButtons.OK, MessageBoxIcon.Warning); } finally { boton.Enabled = true; } };
        return boton;
    }
    public static FlowLayoutPanel Barra() => new() { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8), WrapContents = true };
    public static bool Confirmar(string mensaje) => MessageBox.Show(mensaje, "Confirmar operacion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    public static void Cargar<T>(DataGridView grid, IEnumerable<T> datos)
    {
        var seleccion = grid.CurrentRow?.Cells[0].Value;
        grid.DataSource = datos.ToList();
        foreach (DataGridViewColumn c in grid.Columns)
        {
            if (c.ValueType == typeof(decimal)) c.DefaultCellStyle.Format = "N2";
            if (c.ValueType == typeof(Guid) || c.ValueType == typeof(Guid?) || c.Name is "Version" or "Hash") c.Visible = false;
            c.HeaderText = c.Name switch { "ClienteNombre" => "Cliente", "ClienteVip" => "VIP", "EsKit" => "Kit", "EntregadoEn" => "Entrega registrada", _ => c.Name };
            // AllCells evita redistribuir columnas de vistas que aun no tienen ventana.
        }
        if (seleccion is not null) foreach (DataGridViewRow fila in grid.Rows) if (Equals(fila.Cells[0].Value, seleccion)) { grid.CurrentCell = fila.Cells.Cast<DataGridViewCell>().First(c => c.Visible); break; }
    }
}

public sealed class DialogoCampos : Form
{
    private readonly Dictionary<string, Control> campos = [];
    private readonly TableLayoutPanel tabla = new() { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 2, Padding = new Padding(18) };
    public DialogoCampos(string titulo)
    {
        Text = titulo; Width = 540; Height = 490; StartPosition = FormStartPosition.CenterParent; Font = new Font("Segoe UI", 10);
        tabla.ColumnStyles.Add(new(SizeType.Absolute, 160)); tabla.ColumnStyles.Add(new(SizeType.Percent, 100));
        var pie = Ui.Barra(); pie.Dock = DockStyle.Bottom;
        var aceptar = new Button { Text = "Aceptar", AutoSize = true };
        var errores = new ErrorProvider { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        aceptar.Click += (_, _) => {
            var valido = true;
            foreach (var (nombre, control) in campos)
            {
                var error = control is TextBox texto && string.IsNullOrWhiteSpace(texto.Text) ? "Campo obligatorio."
                    : control is ComboBox combo && combo.SelectedItem is null ? "Seleccione una opcion."
                    : control is NumericUpDown numero && (nombre == "Precio" && numero.Value <= 0 || nombre.StartsWith("Cantidad") && numero.Value == 0) ? "Valor invalido." : "";
                errores.SetError(control, error); valido &= error.Length == 0;
            }
            if (valido) DialogResult = DialogResult.OK;
        };
        Disposed += (_, _) => errores.Dispose();
        pie.Controls.Add(aceptar); pie.Controls.Add(new Button { Text = "Cancelar", DialogResult = DialogResult.Cancel, AutoSize = true });
        Controls.Add(tabla); Controls.Add(pie); AcceptButton = aceptar;
        Shown += (_, _) => Height = Math.Clamp(140 + campos.Count * 48, 210, 650);
    }
    public DialogoCampos Texto(string nombre, string valor = "") => Agregar(nombre, new TextBox { Text = valor, Dock = DockStyle.Top });
    public DialogoCampos Numero(string nombre, decimal valor = 0, int decimales = 0, decimal minimo = 0) => Agregar(nombre, new NumericUpDown { Value = valor, Minimum = minimo, Maximum = 10000000, DecimalPlaces = decimales, Dock = DockStyle.Top });
    public DialogoCampos Lista(string nombre, object[] valores, object? seleccionado = null)
    { var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top }; combo.Items.AddRange(valores); if (seleccionado is not null) combo.SelectedItem = seleccionado; if (combo.SelectedIndex < 0 && valores.Length > 0) combo.SelectedIndex = 0; return Agregar(nombre, combo); }
    public DialogoCampos Casilla(string nombre, bool valor) => Agregar(nombre, new CheckBox { Checked = valor });
    private DialogoCampos Agregar(string nombre, Control control)
    { var fila = tabla.RowCount++; tabla.RowStyles.Add(new(SizeType.AutoSize)); tabla.Controls.Add(new Label { Text = nombre, AutoSize = true, Padding = new Padding(0, 7, 0, 10) }, 0, fila); tabla.Controls.Add(control, 1, fila); campos.Add(nombre, control); return this; }
    public string Texto(string nombre) => ((TextBox)campos[nombre]).Text;
    public decimal Numero(string nombre) => ((NumericUpDown)campos[nombre]).Value;
    public T Seleccion<T>(string nombre) => (T)((ComboBox)campos[nombre]).SelectedItem!;
    public bool Marcado(string nombre) => ((CheckBox)campos[nombre]).Checked;
}

