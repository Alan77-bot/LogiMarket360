using System.Text;
using LogiMarket.Application.Comprobantes;

namespace LogiMarket.Infrastructure.Documents;

public sealed class SalidaArchivoTxt(string directorio) : ISalidaDocumento
{
    public async Task Emitir(string nombre, string contenido)
    { Directory.CreateDirectory(directorio); await File.WriteAllTextAsync(Path.Combine(directorio, Path.GetFileName(nombre) + ".txt"), contenido, Encoding.UTF8); }
}
