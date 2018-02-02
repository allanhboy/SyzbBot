using System;
using System.DrawingCore;
using QRCoder;

namespace WxBot
{
	public static class Common
	{
		public static long ConvertDateTimeToInt(DateTime time)
		{
			var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
			var t = (long)(time - startTime).TotalMilliseconds;
			return t;
		}

		public static Bitmap GenerateQRCode(string text)
		{
			QRCodeGenerator qrGenerator = new QRCodeGenerator();
			QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
			QRCode qrCode = new QRCode(qrCodeData);
			Bitmap qrCodeImage = qrCode.GetGraphic(20);

			return qrCodeImage;
		}
	}
}