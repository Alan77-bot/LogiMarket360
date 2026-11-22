# Revision del panel general

Fecha: 22 de noviembre de 2026.

Se sustituyo el resumen de texto por cuatro tarjetas, conteos por estado, alertas de stock, cinco pedidos recientes y actividad de la sesion. Los valores proceden de las mismas consultas de Application y del Singleton MetricasSesion. Mediator conserva el refresco automatico; Actualizar permite solicitar otra lectura.

Verificacion: compilacion de WinForms sin errores ni advertencias. Se instancio la vista real en un formulario de comprobacion y se ejecuto Refrescar contra una base SQLite temporal independiente, con migraciones y seed. Se revisaron imagenes generadas con DrawToBitmap en anchos de 1120 y 770 pixeles. Se corrigieron textos cortados y la reserva de espacio para el mensaje de inventario. La captura 21 corresponde a este formulario de comprobacion con datos iniciales; no es una captura del flujo completo de venta ni una certificacion de todos los niveles de DPI.

Los cambios afectan solo la presentacion. No se modificaron reglas de negocio, persistencia ni la Release v1.0.0 ya publicada. El PDF sigue pendiente por indicacion del usuario.
