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
					rootPath = LuciaStatic.RootPath,
					mainMenu = LuciaStatic.GetMainMenu(),
					mainDirName,
					mainImageList,
					slideImageList,
					//ImageList = mainImageList.Select(e => "{0}/{1}/{2}".With(LuciaStatic.RootPath, mainDirName, Path.GetFileName(e.Name))).ToList(),
					//ImageList = mainImageList.Select(e => Path.GetFileName(e.Name)).ToList()
				};
				return View["luciaHome", model];
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