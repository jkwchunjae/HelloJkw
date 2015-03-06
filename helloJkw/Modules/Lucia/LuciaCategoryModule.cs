using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw.Modules.Lucia
{
	public class LuciaCategoryModule : NancyModule
	{
		public LuciaCategoryModule()
		{
			Get["/lucia/category/{category}"] = _ =>
			{
				string category = _.category;
				LuciaStatic.UpdateLuciaDir(5);

				//var productList = LuciaStatic.LuciaDir[category].GetDirNames()
				//	.Select(e => new ProductInfo
				//	{
				//		Name = e
				//	}.ToExpando())
				//	.ToList();
				var productList = LuciaStatic.LuciaDir[category]
					.GetSubDirList()
					.Select(e => e.ProductInfo.ToExpando());

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
}
