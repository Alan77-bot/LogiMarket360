# Mapa de los 23 patrones GoF

Las rutas se expresan desde la raiz del repositorio. Buscar el nombre de clase con Ctrl+T en Visual Studio. Una implementacion presente no demuestra por si sola la aprobacion del caso: consultar la matriz y los resultados reales.

| Patron | Problema y participantes | Archivo y recorrido real | Prueba | Beneficio y costo |
|---|---|---|---|---|
| Factory Method | Crear EMAIL/SMS sin acoplar los eventos al concreto; CreadorNotificador, CreadorEmail, CreadorSms | Infrastructure/Notifications/Notificaciones.cs: NotificacionObserver -> Notificar -> Crear -> Enviar | P11_P30 | Se cambia canal sin tocar vistas; mas clases de creacion |
| Abstract Factory | Mantener tarifa, plazo y guia de una misma familia | Domain/Entregas/FamiliasEntrega.cs: ServicioPedidos y ServicioDespachos seleccionan IFabricaEntrega | P12 | Familias coherentes; tres contratos de productos |
| Builder | Evitar pedidos sin cliente o lineas | Domain/Pedidos/Pedido.cs: ServicioPedidos.Preparar -> PedidoBuilder.Construir | P13 | Construccion legible y valida; pasos adicionales |
| Prototype | Reutilizar borradores independientes | Domain/Pedidos/Pedido.cs: ServicioPedidos.Clonar -> ClonarBorrador | P14 | Copia profunda con identidad nueva; hay que mantener la copia al agregar campos |
| Singleton | Contadores compartidos de sesion | Application/Eventos/Eventos.cs y Infrastructure/DependencyInjection/RegistroServicios.cs: AddSingleton<MetricasSesion> | P15 | Identidad unica y concurrencia segura; no sustituye datos historicos |
| Adapter | Legado recibe centavos, aplicacion usa decimal | Infrastructure/Payments/Pasarelas.cs: PasarelaLegadaAdapter -> CobrarCentavos | P16 | Conserva legado y contrato estable; conversion de unidades |
| Bridge | Independencia entre comprobante y salida | Application/Comprobantes/Comprobantes.cs: Venta/Devolucion -> ISalidaDocumento; VistaReportes selecciona salida | P17 | Cuatro combinaciones sin cuatro clases combinadas; hay dos jerarquias |
| Composite | Productos y kits anidados uniformes | Domain/Catalogo/ElementosCatalogo.cs y Application/Catalogo/ServicioCatalogo.cs: CatalogoCompuesto -> PedidoBuilder | P18 | Recursion y calculo uniforme; exige control de ciclos |
| Decorator | Agregar trazas al pago sin alterar proveedor | Infrastructure/Payments/Pasarelas.cs: AuditoriaPagoDecorator.CobrarAsync | P19 | Auditoria transversal; orden de envolturas relevante |
| Facade | Una entrada para confirmar sin logica en el boton | Application/Pedidos/ServicioPedidos.cs: ConfirmacionPedidoFacade.Confirmar | P20 | Coordina sin SQL ni tasas propias; debe evitar crecer como clase Dios |
| Flyweight | Compartir metadatos en 1000 filas | Application/Catalogo/Flyweight.cs y WinForms/Features/VistasReportes.cs: DemostrarFlyweight | P21 | Menos objetos repetidos; cache y separacion de datos externos |
| Proxy | Autorizar antes de logs/proveedor | Application/Seguridad/Seguridad.cs: PagoAutorizadoProxy -> Decorator -> Adapter | P22 | Proteccion fuera de UI; configuracion de cadena |
| Chain of Responsibility | Primera causa de rechazo clara | Application/Pedidos/ValidacionPedido.cs: ClienteActivo -> Lineas -> Stock -> Estado | P23 | Validadores pequenos y ordenados; orden debe documentarse |
| Command | Separar boton de receptor y registrar invocacion | Application/Commands/Comandos.cs: VistaPedidos -> InvocadorComandos -> receptor | P24 | Misma accion desde UI/tests; objetos adicionales |
| Interpreter | Consultas con gramatica acotada segura | Application/Reportes/InterpreterFiltro.cs: Analizar -> arbol -> Evaluar; usado en Pedidos y Reportes | P25-P26 | Sin SQL concatenado y precedencia explicita; parser que mantener |
| Iterator | Recorrer hojas con cantidades y orden estable | Domain/Catalogo/Recorridos.cs: IteradorFisico; expansion usada para instantaneas | P27 | Oculta colecciones; pila y protocolo MoveNext |
| Mediator | Refrescar vistas sin referencias entre ellas | Application/Eventos/MediadorVistas.cs y WinForms/Shared/Ui.cs: FormularioPrincipalMediator; registrado desde Shell y cada vista | P28 automatica y visual | Reduce acoplamiento entre formularios; punto de coordinacion compartido |
| Memento | Deshacer/rehacer cinco estados completos | Domain/Pedidos/HistorialBorrador.cs y WinForms/Features/VistaCarrito.cs | P29 | Recuperacion de ediciones; memoria de copias y limite de historial |
| Observer | Varias reacciones a eventos durables | Application/Eventos/Eventos.cs: PublicadorEventos -> Notificacion/Auditoria/Metricas; Infrastructure/Outbox | P30 | Suscriptores independientes; recepciones y reintentos requieren cuidado |
| State | Transiciones segun estado actual | Domain/Pedidos/Pedido.cs: Estados.Obtener -> EstadoBase.Exigir | P31 | Sin switch central de transiciones; un tipo por estado |
| Strategy | Descuentos excluyentes y extensibles | Domain/Promociones/CalculadoraPrecios.cs: politicas registradas DI, prioridad descendente | P32 | Nueva promocion sin cambiar calculadora; prioridades deben ser unicas y explicitas |
| Template Method | Secuencia fija y politica por motivo | Application/Logistica/ServiciosLogisticos.cs: ProcesarDevolucion.Ejecutar -> Inspeccionar | P33 | Flujo consistente; herencia limita variaciones |
| Visitor | Peso y desglose sin agregarlos a cada entidad | Domain/Catalogo/Recorridos.cs: PesoVisitor/DesgloseVisitor; ServicioCatalogo.Consultar | P34 | Reportes separados; agregar un tipo de elemento obliga a extender visitantes |

Para los archivos abreviados, anteponer `src/LogiMarket.` al nombre del proyecto. Ejemplo: `Domain/Pedidos/Pedido.cs` corresponde a `src/LogiMarket.Domain/Pedidos/Pedido.cs`.
