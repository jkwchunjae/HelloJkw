using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public class BadukModule : JkwModule
	{
		public BadukModule()
		{
			Get["/games/Baduk"] = _ =>
			{
				return View["Games/Baduk/badukMain.cshtml", Model];
			};
		}
	}
}
