using Nancy;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace helloJkw.Modules.Jkw
{
	public class JkwKboChartModule : NancyModule
	{
		public JkwKboChartModule()
		{
			Get["/kbochart/{season?default}"] = _ =>
			{
				dynamic Model = new ExpandoObject();
				string seasonStr = _.season;
				if (seasonStr == "reload")
				{
					KboMatch.Reload();
				}
				KboMatch.Update();

				int season = (seasonStr == "default" || !seasonStr.IsInt()) ? KboMatch.RecentSeason : seasonStr.ToInt();
				Model.chartObject = KboMatch.GetChartObject(season);
				
				return View["jkwKboChart", Model];
			};
		}
	}
}
