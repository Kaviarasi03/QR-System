using QRCoder;
using System.Drawing;
using System.IO;
namespace Login.Helpers
{
    public class QRCodeHelper
    {
        public static byte[] GenerateQRCode(string data)
        {
            using(QRCodeGenerator qrGenerator= new QRCodeGenerator())
            {
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);

                PngByteQRCode qrCode = new PngByteQRCode(qrCodeData); // ✅ Correct way to generate QR

                return qrCode.GetGraphic(20);

            }
        }
    }
}
