using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public class JkwBlogPost : JkwBlogModule
	{
		public JkwBlogPost()
		{
			Get["/blog/post/{postname}"] = _ =>
			{
#if DEBUG
				BlogManager.UpdatePost(0);
#else
				BlogManager.UpdatePost();
#endif
				var postname = _.postname;

				Model.post = BlogManager.PostList
					.Where(e => e.Name == postname)
					.First()
					.ToExpando();
				return View["jkwBlogPost", Model];
			};
		}
	}
}
