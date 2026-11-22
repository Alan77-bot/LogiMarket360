"""Genera matriz e informe desde evidencias reales, sin inventar aprobaciones."""
from pathlib import Path
import json
import xml.etree.ElementTree as ET
from xml.sax.saxutils import escape
from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak, Image, KeepTogether
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.utils import ImageReader
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.graphics.shapes import Drawing, Rect, String, Line, Polygon

ROOT = Path(__file__).resolve().parents[1]
EVID = ROOT / 'docs/evidencias'
NS = {'t': 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'}
trx = ET.parse(EVID / 'aceptacion.trx').getroot()
resultados = trx.findall('.//t:UnitTestResult', NS)
contadores = trx.find('.//t:Counters', NS).attrib
fecha = trx.find('t:Times', NS).attrib['finish']
casos = json.loads((ROOT / 'docs/pruebas/CASOS_OFICIALES.json').read_text(encoding='utf-8-sig'))
mapa = {
 'P01':'P01_', 'P02':'cliente: "Ana"', 'P03':'cliente: "Luis"', 'P04':'cliente: "Carmen"',
 'P05':'P05_', 'P06':'P06_', 'P07':'P07_', 'P08':'P08_', 'P09':'P09_', 'P10':'P10_',
 'P11':'P11_P30', 'P12':'P12_', 'P13':'P13_', 'P14':'P14_', 'P15':'P15_',
 'P16':'P16_P38', 'P17':'P17_', 'P18':'KitAnidadoExpande', 'P19':'P19_', 'P20':'P20_',
 'P21':'P21_', 'P22':'P22_', 'P23':'P23_', 'P24':'P24_', 'P25':'P25_', 'P26':'P26_',
 'P27':'P27_P34', 'P28':'P28_', 'P29':'P29_', 'P30':'P11_P30', 'P31':'P31_',
 'P32':'P32_', 'P33':'P33_', 'P34':'P27_P34', 'P35':'P35_', 'P36':'P36_', 'P37':'P37_', 'P38':'P38_'
}
notas = {
 'P01':'Base nueva creada por migraciones, cinco productos, dos kits, cuatro usuarios y ninguna migracion pendiente; seed repetido sin duplicados.',
 'P02':'Total 59.00, PAGADO y un pago; stock P01=7, P02=4 y P03=19 comprobado.',
 'P03':'Total 102.00 y un pago; stock P04=2 y P05=28 comprobado.',
 'P04':'Total 90.00, descuento 15.00 y stock P03=10 comprobado.',
 'P05':'Rechazo por P02; sin pago, movimientos ni evento de exito y sin descontar P01.',
 'P06':'Cobro rechazado, estado VALIDADO e inventario conservado; sin pago aprobado ni Outbox de exito.',
 'P07':'Mismo identificador de pago en reintentos; un pago y tres movimientos, uno por SKU.',
 'P08':'Cancelar y editar el pedido pagado provocan rechazo; total e historial conservados.',
 'P09':'Despacho sin pago rechazado; BODEGA completa despacho y entrega del pedido pagado.',
 'P10':'Otro proveedor DI abre la misma SQLite y recupera pedido PAGADO, pago, stock P01=7 y auditoria.',
 'P11':'EMAIL y SMS registran el canal elegido; una sola notificacion por evento.',
 'P12':'ESTANDAR: tarifa 5, plazo 3 y prefijo STD. EXPRES: tarifa 12, plazo 1 y prefijo EXP.',
 'P13':'Constructor sin cliente o sin lineas rechazado; agregado completo construido.',
 'P14':'Copia profunda comprobada con cambio de cantidad y arbol historico serializado.',
 'P15':'Dos ambitos reciben la misma instancia; 1000 registros concurrentes del mismo evento cuentan un pago.',
 'P16':'Ambos proveedores aceptan 59.00, conservan referencia en el reintento y rechazan entradas invalidas.',
 'P17':'Cuatro combinaciones ejecutadas; vistas previas reales y TXT en docs/evidencias/comprobantes.',
 'P18':'Precios 50/60, expansion P01/P02/P03 y cantidades propagadas; ciclo indirecto rechazado.',
 'P19':'Secuencia observada INICIO, PASARELA y FIN/ERROR, conservando el resultado del proveedor.',
 'P20':'Fallo del colaborador deja estado VALIDADO, stock P01=8, sin pago ni evento de exito.',
 'P21':'UI: 1000 filas, 1 instancia, ReferenceEquals=True; 104176 frente a 56360 bytes asignados. Medicion de asignaciones del hilo.',
 'P22':'CONSULTA y BODEGA reciben UnauthorizedAccessException antes del proveedor; ningun LOG de COBRO.',
 'P23':'Primero se informa CLIENTE y, al corregirlo, STOCK; no se invoca cobro.',
 'P24':'El boton de cobro y el test invocan ConfirmarPedidoCommand mediante InvocadorComandos.',
 'P25':'ESTADO = PAGADO AND TOTAL >= 50 acepta el pedido pagado de 65.00 y descarta el filtro de BORRADOR.',
 'P26':'AND tiene prioridad sobre OR; parentesis cambian el resultado y sintaxis invalida genera FormatException.',
 'P27':'Iterador propio produce P01, P02 y P03, termina y permanece terminado.',
 'P28':'Verificacion automatica del coordinador y capturas 07, 08 y 09: pedido pagado, stock actualizado y metrica de pago=1.',
 'P29':'Tres estados: deshacer devuelve cantidades 2 y 1; rehacer devuelve 2. Historial bloqueado rechaza deshacer.',
 'P30':'Un aviso, una auditoria de PedidoPagado y una metrica; segundo procesamiento no publica de nuevo.',
 'P31':'Ciclo permitido completo; despacho prematuro, cancelacion y repeticion rechazados.',
 'P32':'Politica nueva aplica 20% sin editar la calculadora; pruebas numericas adicionales verifican VIP, MAYORISTA y NORMAL.',
 'P33':'Arrepentimiento dia 10 rechazado; defecto dia 20 tambien verificado en P35.',
 'P34':'Visitor obtiene tres SKU y peso total 1.00 kg del kit teletrabajo.',
 'P35':'Ana recibe 54.00, sin envio; stock P01 vuelve a 8 y la solicitud repetida se rechaza; una devolucion.',
 'P36':'Con P01=1 y dos confirmaciones concurrentes: una aceptada, un pago y stock final cero.',
 'P37':'Fallo deja evento pendiente con un intento; procesador nuevo lo recupera; un aviso y un pago.',
 'P38':'Referencias inspeccionadas, contratos de proveedores sustituidos y compilacion de los cinco proyectos. El total automatico figura en el TRX.'
}
matriz = []
archivos_tests = [f for f in (ROOT/'tests/LogiMarket.Tests').rglob('*.cs') if 'obj' not in f.parts and 'bin' not in f.parts]
for c in casos:
    pruebas = [r for r in resultados if mapa[c['Id']] in r.attrib.get('testName', '')]
    aprobado = bool(pruebas) and all(r.attrib['outcome'] == 'Passed' for r in pruebas)
    registro = dict(c, Estado='APROBADO' if aprobado else 'PENDIENTE O FALLIDO', Fecha=fecha,
       Obtenido=(f"{len(pruebas)} prueba(s) automatizada(s) aprobada(s). " + notas.get(c['Id'], '') if aprobado else 'Revisar el resultado del TRX.'),
       Archivos=sorted({f.relative_to(ROOT).as_posix() for f in archivos_tests if any('.'+f.stem+'.' in r.attrib['testName'] for r in pruebas)}),
       Metodos=[r.attrib['testName'] for r in pruebas], Evidencia='docs/evidencias/aceptacion.trx')
    matriz.append(registro)
(ROOT / 'docs/pruebas/MATRIZ_RESULTADOS.json').write_text(json.dumps(matriz, ensure_ascii=False, indent=2), encoding='utf-8')
md = ['# Matriz oficial P01-P38', '', f"Fecha de ejecucion: {fecha}. Pruebas automaticas: {contadores['passed']}/{contadores['total']}.", '',
      'Los escenarios y esperados se extrajeron de la guia. Los metodos y estados proceden del TRX; las notas visuales corresponden a capturas reales.', '']
for r in matriz:
    md += [f"## {r['Id']} - {r['Estado']}", '', f"**Precondicion y entrada:** {r['Entrada']}", '', f"**Esperado:** {r['Esperado']}", '',
           f"**Obtenido:** {r['Obtenido']}", '', f"**Fecha:** {r['Fecha']}", '', f"**Evidencia:** {r['Evidencia']}", '', '**Archivos:** ' + '; '.join(r['Archivos']), '', '**Metodos:** ' + '; '.join(r['Metodos']), '']
(ROOT / 'docs/pruebas/MATRIZ.md').write_text('\n'.join(md), encoding='utf-8')

font = Path('C:/Windows/Fonts/segoeui.ttf')
bold = Path('C:/Windows/Fonts/segoeuib.ttf')
pdfmetrics.registerFont(TTFont('Segoe', str(font)))
pdfmetrics.registerFont(TTFont('SegoeBold', str(bold)))
styles = getSampleStyleSheet()
styles['Normal'].fontName = 'Segoe'; styles['Normal'].fontSize = 9; styles['Normal'].leading = 13; styles['Normal'].spaceAfter = 6
for nombre, tam in [('Title', 23), ('Heading1', 16), ('Heading2', 12), ('Heading3', 10)]:
    styles[nombre].fontName = 'SegoeBold'; styles[nombre].fontSize = tam; styles[nombre].textColor = colors.black
    styles[nombre].keepWithNext = True
styles.add(ParagraphStyle('Portada', parent=styles['Normal'], alignment=TA_CENTER, fontSize=12, leading=20))
styles.add(ParagraphStyle('Pequeno', parent=styles['Normal'], fontSize=8, leading=11))
story = []
def p(texto, estilo='Normal'):
    story.append(Paragraph(escape(str(texto)).replace('\n', '<br/>'), styles[estilo]))
def h(texto): p(texto, 'Heading1')
def diagrama(tipo):
    d = Drawing(490, 245)
    def caja(x,y,texto,w=140):
        d.add(Rect(x,y,w,34,fillColor=colors.HexColor('#EEF3F8'),strokeColor=colors.HexColor('#617C96'),rx=3,ry=3))
        d.add(String(x+w/2,y+13,texto,fontName='Segoe',fontSize=9,textAnchor='middle'))
    def flecha(x1,y1,x2,y2):
        d.add(Line(x1,y1,x2,y2,strokeColor=colors.HexColor('#617C96')))
        import math
        largo=math.hypot(x2-x1,y2-y1); ux=(x2-x1)/largo; uy=(y2-y1)/largo
        d.add(Polygon([x2,y2,x2-5*ux-3*uy,y2-5*uy+3*ux,x2-5*ux+3*uy,y2-5*uy-3*ux],fillColor=colors.HexColor('#617C96')))
    if tipo=='capas':
        caja(0,175,'WinForms'); caja(175,175,'Application'); caja(350,175,'Domain')
        flecha(140,192,175,192); flecha(315,192,350,192)
        caja(175,75,'Infrastructure'); flecha(245,109,245,175); flecha(315,92,420,175)
        d.add(String(0,25,'Las flechas representan referencias de compilacion.',fontName='Segoe',fontSize=9))
    elif tipo=='estados':
        for i,nombre in enumerate(['BORRADOR','VALIDADO','PAGADO','DESPACHADO','ENTREGADO']):
            caja(i*100,170,nombre,90)
            if i<4:flecha(i*100+90,187,(i+1)*100,187)
        caja(50,75,'CANCELADO',140);flecha(45,170,90,109);flecha(145,170,145,109)
        d.add(String(0,25,'Cancelar solo desde BORRADOR o VALIDADO.',fontName='Segoe',fontSize=9))
    elif tipo=='datos':
        nombres=[('Cliente',0,190),('Pedido',175,190),('Pago / Despacho',350,190),('Producto',0,115),('PedidoDetalle',175,115),('Devolucion',350,115),('Kit / Componentes',0,40),('DetalleComponente',175,40),('Outbox / Recepcion',350,40)]
        for nombre,x,y in nombres:caja(x,y,nombre)
        for x1,y1,x2,y2 in [(140,207,175,207),(315,207,350,207),(245,190,245,149),(245,115,245,74),(315,190,350,132),(315,175,350,57),(70,115,70,74)]:flecha(x1,y1,x2,y2)
    else:
        aceptada=tipo=='aceptada'
        actores=['Vista','Application','Proveedor','SQLite']
        xs=[45,175,305,440]
        for nombre,x in zip(actores,xs):
            d.add(String(x,225,nombre,fontName='SegoeBold',fontSize=9,textAnchor='middle'));d.add(Line(x,15,x,215,strokeColor=colors.lightgrey))
        pasos=[(0,1,'Command / Facade'),(1,3,'BEGIN IMMEDIATE'),(1,2,'Proxy -> Decorator -> Adapter'),(2,1,'Cobro aceptado'),(1,3,'Pago + stock + Outbox'),(3,1,'COMMIT'),(1,0,'Observer y Mediator')] if aceptada else [(0,1,'Solicitud CONSULTA'),(1,3,'Auditoria de seguridad'),(1,0,'Acceso denegado')]
        for i,(a,b,texto) in enumerate(pasos):
            y=198-i*26; flecha(xs[a],y,xs[b],y);d.add(String(min(xs[a],xs[b])+4,y+5,texto,fontName='Segoe',fontSize=7.5))
        if not aceptada:d.add(String(255,90,'Proveedor no invocado',fontName='Segoe',fontSize=9))
    story.append(d);story.append(Spacer(1,12))
def tabla(filas, anchos):
    datos = [[Paragraph(escape(str(c)), styles['Pequeno']) for c in fila] for fila in filas]
    t = Table(datos, colWidths=anchos, repeatRows=1, hAlign='LEFT')
    t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,0),colors.HexColor('#DCE7F2')), ('GRID',(0,0),(-1,-1),.4,colors.HexColor('#CCCCCC')), ('VALIGN',(0,0),(-1,-1),'TOP'), ('LEFTPADDING',(0,0),(-1,-1),7),('RIGHTPADDING',(0,0),(-1,-1),7),('TOPPADDING',(0,0),(-1,-1),6),('BOTTOMPADDING',(0,0),(-1,-1),6)]))
    story.append(t); story.append(Spacer(1,10))
def markdown(ruta):
    filas = []
    def emitir():
        if not filas: return
        n=len(filas[0]); anchos = [55,285,150] if n==3 else [30,100,130,120,110] if n==5 else [490/n]*n
        tabla(filas,anchos); filas.clear()
    for linea in (ROOT / ruta).read_text(encoding='utf-8-sig').splitlines():
        if not linea.strip() or linea.startswith('```'): continue
        if linea.startswith('|'):
            if linea.startswith('|---'): continue
            celdas = [c.strip() for c in linea.strip('|').split('|')]
            filas.append(celdas); continue
        emitir()
        if linea.startswith('#'): p(linea.lstrip('# '), 'Heading2')
        else: p(linea.replace('`',''), 'Normal')
    emitir()

p('UNIVERSIDAD TECNICA DE AMBATO', 'Portada'); story.append(Spacer(1,35)); p('LogiMarket 360', 'Title'); p('Informe tecnico APE 04', 'Portada')
story.append(Spacer(1,35))
for texto in ['Participante: PENDIENTE', 'Asignatura: PENDIENTE', 'Nivel y paralelo: PENDIENTE', 'Docente: PENDIENTE', 'Fecha de verificacion: 21 de noviembre de 2026']:
    p(texto, 'Portada')
story.append(PageBreak())
h('2 Resumen ejecutivo')
p(f"Se construyo una solucion de cinco proyectos en .NET 8 con Windows Forms y persistencia SQLite/EF Core. La ultima evidencia automatizada contiene {contadores['passed']} pruebas aprobadas de {contadores['total']}, sin omisiones. Los registros anteriores de fallos se conservan para mostrar la correccion del mapeo de identidades y de la prueba de migraciones.")
p('Se ejecuto desde la interfaz la venta de Ana por USD 59.00, despacho, entrega y devolucion por USD 54.00. Las capturas muestran pantallas reales. Los tres escenarios numericos oficiales, concurrencia, idempotencia y recuperacion Outbox tienen pruebas sobre SQLite temporal.')
p('Objetivo general: integrar reglas de ventas y logistica con arquitectura limpia, patrones y evidencia reproducible. Objetivos especificos: preservar invariantes, coordinar operaciones transaccionales, separar colaboradores y comprobar comportamiento funcional y arquitectonico.')
h('3 Problema y requisitos'); markdown('docs/requisitos/REQUISITOS.md')
h('4 Modelo de dominio y datos')
p('El agregado Pedido conserva cliente, entrega, estado, totales, detalle, componentes historicos y cambios de estado. Producto protege precio, peso, stock y version. Kit relaciona productos y otros kits mediante FK y valida ciclos antes de guardar. Cliente protege nombre, categoria y actividad.')
p('Tablas: Usuario, Cliente, Producto, Kit, KitComponente, Pedido, PedidoDetalle, PedidoDetalleComponente, CambioEstado, Pago, MovimientoInventario, Despacho, Devolucion, EventoOutbox, Auditoria, RecepcionEvento y Aviso. Dos migraciones reproducen la creacion y evolucion del esquema.')
diagrama('datos')
p('Claves unicas: SKU, usuario, pago por pedido, referencia de cobro, idempotencia y devolucion por pedido. Los movimientos se identifican por producto, motivo y referencia. Los componentes de catalogo tienen destino exclusivo producto/kit; las cantidades y stocks estan protegidos.')
diagrama('estados')
h('5 Arquitectura y dependencias')
diagrama('capas')
tabla([['Proyecto','Referencias'],['Domain','Ninguna propia'],['Application','Domain'],['Infrastructure','Application y Domain'],['WinForms','Application e Infrastructure para composicion'],['Tests','Domain, Application e Infrastructure']], [140,350])
p('Recorrido aceptado: Vista -> Command -> Facade -> autorizacion -> transaccion -> Chain -> State -> Proxy -> Decorator -> Adapter -> persistencia -> commit -> Outbox -> Observer -> Mediator. Recorrido rechazado: CONSULTA -> Proxy -> auditoria de seguridad -> excepcion; no alcanza al proveedor.')
p('Los diagramas de componentes, datos, estados y ambas secuencias se conservan en docs/arquitectura/DISENO.md en formato Mermaid editable. El punto de composicion es WinForms/Program.cs y RegistroServicios.cs.')
p('Secuencia de confirmacion aceptada', 'Heading2');diagrama('aceptada')
p('Secuencia de operacion rechazada', 'Heading2');diagrama('rechazada')
h('6 Clean Code y principios SOLID')
p('SRP separa calculo, persistencia y presentacion. OCP permite promociones nuevas por IPoliticaDescuento. LSP se prueba con ambos proveedores. ISP conserva contratos pequenos. DIP mantiene EF fuera del nucleo. Los comentarios explican reglas y patrones sin tildes.')
markdown('docs/calidad/SOLID.md')
p('Los riesgos reales se documentan en docs/calidad/RIESGOS.md; no se inventan pruebas rojas previas. La primera ejecucion tuvo 31 aprobadas y 16 fallidas por identidades de hijos. Las dos pruebas posteriores de valoracion fallaron antes de corregir el agregado y ahora forman parte de la ejecucion final.')
lineas = (ROOT/'docs/patrones/MAPA.md').read_text(encoding='utf-8').splitlines()
patrones = [[c.strip() for c in l.strip('|').split('|')] for l in lineas if l.startswith('|') and not l.startswith('|---')][1:]
for titulo, rango in [('7 Cinco patrones creacionales', patrones[:5]), ('8 Siete patrones estructurales', patrones[5:12]), ('9 Once patrones comportamentales', patrones[12:])]:
    h(titulo)
    for fila in rango:
        p(fila[0], 'Heading2'); p(fila[1]); p('Ubicacion y recorrido: ' + fila[2], 'Pequeno'); p('Prueba: ' + fila[3] + '. Beneficio y compromiso: ' + fila[4], 'Pequeno')
h('10 Desarrollo visual y flujo completo')
p('La ventana principal contiene menu lateral, encabezado, usuario, rol y cierre de sesion. Los modulos usan DataGridView, formularios de edicion y confirmaciones. Las vistas se actualizan por Mediator. Los avisos y fallos se simulan localmente.')
capturas = sorted(f for f in (EVID/'capturas').iterdir() if f.suffix.lower() in ('.png','.jpg','.jpeg'))
for f in capturas:
    ancho, alto = ImageReader(str(f)).getSize(); escala = min(490/ancho, 280/alto)
    story.append(KeepTogether([Paragraph(escape(f.stem.replace('-',' ')),styles['Heading3']),Image(str(f),width=ancho*escala,height=alto*escala),Spacer(1,14)]))
h('11 Pruebas y resultados')
p(f"Ejecucion final: {contadores['passed']}/{contadores['total']} aprobadas; fallidas: {contadores.get('failed','0')}; fecha TRX: {fecha}. Cada caso de integracion prepara su propia base SQLite y aplica migraciones y seed. El archivo TRX contiene nombres, duraciones y resultados.")
tabla([['Familia GoF','Implementados','Verificacion'],['Creacionales','5/5','P11-P15'],['Estructurales','7/7','P16-P22'],['Comportamentales','11/11','P23-P34'],['Total','23/23','Mapa de archivos y pruebas']], [180,100,210])
limpio = EVID/'compilacion-limpia.txt'
if limpio.exists() and 'RESULTADO: APROBADO' in limpio.read_text(encoding='utf-8-sig'):
    p('Compilacion desde una copia extraida sin bin/obj: APROBADA. Log, archivo y SHA256 en docs/evidencias/compilacion-limpia.txt.')
tabla([['Caso','Subtotal','Descuento','Envio','Total'],['Ana VIP','60.00','6.00','5.00','59.00'],['Luis NORMAL','90.00','0.00','12.00','102.00'],['Carmen MAYORISTA','100.00','15.00','5.00','90.00']], [130,90,90,90,90])
for r in matriz:
    p(r['Id'] + ' - ' + r['Estado'], 'Heading2'); p('Precondicion y entrada: ' + r['Entrada']); p('Esperado: ' + r['Esperado']); p('Obtenido: ' + r['Obtenido']); p('Archivos: ' + '; '.join(r['Archivos']), 'Pequeno'); p('Metodo: ' + '; '.join(r['Metodos']), 'Pequeno'); p('Evidencia: ' + r['Evidencia'] + '; fecha: ' + r['Fecha'], 'Pequeno')
h('12 Riesgos y limitaciones')
p('Los datos personales de portada estan pendientes por decision del usuario. Se usan proveedores simulados y comprobantes internos. Las metricas son de sesion. Las consultas cargan colecciones completas; para un catalogo grande convendria paginar. La revision visual documenta el flujo probado, no una certificacion exhaustiva de todas las combinaciones de DPI y accesibilidad.')
markdown('docs/calidad/RIESGOS.md')
h('13 Conclusiones vinculadas con los objetivos')
p(f"La separacion de cinco proyectos mantiene el dominio independiente de EF y WinForms y se comprueba con referencias y pruebas. Las ventas oficiales producen 59.00, 102.00 y 90.00 desde calculos reales. La devolucion de Ana excluye el envio y reintegra una vez. Las pruebas de concurrencia e idempotencia verifican que una unidad no produzca dos ventas. La evidencia automatizada final alcanza {contadores['passed']} aprobaciones.")
h('14 Recomendaciones vinculadas con conclusiones')
p('Conservar las pruebas contractuales al cambiar proveedor para mantener LSP. Ejecutar los casos oficiales al modificar descuentos, entrega o devoluciones. Practicar los cambios de docs/DEFENSA.md con el depurador para comprender el recorrido. Medir y agregar paginacion si el volumen crece. Completar los datos institucionales antes de entregar el informe.')
h('15 Referencias IEEE')
for ref in ['[1] Universidad Tecnica de Ambato, GUIA_APE_04, documento proporcionado por el usuario, 2026.', '[2] Microsoft, Using Transactions, https://learn.microsoft.com/en-us/ef/core/saving/transactions, consultado 21 nov. 2026.', '[3] Microsoft, Handling Concurrency Conflicts, https://learn.microsoft.com/en-us/ef/core/saving/concurrency, consultado 21 nov. 2026.', '[4] Microsoft, Windows Forms Overview, https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/, consultado 21 nov. 2026.', '[5] xUnit.net, Getting Started with xUnit.net v2, https://xunit.net/docs/getting-started/v2/getting-started, consultado 21 nov. 2026.']:
    p(ref, 'Pequeno')
h('16 Anexos')
p('Codigo fuente, migraciones, pruebas, guia de defensa, mapa de patrones, diagramas editables, matriz JSON, capturas originales y comprobantes TXT se incluyen en el repositorio y paquete. Los registros fallidos e intermedios se conservan como trazabilidad. La portada requiere los datos indicados como pendientes.')
salida = ROOT / 'output/pdf/APE04_Informe_DATOS_PENDIENTES.pdf'; salida.parent.mkdir(parents=True, exist_ok=True)
def pie(canvas, doc):
    canvas.setFont('Segoe',8); canvas.setFillColor(colors.HexColor('#666666')); canvas.drawString(42,25,'LogiMarket 360 | APE 04'); canvas.drawRightString(553,25,str(doc.page))
SimpleDocTemplate(str(salida), pagesize=(595.28,841.89), rightMargin=42,leftMargin=42,topMargin=42,bottomMargin=45,title='LogiMarket 360 Informe APE 04',author='Datos de autor pendientes').build(story,onFirstPage=pie,onLaterPages=pie)
print(json.dumps({'pdf':str(salida),'casos':len(matriz),'aprobados':sum(r['Estado']=='APROBADO' for r in matriz),'pruebas':contadores}, ensure_ascii=False))
