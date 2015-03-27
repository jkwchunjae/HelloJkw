using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Extensions;
using helloJkw.Extensions;

namespace helloJkw.Lucia
{
	public class ProductInfo
	{
		LuciaDirInfo _dirInfo;
		Dictionary<string, string> InfoDic = new Dictionary<string, string>();

		public string Name { get; set; }
		public int Price { get; set; }
		public int? SalePrice { get; set; }
		public string[] MainContent { get; set; }
		public string MainImage
		{
			get
			{
				string mainImage = "http://placehold.it/320x120";
				if (!ImageList.Any()) return mainImage;

				var MainImageList = ImageList.Where(e => e.Contains("대표"));
				if (MainImageList.Any())
					return MainImageList.ElementAt(StaticRandom.Next(MainImageList.Count()));
				mainImage = ImageList.ElementAt(StaticRandom.Next(ImageList.Count()));

				return mainImage;
			}
		}
		public IEnumerable<string> ImageList
		{
			get
			{
				var ImageExtensionList = new List<string>(){ ".png", ".jpg", ".jpeg", ".gif" };
				var ImageList = _dirInfo.GetFiles()
					.Where(e => ImageExtensionList.Contains(Path.GetExtension(e.FullName).ToLower()))
					.Select(e => e.FullName.RegexReplace(@"\\", "/"))
					.Select(e => e.Substring(e.IndexOf("/{0}/".With(LuciaStatic.RootPath))))
					.Select(e => e.Replace("/{0}/".With(LuciaStatic.RootPath), ""));
				return ImageList;
			}
		}

		public ProductInfo() { }

		public ProductInfo(LuciaDirInfo dirInfo)
		{
			_dirInfo = dirInfo;
			Name = dirInfo.FolderName;

			string productInfoPath = dirInfo.GetFiles().Where(e => e.Name == "정보.txt")
				.Select(e => e.FullName)
				.DefaultIfEmpty(null)
				.FirstOrDefault();

			SetInfoText(productInfoPath);
		}

		private void SetInfoText(string productInfoPath)
		{
			if (productInfoPath == null)
			{
				MainContent = new string[] { };
				return;
			}

			InfoDic = productInfoPath.InfoToDictionary();
			var dic = InfoDic.ToDefaultDictionary("");
			Price = int.Parse(dic["가격"]);
			MainContent = dic["대표설명"].RegexReplace(@"\r", "").Split('\n');
		}
	}

}
