using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace Facturafast.Models
{
    public class LecturaXML
    {
        public void LeerArchivo(HttpPostedFileBase file, String rfc)
        {
            BD_FFEntities db = new BD_FFEntities();
            tbc_Variables_Calculo variable = db.tbc_Variables_Calculo.Single();
            //creamos un flujo el cual recibe nuestro xml
            using (StreamReader reader = new StreamReader(file.InputStream))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(reader);
                try
                {
                    foreach (var Comprobante in xmlDoc.ChildNodes)
                    {
                        if (Comprobante.GetType() == typeof(XmlElement))
                        {
                            var _comprobante = (Comprobante as XmlElement);
                            if (_comprobante.LocalName == "Comprobante")
                            {

                                List<tbd_Conceptos_Factura> conceptos = new List<tbd_Conceptos_Factura>();

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
                                String _certificadoSAT = "";
                                String _selloDigital = "";
                                String _selloSAT = "";


                                //!CartaPorte
                                String _transpInter = "";
                                String _totalDistancia = "";

                                //!CartaPorte - Ubicaciones
                                String _idUbicacionOrigen = "";
                                String _rfcRemDesOrigen = "";
                                String _nombreRemDesOrigen = "";
                                String _distanciaRecorridaOrigen = "";
                                DateTime _fechaHoraOrigen = DateTime.Now;
                                String _calleOrigen = "";
                                String _coloniaOrigen = "";
                                String _localidadOrigen = "";
                                String _municipioOrigen = "";
                                String _estadoOrigen = "";
                                String _paisOrigen = "";
                                String _cpOrigen = "";
                                String _direccionOrigen = "";

                                String _idUbicacionDestino = "";
                                String _rfcRemDesDestino = "";
                                String _nombreRemDesDestino = "";
                                String _distanciaRecorridaDestino = "";
                                DateTime _fechaHoraDestino = DateTime.Now;
                                String _calleDestino = "";
                                String _coloniaDestino = "";
                                String _localidadDestino = "";
                                String _municipioDestino = "";
                                String _estadoDestino = "";
                                String _paisDestino = "";
                                String _cpDestino = "";
                                String _direccionDestino = "";

                                //!CartaPorte - Mercancias
                                String _pesoBrutoTotal = "";
                                String _unidadPeso = "";
                                String _numTotalMercancias = "";

                                //!CartaPorte - Mercancia
                                String _bienesTrans = "";
                                String _descripcionMerca = "";
                                String _cantidadMerca = "";
                                String _claveUnidadMerca = "";
                                String _unidadMerca = "";
                                String _pesoKG = "";

                                //!CartaPorte - AutoTransporte
                                String _permSCT = "";
                                String _numPermisoSCT = "";

                                //!CartaPorte - IdentificacionVehicular
                                String _configVehivular = "";
                                String _placaVM = "";
                                String _anioModelo = "";

                                //!CartaPorte - Seguros
                                String _aseguraRespCivil = "";
                                String _polizaRespCivil = "";

                                //!CartaPorte - Remolques
                                String _subTipoRem = "";
                                String _placaRemolque = "";


                                //!CartaPorte - FiguraTransporte
                                String _tipoFigura = "";
                                String _rfcFigura = "";
                                String _numLicenciaFigura = "";
                                String _nombreFigura = "";


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
                                                        _certificadoSAT = _complemento.HasAttribute("NoCertificadoSAT") ? _complemento.GetAttribute("NoCertificadoSAT").ToString() : "";
                                                        _selloDigital = _complemento.HasAttribute("SelloCFD") ? _complemento.GetAttribute("SelloCFD").ToString() : "";
                                                        _selloSAT = _complemento.HasAttribute("SelloSAT") ? _complemento.GetAttribute("SelloSAT").ToString() : "";


                                                    }

                                                    if (_complemento.LocalName == "CartaPorte")
                                                    {
                                                        _transpInter = _complemento.HasAttribute("TranspInternac") ? _complemento.GetAttribute("TranspInternac").ToString() : "";

                                                        _totalDistancia = _complemento.HasAttribute("TotalDistRec") ? _complemento.GetAttribute("TotalDistRec").ToString() : "";

                                                        foreach (var CartaPorte in _complemento.ChildNodes)
                                                        {
                                                            if (CartaPorte.GetType() == typeof(XmlElement))
                                                            {
                                                                var _cartaPorte = (CartaPorte as XmlElement);



                                                                if (_cartaPorte.LocalName == "Ubicaciones")
                                                                {
                                                                    foreach (var Ubicaciones in _cartaPorte.ChildNodes)
                                                                    {
                                                                        if (Ubicaciones.GetType() == typeof(XmlElement))
                                                                        {
                                                                            var _ubicacion = (Ubicaciones as XmlElement);
                                                                            String tipo_Ubicacion = _ubicacion.HasAttribute("TipoUbicacion") ? _ubicacion.GetAttribute("TipoUbicacion").ToString() : "";
                                                                            if (tipo_Ubicacion == "Origen")
                                                                            {
                                                                                _idUbicacionOrigen = _ubicacion.HasAttribute("IDUbicacion") ? _ubicacion.GetAttribute("IDUbicacion").ToString() : "";
                                                                                _rfcRemDesOrigen = _ubicacion.HasAttribute("RFCRemitenteDestinatario") ? _ubicacion.GetAttribute("RFCRemitenteDestinatario").ToString() : "";
                                                                                _nombreRemDesOrigen = _ubicacion.HasAttribute("NombreRemitenteDestinatario") ? _ubicacion.GetAttribute("NombreRemitenteDestinatario").ToString() : "";
                                                                                _fechaHoraOrigen = _ubicacion.HasAttribute("FechaHoraSalidaLlegada") ? DateTime.Parse(_ubicacion.GetAttribute("FechaHoraSalidaLlegada").ToString()) : DateTime.Now;
                                                                                _distanciaRecorridaOrigen = _ubicacion.HasAttribute("DistanciaRecorrida") ? _ubicacion.GetAttribute("DistanciaRecorrida").ToString() : "";



                                                                                var _domicilio = _ubicacion.FirstChild;
                                                                                if (_domicilio.GetType() == typeof(XmlElement))
                                                                                {
                                                                                    var _domi = (_domicilio as XmlElement);

                                                                                    _calleOrigen = _domi.HasAttribute("Calle") ? _domi.GetAttribute("Calle").ToString() : "";
                                                                                    _coloniaOrigen = _domi.HasAttribute("Colonia") ? _domi.GetAttribute("Colonia").ToString() : "";
                                                                                    _localidadOrigen = _domi.HasAttribute("Localidad") ? _domi.GetAttribute("Localidad").ToString() : "";
                                                                                    _municipioOrigen = _domi.HasAttribute("Municipio") ? _domi.GetAttribute("Municipio").ToString() : "";
                                                                                    _estadoOrigen = _domi.HasAttribute("Estado") ? _domi.GetAttribute("Estado").ToString() : "";
                                                                                    _paisOrigen = _domi.HasAttribute("Pais") ? _domi.GetAttribute("Pais").ToString() : "";
                                                                                    _cpOrigen = _domi.HasAttribute("CodigoPostal") ? _domi.GetAttribute("CodigoPostal").ToString() : "";

                                                                                    _direccionOrigen = _calleOrigen + ", " + _coloniaOrigen + ", " + _localidadOrigen + ", " + _municipioOrigen + ", " + _estadoOrigen + ", " + _paisOrigen + ", " + _cpOrigen;
                                                                                }


                                                                            }
                                                                            else if (tipo_Ubicacion == "Destino")
                                                                            {
                                                                                _idUbicacionDestino = _ubicacion.HasAttribute("IDUbicacion") ? _ubicacion.GetAttribute("IDUbicacion").ToString() : "";
                                                                                _rfcRemDesDestino = _ubicacion.HasAttribute("RFCRemitenteDestinatario") ? _ubicacion.GetAttribute("RFCRemitenteDestinatario").ToString() : "";
                                                                                _nombreRemDesDestino = _ubicacion.HasAttribute("NombreRemitenteDestinatario") ? _ubicacion.GetAttribute("NombreRemitenteDestinatario").ToString() : "";
                                                                                _fechaHoraDestino = _ubicacion.HasAttribute("FechaHoraSalidaLlegada") ? DateTime.Parse(_ubicacion.GetAttribute("FechaHoraSalidaLlegada").ToString()) : DateTime.Now;
                                                                                _distanciaRecorridaDestino = _ubicacion.HasAttribute("DistanciaRecorrida") ? _ubicacion.GetAttribute("DistanciaRecorrida").ToString() : "";



                                                                                var _domicilio = _ubicacion.FirstChild;
                                                                                if (_domicilio.GetType() == typeof(XmlElement))
                                                                                {
                                                                                    var _domi = (_domicilio as XmlElement);

                                                                                    _calleDestino = _domi.HasAttribute("Calle") ? _domi.GetAttribute("Calle").ToString() : "";
                                                                                    _coloniaDestino = _domi.HasAttribute("Colonia") ? _domi.GetAttribute("Colonia").ToString() : "";
                                                                                    _localidadDestino = _domi.HasAttribute("Localidad") ? _domi.GetAttribute("Localidad").ToString() : "";
                                                                                    _municipioDestino = _domi.HasAttribute("Municipio") ? _domi.GetAttribute("Municipio").ToString() : "";
                                                                                    _estadoDestino = _domi.HasAttribute("Estado") ? _domi.GetAttribute("Estado").ToString() : "";
                                                                                    _paisDestino = _domi.HasAttribute("Pais") ? _domi.GetAttribute("Pais").ToString() : "";
                                                                                    _cpDestino = _domi.HasAttribute("CodigoPostal") ? _domi.GetAttribute("CodigoPostal").ToString() : "";

                                                                                    _direccionDestino = _calleDestino + ", " + _coloniaDestino + ", " + _localidadDestino + ", " + _municipioDestino + ", " + _estadoDestino + ", " + _paisDestino + ", " + _cpDestino;
                                                                                }
                                                                            }

                                                                        }
                                                                    }

                                                                }
                                                                else if (_cartaPorte.LocalName == "Mercancias")
                                                                {
                                                                    _pesoBrutoTotal = _cartaPorte.HasAttribute("PesoBrutoTotal") ? _cartaPorte.GetAttribute("PesoBrutoTotal").ToString() : "";
                                                                    _unidadPeso = _cartaPorte.HasAttribute("UnidadPeso") ? _cartaPorte.GetAttribute("UnidadPeso").ToString() : "";
                                                                    _numTotalMercancias = _cartaPorte.HasAttribute("NumTotalMercancias") ? _cartaPorte.GetAttribute("NumTotalMercancias").ToString() : "";



                                                                    foreach (var Mercancias in _cartaPorte.ChildNodes)
                                                                    {
                                                                        if (Mercancias.GetType() == typeof(XmlElement))
                                                                        {
                                                                            var _mercancias = (Mercancias as XmlElement);
                                                                            if (_mercancias.LocalName == "Mercancia")
                                                                            {
                                                                                _bienesTrans = _mercancias.HasAttribute("BienesTransp") ? _mercancias.GetAttribute("BienesTransp").ToString() : "";
                                                                                _descripcionMerca = _mercancias.HasAttribute("Descripcion") ? _mercancias.GetAttribute("Descripcion").ToString() : "";
                                                                                _cantidadMerca = _mercancias.HasAttribute("Cantidad") ? _mercancias.GetAttribute("Cantidad").ToString() : "";
                                                                                _claveUnidadMerca = _mercancias.HasAttribute("ClaveUnidad") ? _mercancias.GetAttribute("ClaveUnidad").ToString() : "";
                                                                                _unidadMerca = _mercancias.HasAttribute("Unidad") ? _mercancias.GetAttribute("Unidad").ToString() : "";
                                                                                _pesoKG = _mercancias.HasAttribute("PesoEnKg") ? _mercancias.GetAttribute("PesoEnKg").ToString() : "";



                                                                            }
                                                                            else if (_mercancias.LocalName == "Autotransporte")
                                                                            {
                                                                                _permSCT = _mercancias.HasAttribute("PermSCT") ? _mercancias.GetAttribute("PermSCT").ToString() : "";
                                                                                _numPermisoSCT = _mercancias.HasAttribute("NumPermisoSCT") ? _mercancias.GetAttribute("NumPermisoSCT").ToString() : "";


                                                                                foreach (var AutoTransportes in _mercancias.ChildNodes)
                                                                                {
                                                                                    if (AutoTransportes.GetType() == typeof(XmlElement))
                                                                                    {
                                                                                        var _autoTransporte = (AutoTransportes as XmlElement);
                                                                                        if (_autoTransporte.LocalName == "IdentificacionVehicular")
                                                                                        {
                                                                                            _configVehivular = _autoTransporte.HasAttribute("ConfigVehicular") ? _autoTransporte.GetAttribute("ConfigVehicular").ToString() : "";
                                                                                            _placaVM = _autoTransporte.HasAttribute("PlacaVM") ? _autoTransporte.GetAttribute("PlacaVM").ToString() : "";
                                                                                            _anioModelo = _autoTransporte.HasAttribute("AnioModeloVM") ? _autoTransporte.GetAttribute("AnioModeloVM").ToString() : "";


                                                                                        }
                                                                                        else if (_autoTransporte.LocalName == "Seguros")
                                                                                        {
                                                                                            _aseguraRespCivil = _autoTransporte.HasAttribute("AseguraRespCivil") ? _autoTransporte.GetAttribute("AseguraRespCivil").ToString() : "";
                                                                                            _polizaRespCivil = _autoTransporte.HasAttribute("PolizaRespCivil") ? _autoTransporte.GetAttribute("PolizaRespCivil").ToString() : "";


                                                                                        }
                                                                                        else if (_autoTransporte.LocalName == "Remolques")
                                                                                        {
                                                                                            foreach (var Remolques in _autoTransporte.ChildNodes)
                                                                                            {
                                                                                                if (Remolques.GetType() == typeof(XmlElement))
                                                                                                {
                                                                                                    var _remolque = (Remolques as XmlElement);
                                                                                                    if (_remolque.LocalName == "Remolque")
                                                                                                    {
                                                                                                        _subTipoRem = _remolque.HasAttribute("SubTipoRem") ? _remolque.GetAttribute("SubTipoRem").ToString() : "";
                                                                                                        _placaRemolque = _remolque.HasAttribute("Placa") ? _remolque.GetAttribute("Placa").ToString() : "";


                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                else if (_cartaPorte.LocalName == "FiguraTransporte")
                                                                {
                                                                    foreach (var FiguraTrans in _cartaPorte.ChildNodes)
                                                                    {
                                                                        if (FiguraTrans.GetType() == typeof(XmlElement))
                                                                        {
                                                                            var _tiposFigura = (FiguraTrans as XmlElement);
                                                                            if (_tiposFigura.LocalName == "TiposFigura")
                                                                            {
                                                                                _tipoFigura = _tiposFigura.HasAttribute("TipoFigura") ? _tiposFigura.GetAttribute("TipoFigura").ToString() : "";
                                                                                _rfcFigura = _tiposFigura.HasAttribute("RFCFigura") ? _tiposFigura.GetAttribute("RFCFigura").ToString() : "";
                                                                                _numLicenciaFigura = _tiposFigura.HasAttribute("NumLicencia") ? _tiposFigura.GetAttribute("NumLicencia").ToString() : "";
                                                                                _nombreFigura = _tiposFigura.HasAttribute("NombreFigura") ? _tiposFigura.GetAttribute("NombreFigura").ToString() : "";


                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
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

                                        else if (_nodo.LocalName == "Conceptos")
                                        {
                                            foreach (var Conceptos in _nodo.ChildNodes)
                                            {
                                                if (Conceptos.GetType() == typeof(XmlElement))
                                                {
                                                    var _concepto = (Conceptos as XmlElement);
                                                    if (_concepto.LocalName == "Concepto")
                                                    {
                                                        tbd_Conceptos_Factura _conceptoFactura = new tbd_Conceptos_Factura();
                                                        _conceptoFactura.c_pord_serv = _concepto.HasAttribute("ClaveProdServ") ? _concepto.GetAttribute("ClaveProdServ").ToString() : "";
                                                        _conceptoFactura.cantidad = _concepto.HasAttribute("Cantidad") ? Convert.ToDecimal(_concepto.GetAttribute("Cantidad").ToString()) : 0;
                                                        _conceptoFactura.c_unidad = _concepto.HasAttribute("ClaveUnidad") ? _concepto.GetAttribute("ClaveUnidad").ToString() : "";
                                                        _conceptoFactura.unidad = _concepto.HasAttribute("Unidad") ? _concepto.GetAttribute("Unidad").ToString() : "";
                                                        _conceptoFactura.descripcion = _concepto.HasAttribute("Descripcion") ? _concepto.GetAttribute("Descripcion").ToString() : "";
                                                        _conceptoFactura.valor_unitario = _concepto.HasAttribute("ValorUnitario") ? Convert.ToDecimal(_concepto.GetAttribute("ValorUnitario").ToString()) : 0;
                                                        _conceptoFactura.importe = _concepto.HasAttribute("Importe") ? Convert.ToDecimal(_concepto.GetAttribute("Importe").ToString()) : 0;
                                                        _conceptoFactura.descuento = _concepto.HasAttribute("Descuento") ? Convert.ToDecimal(_concepto.GetAttribute("Descuento").ToString()) : 0;

                                                        conceptos.Add(_conceptoFactura);
                                                    }
                                                }
                                            }                                            
                                        }
                                    }
                                }

                                //Condicion                                
                                tbd_Facturas factura = db.tbd_Facturas.Where(s => s.uuid == _uuid).SingleOrDefault();
                                if (factura == null)
                                {
                                    tbd_Facturas nuevaFactura = new tbd_Facturas();
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
                                    nuevaFactura.certificado_sat = _certificadoSAT;
                                    nuevaFactura.sello_cfdi = _selloDigital;
                                    nuevaFactura.sello_sat = _selloSAT;

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
                                    //------------------------------ Buscar url -----------------------------
                                    var url_xml_ = 0;
                                    var r_xml = "";
                                    url_xml_ = db.tbd_Cfdi_Uuid.Where(s => s.uuid == _uuid).Select(u => u.id_pre_factura).SingleOrDefault();
                                    if (url_xml_ == 0)
                                    {
                                        url_xml_= db.tbd_Pre_Pagos.Where(s => s.uuid == _uuid).Select(u => u.id_pre_factura).SingleOrDefault();
                                    }
                                    r_xml = db.tbd_Pre_Factura.Where(s => s.id_pre_factura == url_xml_).Select(u => u.url_xml).SingleOrDefault();
                                    if (r_xml == null)
                                    {
                                        r_xml = db.tbd_Pre_Pagos.Where(s => s.id_pre_factura == url_xml_).Select(u => u.url_xml).SingleOrDefault();
                                    }
                                    //----------------------------------------------------------------------------------------------------------------------------- 
                                    String Url_Almacen = HttpContext.Current.Server.MapPath("~") +"\\"+ r_xml;//variable.url_facturas + DirectoryFecha;
                                    if (!Directory.Exists(Url_Almacen)) 
                                        Directory.CreateDirectory(Url_Almacen); //! Directorio Por Fecha (yyyyMMdd) dependiendo de la fecha de timbrado

                                    String Url_XML = Url_Almacen;//+ _uuid + ".xml";

                                    nuevaFactura.url_xml = (DirectoryFecha + _uuid + ".xml").ToUpper();
                                    nuevaFactura.fecha_creacion = DateTime.Now;
                                    nuevaFactura.id_estatus = 5; //Vigente
                                    nuevaFactura.fecha_validacion = DateTime.Now;

                                    if (nuevaFactura.rfc_emisor == rfc || nuevaFactura.rfc_receptor == rfc)
                                    {
                                        try
                                        {
                                            db.tbd_Facturas.Add(nuevaFactura);
                                            db.SaveChanges();

                                            foreach (var item in conceptos)
                                            {
                                                item.id_factura = nuevaFactura.id_factura;
                                            }

                                            db.tbd_Conceptos_Factura.AddRange(conceptos);
                                            db.SaveChanges();

                                            if (!File.Exists(Url_XML))
                                            {
                                                file.SaveAs(Url_XML);
                                            }

                                            //!Carta porte
                                            if (_tipoComprobante == "T")
                                            {
                                                tbd_Carta_Porte carta = new tbd_Carta_Porte();
                                                carta.trans_inter = _transpInter;
                                                carta.total_dist_rec = Convert.ToDecimal(_totalDistancia);
                                                carta.id_origen = _idUbicacionOrigen;
                                                carta.rfc_origen = _rfcRemDesOrigen;
                                                carta.nombre_razon_origen = _nombreRemDesOrigen;
                                                carta.fecha_salida = _fechaHoraOrigen;
                                                carta.direccion_origen = _direccionOrigen;
                                                carta.id_destino = _idUbicacionDestino;
                                                carta.rfc_destino = _rfcRemDesDestino;
                                                carta.nombre_razon_destino = _nombreRemDesDestino;
                                                carta.distancia_recorrida = Convert.ToDecimal(_distanciaRecorridaDestino);
                                                carta.fecha_llegada = _fechaHoraDestino;
                                                carta.direccion_destino = _direccionDestino;
                                                carta.peso_bruto = Convert.ToDecimal(_pesoBrutoTotal);
                                                carta.unidad_peso = _unidadPeso;
                                                carta.num_mercancias = _numTotalMercancias;
                                                carta.bienes_trans = _bienesTrans;
                                                carta.descripcion = _descripcionMerca;
                                                carta.cantidad = Convert.ToDecimal(_cantidadMerca);
                                                carta.clave_unidad = _claveUnidadMerca;
                                                carta.unidad = _unidadMerca;
                                                carta.peso_kg = Convert.ToDecimal(_pesoKG);
                                                carta.permiso_sct = _permSCT;
                                                carta.num_permiso_sct = _numPermisoSCT;
                                                carta.config_vehicular = _configVehivular;
                                                carta.placa = _placaVM;
                                                carta.modelo = _anioModelo;
                                                carta.asegura_resp_civil = _aseguraRespCivil;
                                                carta.poliza_resp_civil = _polizaRespCivil;
                                                carta.sub_tipo_remolque = _subTipoRem;
                                                carta.placa_remolque = _placaRemolque;
                                                carta.tipo_figura = _tipoFigura;
                                                carta.rfc_figura = _rfcFigura;
                                                carta.num_licencia = _numLicenciaFigura;
                                                carta.nomnre_figura = _nombreFigura;
                                                carta.id_factura = nuevaFactura.id_factura;
                                                carta.url_pdf = "";
                                                db.tbd_Carta_Porte.Add(carta);
                                                db.SaveChanges();
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            tbd_Log_Errores error = new tbd_Log_Errores();
                                            error.fecha = DateTime.Now;
                                            error.funcion = "LecturaXML";
                                            error.mensaje = ex.Message + "[" + file.FileName + "]";
                                            db.tbd_Log_Errores.Add(error);
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    tbd_Log_Errores error = new tbd_Log_Errores();
                    error.fecha = DateTime.Now;
                    error.funcion = "LecturaXML";
                    error.mensaje = ex.Message + "[" + file.FileName + "]";
                    db.tbd_Log_Errores.Add(error);
                    db.SaveChanges();
                }
            }
        }
    }
}