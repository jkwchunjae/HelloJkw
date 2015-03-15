using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using helloJkw.Extensions;

namespace helloJkw.Modules.Lucia
{
	public class LuciaProductModule : NancyModule
	{
		public LuciaProductModule()
		{
			Get["/lucia/{device?web}/product/{category}/{productName}"] = _ =>
			{
				LuciaStatic.UpdateLuciaDir(5);
				string device = _.device;
				string category = _.category;
				string productName = _.productName;

				var productList = LuciaStatic.LuciaDir[category]
					.GetSubDirList()
					.Select(e => e.ProductInfo.ToExpando());

				var productInfo = LuciaStatic.LuciaDir[category]
					.GetSubDirList()
					.Where(e => e.ProductInfo.Name == productName)
					.Select(e => e.ProductInfo)
					.FirstOrDefault();

				var model = new
				{
					rootPath = (device == "m" ? LuciaStatic.RootPathMobile : LuciaStatic.RootPathWeb),
					device,
					mainMenu = LuciaStatic.GetMainMenu(),
					productList,
					category,
					productInfo,
				};
				return View["luciaProduct", model];
			};
		}
	}
}
