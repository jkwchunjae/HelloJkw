using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using helloJkw.Utils;

namespace helloJkw
{
	public class LuciaCategoryModule : NancyModule
	{
		public LuciaCategoryModule()
		{
			Get["/lucia/{device?web}/category/{category}"] = _ =>
			{
				LuciaStatic.UpdateLuciaDir();
				string device = _.device;
				string category = _.category;
				HitCounter.Hit("lucia/category/" + category);

				try
				{
					var productList = LuciaStatic.LuciaDir[category]
						.GetProductList()
						.Select(e => e.ToExpando());
					var imageList = LuciaStatic.LuciaDir[category]
						.GetFiles()
						.Select(e => e.FullName.Replace('\\', '/').RegexReplace(@".*/lucia/", ""));

					var model = new
					{
						rootPath = (device == "m" ? LuciaStatic.RootPathMobile : LuciaStatic.RootPathWeb),
						mobilePath = LuciaStatic.RootPathMobile,
						device,
						mainMenu = LuciaStatic.GetMainMenu(),
						category,
						productList,
						imageList,
					};
					return View["luciaCategory", model];
				}
				catch
				{
					return "wrong url";
				}
			};
		}
	}
}
