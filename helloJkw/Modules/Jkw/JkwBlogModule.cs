using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public class JkwBlogModule : NancyModule
	{
		public JkwBlogModule()
		{
			Get["/blog"] = _ => 
			{
				BlogManager.UpdatePost();
				var mainPostList = BlogManager.GetLastDatePost()
					.Select(e => e.ToExpando())
					.Take(3);
				var categoryList = BlogManager.CategoryList
					.OrderByDescending(e => e.Count)
					.Select(e => e.ToExpando());
				var tagList = BlogManager.TagList;

				var model = new
				{
					mainPostList
					, categoryList
					, tagList
				};
				return View["jkwBlogHome", model];
			};

			Get["/blog/category/{category}"] = _ =>
			{
				BlogManager.UpdatePost();
				var category = _.category;

				var postList = BlogManager.PostList
					.Where(e => e.CategoryUrl == category)
					.Select(e => e.ToExpando());
				var categoryList = BlogManager.CategoryList
					.OrderByDescending(e => e.Count)
					.Select(e => e.ToExpando());
				var tagList = BlogManager.TagList;

				var model = new
				{
					postList
					, categoryList
					, tagList
				};
				return View["jkwBlogCategory", model];
			};

			Get["/blog/tag/{tag}"] = _ =>
			{
				BlogManager.UpdatePost();
				var tag = _.tag;

				var postList = BlogManager.PostList
					.Where(e => e.Tags.Contains(tag))
					.Select(e => e.ToExpando());
				var categoryList = BlogManager.CategoryList
					.OrderByDescending(e => e.Count)
					.Select(e => e.ToExpando());
				var tagList = BlogManager.TagList;

				var model = new
				{
					postList
					, categoryList
					, tagList
				};
				return View["jkwBlogTag", model];
			};
		}
	}
}
