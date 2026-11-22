using LogiMarket.Domain.Modelo;

namespace LogiMarket.WinForms.Shared;

// Solo decide la presentacion. Application sigue autorizando cada operacion real.
public sealed class PermisosVista(Rol? rol)
{
    public bool Administrar => rol == Rol.ADMIN;
    public bool Vender => rol is Rol.ADMIN or Rol.VENTAS;
    public bool OperarBodega => rol is Rol.ADMIN or Rol.BODEGA;
    public bool Consultar => rol is Rol.ADMIN or Rol.VENTAS or Rol.BODEGA or Rol.CONSULTA;
    public bool Ver(string apartado) => apartado switch
    {
        "Panel general" or "Catalogo y kits" => Consultar,
        "Clientes" or "Pedidos" or "Reportes" => Vender || rol == Rol.CONSULTA,
        "Carrito" or "Cobro" => Vender,
        "Despacho y entrega" or "Inventario" => OperarBodega || rol == Rol.CONSULTA,
        "Devoluciones" => Vender || rol == Rol.CONSULTA,
        "Auditoria y demo" => Administrar,
        _ => false
    };
}
