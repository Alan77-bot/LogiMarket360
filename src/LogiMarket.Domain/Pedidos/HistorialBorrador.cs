namespace LogiMarket.Domain.Pedidos;

// Memento: cada entrada es una copia profunda; el historial nunca revierte pagos.
public sealed class HistorialBorrador
{
    private readonly List<Pedido> estados = [];
    private int posicion = -1;
    private bool bloqueado;
    public void Capturar(Pedido pedido)
    {
        Exigir(); pedido.ExigirEditable();
        estados.RemoveRange(posicion + 1, estados.Count - posicion - 1);
        estados.Add(pedido.ClonarBorrador());
        if (estados.Count > 5) estados.RemoveAt(0);
        posicion = estados.Count - 1;
    }
    public Pedido Deshacer() { Exigir(); if (posicion <= 0) throw new InvalidOperationException("No hay cambios para deshacer."); return estados[--posicion].ClonarBorrador(); }
    public Pedido Rehacer() { Exigir(); if (posicion >= estados.Count - 1) throw new InvalidOperationException("No hay cambios para rehacer."); return estados[++posicion].ClonarBorrador(); }
    public void Bloquear() { estados.Clear(); posicion = -1; bloqueado = true; }
    private void Exigir() { if (bloqueado) throw new InvalidOperationException("El pedido confirmado no admite deshacer."); }
}
