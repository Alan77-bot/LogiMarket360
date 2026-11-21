# LogiMarket 360

Aplicacion academica de escritorio para ventas, kits anidados, inventario, cobros simulados, despacho, devoluciones y auditoria. C# 12, .NET 8, Windows Forms, EF Core 8 y SQLite. Sin pagos ni mensajes externos reales.

## Abrir y ejecutar

Requisitos: Windows, Visual Studio 2022 con desarrollo de escritorio .NET y SDK 8.0.428. Abrir `LogiMarket360.sln` y establecer `LogiMarket.WinForms` como proyecto de inicio.

Desde PowerShell, en la raiz:

```powershell
dotnet restore LogiMarket360.sln
dotnet build LogiMarket360.sln
dotnet run --project src/LogiMarket.WinForms
```

Al iniciar se aplican las migraciones y el seed idempotente. La base se guarda en `%LOCALAPPDATA%\LogiMarket360\logimarket.db`; los TXT, en la subcarpeta `comprobantes`. No es necesario editar tablas manualmente.

## Cuentas ficticias

| Usuario | Clave de demostracion | Rol |
|---|---|---|
| admin | Demo123! | ADMIN |
| ventas | Demo123! | VENTAS |
| bodega | Demo123! | BODEGA |
| consulta | Demo123! | CONSULTA |

Las claves anteriores son exclusivamente datos publicos de demostracion. La base almacena PBKDF2-SHA256 con sal aleatoria, 210000 iteraciones y comparacion de tiempo constante.

## Recorrido de venta

1. Acceder como admin o ventas.
2. En Carrito, seleccionar Ana, ESTANDAR y KIT_TELETRABAJO; cantidad 1. Agregar y guardar.
3. En Pedidos, seleccionar el borrador y Validar.
4. En Cobro, confirmar usando la clave propuesta. Total: USD 59.00.
5. En Despacho y entrega, registrar despacho y entrega con ADMIN/BODEGA.
6. En Devoluciones, seleccionar DEFECTO: reembolso USD 54.00, sin envio.
7. Consultar movimientos, Outbox, auditoria y comprobantes.

Repetir un cobro con la misma clave devuelve el pago original. Una clave distinta sobre un pedido pagado se rechaza. Las reglas se verifican dentro de Application, aunque se invoquen sin interfaz.

En Auditoria y demo, ADMIN puede cambiar proveedor, rechazar cobros, fallar notificaciones, alternar EMAIL/SMS y recuperar Outbox. Los avisos son registros locales.

## Pruebas

```powershell
dotnet test tests/LogiMarket.Tests --logger "trx;LogFileName=aceptacion.trx" --results-directory docs/evidencias/TestResults
```

Cada escenario crea su propia base temporal con migraciones y seed, y elimina solamente sus archivos al terminar. No se necesita borrar la base de ejecucion para ejecutar pruebas. Si el aislamiento de una herramienta impide la conexion entre VSTest y testhost, ejecutar el mismo comando en una terminal local autorizada.

Para reiniciar una demostracion manual, cerrar la aplicacion y **renombrar como respaldo** la carpeta `%LOCALAPPDATA%\LogiMarket360`. La siguiente apertura creara otra base. No borrar una carpeta con datos que se quieran conservar.

## Migraciones

```powershell
dotnet tool restore
dotnet ef migrations list --project src/LogiMarket.Infrastructure
```

`dotnet-ef` es una herramienta local del repositorio, no global. La fabrica de diseno usa `logimarket-design.db`; la aplicacion utiliza la ruta de LocalApplicationData indicada arriba.

## Arquitectura y defensa

- `src/LogiMarket.Domain`: invariantes, estados, estrategias, Composite, Builder, Prototype, Iterator, Visitor y Memento.
- `src/LogiMarket.Application`: casos de uso, autorizacion, contratos, comandos, Facade, Chain, Interpreter, Bridge, Observer y metricas.
- `src/LogiMarket.Infrastructure`: EF/SQLite, migraciones, repositorios, transacciones, proveedores, Adapter, Decorator, notificaciones y Outbox.
- `src/LogiMarket.WinForms`: vistas, Mediator y composition root.
- `tests/LogiMarket.Tests`: dominio, patrones, integracion, aceptacion y arquitectura.

Consultar `docs/patrones/MAPA.md`, `docs/DEFENSA.md`, `docs/arquitectura/DISENO.md`, `docs/calidad/RIESGOS.md` y la matriz de pruebas. Los resultados reales estan en los TRX; una prueba automatizada de contenido no equivale a una comprobacion visual de pantalla.

## Entrega

El paquete de codigo debe excluir bin, obj, .vs, bases de ejecucion, secretos y temporales. El informe debe identificar al estudiante y asignatura con los datos reales proporcionados por el autor; no se inventan nombres ni firmas.
