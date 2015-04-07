using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using System.Dynamic;

namespace helloJkw
{
	public class JkwBlogModule : NancyModule
	{
		public dynamic Model = new ExpandoObject();
		public JkwBlogModule()
		{
			Model.categoryList = BlogManager.CategoryList
				.OrderByDescending(e => e.Count)
				.Select(e => e.ToExpando());
			Model.tagList = BlogManager.TagList;
		}
	}

	public class JkwBlogHomeModule : JkwBlogModule
	{
		public JkwBlogHomeModule()
			:base()
		{
			Get["/blog/{getCount?3}"] = _ =>
			{
				BlogManager.UpdatePost();
				string getCount = _.getCount;
				Model.mainPostList = BlogManager.GetLastPosts(getCount.ToInt())
					.Select(e => e.ToExpando());

				return View["jkwBlogHome", Model];
			};

			Get["/blog/category/{category}"] = _ =>
			{
				BlogManager.UpdatePost();
				string category = _.category;

				Model.postList = BlogManager.PostList
					.Where(e => e.CategoryUrl == category)
					.OrderByDescending(e => e.Date)
					.Select(e => e.ToExpando());

				return View["jkwBlogCategory", Model];
			};

			Get["/blog/tag/{tag}"] = _ =>
			{
				BlogManager.UpdatePost();
				string tag = _.tag;

				Model.postList = BlogManager
					.ContainsTagPostList(tag)
					.OrderByDescending(e => e.Date)
					.Select(e => e.ToExpando());
					//.PostList
					//.Where(e => e.Tags.Contains(tag))

				return View["jkwBlogTag", Model];
			};
		}
	}
}
