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
				var slideImageList = LuciaStatic.LuciaDir[mainDirName]["slide"].GetFiles()
					.Select(e => Path.GetFileName(e.Name));

				var model = new
				{
					RootPath = LuciaStatic.RootPath,
					MainMenu = LuciaStatic.GetMainMenu(),
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

	public class LuciaCategoryModule : NancyModule
	{
		public LuciaCategoryModule()
		{
			Get["/category/{category}"] = _ =>
			{
				string category = _.category;

				var productList = LuciaStatic.LuciaDir[category].GetDirNames()
					.Select(e => new ProductInfo
					{
						Name = e
					}.ToExpando())
					.ToList();

				var model = new
				{
					RootPath = LuciaStatic.RootPath,
					MainMenu = LuciaStatic.GetMainMenu(),
					Category = category,
					ProductList = productList, 
				};
				return View["luciaCategory", model];
			};
		}
	}

	public class LuciaProductModule : NancyModule
	{
		public LuciaProductModule()
		{
			Get["/product/{category}/{product}"] = _ =>
			{
				string category = _.category;
				string product = _.product;
				return "{0}/{1}".With(category, product);
			};
		}
	}

	public class LuciaRefreshDir : NancyModule
	{
		public LuciaRefreshDir()
		{
			Get["/refresh"] = _ =>
			{
				LuciaStatic.LuciaDir = LuciaStatic.RootPath.CreateDirInfo();
				return "완료";
			};
		}
	}
}