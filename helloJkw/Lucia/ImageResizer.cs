using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace helloJkw
{
	public static class ImageResizer
	{
		private static string _sourceFolder;
		private static string _targetFolder;
		private static int _optimalWidth;
		private static int _optimalHeight;

		public static void SyncImages(string sourcePath, string sourceFolder, string targetFolder, int optimalWidth, int optimalHeight)
		{
			_sourceFolder = sourceFolder;
			_targetFolder = targetFolder;
			_optimalWidth = optimalWidth;
			_optimalHeight = optimalHeight;
			var targetPath = sourcePath.MakeTargetPath(_sourceFolder, _targetFolder);
			SyncDir(sourcePath, targetPath);
		}

		private static void SyncDir(string sourcePath, string targetPath)
		{
			if (!Directory.Exists(sourcePath)) return;
			if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

			#region Sync Diectory
			foreach (var nextSourcePath in Directory.GetDirectories(sourcePath))
			{
				var nextTargetPath = nextSourcePath.MakeTargetPath(_sourceFolder, _targetFolder);
				SyncDir(nextSourcePath, nextTargetPath);
			}
			#endregion

			#region Sync ImageFile
			Parallel.ForEach(Directory.GetFiles(sourcePath), sourceFile =>
			{
				var targetFile = sourceFile.MakeTargetPath(_sourceFolder, _targetFolder);
				SyncImage(sourceFile, targetFile);
			});
			#endregion
		}

		private static void SyncImage(string sourceFile, string targetFile)
		{
			try
			{
				var ext = Path.GetExtension(sourceFile).ToLower();
				var ImageExtensionList = new List<string>() { ".png", ".jpg", ".jpeg", ".gif" };
				if (!ImageExtensionList.Contains(ext)) return;
				//if (File.Exists(targetFile)) File.Delete(targetFile);
				if (File.Exists(targetFile)) return;
				sourceFile.ResizeAndSave(targetFile, _optimalWidth, _optimalHeight);
			}
			catch { }
		}

		public static void ResizeAndSave(this string sourceFile, string targetFile, int optimalWidth, int optimalHeight)
		{
			var sourceImage = new Bitmap(sourceFile);

			var width = sourceImage.Width;
			var height = sourceImage.Height;
			double ratio = (width >= height) ? (double)optimalWidth / width : (double)optimalHeight / height;

			int newWidth = (int)(width * ratio);
			int newHeight = (int)(height * ratio);

			var newImage = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppRgb);

			using (var graphics = Graphics.FromImage(newImage))
			{
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
				graphics.SmoothingMode = SmoothingMode.HighQuality;
				graphics.DrawImage(sourceImage, 0, 0, newWidth, newHeight);
			}

			var imageCodecInfo = ImageCodecInfo.GetImageDecoders().Where(e => e.FormatID == ImageFormat.Jpeg.Guid).FirstOrDefault();

			var encoder = System.Drawing.Imaging.Encoder.Quality;
			var encoderParameters = new EncoderParameters(1);
			encoderParameters.Param[0] = new EncoderParameter(encoder, 100L);
			newImage.Save(targetFile, imageCodecInfo, encoderParameters);
			newImage.Dispose();
			sourceImage.Dispose();
		}

		private static string MakeTargetPath(this string sourcePath, string find, string replace)
		{
			return sourcePath.Replace(find, replace).Replace(@"\", "/");
		}
	}
}
