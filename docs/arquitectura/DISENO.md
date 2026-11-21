# Arquitectura y decisiones

## Dependencias

```mermaid
flowchart LR
  UI[WinForms] --> A[Application]
  UI --> I[Infrastructure solo composicion]
  I --> A
  I --> D[Domain]
  A --> D
  T[Tests] --> A
  T --> I
  T --> D
```

Domain no referencia EF, SQLite ni interfaz. Application utiliza puertos; Infrastructure implementa los puertos. La interfaz registra concretos en Program y utiliza servicios de aplicacion. Cada operacion visual crea un ambito DI nuevo: no se conserva un DbContext entre formularios.

## Estados

## Contexto del sistema

```mermaid
flowchart LR
  Admin[ADMIN] --> LM[LogiMarket 360 WinForms]
  Ventas[VENTAS] --> LM
  Bodega[BODEGA] --> LM
  Consulta[CONSULTA lectura] --> LM
  LM --> DB[(SQLite local)]
  LM --> Pago[Proveedores de pago simulados]
  LM --> Avisos[EMAIL y SMS simulados locales]
  LM --> TXT[Comprobantes internos TXT]
```

## Clases criticas

```mermaid
classDiagram
  PedidoBuilder --> Pedido : construye
  Pedido "1" *-- "1..*" PedidoDetalle
  PedidoDetalle "1" *-- "1..*" PedidoDetalleComponente
  Pedido --> EstadoBase : valida transicion
  Pedido --> CalculadoraPrecios : valora
  CalculadoraPrecios --> IPoliticaDescuento
  ConfirmacionPedidoFacade --> IRepositorio
  ConfirmacionPedidoFacade --> ITransacciones
  ConfirmacionPedidoFacade --> IPasarelaPago
  PagoAutorizadoProxy ..|> IPasarelaPago
  AuditoriaPagoDecorator ..|> IPasarelaPago
  PasarelaLegadaAdapter ..|> IPasarelaPago
  RepositorioEf ..|> IRepositorio
  RepositorioEf ..|> ITransacciones
```

## Ciclo de estados

```mermaid
stateDiagram-v2
  [*] --> BORRADOR
  BORRADOR --> VALIDADO
  BORRADOR --> CANCELADO
  VALIDADO --> CANCELADO
  VALIDADO --> PAGADO
  PAGADO --> DESPACHADO
  DESPACHADO --> ENTREGADO
```

## Confirmacion aceptada

```mermaid
sequenceDiagram
  actor Usuario
  Usuario->>Vista: Confirmar
  Vista->>Invocador: ConfirmarPedidoCommand
  Invocador->>Facade: Confirmar(id, clave)
  Facade->>Autorizador: Exigir VENTAS/ADMIN
  Facade->>Repositorio: BEGIN IMMEDIATE
  Facade->>Repositorio: Consultar clave, pedido y stock
  Facade->>Chain: Validar cliente, lineas, stock, estado
  Facade->>Proxy: CobrarAsync
  Proxy->>Decorator: CobrarAsync autorizado
  Decorator->>Adapter: CobrarAsync
  Adapter->>Legado: CobrarCentavos
  Facade->>Repositorio: Pago, movimientos, estado y Outbox
  Repositorio-->>Facade: COMMIT
  Vista->>Outbox: Procesar
  Outbox->>Observer: Publicar con recepciones unicas
  Vista->>Mediator: Actualizar vistas
```

## Operacion rechazada

```mermaid
sequenceDiagram
  actor Consulta
  Consulta->>Proxy: CobrarAsync
  Proxy->>Autorizador: Exigir VENTAS/ADMIN
  Autorizador->>Bitacora: Seguridad DENEGADO
  Autorizador-->>Proxy: UnauthorizedAccessException
  Note over Proxy: No invoca Decorator ni Adapter
```

## Modelo relacional

```mermaid
erDiagram
  Cliente ||--o{ Pedido : registra
  Pedido ||--|{ PedidoDetalle : contiene
  PedidoDetalle ||--|{ PedidoDetalleComponente : conserva
  Pedido ||--|{ CambioEstado : registra
  Pedido ||--o| Pago : cobra
  Pedido ||--o| Despacho : envia
  Pedido ||--o| Devolucion : devuelve
  Pedido ||--o{ EventoOutbox : produce
  EventoOutbox ||--o{ RecepcionEvento : publica
  EventoOutbox ||--o| Aviso : notifica
  Producto ||--o{ MovimientoInventario : mueve
  Kit ||--|{ KitComponente : contiene
  Producto o|--o{ KitComponente : fisico
  Kit o|--o{ KitComponente : anidado
```

Usuario y Auditoria complementan el modelo. Usuario conserva rol y hash. Auditoria conserva usuario, fecha, operacion, resultado y correlacion. Los componentes de una venta conservan SKU, precio, peso y cantidad historicos; ComposicionJson conserva el arbol anidado. Las relaciones de catalogo utilizan FK reales y las ventas conservan instantaneas independientes.

## Consistencia

SQLite usa BEGIN IMMEDIATE antes de leer stock. La base serializa escritores incluso entre procesos. Producto y Pedido ademas tienen tokens de version. Existen indices unicos para pedido pagado, clave, referencia, devolucion y movimientos por referencia/producto/motivo. Los cobros simulados tienen idempotencia en sesion y la persistencia es la fuente de verdad despues del commit. No se pretende simular una transaccion bancaria distribuida.

Las notificaciones son locales y se guardan con su recepcion en la misma transaccion del procesador. El procesador serializa publicaciones; registra recepciones por evento/suscriptor y permite recuperar eventos pendientes. MetricasSesion es memoria de sesion, no un reporte historico de ventas.

ADMIN administra catalogo y demostracion. VENTAS administra clientes, pedidos, cobros y devoluciones. BODEGA registra movimientos, despacho y entrega. CONSULTA solo lee. Los permisos de devolucion y ajustes son decisiones documentadas para completar los aspectos no detallados de la matriz de roles de la guia.

## SOLID

SRP: CalculadoraPrecios calcula y RepositorioEf persiste. OCP: politicas adicionales implementan IPoliticaDescuento. LSP: ambos proveedores cumplen los mismos tests contractuales. ISP: IPasarelaPago e ISalidaDocumento tienen una sola responsabilidad; las consultas operativas se separan de transacciones. DIP: Facade depende de IRepositorio, ITransacciones e IPasarelaPago.

Compromisos: las consultas utilizan entidades de dominio desconectadas y colecciones de lectura para mantener un alcance academico manejable; para grandes volumenes se recomendaria paginacion y proyecciones DTO. No se agregan microservicios, web ni pagos externos.
