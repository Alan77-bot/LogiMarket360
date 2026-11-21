# Matriz oficial P01-P38

Fecha de ejecucion: 2026-11-21T13:42:19.7250364-05:00. Pruebas automaticas: 56/56.

Los escenarios y esperados se extrajeron de la guia. Los metodos y estados proceden del TRX; las notas visuales corresponden a capturas reales.

## P01 - APROBADO

**Precondicion y entrada:** Creación de DB desde archivo inexistente y seed.

**Esperado:** Migraciones aplicadas; cinco productos, dos kits y roles; sin duplicados al relanzar seed.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P01_MigracionesYSeedIdempotente

## P02 - APROBADO

**Precondicion y entrada:** Ana VIP compra KIT_TELETRABAJO ×1 ESTÁNDAR, base recién preparada.

**Esperado:** Total 59.00; stock P01=7, P02=4, P03=19; estado PAGADO y un pago.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P02_P04_CasosOficiales(cliente: "Ana", entrega: ESTANDAR, sku: "KIT_TELETRABAJO", cantidad: 1, otro: "", otraCantidad: 0, total: 59, stockSku: "P01", stock: 7)

## P03 - APROBADO

**Precondicion y entrada:** Luis NORMAL compra 2 P04 + 2 P05 EXPRÉS, base recién preparada.

**Esperado:** Total 102.00; P04=2, P05=28; un pago.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P02_P04_CasosOficiales(cliente: "Luis", entrega: EXPRES, sku: "P04", cantidad: 2, otro: "P05", otraCantidad: 2, total: 102, stockSku: "P04", stock: 2)

## P04 - APROBADO

**Precondicion y entrada:** Carmen MAYORISTA compra P03 ×10 ESTÁNDAR, base recién preparada.

**Esperado:** Total 90.00; P03=10; descuento 15.00.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P02_P04_CasosOficiales(cliente: "Carmen", entrega: ESTANDAR, sku: "P03", cantidad: 10, otro: "", otraCantidad: 0, total: 90, stockSku: "P03", stock: 10)

## P05 - APROBADO

**Precondicion y entrada:** KIT_TELETRABAJO ×6 con stock inicial, intentar confirmar.

**Esperado:** Rechazo por P02 insuficiente; sin pago/stock/evento de éxito.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P05_KitPorSeisSinEfectos

## P06 - APROBADO

**Precondicion y entrada:** Simular error de cobro en pedido válido.

**Esperado:** Se conserva inventario y estado previo; sin pago aprobado ni Outbox de éxito.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P06_RechazoNoDescuentaStock

## P07 - APROBADO

**Precondicion y entrada:** Repetir confirmación con misma clave de idempotencia.

**Esperado:** Un pago y un movimiento por SKU; devuelve el resultado original sin cobro doble.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P07_ConfirmacionIdempotente

## P08 - APROBADO

**Precondicion y entrada:** Pedido PAGADO; intentar cancelar y modificar precio.

**Esperado:** Ambas operaciones denegadas; datos e historial intactos.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P08_PagadoNoAdmiteCancelarNiEditar

## P09 - APROBADO

**Precondicion y entrada:** Despachar y entregar con BODEGA; intentar despachar sin pago.

**Esperado:** Éxito solo en secuencia permitida; rechazo sin cambios en estado inválido.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P09_DespachoEntregaYRechazoSinPago

## P10 - APROBADO

**Precondicion y entrada:** Cerrar y reabrir; consultar pedido, stock, pago y auditoría.

**Esperado:** Información persiste en SQLite y coincide con operaciones anteriores.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P10_PersistenciaConOtroProveedorDi

## P11 - APROBADO

**Precondicion y entrada:** Factory Method: alternar EMAIL/SMS tras evento.

**Esperado:** Cada creador retorna notificador concreto y registra el aviso correcto.

**Obtenido:** 2 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P11_P30_ObserverYFactoryPublicanUnaVez(canal: "SMS"); LogiMarket.Tests.Acceptance.FlujosTests.P11_P30_ObserverYFactoryPublicanUnaVez(canal: "EMAIL")

## P12 - APROBADO

**Precondicion y entrada:** Abstract Factory: ESTÁNDAR/EXPRÉS.

**Esperado:** Tarifas 5/12 y plazos 3/1 días; guías coherentes con la familia.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P12_FamiliasCompatibles

## P13 - APROBADO

**Precondicion y entrada:** Builder: pedido vacío y pedido completo.

**Esperado:** Incompleto rechazado; completo crea agregado válido.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P13_BuilderRechazaIncompleto

## P14 - APROBADO

**Precondicion y entrada:** Prototype: clonar KIT_TELETRABAJO; editar cantidad.

**Esperado:** Copia profunda; plantilla/copia previa permanecen intactas.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Copia profunda comprobada con cambio de cantidad y arbol historico serializado.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P14_PrototypeProfundo

## P15 - APROBADO

**Precondicion y entrada:** Singleton: consultar métricas desde dos vistas.

**Esperado:** Misma instancia compartida, contadores observables y thread-safe.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P15_SingletonIdentidadYConcurrencia

## P16 - APROBADO

**Precondicion y entrada:** Adapter: proveedor legado y alternativo.

**Esperado:** Cobros desde ambos con mismo contrato; legado intacto.

**Obtenido:** 2 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P16_P38_LspProveedores(alternativo: False); LogiMarket.Tests.Patterns.PatronesTests.P16_P38_LspProveedores(alternativo: True)

## P17 - APROBADO

**Precondicion y entrada:** Bridge: dos comprobantes × dos medios.

**Esperado:** Cuatro combinaciones; VistaPrevia visible y TXT real.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Cuatro combinaciones ejecutadas; vistas previas reales y TXT en docs/evidencias/comprobantes.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P17_BridgeCuatroCombinacionesContenidoYArchivo

## P18 - APROBADO

**Precondicion y entrada:** Composite: kits y composición anidada; intentar ciclo.

**Esperado:** Precios 50/60; expansión correcta; ciclo rechazado.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.ReglasInicialesTests.KitAnidadoExpandeCantidadesYRechazaCicloIndirecto

## P19 - APROBADO

**Precondicion y entrada:** Decorator: cobro autorizado, exitoso/fallido.

**Esperado:** LOG de inicio y fin/error rodea la ejecución, no altera resultado.

**Obtenido:** 2 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P19_DecoradorRodeaEjecucion(acepta: True); LogiMarket.Tests.Patterns.PatronesTests.P19_DecoradorRodeaEjecucion(acepta: False)

## P20 - APROBADO

**Precondicion y entrada:** Facade: confirmar con fallo en un colaborador.

**Esperado:** Operación consistente; ninguna persistencia parcial indebida.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.IntegridadTests.P20_FalloColaboradorNoDejaPersistenciaParcial

## P21 - APROBADO

**Precondicion y entrada:** Flyweight: 1.000 filas con metadatos repetidos.

**Esperado:** ReferenceEquals true; identidad y memoria documentadas.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). UI: 1000 filas, 1 instancia, ReferenceEquals=True; 104176 frente a 56360 bytes asignados. Medicion de asignaciones del hilo.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Architecture.ArquitecturaTests.P21_MilFilasCompartenIdentidadConComparacionEquivalente

## P22 - APROBADO

**Precondicion y entrada:** Proxy: rol CONSULTA/BODEGA intenta pagar.

**Esperado:** Rechazo antes de Decorator/Adapter; sin cobro ni LOG de cobro.

**Obtenido:** 2 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P22_ProxyDetieneAntesDeCobro(usuario: "consulta"); LogiMarket.Tests.Acceptance.FlujosTests.P22_ProxyDetieneAntesDeCobro(usuario: "bodega")

## P23 - APROBADO

**Precondicion y entrada:** Chain: pedido sin cliente y sin stock; corregir un error.

**Esperado:** Se informa primera validación y luego la siguiente; sin ejecución de cobro.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P23_CadenaPrimeroClienteLuegoStock

## P24 - APROBADO

**Precondicion y entrada:** Command: confirmar desde UI e invocador de prueba.

**Esperado:** Mismo comando y resultado funcional; historial de invocación.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). El boton de cobro y el test invocan ConfirmarPedidoCommand mediante InvocadorComandos.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P24_ComandoInvocadoRegistraResultado

## P25 - APROBADO

**Precondicion y entrada:** Interpreter: ESTADO = PAGADO AND TOTAL >= 50.

**Esperado:** Devuelve únicamente pedidos que cumplen el filtro.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P25_InterpreterSeleccionaPagado

## P26 - APROBADO

**Precondicion y entrada:** Interpreter: sintaxis errónea y paréntesis.

**Esperado:** Error legible sin SQL concatenado; precedencia correcta.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P26_InterpreterPrecedenciaYErrores

## P27 - APROBADO

**Precondicion y entrada:** Iterator: recorrer KIT_TELETRABAJO.

**Esperado:** P01, P02, P03 una vez por unidad requerida, sin bucle.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P27_P34_IteratorYVisitorKits

## P28 - APROBADO

**Precondicion y entrada:** Mediator: confirmar y revisar tres vistas.

**Esperado:** Catálogo, pedidos y métricas se refrescan sin acoplamiento directo.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Verificacion automatica del coordinador y capturas 07, 08 y 09: pedido pagado, stock actualizado y metrica de pago=1.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.MediatorTests.P28_MediatorRefrescaStockPedidosYMetricas

## P29 - APROBADO

**Precondicion y entrada:** Memento: tres ediciones, dos deshacer y un rehacer.

**Esperado:** Se restauran borradores completos; no se deshace un pago.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P29_MementoTresCambiosDosDeshacerUnRehacer

## P30 - APROBADO

**Precondicion y entrada:** Observer: PedidoPagado con tres suscriptores.

**Esperado:** Notificación, auditoría y métrica actualizadas una vez.

**Obtenido:** 2 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P11_P30_ObserverYFactoryPublicanUnaVez(canal: "SMS"); LogiMarket.Tests.Acceptance.FlujosTests.P11_P30_ObserverYFactoryPublicanUnaVez(canal: "EMAIL")

## P31 - APROBADO

**Precondicion y entrada:** State: ciclo válido y PAGADO→CANCELADO inválido.

**Esperado:** Transiciones con reglas por estado, sin escritura directa.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P31_StateRechazaRetrocesoYRepeticion

## P32 - APROBADO

**Precondicion y entrada:** Strategy: VIP, MAYORISTA, NORMAL y nueva promoción.

**Esperado:** Descuentos 6/15/0; extensión sin editar la calculadora central.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P32_ExtenderStrategySinCambiarCalculadora

## P33 - APROBADO

**Precondicion y entrada:** Template Method: defecto día 20; arrepentimiento día 10.

**Esperado:** Defecto aceptado; arrepentimiento rechazado sin reembolso/stock.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Arrepentimiento dia 10 rechazado; defecto dia 20 tambien verificado en P35.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P33_ArrepentimientoDiaDiezNoAlteraStock

## P34 - APROBADO

**Precondicion y entrada:** Visitor: KIT_TELETRABAJO.

**Esperado:** Detalle P01/P02/P03; peso total 1.00 kg.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P27_P34_IteratorYVisitorKits

## P35 - APROBADO

**Precondicion y entrada:** Devolver pedido ENTREGADO por defecto y repetir solicitud.

**Esperado:** Para Ana: USD 54.00 de reembolso sin flete; un solo reintegro y devolución única.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P35_DevolucionAnaUnicaPor54

## P36 - APROBADO

**Precondicion y entrada:** Dos cobros concurrentes sobre un SKU cuyo stock se configuró previamente en una unidad.

**Esperado:** Máximo un cobro aprobado; stock no negativo; sin sobreventa.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P36_DosConfirmacionesConcurrentesSinSobreventa

## P37 - APROBADO

**Precondicion y entrada:** Outbox: falla notificador y reiniciar procesador.

**Esperado:** Evento pendiente se recupera, se marca procesado una vez y sin duplicar cobro.

**Obtenido:** 1 prueba(s) automatizada(s) aprobada(s). Las aserciones verifican el esperado y los efectos colaterales del escenario.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Acceptance.FlujosTests.P37_RecuperacionOutboxConNuevoProcesador

## P38 - APROBADO

**Precondicion y entrada:** Pruebas de LSP/ISP/DIP y build desde código limpio.

**Esperado:** Reemplazo de proveedor, contratos segregados, referencias correctas y 25 pruebas automatizadas aprobadas.

**Obtenido:** 3 prueba(s) automatizada(s) aprobada(s). Referencias inspeccionadas, contratos de proveedores sustituidos y compilacion de los cinco proyectos. El total automatico figura en el TRX.

**Fecha:** 2026-11-21T13:42:19.7250364-05:00

**Evidencia:** docs/evidencias/aceptacion.trx

**Archivo/metodo:** LogiMarket.Tests.Patterns.PatronesTests.P16_P38_LspProveedores(alternativo: False); LogiMarket.Tests.Architecture.ArquitecturaTests.P38_DependenciasInternasSinFrameworksDePersistenciaNiUi; LogiMarket.Tests.Patterns.PatronesTests.P16_P38_LspProveedores(alternativo: True)
