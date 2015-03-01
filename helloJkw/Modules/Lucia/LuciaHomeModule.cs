using System;
using System.IO;
using System.Linq;
using Nancy;
using Extensions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using helloJkw.Extensions;

namespace helloJkw.Modules.Lucia
{
	public class LuciaHomeModule : NancyModule
	{
		public LuciaHomeModule()
		{
			Get["/"] = _ =>
			{
				var mainDirName = LuciaStatic.MainDirName;
				var mainImageList = LuciaStatic.LuciaDir[mainDirName].GetFiles()
					.Select(e => Path.GetFileName(e.Name));
				var mainMenuList = LuciaStatic.LuciaDir.GetDirNames()
					.Where(e => e != mainDirName)
					.Select(e => e.RemovePrefixNumber());
				var slideImageList = LuciaStatic.LuciaDir[mainDirName]["slide"].GetFiles()
					.Select(e => Path.GetFileName(e.Name));

				var model = new
				{
					RootPath = LuciaStatic.RootPath,
					MainMenu = mainMenuList.ToList(),
					MainDirName = mainDirName,
					MainImageList = mainImageList,
					SlideImageList = slideImageList,
					//ImageList = mainImageList.Select(e => "{0}/{1}/{2}".With(LuciaStatic.RootPath, mainDirName, Path.GetFileName(e.Name))).ToList(),
					//ImageList = mainImageList.Select(e => Path.GetFileName(e.Name)).ToList()
				};
				return View["luciaHome", model];
			};
		}
	}

	public class LuciaCardModule : NancyModule
	{
		public LuciaCardModule()
		{
			Get["/category/{category}"] = _ =>
			{
				var mainDirName = LuciaStatic.MainDirName;
				var mainMenuList = LuciaStatic.LuciaDir.GetDirNames()
					.Where(e => e != mainDirName)
					.Select(e => Regex.Replace(e, @"\d*\.\s", "")); // 000. XXXX 이런 형태에서 XXX 만 가져온다.
				var category = (string)_.category;

				var productList = LuciaStatic.LuciaDir[category].GetDirNames()
					.Select(e => new ProductInfo
					{
						Name = e
					}.ToExpando())
					.ToList();

				var model = new
				{
					RootPath = LuciaStatic.RootPath,
					MainMenu = mainMenuList,
					Category = category,
					ProductList = productList, 
				};
				return View["luciaCategory", model];
			};
		}
	}
}