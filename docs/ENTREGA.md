# Entrega y comprobaciones

## Archivos finales

- `output/APE04_LogiMarket360_DATOS_PENDIENTES.zip`: codigo versionado, proyectos, migraciones, seed, pruebas, documentacion y evidencias. Sin bin/obj/.vs ni base de ejecucion.
- `output/pdf/APE04_Informe_DATOS_PENDIENTES.pdf`: informe tecnico con el orden de la guia, resultados, diagramas, patrones, riesgos y capturas reales.

Los datos de portada permanecen pendientes por solicitud expresa del usuario. No se inventaron nombres institucionales ni firmas.

## Verificacion

- Cinco proyectos: Domain, Application, Infrastructure, WinForms y Tests.
- Framework objetivo: .NET 8; WinForms usa net8.0-windows.
- 58 pruebas automaticas aprobadas en el TRX final.
- Casos oficiales P01-P38 conservados y enlazados a archivos/metodos y evidencias.
- 23 patrones localizados en docs/patrones/MAPA.md.
- Compilacion de una copia sin bin/obj: cero errores y advertencias; codigo del commit 0c3c25f.
- Venta, despacho, entrega, devolucion y persistencia observados en la interfaz; registro en docs/evidencias/REVISION_VISUAL.md.
- Comentarios C# sin tildes; guia de modificacion para la defensa en docs/DEFENSA.md.

El historial es real: no se reconstruyeron commits anteriores para simular desarrollo por etapas. Las correcciones que no tuvieron prueba roja previa estan identificadas como revision de diseno en la documentacion.

## Reproducir

1. Extraer el ZIP y abrir LogiMarket360.sln en Visual Studio 2022.
2. Restaurar y compilar. Iniciar LogiMarket.WinForms.
3. La base nueva recibe migraciones y seed automaticamente.
4. Acceder con admin / Demo123! (cuenta ficticia).
5. Seguir README.md para venta, pruebas y reinicio seguro de datos.

El PDF se regenera con el script Python y ReportLab. Los PNG temporales de revision no forman parte del paquete.
