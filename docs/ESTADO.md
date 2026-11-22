# Seguimiento APE 04

Fuente: GUIA_APE_04.docx proporcionada por el usuario. No modificar los escenarios P01–P38 ni usar las cifras ilustrativas de los wireframes como reglas.

Estado inicial: únicamente global.json; SDK 8.0.428 y 10.0.401; Git 2.55.0; sin solución ni repositorio previo.

Verificacion del 21 de noviembre de 2026: cinco proyectos .NET 8, 23 patrones integrados y localizados en docs/patrones/MAPA.md, dos migraciones EF8/SQLite, interfaz WinForms ejecutada, 58 pruebas automaticas aprobadas y matriz P01-P38 enlazada a evidencias reales. El codigo del commit 0c3c25f se exporto a una copia sin bin/obj y compilo con cero errores y advertencias.

Se ejecuto visualmente la venta de Ana por USD 59.00, despacho, entrega, devolucion por USD 54.00, cuatro combinaciones de comprobantes y reapertura con persistencia. Se corrigio un fallo visual de redistribucion de columnas y se conservaron logs y capturas. Datos institucionales pendientes por solicitud del usuario.

La revision visual no certifica todas las combinaciones posibles de resolucion, escala y accesibilidad. El registro de riesgos distingue correcciones de diseno frente a pruebas rojas efectivamente ejecutadas.

Se corrigio ademas la valoracion de cantidades editadas: las dos pruebas previas fallidas se conservan y ahora pasan dentro del conjunto completo. Los importes se recalculan desde las instantaneas antes de validar.

22 de noviembre de 2026: v1.1.0 (2d35606) verificada nuevamente: 58/58 pruebas, cero omitidas, y compilacion limpia del ZIP publicado con cero errores y advertencias. Once vistas instanciadas con base temporal y revisadas en ventanas de 1400x850 y 1050x650. El recorrido programatico SelectNextControl no reemplaza una prueba manual de TAB/foco ni una revision de DPI real. PDF final pendiente por indicacion del usuario. Ver VERIFICACION_V1_1_0.md para limites y evidencias.
