using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Extensions;

namespace helloJkw
{
	public class ProductInfo
	{
		LuciaDirInfo _dirInfo;
		public Dictionary<string, string> InfoDic = new Dictionary<string, string>();
		public List<Tuple<string, string>> InfoList = new List<Tuple<string, string>>();

		public string Name { get; set; }
		public int Price { get; set; }
		public int? SalePrice { get; set; }
		public string[] MainContent { get; set; }
		public string MainImage
		{
			get
			{
				if (!ImageList.Any()) return "http://placehold.it/320x120";

				var MainImageList = ImageList.Where(e => e.Contains("대표"));
				if (MainImageList.Any())
					return MainImageList.GetRandom();
				return ImageList.GetRandom();
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
				InfoList.Insert(0, Tuple.Create("상품명", Name));
				return;
			}

			InfoList = productInfoPath.InfoToList();
			InfoList.Insert(0, Tuple.Create("상품명", Name));
			InfoDic = InfoList.GroupBy(e => e.Item1).ToDictionary(e => e.Key, e => e.First().Item2);
			var dic = InfoDic.ToDefaultDictionary("");
			Price = int.Parse(dic["가격"]);
			MainContent = dic["대표설명"].RegexReplace(@"\r", "").Split('\n');
		}
	}

	public static class ProductStatic
	{
		public static List<Tuple<string, string>> InfoToList(this string filepath)
		{
			var result = new List<Tuple<string, string>>();
			string currentKey = null;
			string currentValue = null;
			foreach (var line in File.ReadAllLines(filepath, Encoding.Default))
			{
				if (line.Contains('='))
				{
					var splitted = line.Split('=');
					currentKey = splitted[0].Trim();
					currentValue = "";
					if (splitted.Count() > 1)
						currentValue = splitted[1].Trim();
					result.Add(Tuple.Create(currentKey, currentValue));
				}
				else
				{
					result.Remove(Tuple.Create(currentKey, currentValue));
					currentValue += Environment.NewLine + line.Trim();
					result.Add(Tuple.Create(currentKey, currentValue));
				}
			}
			return result;
		}

		//public static Dictionary<string, string> InfoToDictionary(this string filepath)
		//{
		//	var infoDic = new Dictionary<string, string>();
		//	string currentKey = null;
		//	foreach (var line in File.ReadAllLines(filepath, Encoding.Default))
		//	{
		//		if (line.Contains('='))
		//		{
		//			var splitted = line.Split('=');
		//			currentKey = splitted[0].Trim();
		//			string value = "";
		//			if (splitted.Count() > 1)
		//				value = splitted[1];
		//			infoDic.Add(currentKey, value);
		//		}
		//		else
		//		{
		//			infoDic[currentKey] = (infoDic[currentKey] += Environment.NewLine + line).Trim();
		//		}
		//	}
		//	return infoDic;
		//}
	}
}
