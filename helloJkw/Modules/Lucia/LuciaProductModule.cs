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
	public class LuciaProductModule : NancyModule
	{
		public LuciaProductModule()
		{
			Get["/lucia/{device?web}/product/{category}/{productName}"] = _ =>
			{
				LuciaStatic.UpdateLuciaDir();
				string device = _.device;
				string category = _.category;
				string productName = _.productName;
				HitCounter.Hit("lucia/product/{0}/{1}".With(category, productName));
				Logger.Log("viewLog - lucia/product/{0}/{1}".With(category, productName));

				var productList = LuciaStatic.LuciaDir[category]
					.GetProductList()
					.Select(e => e.ToExpando());

				var productInfo = LuciaStatic.LuciaDir[category]
					.GetProductList()
					.Where(e => e.Name == productName)
					.FirstOrDefault();

				if (productInfo == null)
					return "wrong url";

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
