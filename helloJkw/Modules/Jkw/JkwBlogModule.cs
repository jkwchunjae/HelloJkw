using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using Extensions;
using helloJkw.Utils;
using System.Diagnostics;
using System.IO;

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

		public bool IsEditor()
		{
			return Model.isDebug || session?.User?.Grade == UserGrade.Admin;
		}
	}

	public class JkwBlogHomeModule : JkwBlogModule
	{
		public JkwBlogHomeModule()
			:base()
		{
			Get["/blog/{getCount?20}"] = _ =>
			{
				Model.isEditor = IsEditor();
				HitCounter.Hit("blog/main");

#if DEBUG
				BlogManager.UpdatePost(0);
#else
				BlogManager.UpdatePost();
#endif
				string getCount = _.getCount;
				Model.postList = BlogManager.GetLastPosts(getCount.ToInt(), IsEditor());

				return View["blog/jkwBlogHome", Model];
			};

			Get["/blog/post/{postname}"] = _ =>
			{
				Model.isEditor = IsEditor();
#if DEBUG
				BlogManager.UpdatePost(0);
#else
				BlogManager.UpdatePost();
#endif
				string postname = _.postname; // name (without date)
				HitCounter.Hit("blog/post/" + postname);

				var post = BlogManager.PostList
					.Where(e => e.Name == postname)
					.Where(x => x.IsPublish || IsEditor())
					.FirstOrDefault();
				if (post == null)
					return "wrong";

				var categoryList = BlogManager.PostList
					.Where(e => e.CategoryUrl == post.CategoryUrl)
					.OrderBy(e => e.PublishDate)
					.ThenBy(e => e.Name)
					.Select((e, i) => new { Index = i, Post = e })
					.ToList();

				var postIndex = categoryList.Where(e => e.Post.Name == post.Name).First().Index;

				Model.post = post;
				Model.Title = "jkw's " + post.Title;
				Model.PrevPost = postIndex == 0 ? null : categoryList[postIndex - 1].Post;
				Model.NextPost = postIndex == categoryList.Count() - 1 ? null : categoryList[postIndex + 1].Post;

				return View["blog/jkwBlogPost", Model];
			};

			Get["/blog/post/edit/{postname}"] = _ =>
			{
				Model.isEditor = IsEditor();
				if (!IsEditor())
					return "wrong";
#if DEBUG
				BlogManager.UpdatePost(0);
#else
				BlogManager.UpdatePost();

				if (session.User?.Grade != UserGrade.Admin)
				{
					return Response.AsRedirect("/blog/post/{PostName}".WithVar(new { PostName = _.postname }));
				}
#endif
				string postname = _.postname; // name (without date)
				var post = BlogManager.PostList
					.Where(e => e.Name == postname)
					.Where(x => IsEditor())
					.FirstOrDefault();
				if (post == null)
					return "wrong";

				Model.post = post;
				Model.Title = "[Edit] jkw's " + post.Title;

				return View["blog/jkwBlogEdit", Model];
			};

			Get["/blog/category/{category}"] = _ =>
			{
				Model.isEditor = IsEditor();
				BlogManager.UpdatePost();
				string category = _.category;
				HitCounter.Hit("blog/category/" + category);

				Model.postList = BlogManager.PostList
					.Where(e => e.CategoryUrl == category)
					.Where(x => x.IsPublish || IsEditor())
					.OrderByDescending(e => e.PublishDate)
					.ThenByDescending(e => e.Name);

				return View["blog/jkwBlogHome", Model];
			};

			Get["/blog/tag/{tag}"] = _ =>
			{
				Model.isEditor = IsEditor();
				BlogManager.UpdatePost();
				string tag = _.tag;
				HitCounter.Hit("blog/tag/" + tag);

				Model.postList = BlogManager
					.ContainsTagPostList(tag)
					.Where(x => x.IsPublish || IsEditor())
					.OrderByDescending(e => e.PublishDate)
					.ThenByDescending(e => e.Name);

				return View["blog/jkwBlogHome", Model];
			};
		}
	}
}
