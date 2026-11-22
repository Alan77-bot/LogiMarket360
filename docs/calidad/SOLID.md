# SOLID en el codigo real

## SRP

`CalculadoraPrecios.Calcular` decide importes. `RepositorioEf.Ejecutar` controla persistencia. `VistaPedidos.Cobrar` captura la clave y solicita un comando. Cada colaborador cambia por un motivo distinto.

## OCP

La calculadora recibe `IEnumerable<IPoliticaDescuento>` y elige por prioridad. Una promocion nueva implementa `Aplica`, `Tasa` y `Prioridad`, y se registra en DI. `P32_ExtenderStrategySinCambiarCalculadora` incorpora una politica nueva sin editar la calculadora.

## LSP

`PasarelaLegadaAdapter` y `PasarelaAlternativaSimulada` implementan `IPasarelaPago.CobrarAsync(decimal importe, string clave)`. `P16_P38_LspProveedores` ejecuta los mismos escenarios para ambos: importe valido, reintento, entrada invalida y rechazo. Se conservan resultados y excepciones compatibles.

## ISP

`IPasarelaPago` solo cobra; `IReembolsosSimulados` solo reembolsa; `ISalidaDocumento` solo emite documentos. Los notificadores no deben implementar pagos ni consultas. Las consultas operativas y el control transaccional tienen contratos distintos.

## DIP

`ConfirmacionPedidoFacade` recibe `IRepositorio`, `ITransacciones`, `Autorizador`, `IPasarelaPago`, `SesionActual` e `IReloj` por constructor. No instancia EF ni SQLite. El concreto se registra en `RegistroServicios.AgregarLogiMarket`. `P20_FalloColaboradorNoDejaPersistenciaParcial` sustituye el puerto para comprobar el fallo.

## Recorrido para demostrarlo

Poner puntos de interrupcion en `VistaPedidos.Cobrar`, `InvocadorComandos.Ejecutar`, `ConfirmacionPedidoFacade.Confirmar`, `PagoAutorizadoProxy.CobrarAsync` y `RepositorioEf.Ejecutar`. Confirmar una venta y observar como cada responsabilidad se delega.
