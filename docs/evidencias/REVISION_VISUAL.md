# Registro de ejecucion visual

Fecha: 21 de noviembre de 2026, zona America/Bogota. Equipo Windows del usuario. Aplicacion compilada y ejecutada mediante su exe de WinForms; cuentas y datos ficticios del seed.

1. Acceso con admin / Demo123!: aceptado.
2. Catalogo: cinco productos, dos kits; precios 50.00 y 60.00; peso del kit teletrabajo 1.00 kg.
3. Clientes: Ana VIP, Carmen NORMAL y Luis NORMAL, activos.
4. Carrito: Ana + KIT_TELETRABAJO x1 ESTANDAR: 60.00 - 6.00 + 5.00 = 59.00.
5. Guardado y validacion: pedido 1a5debe4-7428-485d-9b9f-acf5c2e6603f.
6. Cobro desde ConfirmarPedidoCommand: PAGADO por USD 59.00.
7. Refresco Mediator: catalogo P01=7, P02=4, P03=19; listado PAGADO; metrica pagos=1.
8. Despacho y entrega registrados: ENTREGADO.
9. Devolucion total por DEFECTO: reembolso USD 54.00 y stock reintegrado a 8/5/20.
10. Bridge: vistas previas de venta y devolucion observadas; ambos TXT generados y copiados a comprobantes/.
11. Flyweight: 1000 filas, 1 instancia, ReferenceEquals True; asignaciones 104176 sin compartir frente a 56360 compartiendo, bajo la misma estructura externa. No se mide memoria total del proceso.
12. Cierre y reapertura: un pedido conservado, ENTREGADO, total 59.00. Metricas reiniciadas al ser de sesion.
13. Cancelacion de pedido entregado: rechazada por State, sin cambio del estado.

Fallo encontrado durante la revision: DataGridViewBand.set_Thickness al configurar MinimumWidth con Fill en una vista oculta. Log en error-visual.txt. Correccion: AllCells sin redistribucion manual. Reapertura posterior comprobada sin ese aviso; capturas 18, 19 y 20.

Las capturas iniciales preceden a los ajustes cosmeticos de columnas y fecha local. Las capturas 18-20 corresponden a la version corregida. Las pruebas automaticas complementan la ejecucion visual; no se afirma haber ejercitado manualmente cada variante de todos los formularios.
