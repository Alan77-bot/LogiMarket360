# Entrega y comprobaciones

## Archivos finales

- `output/APE04_LogiMarket360_DATOS_PENDIENTES.zip`: codigo versionado, proyectos, migraciones, seed, pruebas, documentacion y evidencias. Sin bin/obj/.vs ni base de ejecucion.
- PDF final: PENDIENTE por indicacion expresa del usuario. El archivo local anterior es un borrador, no una entrega final.
- `output/LogiMarket360-v1.1.0.zip`: paquete publicado con la mejora del panel general.

Los datos de portada permanecen pendientes por solicitud expresa del usuario. No se inventaron nombres institucionales ni firmas.

## Verificacion

- Cinco proyectos: Domain, Application, Infrastructure, WinForms y Tests.
- Framework objetivo: .NET 8; WinForms usa net8.0-windows.
- 58 pruebas automaticas aprobadas en el TRX final.
- Casos oficiales P01-P38 conservados y enlazados a archivos/metodos y evidencias.
- 23 patrones localizados en docs/patrones/MAPA.md.
- Compilacion del ZIP publicado v1.1.0, commit 2d35606, en una copia sin bin/obj: cero errores y advertencias.
- Venta, despacho, entrega, devolucion y persistencia observados en la interfaz; registro en docs/evidencias/REVISION_VISUAL.md.
- Comentarios C# sin tildes; guia de modificacion para la defensa en docs/DEFENSA.md.

El historial es real: no se reconstruyeron commits anteriores para simular desarrollo por etapas. Las correcciones que no tuvieron prueba roja previa estan identificadas como revision de diseno en la documentacion.

La comprobacion del 22 de noviembre de 2026 esta en `docs/evidencias/VERIFICACION_V1_1_0.md`. No se declara cumplimiento total del requisito de diez refactorizaciones con Git y pruebas antes/despues ni de la revision de accesibilidad en todas las escalas de monitor.

## Reproducir

1. Extraer el ZIP y abrir LogiMarket360.sln en Visual Studio 2022.
2. Restaurar y compilar. Iniciar LogiMarket.WinForms.
3. La base nueva recibe migraciones y seed automaticamente.
4. Acceder con admin / Demo123! (cuenta ficticia).
5. Seguir README.md para venta, pruebas y reinicio seguro de datos.

El PDF se regenera con el script Python y ReportLab. Los PNG temporales de revision no forman parte del paquete.
