using LogiMarket.Domain.Modelo;
using LogiMarket.Domain.Pedidos;

namespace LogiMarket.Application.Pedidos;

public sealed record ContextoValidacion(Pedido Pedido, Cliente? Cliente, IReadOnlyList<Producto> Productos);
public sealed record ResultadoValidacion(bool Valido, string Codigo, string Mensaje)
{ public static ResultadoValidacion Correcto => new(true, "OK", "Pedido valido."); }

// Chain of Responsibility: se detiene en la primera causa verificable.
public abstract class ValidadorPedido
{
    private ValidadorPedido? siguiente;
    public ValidadorPedido Luego(ValidadorPedido valor) { siguiente = valor; return valor; }
    protected abstract ResultadoValidacion Revisar(ContextoValidacion c);
    public ResultadoValidacion Validar(ContextoValidacion c)
    { var resultado = Revisar(c); return resultado.Valido ? siguiente?.Validar(c) ?? resultado : resultado; }
}
public sealed class ClienteActivoValidador : ValidadorPedido
{
    protected override ResultadoValidacion Revisar(ContextoValidacion c) => c.Cliente is { Activo: true } ? ResultadoValidacion.Correcto : new(false, "CLIENTE", "Cliente inexistente o inactivo.");
}
public sealed class LineasValidador : ValidadorPedido
{
    protected override ResultadoValidacion Revisar(ContextoValidacion c) => c.Pedido.Detalles.Count > 0 && c.Pedido.Detalles.All(d => d.Cantidad > 0 && d.Componentes.Count > 0) ? ResultadoValidacion.Correcto : new(false, "LINEAS", "Lineas incompletas.");
}
public sealed class StockValidador : ValidadorPedido
{
    protected override ResultadoValidacion Revisar(ContextoValidacion c)
    {
        foreach (var grupo in c.Pedido.Detalles.SelectMany(d => d.Componentes).GroupBy(p => p.Sku))
            if (c.Productos.SingleOrDefault(p => p.Sku == grupo.Key)?.Stock is not int stock || stock < grupo.Sum(p => p.Cantidad))
                return new(false, "STOCK", $"Stock insuficiente: {grupo.Key}.");
        return ResultadoValidacion.Correcto;
    }
}
public sealed class EstadoConfirmableValidador : ValidadorPedido
{
    protected override ResultadoValidacion Revisar(ContextoValidacion c) => c.Pedido.Estado == EstadoPedido.VALIDADO ? ResultadoValidacion.Correcto : new(false, "ESTADO", "Valide el pedido antes de cobrar.");
}
public static class CadenaConfirmacion
{
    public static ResultadoValidacion Validar(ContextoValidacion c, bool exigirValidado = true)
    {
        var primero = new ClienteActivoValidador(); var ultimo = primero.Luego(new LineasValidador()).Luego(new StockValidador());
        if (exigirValidado) ultimo.Luego(new EstadoConfirmableValidador());
        return primero.Validar(c);
    }
}
