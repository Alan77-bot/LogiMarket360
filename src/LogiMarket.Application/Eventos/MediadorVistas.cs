namespace LogiMarket.Application.Eventos;

// El nucleo de coordinacion admite pruebas sin cargar Windows Forms.
public class MediadorVistas
{
    private readonly List<Func<Task>> vistas = [];
    public void Registrar(Func<Task> refrescar) => vistas.Add(refrescar);
    public async Task Actualizar()
    {
        foreach (var vista in vistas)
            try { await vista(); }
            catch (Exception ex) { throw new InvalidOperationException($"No se pudo actualizar {vista.Method.DeclaringType?.Name}.", ex); }
    }
}
