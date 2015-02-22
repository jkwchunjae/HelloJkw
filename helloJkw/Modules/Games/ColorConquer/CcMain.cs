using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw.Modules.Games.ColorConquer
{
	public class CcMain : NancyModule
	{
		public CcMain()
		{
			Get["/game/colorconquer"] = _ =>
				{
					return View["ccMain"];
				};
		}
	}
}
