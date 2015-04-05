using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public class JkwBlogPost : NancyModule
	{
		public JkwBlogPost()
		{
			Get["/blog/post/{postname}"] = _ =>
			{
				BlogManager.UpdatePost();
				var postname = _.postname;

				var post = BlogManager.PostList
					.Where(e => e.Name == postname)
					.First()
					.ToExpando();
				var categoryList = BlogManager.CategoryList
					.OrderByDescending(e => e.Count)
					.Select(e => e.ToExpando());
				var tagList = BlogManager.TagList;

				var model = new
				{
					post
					, categoryList
					, tagList
				};
				return View["jkwBlogPost", model];
			};
		}
	}
}
