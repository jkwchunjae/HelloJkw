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
			Get["/kbochart/{season}"] = _ =>
			{
				dynamic Model = new ExpandoObject();
				KboMatch.Update();
				string season = _.season;
				Model.chartObject = KboMatch.GetChartObject(season.ToInt());
				
				return View["jkwKboChart", Model];
			};
		}
	}
}
