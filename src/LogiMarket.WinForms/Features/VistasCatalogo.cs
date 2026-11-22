using LogiMarket.Application.Abstractions;
using LogiMarket.Application.Catalogo;
using LogiMarket.Application.Clientes;
using LogiMarket.Application.Logistica;
using LogiMarket.Domain.Modelo;
using LogiMarket.WinForms.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace LogiMarket.WinForms.Features;

public sealed class VistaCatalogo : UserControl
{
    private readonly OperacionesUi operaciones;
    private readonly FormularioPrincipalMediator mediator;
    private readonly DataGridView grid = Ui.Grilla();
    private readonly TextBox buscar = new() { Width = 220, PlaceholderText = "SKU o nombre" };
    public VistaCatalogo(OperacionesUi operaciones, FormularioPrincipalMediator mediator, PermisosVista permisos)
    {
        this.operaciones = operaciones; this.mediator = mediator; Dock = DockStyle.Fill;
        var barra = Ui.Barra(); barra.Controls.Add(buscar);
        barra.Controls.Add(Ui.Boton("Buscar", Refrescar));
        if (permisos.Administrar)
        {
            barra.Controls.Add(Ui.Boton("Nuevo producto", () => EditarProducto(null)));
            barra.Controls.Add(Ui.Boton("Editar", Editar)); barra.Controls.Add(Ui.Boton("Nuevo kit", () => EditarKit(null)));
            barra.Controls.Add(Ui.Boton("Eliminar", Eliminar));
        }
        Controls.Add(grid); Controls.Add(barra); mediator.Registrar(Refrescar);
    }
    public async Task Refrescar() => Ui.Cargar(grid, await operaciones.Consultar(s => s.GetRequiredService<ServicioCatalogo>().Consultar(buscar.Text)));
    private FilaCatalogo Seleccion() => grid.CurrentRow?.DataBoundItem as FilaCatalogo ?? throw new InvalidOperationException("Seleccione una fila.");
    private Task Editar() { var fila = Seleccion(); return fila.EsKit ? EditarKit(fila) : EditarProducto(fila); }
    private async Task EditarProducto(FilaCatalogo? fila)
    {
        using var d = new DialogoCampos("Producto").Texto("SKU", fila?.Sku ?? "").Texto("Nombre", fila?.Nombre ?? "")
            .Numero("Precio", fila?.Precio ?? 1, 2).Numero("Peso", fila?.Peso ?? 0, 2).Numero("Stock inicial", fila?.Stock ?? 0);
        if (d.ShowDialog(this) != DialogResult.OK) return;
        await operaciones.Ejecutar(s => s.GetRequiredService<ServicioCatalogo>().GuardarProducto(fila?.Id, d.Texto("SKU"), d.Texto("Nombre"), d.Numero("Precio"), d.Numero("Peso"), (int)d.Numero("Stock inicial")));
        await mediator.Actualizar();
    }
    private async Task EditarKit(FilaCatalogo? fila)
    {
        var datos = await operaciones.Consultar(async s => (Filas: await s.GetRequiredService<ServicioCatalogo>().Consultar(), Kits: await s.GetRequiredService<IRepositorio>().Kits()));
        var actual = datos.Kits.SingleOrDefault(k => k.Id == fila?.Id);
        var texto = actual is null ? "P01:1,P02:1" : string.Join(",", actual.Componentes.Select(c => $"{datos.Filas.Single(f => f.Id == (c.ProductoId ?? c.KitHijoId)).Sku}:{c.Cantidad}"));
        using var d = new DialogoCampos("Composicion del kit").Texto("SKU", fila?.Sku ?? "").Texto("Nombre", fila?.Nombre ?? "").Texto("SKU:cantidad", texto);
        if (d.ShowDialog(this) != DialogResult.OK) return;
        var componentes = d.Texto("SKU:cantidad").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => {
            var partes = t.Split(':'); if (partes.Length != 2 || !int.TryParse(partes[1], out var cantidad)) throw new ArgumentException("Use P01:1,P02:2.");
            var elegido = datos.Filas.Single(f => f.Sku.Equals(partes[0].Trim(), StringComparison.OrdinalIgnoreCase)); return new ComponenteEntrada(elegido.Id, elegido.EsKit, cantidad);
        }).ToArray();
        await operaciones.Ejecutar(s => s.GetRequiredService<ServicioCatalogo>().GuardarKit(fila?.Id, d.Texto("SKU"), d.Texto("Nombre"), componentes)); await mediator.Actualizar();
    }
    private async Task Eliminar()
    { var fila = Seleccion(); if (Ui.Confirmar($"Eliminar {fila.Sku}? Las referencias existentes pueden impedirlo.")) { await operaciones.Ejecutar(s => s.GetRequiredService<ServicioCatalogo>().Eliminar(fila.Id, fila.EsKit)); await mediator.Actualizar(); } }
}

public sealed class VistaClientes : UserControl
{
    private readonly DataGridView grid = Ui.Grilla();
    private readonly TextBox buscar = new() { Width = 220, PlaceholderText = "Nombre" };
    private readonly OperacionesUi operaciones;
    private readonly FormularioPrincipalMediator mediator;
    public VistaClientes(OperacionesUi operaciones, FormularioPrincipalMediator mediator, PermisosVista permisos)
    {
        this.operaciones = operaciones; this.mediator = mediator; Dock = DockStyle.Fill;
        var barra = Ui.Barra(); barra.Controls.Add(buscar); barra.Controls.Add(Ui.Boton("Buscar", Refrescar));
        if (permisos.Vender)
        {
            barra.Controls.Add(Ui.Boton("Nuevo cliente", () => Editar(null))); barra.Controls.Add(Ui.Boton("Editar cliente", () => Editar(grid.CurrentRow?.DataBoundItem as Cliente ?? throw new InvalidOperationException("Seleccione cliente."))));
        }
        Controls.Add(grid); Controls.Add(barra); mediator.Registrar(Refrescar);
    }
    public async Task Refrescar() => Ui.Cargar(grid, await operaciones.Consultar(s => s.GetRequiredService<ServicioClientes>().Consultar(buscar.Text)));
    private async Task Editar(Cliente? cliente)
    {
        using var d = new DialogoCampos("Cliente").Texto("Nombre", cliente?.Nombre ?? "").Lista("Categoria", Enum.GetValues<CategoriaCliente>().Cast<object>().ToArray(), cliente?.Categoria).Casilla("Activo", cliente?.Activo ?? true);
        if (d.ShowDialog(this) != DialogResult.OK) return;
        await operaciones.Ejecutar(s => s.GetRequiredService<ServicioClientes>().Guardar(cliente?.Id, d.Texto("Nombre"), d.Seleccion<CategoriaCliente>("Categoria"), d.Marcado("Activo"))); await mediator.Actualizar();
    }
}

public sealed class VistaInventario : UserControl
{
    private readonly DataGridView stock = Ui.Grilla();
    private readonly DataGridView movimientos = Ui.Grilla();
    private readonly OperacionesUi operaciones;
    private readonly FormularioPrincipalMediator mediator;
    public VistaInventario(OperacionesUi operaciones, FormularioPrincipalMediator mediator, PermisosVista permisos)
    {
        this.operaciones = operaciones; this.mediator = mediator; Dock = DockStyle.Fill;
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal }; split.Panel1.Controls.Add(stock); split.Panel2.Controls.Add(movimientos);
        var barra = Ui.Barra();
        if (permisos.OperarBodega) barra.Controls.Add(Ui.Boton("Registrar entrada / salida", Mover));
        barra.Controls.Add(Ui.Boton("Actualizar", Refrescar));
        Controls.Add(split); Controls.Add(barra); mediator.Registrar(Refrescar);
    }
    public async Task Refrescar()
    {
        var datos = await operaciones.Consultar(async s => (Productos: await s.GetRequiredService<IRepositorio>().Productos(), Movimientos: await s.GetRequiredService<IConsultasOperativas>().Movimientos()));
        Ui.Cargar(stock, datos.Productos.OrderBy(p => p.Sku));
        Ui.Cargar(movimientos, datos.Movimientos.OrderByDescending(m => m.Fecha).Select(m => new { Sku = datos.Productos.Single(p => p.Id == m.ProductoId).Sku, m.Cantidad, m.Motivo, m.Referencia, m.Fecha }));
    }
    private async Task Mover()
    {
        var producto = stock.CurrentRow?.DataBoundItem as Producto ?? throw new InvalidOperationException("Seleccione producto.");
        using var d = new DialogoCampos($"Movimiento {producto.Sku}").Numero("Cantidad con signo", 1, 0, -1000000).Texto("Motivo", "AJUSTE").Texto("Referencia", Guid.NewGuid().ToString());
        if (d.ShowDialog(this) != DialogResult.OK || !Ui.Confirmar("Registrar este movimiento de inventario?")) return;
        await operaciones.Ejecutar(s => s.GetRequiredService<ServicioInventario>().Mover(producto.Id, (int)d.Numero("Cantidad con signo"), d.Texto("Motivo"), d.Texto("Referencia"))); await mediator.Actualizar();
    }
}
