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
	}

	public class JkwBlogHomeModule : JkwBlogModule
	{
		public JkwBlogHomeModule()
			:base()
		{
			Get["/blog/{getCount?20}"] = _ =>
			{
				HitCounter.Hit("blog/main");

#if DEBUG
				BlogManager.UpdatePost(0);
#else
				BlogManager.UpdatePost();
#endif
				string getCount = _.getCount;
				Model.mainPostList = BlogManager.GetLastPosts(getCount.ToInt());

				return View["jkwBlogHome", Model];
			};

			Get["/blog/manage"] = _ =>
			{
#if DEBUG
				var post = BlogManager.PostList
					.OrderByDescending(e => e.Filepath);
				Model.post = post;
				return View["jkwBlogManage", Model];
#else
				return "잘못된 접근입니다.";
#endif
			};

			Post["/blog/edit/{postname}"] = _ =>
			{
#if DEBUG
				string filename = _.postname; // yyyyMMdd-name
				var post = BlogManager.PostList
					.Where(e => e.Filepath.Contains(filename))
					.FirstOrDefault();
				if (post == null)
					return "wrong";

				var postPath = Environment.CurrentDirectory + @"/jkw/blog/posts";
				var filePath = "{postPath}/{filename}.txt".WithVar(new {postPath, filename});
				filePath.Dump();

				try
				{
					var winword = Process.Start("winword.exe", filePath);
					return "success";
				}
				catch (Exception ex)
				{
					Logger.Log(ex);
					return "error";
				}
#else
				return "잘못된 접근입니다.";
#endif
			};

			Get["/blog/post/{postname}"] = _ =>
			{
#if DEBUG
				BlogManager.UpdatePost(0);
#else
				BlogManager.UpdatePost();
#endif
				string postname = _.postname; // name (without date)
				HitCounter.Hit("blog/post/" + postname);

				var post = BlogManager.PostList
					.Where(e => e.Name == postname)
					.FirstOrDefault();
				if (post == null)
					return "wrong";

				var categoryList = BlogManager.PostList
					.Where(e => e.CategoryUrl == post.CategoryUrl)
					.OrderBy(e => e.Date)
					.ThenBy(e => e.Name)
					.Select((e, i) => new { Index = i, Post = e })
					.ToList();

				var postIndex = categoryList.Where(e => e.Post.Name == post.Name).First().Index;

				Model.post = post;
				Model.Title = "jkw's " + post.Title;
				Model.PrevPost = postIndex == 0 ? null : categoryList[postIndex - 1].Post;
				Model.NextPost = postIndex == categoryList.Count() - 1 ? null : categoryList[postIndex + 1].Post;

				return View["jkwBlogPost", Model];
			};

			Get["/blog/category/{category}"] = _ =>
			{
				BlogManager.UpdatePost();
				string category = _.category;
				HitCounter.Hit("blog/category/" + category);

				Model.postList = BlogManager.PostList
					.Where(e => e.CategoryUrl == category)
					.OrderByDescending(e => e.Date)
					.ThenByDescending(e => e.Name);

				return View["jkwBlogCategory", Model];
			};

			Get["/blog/tag/{tag}"] = _ =>
			{
				BlogManager.UpdatePost();
				string tag = _.tag;
				HitCounter.Hit("blog/tag/" + tag);

				Model.postList = BlogManager
					.ContainsTagPostList(tag)
					.OrderByDescending(e => e.Date)
					.ThenByDescending(e => e.Name);

				return View["jkwBlogTag", Model];
			};
		}
	}
}
