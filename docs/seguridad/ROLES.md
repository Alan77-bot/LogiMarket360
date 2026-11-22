# Acceso visual por rol

| Apartado | ADMIN | VENTAS | BODEGA | CONSULTA |
|---|---|---|---|---|
| Panel general | Consultar | Consultar | Consultar | Consultar |
| Catalogo y kits | Administrar | Consultar | Consultar | Consultar |
| Clientes | Crear y editar | Crear y editar | No visible | Consultar |
| Carrito | Operar | Operar | No visible | No visible |
| Pedidos | Editar, validar y cancelar | Editar, validar y cancelar | No visible | Consultar detalle e historial |
| Cobro | Cobrar | Cobrar | No visible | No visible |
| Despacho y entrega | Despachar y entregar | No visible | Despachar y entregar | Consultar seguimiento |
| Devoluciones | Procesar | Procesar | No visible | Consultar pedidos entregados |
| Inventario | Registrar movimientos | No visible | Registrar movimientos | Consultar |
| Reportes | Consultar y exportar | Consultar y exportar | No visible | Consultar y exportar |
| Auditoria y demo | Administrar | No visible | No visible | No visible |

Todos pueden cerrar sesion. Exportar un comprobante crea una salida local y no cambia el pedido, stock ni pago. El catalogo expone existencias para que VENTAS consulte disponibilidad sin registrar movimientos. CONSULTA no dispone de acciones de escritura de negocio.

## Ubicacion y seguridad

`src/LogiMarket.WinForms/Shared/PermisosVista.cs` decide que se presenta. `FormularioPrincipal` utiliza fabricas para instanciar solamente las vistas permitidas; las demas no se registran en Mediator. Catalogo, clientes, pedidos e inventario omiten los botones de escritura segun los permisos. No se crea un carrito para BODEGA ni CONSULTA. La politica visual no concede permisos de negocio ni sustituye Autorizador.

Los servicios de Application conservan sus comprobaciones anteriores: catalogo solo ADMIN; clientes, borradores, cobro y devoluciones ADMIN/VENTAS; despacho e inventario ADMIN/BODEGA. El Proxy sigue rechazando pagos no autorizados antes del Decorator/proveedor. Los servicios de consulta comunes permiten lectura a los cuatro roles; la matriz anterior define la navegacion de cada perfil.

## Evidencias

`scripts/verificar_roles.ps1` compila un ejecutor temporal que usa formularios reales, inicializa una base nueva, autentica las cuatro cuentas, compara el menu con una matriz esperada independiente y verifica las acciones de las pantallas compartidas y sus tablas de solo lectura. El cierre de sesion y la reconstruccion del formulario se ejercitan entre perfiles. No se envian pagos ni notificaciones externas.

Resultado en `docs/evidencias/roles-ui.txt`; capturas por perfil en `docs/evidencias/capturas/roles-*.png`. La comprobacion usa DrawToBitmap y no equivale a una certificacion manual de DPI/accesibilidad.

`tests/LogiMarket.Tests/Acceptance/PermisosRolesTests.cs` invoca servicios directamente, comprueba rechazos sin cambios en pedido/stock/clientes/pagos/devoluciones y ejecuta venta con VENTAS, despacho/entrega con BODEGA, lectura con CONSULTA y devolucion con VENTAS. Resultado completo: 62 pruebas aprobadas, 0 fallidas y 0 omitidas, registrado en `docs/evidencias/roles-pruebas.trx`.
