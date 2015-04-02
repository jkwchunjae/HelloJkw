using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw.Modules.Jkw
{
	public class JkwBlogModule : NancyModule
	{
		public JkwBlogModule()
		{
			Get["/blog"] = _ => 
			{
				return "blog";
			};
		}
	}
}
