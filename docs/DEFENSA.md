# Guia practica para la defensa

## Como explicar el proyecto

Comenzar por una venta de Ana: Carrito crea un borrador, Pedidos valida, Cobro confirma USD 59.00, Inventario muestra el descuento por SKU, Despacho registra la guia y entrega, Devoluciones reembolsa USD 54.00. Despues mostrar el mismo recorrido en el codigo y en una prueba.

El formulario captura datos y llama a Application. Application autoriza y coordina. Domain decide reglas. Infrastructure persiste y simula colaboradores externos. No hay SQL en formularios ni EF en Domain/Application.

## Archivos para estudiar primero

1. `src/LogiMarket.Domain/Pedidos/Pedido.cs`: agregado, estados, Builder, Prototype e instantaneas.
2. `src/LogiMarket.Application/Pedidos/ServicioPedidos.cs`: preparacion, validacion y Facade transaccional.
3. `src/LogiMarket.Infrastructure/Persistence/RepositorioEf.cs`: transaccion real y bloqueo previo a stock.
4. `src/LogiMarket.Infrastructure/DependencyInjection/RegistroServicios.cs`: contratos, implementaciones y orden del pago.
5. `src/LogiMarket.WinForms/Features/VistaPedidos.cs`: botones que invocan comandos.
6. `tests/LogiMarket.Tests/Acceptance/FlujosTests.cs`: cifras oficiales y efectos colaterales.

## Cambios que pueden pedir

| Solicitud | Donde cambiar | Que demostrar |
|---|---|---|
| Agregar promocion de 20% | Nueva IPoliticaDescuento en Domain; registrarla en DI | Aplica, prioridad y prueba de exclusion; no editar CalculadoraPrecios |
| Cambiar tarifa ESTANDAR | EntregaEstandarFactory.CrearTarifa | Nueva tarifa y total calculado; durante evaluacion conservar reglas oficiales salvo instruccion explicita |
| Agregar una familia de entrega | Enum TipoEntrega y nueva IFabricaEntrega registrada | Tarifa, plazo y prefijo compatibles |
| Cambiar proveedor | Nueva IPasarelaPago y registro/selector | Misma prueba contractual, idempotencia y rechazo |
| Agregar motivo de devolucion | Enum y subclase de ProcesarDevolucion | Cambiar solo Inspeccionar y seleccion del caso de uso |
| Agregar validacion | Nueva subclase ValidadorPedido y encadenarla | Primera causa y ausencia de pago/stock |
| Agregar salida de comprobante | Nueva ISalidaDocumento | Venta y devolucion usan la nueva salida |
| Agregar suscriptor | Nueva ISuscriptorEvento y registro DI | Recepcion una vez, recuperacion ante fallo |
| Cambiar limite de deshacer | HistorialBorrador.Capturar | Prueba del limite y bloqueo tras confirmacion |
| Agregar filtro | Nodo IExpresionPedido y regla en InterpreterFiltro | Precedencia, errores y evaluacion sin SQL |

## Preguntas y respuestas que debes poder desarrollar

- Por que MAYORISTA gana a VIP: politicas ordenadas por prioridad, solo se elige la primera aplicable.
- Por que un kit no tiene stock independiente: sus hojas fisicas consumen existencias; la disponibilidad es el minimo de stock/cantidad requerida.
- Que es una instantanea: copia historica del precio y composicion. Editar el catalogo no cambia una venta ya registrada.
- Por que usar decimal: evita errores binarios en importes; el descuento se redondea AwayFromZero.
- Por que Proxy esta afuera del Decorator: un rol no autorizado nunca debe alcanzar trazas de cobro ni al proveedor.
- Que pasa si falla el pago: rollback de negocio; quedan trazas del intento y ningun pago aprobado ni evento de exito.
- Que pasa si falla el correo: el pago ya confirmado permanece; Outbox queda pendiente y se reintenta.
- Que impide dos pagos simultaneos: bloqueo de escritura SQLite antes de leer stock, versiones y restricciones unicas.
- Que se conserva al reiniciar: pedidos, inventario, pagos, historial, auditoria y Outbox. Las metricas son de sesion.
- Por que no se devuelve el envio: regla explicita; reembolso = subtotal - descuento.

## Ejercicio recomendado

Crear una rama de practica y agregar una estrategia nueva con una prueba. Explicar primero el contrato, despues el registro DI y finalmente el resultado. Repetir con una salida de documento. No memorizar solo nombres de patrones: seguir una llamada real con el depurador, colocando puntos de interrupcion en Facade, Proxy, Decorator, Adapter y RepositorioEf.

El codigo incluye comentarios sin tildes. La comprension y la defensa requieren que ejecutes y practiques estos cambios; este documento no sustituye ese trabajo.
