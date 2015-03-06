using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using System.IO;
using System.Text.RegularExpressions;

namespace helloJkw.Modules.Lucia
{
	public class ProductInfo
	{
		LuciaDirInfo _dirInfo;
		Dictionary<string, string> InfoDic = new Dictionary<string, string>();

		public string Name { get; set; }
		public int Price { get; set; }
		public int? SalePrice { get; set; }
		public string MainContent { get; set; }
		public string MainImage
		{
			get
			{
				var ImageExtensionList = new List<string>(){ "png", "jpg", "jpeg", "gif" };
				ImageExtensionList = ImageExtensionList.Select(e => ".{0}".With(e)).ToList();
				var ImageList = _dirInfo.GetFiles()
					.Where(e => ImageExtensionList.Contains(Path.GetExtension(e.FullName).ToLower()))
					.Select(e => e.FullName.RegexReplace(@"\\", "/"))
					.Select(e => e.Substring(e.IndexOf("/{0}/".With(LuciaStatic.RootPath))))
					.ToList();

				var test = _dirInfo.GetFiles()
					.Select(e => Path.GetExtension(e.FullName))
					.ToList();

				string mainImage = "http://placehold.it/320x120";
				if (!ImageList.Any()) return mainImage;

				var MainImageList = ImageList.Where(e => e.Contains("대표"));
				if (MainImageList.Any())
					mainImage = MainImageList.ElementAt(StaticRandom.Next(MainImageList.Count()));
				mainImage = ImageList.ElementAt(StaticRandom.Next(ImageList.Count()));

				return mainImage;
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
			if (productInfoPath == null) return;

			string currentKey = null;
			foreach (var line in File.ReadAllLines(productInfoPath, Encoding.Default))
			{
				if (line.Contains('='))
				{
					var splitted= line.Split('=');
					currentKey = splitted[0].Trim();
					string value = "";
					if (splitted.Count() > 1)
						value = splitted[1];
					InfoDic.Add(currentKey, value);
				}
				else
				{
					InfoDic[currentKey] = (InfoDic[currentKey] += Environment.NewLine + line).Trim();
				}
			}

			var dic = InfoDic.ToDefaultDictionary();
			Price = int.Parse(dic["가격"]);
			MainContent = dic["대표설명"];
		}

		public ExpandoObject ToExpando()
		{
			IDictionary<string, object> expando = new ExpandoObject();
			var type = this.GetType();
			foreach (var field in type.GetFields())
				expando.Add(field.Name, field.GetValue(this));
			foreach (var properity in type.GetProperties())
				expando.Add(properity.Name, properity.GetValue(this));

			return (ExpandoObject)expando;
		}
	}

}
