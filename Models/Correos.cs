using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using Facturafast.Models;

namespace Facturafast.Models
{
    public class Correos
    {
        public void emailEnvioNotaVenta(ListaCorreos correo, Int32 idNota, tbc_Usuarios usuario, string ruta)
        {
            BD_FFEntities db = new BD_FFEntities();
            tbd_Notas_Venta nota = db.tbd_Notas_Venta.Where(s => s.id_nota_venta == idNota).Single();
            tbc_Variables_Calculo variables = db.tbc_Variables_Calculo.Single();
            tbd_Envios_Correo_Nota nuevoCorreo = new tbd_Envios_Correo_Nota();

            String url = "https://castelanauditores.com/FFDemo/img/cuentas/";

            nuevoCorreo.id_cliente = nota.id_cliente;
            nuevoCorreo.id_nota_venta = idNota;
            nuevoCorreo.id_usuario = usuario.id_usuario;
            nuevoCorreo.fecha_enviado = DateTime.Now;
            nuevoCorreo.correo = correo.correo;
            nuevoCorreo.enviado = true;
            nuevoCorreo.mensaje = "El correo se envió correctamente.";
            nuevoCorreo.nombre = correo.nombre;
            nuevoCorreo.rfc_usuario = usuario.rfc;            

            String cuerpo =
                @"<center>
                <style>.formEmail{font-family:'Open Sans',sans-serif;width:750px;text-align:center;}.formBorder{width:100%;height:30px;background-color:rgb(0,33,96);}</style>
                <div class='formEmail'>
                    <div class='formBorder'></div>
                    <table style='border-collapse:collapse; width:100%;'>
                        <tr>                            
                            <td style='padding:20px;text-align:center;'>
                                <h2 style='font-weight:bold;'>Apreciable</h2>
                                <h3 style='font-weight:bold;'>" + correo.nombre_razon + @"</h3>
                                <h4 style='font-weight:bold;'>" + correo.rfc + @"</h4>                             
                                <p>Es un gusto para mi poder saludar y reiterarme a sus órdenes!</p>
                                <p>Me permito extender la presente invitación a realizar el pago mensual de su contabilidad de manera puntual y constante del 01 al 10 de cada mes, y con ello nos permita seguir otorgando los servicios y requerimientos que el SAT solicite en tiempo y forma debida.</p>
                                <p>Reitero nuevamente nuestro agradecimiento, quedando a sus órdenes.</p><br /><br />
                                <table style='border-collapse:collapse; width:100%;'>" +
                                    @"<tr>                                        
                                        <td style='color:red;text-align:center;'>
                                            <strong>" + (nota.serie + "-" + nota.folio) + @"</strong>
                                        </td>
                                    </tr>
                                    <tr>                                        
                                        <td style='text-align:center;'>
                                            " + String.Join(", ", db.tbd_Conceptos_Nota_Venta.Where(s=> s.id_nota_venta == idNota).Select(s=> s.concepto).ToList()) + @"
                                        </td>
                                    </tr>
                                    <tr>
                                         <td style='text-align:center;'>
                                            <strong>Total: " + nota.total.ToString("c") + @" </strong>
                                        </td>                                        
                                    </tr>
                                    <tr>                                        
                                        <td style='text-align:center;'>
                                            " + nota.total.NumeroALetras() + @"
                                        </td>
                                    </tr>
                                </table>
                                <br>
                                <img style='height:180px;' src='"+ (url + (nota.id_cuenta_bancaria == 1 ? "banorte.jpg": "hsbc.jpg")) +@"' />
                            </td>
                        </tr>
                    </table>
                    <br /><br />
                    <p>&copy; 2022 <strong>CASTELÁN AUDITORES S.C.</strong></p>
                    <div class='formBorder'></div>
                </div>
                </center>";

            try
            {
                //string email = "contabilidad@consultoriacastelan.com";
                string email = "cobranza@consultoriacastelan.com";

                MailMessage msg = new MailMessage();
                string DireccionaEnviar = correo.correo;
                msg.To.Add(DireccionaEnviar);
                msg.From = new MailAddress(email, "CASTELÁN AUDITORES S.C.", System.Text.Encoding.UTF8);
                //msg.From = new MailAddress("comunicados@facturafast.mx", "FACTURAFAST ", System.Text.Encoding.UTF8);

                msg.Subject = "Honorario Mensual Contable.";
                msg.SubjectEncoding = System.Text.Encoding.UTF8;
                msg.Body = cuerpo;
                /* Archivo adjunto */
                
                string fullPath = ruta +@"Plantillas/"+nota.url_pdf;
                string fullPathXML = ruta + @"Plantillas/" + nota.url_xml;
                Attachment data = new Attachment(fullPath, MediaTypeNames.Application.Pdf);
                Attachment dataXML = new Attachment(fullPathXML);
                msg.Attachments.Add(data);
                msg.Attachments.Add(dataXML);
                /*******/
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.IsBodyHtml = true;

                SmtpClient client = new SmtpClient();
                //client.Credentials = new NetworkCredential(email, "29tR#+54thfq");
                client.Credentials = new NetworkCredential(email, "C0nsultor1a*128");

                client.Port = 587;
                client.Host = "mail.consultoriacastelan.com";
                client.EnableSsl = false;
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chai, SslPolicyErrors sslPolicyErrors)
                { return true; };
                
                client.Send(msg);
                db.tbd_Envios_Correo_Nota.Add(nuevoCorreo);
                db.SaveChanges();

            }
            catch (Exception ex)
            {
                nuevoCorreo.enviado = false;
                nuevoCorreo.mensaje = ex.Message;
                db.tbd_Envios_Correo_Nota.Add(nuevoCorreo);
                db.SaveChanges();
            }
            finally
            {
                GC.Collect();
            }
        }
    }
}