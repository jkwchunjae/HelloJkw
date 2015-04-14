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
#if (DEBUG)
				KboMatch.Update(0);
#else
				KboMatch.Update();
#endif

				int year = (seasonStr == "default" || !seasonStr.IsInt()) ? KboMatch.RecentSeason : seasonStr.ToInt();
				var chartObject = KboMatch.GetChartObject(year);
				Model.chartObject = chartObject;
				Model.DateCount = chartObject.DateList.Split(',').Count();
				Model.YearList = KboMatch.SeasonList.Select(e => e.Year).OrderByDescending(e => e).ToList();
				Model.Title = "jkw's KBO Chart {Year}".WithVar(new { chartObject.Year });
				Model.Desc = "KBO {Year} 시즌 게임차 그래프".WithVar(new { chartObject.Year });
				
				return View["jkwKboChart", Model];
			};
		}
	}
}
