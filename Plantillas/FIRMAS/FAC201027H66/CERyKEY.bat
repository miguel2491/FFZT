C:\OpenSSL\bin\openssl.exe x509 -inform DER -outform PEM -in C:\Users\Desarrollo\source\repos\Facturafast\Plantillas\Firmas\FAC201027H66\8f54f441-3362-4713-827c-9436c31f33e0.cer -pubkey -out C:\Users\Desarrollo\source\repos\Facturafast\\Plantillas\Firmas\FAC201027H66\8f54f441-3362-4713-827c-9436c31f33e0.cer.pem
C:\OpenSSL\bin\openssl.exe pkcs8 -inform DER -in C:\Users\Desarrollo\source\repos\Facturafast\Plantillas\Firmas\FAC201027H66\2d002c82-7049-4019-888d-f6be71b77922.key -passin pass:HUEXOTITLA2021 -out C:\Users\Desarrollo\source\repos\Facturafast\\Plantillas\Firmas\FAC201027H66\2d002c82-7049-4019-888d-f6be71b77922.key.pem
C:\OpenSSL\bin\openssl.exe rsa -in C:\Users\Desarrollo\source\repos\Facturafast\\Plantillas\Firmas\FAC201027H66\2d002c82-7049-4019-888d-f6be71b77922.key.pem -des3 -out C:\Users\Desarrollo\source\repos\Facturafast\\Plantillas\Firmas\FAC201027H66\2d002c82-7049-4019-888d-f6be71b77922.key.enc -passout pass:F4ctur4f4st_C@st3l4n