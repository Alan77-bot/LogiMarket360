# Matriz oficial P01-P38

Fecha de ejecucion: 2026-11-21T14:31:49.7561172-05:00. Pruebas automaticas: 58/58.

Los escenarios y esperados se extrajeron de la guia. Los metodos y estados proceden del TRX; las notas visuales corresponden a capturas reales.

## P01 - APROBADO

**Precondicion y entrada:** Creación de DB desde archivo inexistente y seed.

**Esperado:** Migraciones aplicadas; cinco productos, dos kits y roles; sin duplicados al relanzar seed.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Base nueva creada por migraciones, cinco productos, dos kits, cuatro usuarios y ninguna migracion pendiente; seed repetido sin duplicados.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P01_MigracionesYSeedIdempotente

## P02 - APROBADO

**Precondicion y entrada:** Ana VIP compra KIT_TELETRABAJO ×1 ESTÁNDAR, base recién preparada.

**Esperado:** Total 59.00; stock P01=7, P02=4, P03=19; estado PAGADO y un pago.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Total 59.00, PAGADO y un pago; stock P01=7, P02=4 y P03=19 comprobado.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P02_P04_CasosOficiales(cliente: "Ana", entrega: ESTANDAR, sku: "KIT_TELETRABAJO", cantidad: 1, otro: "", otraCantidad: 0, total: 59, stockSku: "P01", stock: 7)

## P03 - APROBADO

**Precondicion y entrada:** Luis NORMAL compra 2 P04 + 2 P05 EXPRÉS, base recién preparada.

**Esperado:** Total 102.00; P04=2, P05=28; un pago.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Total 102.00 y un pago; stock P04=2 y P05=28 comprobado.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P02_P04_CasosOficiales(cliente: "Luis", entrega: EXPRES, sku: "P04", cantidad: 2, otro: "P05", otraCantidad: 2, total: 102, stockSku: "P04", stock: 2)

## P04 - APROBADO

**Precondicion y entrada:** Carmen MAYORISTA compra P03 ×10 ESTÁNDAR, base recién preparada.

**Esperado:** Total 90.00; P03=10; descuento 15.00.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Total 90.00, descuento 15.00 y stock P03=10 comprobado.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P02_P04_CasosOficiales(cliente: "Carmen", entrega: ESTANDAR, sku: "P03", cantidad: 10, otro: "", otraCantidad: 0, total: 90, stockSku: "P03", stock: 10)

## P05 - APROBADO

**Precondicion y entrada:** KIT_TELETRABAJO ×6 con stock inicial, intentar confirmar.

**Esperado:** Rechazo por P02 insuficiente; sin pago/stock/evento de éxito.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Rechazo por P02; sin pago, movimientos ni evento de exito y sin descontar P01.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P05_KitPorSeisSinEfectos

## P06 - APROBADO

**Precondicion y entrada:** Simular error de cobro en pedido válido.

**Esperado:** Se conserva inventario y estado previo; sin pago aprobado ni Outbox de éxito.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Cobro rechazado, estado VALIDADO e inventario conservado; sin pago aprobado ni Outbox de exito.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P06_RechazoNoDescuentaStock

## P07 - APROBADO

**Precondicion y entrada:** Repetir confirmación con misma clave de idempotencia.

**Esperado:** Un pago y un movimiento por SKU; devuelve el resultado original sin cobro doble.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Mismo identificador de pago en reintentos; un pago y tres movimientos, uno por SKU.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P07_ConfirmacionIdempotente

## P08 - APROBADO

**Precondicion y entrada:** Pedido PAGADO; intentar cancelar y modificar precio.

**Esperado:** Ambas operaciones denegadas; datos e historial intactos.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Cancelar y editar el pedido pagado provocan rechazo; total e historial conservados.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P08_PagadoNoAdmiteCancelarNiEditar

## P09 - APROBADO

**Precondicion y entrada:** Despachar y entregar con BODEGA; intentar despachar sin pago.

**Esperado:** Éxito solo en secuencia permitida; rechazo sin cambios en estado inválido.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Despacho sin pago rechazado; BODEGA completa despacho y entrega del pedido pagado.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P09_DespachoEntregaYRechazoSinPago

## P10 - APROBADO

**Precondicion y entrada:** Cerrar y reabrir; consultar pedido, stock, pago y auditoría.

**Esperado:** Información persiste en SQLite y coincide con operaciones anteriores.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Otro proveedor DI abre la misma SQLite y recupera pedido PAGADO, pago, stock P01=7 y auditoria.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P10_PersistenciaConOtroProveedorDi

## P11 - APROBADO

**Precondicion y entrada:** Factory Method: alternar EMAIL/SMS tras evento.

**Esperado:** Cada creador retorna notificador concreto y registra el aviso correcto.

**Obtenido:** 2 prueba(s) automatizada(s) aprobada(s). EMAIL y SMS registran el canal elegido; una sola notificacion por evento.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P11_P30_ObserverYFactoryPublicanUnaVez(canal: "SMS"); LogiMarket.Tests.Acceptance.FlujosTests.P11_P30_ObserverYFactoryPublicanUnaVez(canal: "EMAIL")

## P12 - APROBADO

**Precondicion y entrada:** Abstract Factory: ESTÁNDAR/EXPRÉS.

**Esperado:** Tarifas 5/12 y plazos 3/1 días; guías coherentes con la familia.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). ESTANDAR: tarifa 5, plazo 3 y prefijo STD. EXPRES: tarifa 12, plazo 1 y prefijo EXP.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P12_FamiliasCompatibles

## P13 - APROBADO

**Precondicion y entrada:** Builder: pedido vacío y pedido completo.

**Esperado:** Incompleto rechazado; completo crea agregado válido.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Constructor sin cliente o sin lineas rechazado; agregado completo construido.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P13_BuilderRechazaIncompleto

## P14 - APROBADO

**Precondicion y entrada:** Prototype: clonar KIT_TELETRABAJO; editar cantidad.

**Esperado:** Copia profunda; plantilla/copia previa permanecen intactas.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Copia profunda comprobada con cambio de cantidad y arbol historico serializado.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P14_PrototypeProfundo

## P15 - APROBADO

**Precondicion y entrada:** Singleton: consultar métricas desde dos vistas.

**Esperado:** Misma instancia compartida, contadores observables y thread-safe.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Dos ambitos reciben la misma instancia; 1000 registros concurrentes del mismo evento cuentan un pago.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P15_SingletonIdentidadYConcurrencia

## P16 - APROBADO

**Precondicion y entrada:** Adapter: proveedor legado y alternativo.

**Esperado:** Cobros desde ambos con mismo contrato; legado intacto.

**Obtenido:** 2 prueba(s) automatizada(s) aprobada(s). Ambos proveedores aceptan 59.00, conservan referencia en el reintento y rechazan entradas invalidas.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P16_P38_LspProveedores(alternativo: True); LogiMarket.Tests.Patterns.PatronesTests.P16_P38_LspProveedores(alternativo: False)

## P17 - APROBADO

**Precondicion y entrada:** Bridge: dos comprobantes × dos medios.

**Esperado:** Cuatro combinaciones; VistaPrevia visible y TXT real.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Cuatro combinaciones ejecutadas; vistas previas reales y TXT en docs/evidencias/comprobantes.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P17_BridgeCuatroCombinacionesContenidoYArchivo

## P18 - APROBADO

**Precondicion y entrada:** Composite: kits y composición anidada; intentar ciclo.

**Esperado:** Precios 50/60; expansión correcta; ciclo rechazado.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Precios 50/60, expansion P01/P02/P03 y cantidades propagadas; ciclo indirecto rechazado.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/ReglasInicialesTests.cs

**Metodos:** LogiMarket.Tests.ReglasInicialesTests.KitAnidadoExpandeCantidadesYRechazaCicloIndirecto

## P19 - APROBADO

**Precondicion y entrada:** Decorator: cobro autorizado, exitoso/fallido.

**Esperado:** LOG de inicio y fin/error rodea la ejecución, no altera resultado.

**Obtenido:** 2 prueba(s) automatizada(s) aprobada(s). Secuencia observada INICIO, PASARELA y FIN/ERROR, conservando el resultado del proveedor.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P19_DecoradorRodeaEjecucion(acepta: True); LogiMarket.Tests.Patterns.PatronesTests.P19_DecoradorRodeaEjecucion(acepta: False)

## P20 - APROBADO

**Precondicion y entrada:** Facade: confirmar con fallo en un colaborador.

**Esperado:** Operación consistente; ninguna persistencia parcial indebida.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Fallo del colaborador deja estado VALIDADO, stock P01=8, sin pago ni evento de exito.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/IntegridadTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.IntegridadTests.P20_FalloColaboradorNoDejaPersistenciaParcial

## P21 - APROBADO

**Precondicion y entrada:** Flyweight: 1.000 filas con metadatos repetidos.

**Esperado:** ReferenceEquals true; identidad y memoria documentadas.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). UI: 1000 filas, 1 instancia, ReferenceEquals=True; 104176 frente a 56360 bytes asignados. Medicion de asignaciones del hilo.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Architecture/ArquitecturaTests.cs

**Metodos:** LogiMarket.Tests.Architecture.ArquitecturaTests.P21_MilFilasCompartenIdentidadConComparacionEquivalente

## P22 - APROBADO

**Precondicion y entrada:** Proxy: rol CONSULTA/BODEGA intenta pagar.

**Esperado:** Rechazo antes de Decorator/Adapter; sin cobro ni LOG de cobro.

**Obtenido:** 2 prueba(s) automatizada(s) aprobada(s). CONSULTA y BODEGA reciben UnauthorizedAccessException antes del proveedor; ningun LOG de COBRO.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P22_ProxyDetieneAntesDeCobro(usuario: "consulta"); LogiMarket.Tests.Acceptance.FlujosTests.P22_ProxyDetieneAntesDeCobro(usuario: "bodega")

## P23 - APROBADO

**Precondicion y entrada:** Chain: pedido sin cliente y sin stock; corregir un error.

**Esperado:** Se informa primera validación y luego la siguiente; sin ejecución de cobro.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Primero se informa CLIENTE y, al corregirlo, STOCK; no se invoca cobro.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P23_CadenaPrimeroClienteLuegoStock

## P24 - APROBADO

**Precondicion y entrada:** Command: confirmar desde UI e invocador de prueba.

**Esperado:** Mismo comando y resultado funcional; historial de invocación.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). El boton de cobro y el test invocan ConfirmarPedidoCommand mediante InvocadorComandos.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P24_ComandoInvocadoRegistraResultado

## P25 - APROBADO

**Precondicion y entrada:** Interpreter: ESTADO = PAGADO AND TOTAL >= 50.

**Esperado:** Devuelve únicamente pedidos que cumplen el filtro.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). ESTADO = PAGADO AND TOTAL >= 50 acepta el pedido pagado de 65.00 y descarta el filtro de BORRADOR.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P25_InterpreterSeleccionaPagado

## P26 - APROBADO

**Precondicion y entrada:** Interpreter: sintaxis errónea y paréntesis.

**Esperado:** Error legible sin SQL concatenado; precedencia correcta.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). AND tiene prioridad sobre OR; parentesis cambian el resultado y sintaxis invalida genera FormatException.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P26_InterpreterPrecedenciaYErrores

## P27 - APROBADO

**Precondicion y entrada:** Iterator: recorrer KIT_TELETRABAJO.

**Esperado:** P01, P02, P03 una vez por unidad requerida, sin bucle.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Iterador propio produce P01, P02 y P03, termina y permanece terminado.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P27_P34_IteratorYVisitorKits

## P28 - APROBADO

**Precondicion y entrada:** Mediator: confirmar y revisar tres vistas.

**Esperado:** Catálogo, pedidos y métricas se refrescan sin acoplamiento directo.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Verificacion automatica del coordinador y capturas 07, 08 y 09: pedido pagado, stock actualizado y metrica de pago=1.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/MediatorTests.cs

**Metodos:** LogiMarket.Tests.Patterns.MediatorTests.P28_MediatorRefrescaStockPedidosYMetricas

## P29 - APROBADO

**Precondicion y entrada:** Memento: tres ediciones, dos deshacer y un rehacer.

**Esperado:** Se restauran borradores completos; no se deshace un pago.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Tres estados: deshacer devuelve cantidades 2 y 1; rehacer devuelve 2. Historial bloqueado rechaza deshacer.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P29_MementoTresCambiosDosDeshacerUnRehacer

## P30 - APROBADO

**Precondicion y entrada:** Observer: PedidoPagado con tres suscriptores.

**Esperado:** Notificación, auditoría y métrica actualizadas una vez.

**Obtenido:** 2 prueba(s) automatizada(s) aprobada(s). Un aviso, una auditoria de PedidoPagado y una metrica; segundo procesamiento no publica de nuevo.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P11_P30_ObserverYFactoryPublicanUnaVez(canal: "SMS"); LogiMarket.Tests.Acceptance.FlujosTests.P11_P30_ObserverYFactoryPublicanUnaVez(canal: "EMAIL")

## P31 - APROBADO

**Precondicion y entrada:** State: ciclo válido y PAGADO→CANCELADO inválido.

**Esperado:** Transiciones con reglas por estado, sin escritura directa.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Ciclo permitido completo; despacho prematuro, cancelacion y repeticion rechazados.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P31_StateRechazaRetrocesoYRepeticion

## P32 - APROBADO

**Precondicion y entrada:** Strategy: VIP, MAYORISTA, NORMAL y nueva promoción.

**Esperado:** Descuentos 6/15/0; extensión sin editar la calculadora central.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Politica nueva aplica 20% sin editar la calculadora; pruebas numericas adicionales verifican VIP, MAYORISTA y NORMAL.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P32_ExtenderStrategySinCambiarCalculadora

## P33 - APROBADO

**Precondicion y entrada:** Template Method: defecto día 20; arrepentimiento día 10.

**Esperado:** Defecto aceptado; arrepentimiento rechazado sin reembolso/stock.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Arrepentimiento dia 10 rechazado; defecto dia 20 tambien verificado en P35.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P33_ArrepentimientoDiaDiezNoAlteraStock

## P34 - APROBADO

**Precondicion y entrada:** Visitor: KIT_TELETRABAJO.

**Esperado:** Detalle P01/P02/P03; peso total 1.00 kg.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Visitor obtiene tres SKU y peso total 1.00 kg del kit teletrabajo.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Patterns.PatronesTests.P27_P34_IteratorYVisitorKits

## P35 - APROBADO

**Precondicion y entrada:** Devolver pedido ENTREGADO por defecto y repetir solicitud.

**Esperado:** Para Ana: USD 54.00 de reembolso sin flete; un solo reintegro y devolución única.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Ana recibe 54.00, sin envio; stock P01 vuelve a 8 y la solicitud repetida se rechaza; una devolucion.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P35_DevolucionAnaUnicaPor54

## P36 - APROBADO

**Precondicion y entrada:** Dos cobros concurrentes sobre un SKU cuyo stock se configuró previamente en una unidad.

**Esperado:** Máximo un cobro aprobado; stock no negativo; sin sobreventa.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Con P01=1 y dos confirmaciones concurrentes: una aceptada, un pago y stock final cero.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P36_DosConfirmacionesConcurrentesSinSobreventa

## P37 - APROBADO

**Precondicion y entrada:** Outbox: falla notificador y reiniciar procesador.

**Esperado:** Evento pendiente se recupera, se marca procesado una vez y sin duplicar cobro.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Fallo deja evento pendiente con un intento; procesador nuevo lo recupera; un aviso y un pago.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Acceptance/FlujosTests.cs

**Metodos:** LogiMarket.Tests.Acceptance.FlujosTests.P37_RecuperacionOutboxConNuevoProcesador

## P38 - APROBADO

**Precondicion y entrada:** Pruebas de LSP/ISP/DIP y build desde código limpio.

**Esperado:** Reemplazo de proveedor, contratos segregados, referencias correctas y 25 pruebas automatizadas aprobadas.

**Obtenido:** 3 prueba(s) automatizada(s) aprobada(s). Referencias inspeccionadas, contratos de proveedores sustituidos y compilacion de los cinco proyectos. El total automatico figura en el TRX.

**Fecha:** 2026-11-21T14:31:49.7561172-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivos:** tests/LogiMarket.Tests/Architecture/ArquitecturaTests.cs; tests/LogiMarket.Tests/Patterns/PatronesTests.cs

**Metodos:** LogiMarket.Tests.Architecture.ArquitecturaTests.P38_DependenciasInternasSinFrameworksDePersistenciaNiUi; LogiMarket.Tests.Patterns.PatronesTests.P16_P38_LspProveedores(alternativo: True); LogiMarket.Tests.Patterns.PatronesTests.P16_P38_LspProveedores(alternativo: False)
