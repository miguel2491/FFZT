using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Facturafast.Models
{
    public class PFX
    {
        public PFXResponse CreaPFX(string RutaCer, string RutaKey, string Contraseña, string UrlPEM, string UrlPFX)
        {

            var RutaRelativaPEM = Path.Combine(UrlPEM, Guid.NewGuid() + ".PEM");
            var nombrePFX = Guid.NewGuid() + ".PFX";
            var RutaRelativaPFX = Path.Combine(UrlPFX, nombrePFX);

            Chilkat.Cert cert = new Chilkat.Cert();
            Chilkat.PrivateKey privKey = new Chilkat.PrivateKey();

            // Carga el Certificado 
            bool success = cert.LoadFromFile(RutaCer);
            if (success != true)
            {
                return new PFXResponse { Mensaje = "No se pudo cargar el certificado ingresado", Estatus = 0 };
            }

            // Carga la llave privada.
            success = privKey.LoadPkcs8EncryptedFile(RutaKey, Contraseña);
            if (success != true)
            {
                return new PFXResponse { Mensaje = "No se pudo cargar la llave ingresada, una de las razones puede ser a causa de una contraseña incorrecta.", Estatus = 0 };
            }

            // Escribe el certificado a formato PEM
            success = cert.ExportCertPemFile(RutaRelativaPEM);

            // Asocia la llave privada con el certificado
            success = cert.SetPrivateKey(privKey);
            if (success != true)
            {
                return new PFXResponse { Mensaje = "No se pudo generar el archivo .PEM, una de las causas es que los archivos cargados no son de FIEL.", Estatus = 0 };
            }
            // Escribe el cert + private key a .pfx file.
            success = cert.ExportToPfxFile(RutaRelativaPFX, Contraseña, true);

            if (success != true)
            {
                return new PFXResponse { Mensaje = "No se pudo generar el archivo .PFX", Estatus = 0 };
            }
            return new PFXResponse { Mensaje = "El archivo PFX se ha creado de forma exitosa en la carpeta definida, la contraseña es la misma definida en la llave de la FIEL, para iniciar la descarga cargue el PFX generado", Estatus = 1 , URL = nombrePFX };            
        }
    }
}