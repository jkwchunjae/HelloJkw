using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using Extensions;
using helloJkw.Utils;

namespace helloJkw
{
	public class JkwBlogModule : JkwModule
	{
		public JkwBlogModule()
		{
			Model.categoryList = BlogManager.CategoryList
				.OrderByDescending(e => e.Count);
			Model.tagList = BlogManager.TagList;
			Model.Title = "jkw's Blog";
		}
	}

	public class JkwBlogHomeModule : JkwBlogModule
	{
		public JkwBlogHomeModule()
			:base()
		{
			Get["/blog/{getCount?20}"] = _ =>
			{
				HitCounter.Hit("blog/main");

				BlogManager.UpdatePost();
				string getCount = _.getCount;
				Model.mainPostList = BlogManager.GetLastPosts(getCount.ToInt());

				return View["jkwBlogHome", Model];
			};

			Get["/blog/post/{postname}"] = _ =>
			{
#if DEBUG
				BlogManager.UpdatePost(0);
#else
				BlogManager.UpdatePost();
#endif
				string postname = _.postname;
				HitCounter.Hit("blog/post/" + postname);

				var post = BlogManager.PostList
					.Where(e => e.Name == postname)
					.FirstOrDefault();
				if (post == null)
					return "worng";

				Model.post = post;
				Model.Title = "jkw's " + post.Title;

				return View["jkwBlogPost", Model];
			};

			Get["/blog/category/{category}"] = _ =>
			{
				BlogManager.UpdatePost();
				string category = _.category;
				HitCounter.Hit("blog/category/" + category);

				Model.postList = BlogManager.PostList
					.Where(e => e.CategoryUrl == category)
					.OrderByDescending(e => e.Date);

				return View["jkwBlogCategory", Model];
			};

			Get["/blog/tag/{tag}"] = _ =>
			{
				BlogManager.UpdatePost();
				string tag = _.tag;
				HitCounter.Hit("blog/tag/" + tag);

				Model.postList = BlogManager
					.ContainsTagPostList(tag)
					.OrderByDescending(e => e.Date);

				return View["jkwBlogTag", Model];
			};
		}
	}
}
