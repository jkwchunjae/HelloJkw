using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public class ColorConquerMain : NancyModule
	{
		public ColorConquerMain()
		{
			Get["/games/ColorConquer"] = _ =>
				{
					//return "test";
					return View["colorConquerMain"];
				};
		}
	}
}
