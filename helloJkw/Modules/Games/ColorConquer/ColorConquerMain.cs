using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public class ColorConquerMain : JkwModule
	{
		public ColorConquerMain()
		{
			Get["/games/ColorConquer"] = _ =>
				{
					if (!session.IsLogin)
					{
						return "require login";
					}
					return View["colorConquerMain", Model];
				};
		}
	}
}
