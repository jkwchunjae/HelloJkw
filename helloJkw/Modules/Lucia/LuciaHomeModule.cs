using System;
using System.IO;
using System.Linq;
using Nancy;
using Extensions;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace helloJkw
{
	public class LuciaHomeModule : NancyModule
	{
		public LuciaHomeModule()
		{
			Get["/lucia/{device?web}"] = _ =>
			{
				LuciaStatic.UpdateLuciaDir();
				string device = _.device;

				var mainDirName = LuciaStatic.MainDirName;
				var mainImageList = LuciaStatic.LuciaDir[mainDirName].GetFiles()
					.Select(e => Path.GetFileName(e.Name));
				var slideImageList = LuciaStatic.LuciaDir[mainDirName]["slide"].GetFiles()
					.Select(e => Path.GetFileName(e.Name));

				var model = new
				{
					rootPath = (device == "m" ? LuciaStatic.RootPathMobile : LuciaStatic.RootPathWeb),
					device = device,
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
				LuciaStatic.UpdateLuciaDir(0);
				return "완료";
			};
		}
	}
}