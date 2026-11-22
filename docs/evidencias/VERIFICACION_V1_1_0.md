# Comprobacion de v1.1.0

Fecha: 22 de noviembre de 2026. Codigo revisado: 2d35606; etiqueta v1.1.0.

## Pruebas automaticas

Se ejecuto `dotnet test tests/LogiMarket.Tests --no-restore -m:1` con salida TRX. Resultado: 58 aprobadas, 0 fallidas, 0 omitidas. Evidencia: `v1.1.0-pruebas.trx`. Las bases de integracion son independientes de la base del usuario.

## Compilacion desde el ZIP publicado

Se ejecuto `scripts/verificar_paquete.ps1` sobre `output/LogiMarket360-v1.1.0.zip`. Se extrajeron cinco proyectos en una carpeta nueva sin bin/obj/.vs, se restauraron dependencias y se compilo la solucion: 0 errores, 0 advertencias. Ruta de extraccion y SHA256 en `compilacion-limpia.txt`.

## Presentacion y navegacion

Un ejecutor temporal instancio FormularioPrincipal con los servicios reales y una base SQLite nueva. Abrio las once vistas navegables y ejecuto sus consultas. Se revisaron imagenes DrawToBitmap en ventanas 1400x850 y 1050x650. La captura 22 agrupa las once vistas en el tamano menor. El menu y el panel general ofrecen desplazamiento cuando no cabe todo; las barras de acciones se redistribuyen y las tablas conservan desplazamiento horizontal.

Se recorrio programaticamente cada vista mediante SelectNextControl. Todas permitieron avanzar. El registro de cantidades y destinos esta en `v1.1.0-navegacion.txt`; destinos con el mismo tipo/texto pueden agruparse, por lo que esa cifra no cuenta identidades unicas de controles. Esto comprueba navegabilidad programatica; no se presenta como una prueba manual de la tecla TAB ni como una comprobacion de foco visible en cada control. Tampoco se cambiaron las escalas de monitor: resize y DPI son comprobaciones diferentes.

## Pendientes reales de la guia

- PDF final, datos de portada y nombres definitivos de archivos: pendientes por decision del usuario.
- Existen 14 riesgos documentados, pero no diez refactorizaciones con commits separados y pruebas rojas/verdes previas/posteriores. El historial conservado no permite afirmar ese cumplimiento. No se inventaron defectos, commits ni capturas retrospectivos.
- Queda revision manual completa de TAB/foco, dialogos y DPI real. Las comprobaciones anteriores cubren las vistas principales y dos tamanos de ventana.

No se modifico codigo de produccion durante esta comprobacion. Los archivos de evidencia se agregan en un commit posterior; la etiqueta y el ZIP v1.1.0 conservan su codigo y contenido originales.
