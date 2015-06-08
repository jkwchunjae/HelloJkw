using System;
using System.IO;
using System.Linq;
using Nancy;
using Extensions;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Dynamic;
using helloJkw.Utils;

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
				HitCounter.Hit("lucia/main");

				var mainDirName = LuciaStatic.MainDirName;
				dynamic Model = new ExpandoObject();

				Model.rootPath = (device == "m" ? LuciaStatic.RootPathMobile : LuciaStatic.RootPathWeb);
				Model.device = device;
				Model.mainMenu = LuciaStatic.GetMainMenu();
				Model.categorys = LuciaStatic.LuciaDir.GetSubDirList()
					.Where(e => !e.FolderName.Contains("main"))
					.Where(e => e.GetProductList().Count() > 0 || e.GetFiles().Count() > 0)
					.Select(e => new
					{
						Name = e.FolderName.RemovePrefixNumber(),
						MainImage = e.GetProductList().Count() > 0 ? e.GetProductList().GetRandom().MainImage : e.GetFiles().GetRandom().FullName.Replace('\\', '/').RegexReplace(@".*/lucia/", "")
					})
					.Select(e => e.ToExpando());

				return View["luciaHome", Model];
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