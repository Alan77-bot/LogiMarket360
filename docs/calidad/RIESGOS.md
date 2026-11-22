# Riesgos reales y correcciones

No se simula un historial previo. Los riesgos siguientes se encontraron al construir esta version. Donde no hubo una prueba roja anterior se indica revision de diseno, no fallo ejecutado. Los commits se completan con hashes reales al verificar cada etapa.

| ID | Archivo y metodo | Riesgo / consecuencia | Medida aplicada | Evidencia |
|---|---|---|---|---|
| R01 | Program.Main | Application colisionaba con el namespace de negocio; no compilaba | Nombre completo System.Windows.Forms.Application.Run | Error CS0234 observado; build posterior correcto |
| R02 | LogiMarketDbContext.OnModelCreating | Guid de hijos interpretado como registro existente; validar fallaba | ValueGenerated.Never para identidades de dominio | ejecucion-fallida.trx: 16 fallos; correccion-identidades.trx: 3 aprobadas |
| R03 | Pedido.Detalles, Historial y PedidoDetalle.Componentes | Una List publica permitia saltar invariantes | Colecciones de solo lectura y metodos de agregado | Revision de diseno; P14 y P08 |
| R04 | ServicioPedidos.Guardar | Reemplazar eliminando creaba otra identidad e historial | AplicarCambios conserva identidad | EdicionConservaIdentidadYReemplazaLineas |
| R05 | ServicioCatalogo.GuardarProducto | Renombrar SKU historico impedia localizar stock en devolucion | Rechazar cambio de SKU referenciado | ProductoUsadoNoSeEliminaNiCambiaSku |
| R06 | RepositorioEf.Ejecutar | Dos escritores podian leer el mismo stock | BEGIN IMMEDIATE y versiones | P36 |
| R07 | ConfirmacionPedidoFacade.Confirmar | Reintento podia cobrar y descontar otra vez | Clave unica, consulta previa y pago unico por pedido | P07 |
| R08 | BitacoraEf.Registrar | Otro contexto escribiendo dentro del bloqueo podia quedar esperando | Agregar trazas al contexto actual; conservarlas al rollback | P06 y P19 |
| R09 | PedidoDetalle constructor y Copiar | Desglose plano perdia estructura anidada de kit | Arbol historico serializado e instantanea relacional | InstantaneaConservaArbolAnidadoYClon |
| R10 | ProcesadorOutbox.Procesar | Reintento podia repetir observadores | Recepcion por evento/suscriptor y procesamiento serializado | P30 y P37 |
| R11 | ServicioCatalogo.Eliminar | Borrar producto usado rompia devoluciones historicas | Comprobar referencias de venta y FK restrictivas | ProductoUsadoNoSeEliminaNiCambiaSku |
| R12 | CatalogoCompuesto.Construir | Orden por Guid aleatorio hacia variable el recorrido | Orden ordinal por SKU y deteccion de ciclos | P18, P27 y KitCircularPersistidoSeRevierte |

Las pruebas de rechazo verifican tanto el error como la ausencia de cambios no permitidos. La matriz final distingue pruebas automaticas, comprobaciones visuales y aspectos pendientes.

Commit verificado del nucleo: `62dad55`. R01-R12 quedaron incorporados en ese primer registro real. Los TRX conservan fallos anteriores a la correccion; no se crearon commits retrospectivos para simular etapas que no se versionaron por separado. Las mejoras posteriores incluyen el coordinador comprobable, un colaborador de reembolsos y la auditoria general de transacciones.

R13: durante la reapertura visual, Ui.Cargar cambiaba MinimumWidth en columnas con modo Fill antes de que la vista tuviera ventana; Windows Forms lanzo NullReferenceException en DataGridViewBand.set_Thickness. El log real se conserva en docs/evidencias/error-visual.txt. Se sustituyo Fill por AllCells y se elimino la redistribucion manual. Reapertura posterior correcta en REVISION_VISUAL.md y capturas 18-20; commit e87a3db.

R14: `Pedido.CambiarCantidad` invalidaba los componentes pero no impedia validar los totales anteriores. Las dos pruebas de `ValoracionTests` fallaron realmente antes de la correccion: se obtenia 59.00 donde correspondia 113.00. Se marco la valoracion como pendiente dentro del agregado y `ServicioPedidos.Validar` recalcula desde las instantaneas. Evidencia previa: `docs/evidencias/valoracion-antes.trx`; evidencia posterior: el TRX final de aceptacion. Commit: 0c3c25f.
