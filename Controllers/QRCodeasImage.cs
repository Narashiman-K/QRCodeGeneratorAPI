using GdPicture14;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.IO;

namespace QRCodeGeneratorAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QRCodeasImage : ControllerBase
    {
        [HttpGet("generate")]
        public IActionResult GenerateQRCode(
            [FromQuery] string qrCodeText = "Accenture QR Code",
            [FromQuery] int size = 200) // QR code image size in pixels
        {
            using (GdPictureImaging gdpictureImaging = new GdPictureImaging())
            {
                // Define colors using ARGB (Avoids System.Drawing)
                Color backColor =  gdpictureImaging.ARGB(255, 255, 255, 255); // White Background
                Color qrColor = gdpictureImaging.ARGB(255, 0, 0, 0); // Black QR Code
                
                // Create a blank image
                int qrImageId = gdpictureImaging.CreateNewGdPictureImage(size, size, 32, backColor);

                if (qrImageId == 0)
                {
                    return BadRequest("Failed to create QR code image.");
                }

                // Generate the QR Code using BarcodeQRWrite
                GdPictureStatus status = gdpictureImaging.BarcodeQRWrite(
                    qrImageId,
                    qrCodeText,
                    BarcodeQREncodingMode.BarcodeQREncodingModeUndefined,
                    BarcodeQRErrorCorrectionLevel.BarcodeQRErrorCorrectionLevelM,
                    0,  // Auto Version
                    4,  // Quiet Zone
                    5,  // Module Size
                    10, // X Position
                    10, // Y Position
                    0,  // No Rotation
                    qrColor,  // QR Code Color (Black)
                    backColor  // Background Color (White)
                );

                if (status != GdPictureStatus.OK)
                {
                    gdpictureImaging.ReleaseGdPictureImage(qrImageId);
                    return BadRequest("Failed to generate QR code.");
                }

                // Save QR code to a temporary file
                string tempFilePath = Path.GetTempFileName() + ".png";
                gdpictureImaging.SaveAsPNG(qrImageId, tempFilePath);
                gdpictureImaging.ReleaseGdPictureImage(qrImageId);

                // Read the file into a MemoryStream and return as a response
                byte[] imageBytes = System.IO.File.ReadAllBytes(tempFilePath);
                System.IO.File.Delete(tempFilePath); // Clean up

                return File(imageBytes, "image/png", "QRCode.png");
            }
        }
    }
}
