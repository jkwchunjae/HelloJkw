using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;

namespace helloJkw
{
	public static class ImageResizer
	{
		private static string _sourceFolder;
		private static string _targetFolder;
		private static double _ratio; // 그림 확대/축소 비율

		public static void SyncImages(string sourcePath, string sourceFolder, string targetFolder, double ratio = 1)
		{
			_sourceFolder = sourceFolder;
			_targetFolder = targetFolder;
			_ratio = ratio;
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
			foreach (var sourceFile in Directory.GetFiles(sourcePath))
			{
				var targetFile = sourceFile.MakeTargetPath(_sourceFolder, _targetFolder);
				SyncImage(sourceFile, targetFile, ratio: _ratio);
			}
			#endregion
		}

		private static void SyncImage(string sourceFile, string targetFile, double ratio = 1)
		{
			try
			{
				var ext = Path.GetExtension(sourceFile).ToLower();
				var ImageExtensionList = new List<string>() { ".png", ".jpg", ".jpeg", ".gif" };
				if (!ImageExtensionList.Contains(ext)) return;
				//if (File.Exists(targetFile)) File.Delete(targetFile);
				if (File.Exists(targetFile)) return;
				sourceFile.ResizeAndSave(targetFile, ratio: _ratio);
			}
			catch { }
		}

		public static void ResizeAndSave(this string sourceFile, string targetFile, double ratio = 1)
		{
			try
			{
				using (var imageFactory = new ImageFactory(preserveExifData: true))
				{
					var sourceImage = imageFactory.Load(sourceFile);

					#region format
					ISupportedImageFormat format = null;
					var ext = Path.GetExtension(sourceFile).ToLower();
					if (ext == ".jpg" || ext == ".jpeg")
						format = new JpegFormat { Quality = 100 };
					if (ext == ".png")
						format = new PngFormat { Quality = 100 };
					if (ext == ".gif")
						format = new GifFormat { Quality = 100 };
					#endregion

					#region size
					var width = sourceImage.Image.Width;
					var height = sourceImage.Image.Height;
					var size = new Size((int)(width * ratio), (int)(height * ratio));
					#endregion

					sourceImage
						.Resize(size)
						.Format(format)
						.Save(targetFile);
				}
			}
			catch { }
		}

		private static string MakeTargetPath(this string sourcePath, string find, string replace)
		{
			return sourcePath.Replace(find, replace).Replace(@"\", "/");
		}
	}
}
