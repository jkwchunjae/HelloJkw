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
			Get["/lucia"] = _ =>
			{
				LuciaStatic.UpdateLuciaDir(5);
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

	public class LuciaProductModule : NancyModule
	{
		public LuciaProductModule()
		{
			Get["/lucia/product/{category}/{product}"] = _ =>
			{
				string category = _.category;
				string product = _.product;
				LuciaStatic.UpdateLuciaDir(5);
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
				LuciaStatic.UpdateLuciaDir();
				return "완료";
			};
		}
	}
}