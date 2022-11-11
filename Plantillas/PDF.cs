using System;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using System.Drawing;
using Spire.Pdf.AutomaticFields;
using System.IO;
using System.Xml;
using Facturafast.Models;
using System.Linq;

namespace Facturafast.Plantillas
{
    public class PDF
    {
        String _mapserver;
        public PDF(String MapServer)
        {
            _mapserver = MapServer;
        }

        public void GenerarDocA(String url)
        {
            BD_FFEntities db = new BD_FFEntities();
            String _version = "";

            //!Comprobante
            DateTime _fechaEmision;
            Decimal _tipoCambio = 0;
            String _serie = "";
            String _folio = "";
            String _moneda = "";
            Decimal _total_original = 0;
            Decimal _subtotal = 0;
            String _metodoPago = "";
            String _formaPago = "";
            String _tipoComprobante = "";
            Decimal _descuento = 0;
            String _lugarExpedicion = "";
            String _exportacion = "";

            //Emisor
            String _rfcEmisor = "";
            String _nombreEmisor = "";
            String _regimenFiscalEmisor = "";


            //!Receptor
            String _rfcReceptor = "";
            String _nombreReceptor = "";
            String _usoCFDI = "";
            String _regimenFiscalReceptor = "";
            String _domicilioFiscalReceptor = "";

            //!TimbreFiscalDigital
            DateTime _fechaTimbrado = DateTime.Now;
            String _uuid = "";
            String _versionTimbreFiscalDigital = "";
            tbd_Facturas nuevaFactura = new tbd_Facturas();
            //creamos un flujo el cual recibe nuestro xml
            using (StreamReader reader = new StreamReader(url))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(reader);

                foreach (var Comprobante in xmlDoc.ChildNodes)
                {
                    if (Comprobante.GetType() == typeof(XmlElement))
                    {
                        var _comprobante = (Comprobante as XmlElement);
                        if (_comprobante.LocalName == "Comprobante")
                        {
                            

                            //! Version del CFDI
                            if (_comprobante.HasAttribute("Version"))
                            {
                                _version = _comprobante.GetAttribute("Version").ToString();
                            }

                            //! Datos generales
                            _fechaEmision = _comprobante.HasAttribute("Fecha") ? DateTime.Parse(_comprobante.GetAttribute("Fecha").ToString()) : DateTime.Now;
                            _tipoCambio = _comprobante.HasAttribute("TipoCambio") ? Convert.ToDecimal(_comprobante.GetAttribute("TipoCambio").ToString()) : 1;
                            _serie = _comprobante.HasAttribute("Serie") ? _comprobante.GetAttribute("Serie").ToString() : "";
                            _folio = _comprobante.HasAttribute("Folio") ? _comprobante.GetAttribute("Folio").ToString() : "";
                            _lugarExpedicion = _comprobante.HasAttribute("LugarExpedicion") ? _comprobante.GetAttribute("LugarExpedicion").ToString() : "";
                            _exportacion = _comprobante.HasAttribute("Exportacion") ? _comprobante.GetAttribute("Exportacion").ToString() : "";
                            _moneda = _comprobante.HasAttribute("Moneda") ? _comprobante.GetAttribute("Moneda").ToString() : "";
                            _total_original = _comprobante.HasAttribute("Total") ? Convert.ToDecimal(_comprobante.GetAttribute("Total").ToString()) : 0;
                            _subtotal = _comprobante.HasAttribute("SubTotal") ? Convert.ToDecimal(_comprobante.GetAttribute("SubTotal").ToString()) : 0;
                            _tipoComprobante = _comprobante.HasAttribute("TipoDeComprobante") ? _comprobante.GetAttribute("TipoDeComprobante").ToString() : "";
                            _formaPago = _comprobante.HasAttribute("FormaPago") ? _comprobante.GetAttribute("FormaPago").ToString() : "";
                            _metodoPago = _comprobante.HasAttribute("MetodoPago") ? _comprobante.GetAttribute("MetodoPago").ToString() : "";
                            _descuento = _comprobante.HasAttribute("Descuento") ? Convert.ToDecimal(_comprobante.GetAttribute("Descuento").ToString()) : 0;

                            //!Nodo Principales
                            foreach (var Nodos in _comprobante.ChildNodes)
                            {
                                if (Nodos.GetType() == typeof(XmlElement))
                                {
                                    var _nodo = (Nodos as XmlElement);
                                    if (_nodo.LocalName == "Complemento")
                                    {
                                        //!Complementos
                                        foreach (var Complemento in _nodo.ChildNodes)
                                        {
                                            if (Complemento.GetType() == typeof(XmlElement))
                                            {
                                                var _complemento = (Complemento as XmlElement);
                                                if (_complemento.LocalName == "TimbreFiscalDigital")
                                                {
                                                    //!Timbre Fiscal Digital
                                                    _uuid = _complemento.HasAttribute("UUID") ? _complemento.GetAttribute("UUID").ToString() : "";
                                                    _fechaTimbrado = _complemento.HasAttribute("FechaTimbrado") ? DateTime.Parse(_complemento.GetAttribute("FechaTimbrado").ToString()) : DateTime.Now;
                                                    _versionTimbreFiscalDigital = _complemento.HasAttribute("Version") ? _complemento.GetAttribute("Version").ToString() : "";
                                                }
                                            }
                                        }
                                    }
                                    if (_nodo.LocalName == "Emisor")
                                    {
                                        //!Emisor
                                        _rfcEmisor = _nodo.HasAttribute("Rfc") ? _nodo.GetAttribute("Rfc").ToString() : "";
                                        _nombreEmisor = _nodo.HasAttribute("Nombre") ? _nodo.GetAttribute("Nombre").ToString() : "";
                                        _regimenFiscalEmisor = _nodo.HasAttribute("RegimenFiscal") ? _nodo.GetAttribute("RegimenFiscal").ToString() : "";
                                    }
                                    else if (_nodo.LocalName == "Receptor")
                                    {
                                        //!Receptor
                                        _rfcReceptor = _nodo.HasAttribute("Rfc") ? _nodo.GetAttribute("Rfc").ToString() : "";
                                        _nombreReceptor = _nodo.HasAttribute("Nombre") ? _nodo.GetAttribute("Nombre").ToString() : "";
                                        _usoCFDI = _nodo.HasAttribute("UsoCFDI") ? _nodo.GetAttribute("UsoCFDI").ToString() : "";
                                        _regimenFiscalReceptor = _nodo.HasAttribute("RegimenFiscalReceptor") ? _nodo.GetAttribute("RegimenFiscalReceptor").ToString() : "";
                                        _domicilioFiscalReceptor = _nodo.HasAttribute("DomicilioFiscalReceptor") ? _nodo.GetAttribute("DomicilioFiscalReceptor").ToString() : "";
                                    }
                                }
                            }

                           
                            nuevaFactura.version_cfdi = _version;
                            nuevaFactura.fecha_emision = _fechaEmision;
                            nuevaFactura.tipo_cambio = _tipoCambio;
                            nuevaFactura.serie = _serie;
                            nuevaFactura.folio = _folio;
                            nuevaFactura.lugar_expedicion = _lugarExpedicion;
                            nuevaFactura.exportacion = _exportacion;
                            nuevaFactura.moneda = _moneda;
                            nuevaFactura.total_original = _total_original;

                            tbc_Tipos_Comprobante tbc_Tipos_Comprobante = db.tbc_Tipos_Comprobante.Where(s => s.tipo_comprobante == _tipoComprobante).SingleOrDefault();
                            nuevaFactura.id_tipo_comprobante = tbc_Tipos_Comprobante != null ? tbc_Tipos_Comprobante.id_tipo_comprobante : 1;

                            tbc_Formas_Pago tbc_Formas_Pago = db.tbc_Formas_Pago.Where(s => s.clave == _formaPago).SingleOrDefault();
                            nuevaFactura.id_forma_pago = tbc_Formas_Pago != null ? tbc_Formas_Pago.id_forma_pago : 1;

                            tbc_Metodos_Pago tbc_Metodos_Pago = db.tbc_Metodos_Pago.Where(s => s.clave == _metodoPago).SingleOrDefault();
                            nuevaFactura.id_metodo_pago = tbc_Metodos_Pago != null ? tbc_Metodos_Pago.id_metodo_pago : 1;

                            nuevaFactura.uuid = _uuid;
                            nuevaFactura.fecha_timbrado = _fechaTimbrado;
                            nuevaFactura.rfc_emisor = _rfcEmisor;
                            nuevaFactura.nombre_emisor = _nombreEmisor;

                            tbc_Regimenes tbc_Regimenes = db.tbc_Regimenes.Where(s => s.clave == _regimenFiscalEmisor).SingleOrDefault();
                            nuevaFactura.id_regimen_fiscal_emisor = tbc_Regimenes != null ? tbc_Regimenes.id_regimen_fiscal : 0;

                            nuevaFactura.rfc_receptor = _rfcReceptor;
                            nuevaFactura.nombre_receptor = _nombreReceptor;

                            tbc_Usos_CFDI tbc_Usos_CFDI = db.tbc_Usos_CFDI.Where(s => s.clave == _usoCFDI).Single();
                            nuevaFactura.id_uso_cfdi = tbc_Usos_CFDI != null ? tbc_Usos_CFDI.id_uso_cfdi : 13;

                            tbc_Regimenes tbc_Regimenes_Receptor = db.tbc_Regimenes.Where(s => s.clave == _regimenFiscalReceptor).SingleOrDefault();
                            nuevaFactura.id_regimen_fiscal_receptor = tbc_Regimenes_Receptor != null ? tbc_Regimenes_Receptor.id_regimen_fiscal : 0;

                            nuevaFactura.domicio_fiscal_receptor = _domicilioFiscalReceptor;


                            nuevaFactura.subtotal = _subtotal * _tipoCambio;
                            nuevaFactura.total = _total_original * _tipoCambio;
                            nuevaFactura.descuento = _descuento * _tipoCambio;

                            String DirectoryFecha = _fechaTimbrado.ToString("yyyyMMdd") + "\\";
                           

                            nuevaFactura.url_xml = (DirectoryFecha + _uuid + ".xml").ToUpper();
                            nuevaFactura.fecha_creacion = DateTime.Now;
                            nuevaFactura.id_estatus = 5; //Vigente
                            nuevaFactura.fecha_validacion = DateTime.Now;
                            

                        }
                    }
                }
            }


            String urlPDF = _mapserver + @"doc\pdf\"+_uuid+".pdf";
            //Creamos el Archivo PDF
            PdfDocument pdf = new PdfDocument();

            

            // Configuracion de los margenes del contenido
            PdfUnitConvertor unitCvtr = new PdfUnitConvertor();
            PdfMargins margin = new PdfMargins();
            margin.Top = unitCvtr.ConvertUnits(4.00f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            margin.Bottom = unitCvtr.ConvertUnits(3.00f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            margin.Left = unitCvtr.ConvertUnits(0.80f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            margin.Right = margin.Left;


            //Agregamos la Pagina
            PdfPageBase page = pdf.Pages.Add(PdfPageSize.Letter, margin);
            page.BackgroundColor = Color.Chocolate;
            float pageWidth = page.Canvas.ClientSize.Width;
            float y = 0;
            for (int i = 0; i < 50; i++)
            {
                PdfBrush brush2 = new PdfSolidBrush(Color.Black);
                PdfTrueTypeFont font2 = new PdfTrueTypeFont(new Font("Arial", 16f, FontStyle.Bold));
                PdfStringFormat format2 = new PdfStringFormat(PdfTextAlignment.Center);
                format2.CharacterSpacing = 1f;
                String text = "Summary of Science";
                page.Canvas.DrawString(text, font2, brush2, pageWidth / 2, y, format2);
                SizeF size = font2.MeasureString(text, format2);
                y = y + size.Height + 6;
            }
            

            //page.Canvas.DrawString()
            //Agregamos Encabezado
            DrawHeaderDocA(pdf, nuevaFactura);

            //Agregamos Pie
            DrawFooterDocA(pdf);


            // Save and open documents
            pdf.SaveToFile(urlPDF);
            pdf.Close();

            //// Create a new PdfDocument class object and add a page
            //PdfDocument pdf = new PdfDocument(url);            
            //PdfPageBase page = pdf.Pages.Add();

            //// Setting up margin
            //PdfUnitConvertor unitCvtr = new PdfUnitConvertor();
            //PdfMargins margin = new PdfMargins();
            //margin.Top = unitCvtr.ConvertUnits(2.54f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            //margin.Bottom = margin.Top;
            //margin.Left = unitCvtr.ConvertUnits(4.17f, PdfGraphicsUnit.Centimeter, PdfGraphicsUnit.Point);
            //margin.Right = margin.Left;


            //// Save and open documents
            //pdf.SaveToFile(path);
            //pdf.Close();

            //// Load a test document
            //PdfDocument existingPdf = new PdfDocument();
            //existingPdf.LoadFromFile(path);

            //DrawHeader(existingPdf, nuevaFactura);

            //// Call the DrawFooter method to add footers to existing documents
            //DrawFooter(existingPdf);

            //// Save and open documents
            //existingPdf.SaveToFile(path2);

        }

        void DrawHeaderDocA(PdfDocument doc, tbd_Facturas fac)
        {
            // Get page size
            SizeF pageSize = doc.Pages[0].Size;

            // Declare two float-type variables X and Y
            float x = 90;
            float y = 20;

            for (int i = 0; i < doc.Pages.Count; i++)
            {
                //// Draw pictures at the point of each page
                //PdfImage headerImage = PdfImage.FromFile(path);
                float width = 10 / 7;
                float height = 10 / 7;
                //doc.Pages[i].Canvas.DrawImage(headerImage, x, y, width, height);

                // Draw horizontal lines at the point of each page
                PdfPen pen = new PdfPen(PdfBrushes.Gray, 0.5f);
                doc.Pages[i].Canvas.DrawLine(pen, x, y + height + 2, pageSize.Width - x, y + height + 2);
            }
        }

        // Draw the footer in the blank area at the bottom of the page
        void DrawFooterDocA(PdfDocument doc)
        {
            // Get page size
            SizeF pageSize = doc.Pages[0].Size;

            // Declare two float-type variables X and Y
            float x = 90;
            float y = pageSize.Height - 72;

            for (int i = 0; i < doc.Pages.Count; i++)
            {
                // Draw horizontal lines at the point of each page
                PdfPen pen = new PdfPen(PdfBrushes.Gray, 0.5f);
                doc.Pages[i].Canvas.DrawLine(pen, x, y, pageSize.Width - x, y);

                // Draw text at the point of each page
                y = y + 5;
                PdfTrueTypeFont font = new PdfTrueTypeFont(new Font("blackbody", 10f, FontStyle.Bold), true);
                PdfStringFormat format = new PdfStringFormat(PdfTextAlignment.Left);
                String footerText = " Website\n https://g20.org/";
                doc.Pages[i].Canvas.DrawString(footerText, font, PdfBrushes.Black, x, y, format);
                // Locate the current page number and total page number at the finger of each page.
                PdfPageNumberField number = new PdfPageNumberField();
                PdfPageCountField count = new PdfPageCountField();
                PdfCompositeField compositeField = new PdfCompositeField(font, PdfBrushes.Black, "{0}/{1}", number, count);
                compositeField.StringFormat = new PdfStringFormat(PdfTextAlignment.Right, PdfVerticalAlignment.Top);
                SizeF size = font.MeasureString(compositeField.Text);
                compositeField.Bounds = new RectangleF(pageSize.Width - x - size.Width, y, size.Width, size.Height);
                compositeField.Draw(doc.Pages[i].Canvas);
            }
        }


    }
}