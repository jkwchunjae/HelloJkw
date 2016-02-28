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
	public class JkwBlogManageModule : JkwBlogModule
	{
		public JkwBlogManageModule()
			:base()
		{
			Get["/blog/manage"] = _ =>
			{
#if DEBUG
				BlogManager.UpdatePost(0);
				var post = BlogManager.PostList
					.OrderByDescending(e => e.FilePath);
				Model.post = post;
				return View["blog/jkwBlogManage", Model];
#else
				return "wrong";
#endif
			};

			Post["/blog/new/{date}/{name}"] = _ =>
			{
#if DEBUG
				"blog/new/".Dump();
				BlogManager.UpdatePost(0);

				try
				{
					int date = ((string)_.date).ToInt();
					string postName = _.name;

					//if (date == 0 || postName == null || postTitle == null || postName == "" || postTitle == "")
					if (date == 0 || postName == null || postName == "")
						return "wrong";

					var postPath = Environment.CurrentDirectory + @"/jkw/blog/posts";
					var filename = "{0}-{1}".With(date, postName);
					var filePath = "{postPath}/{filename}.txt".WithVar(new { postPath, filename });
					filePath.Dump();

					string template = @"@title : 제목입력

@category : Test

@tags : test

@publishDate: 

@isPublish : false

@content
임시 작성 포스트
";

					if (!File.Exists(filePath))
						File.WriteAllText(filePath, template, Encoding.UTF8);

					var winword = Process.Start("winword.exe", filePath);
				}
				catch (Exception ex)
				{
					Logger.Log(ex);
				}
				return "success";
#else
				return "wrong";
#endif
			};

			Post["/blog/edit/{postname}"] = _ =>
			{
#if DEBUG
				BlogManager.UpdatePost(0);
				string filename = _.postname; // yyyyMMdd-name
				var post = BlogManager.PostList
					.Where(e => e.FilePath.Contains(filename))
					.FirstOrDefault();
				if (post == null)
					return "wrong";

				var postPath = Environment.CurrentDirectory + @"/jkw/blog/posts";
				var filePath = "{postPath}/{filename}.txt".WithVar(new {postPath, filename});

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
				return "wrong";
#endif
			};

			Post["/blog/rename/{oldname}/{newname}"] = _ =>
			{
#if DEBUG
				BlogManager.UpdatePost(0);
				string oldFilename = _.oldname; // yyyyMMdd-name
				string newFilename = _.newname; // yyyyMMdd-name
				var post = BlogManager.PostList
					.Where(e => e.FilePath.Contains(oldFilename))
					.FirstOrDefault();
				if (post == null)
					return oldFilename;

				try
				{
					var postPath = Environment.CurrentDirectory + @"/jkw/blog/posts";
					var oldFilePath = "{postPath}/{oldFilename}.txt".WithVar(new { postPath, oldFilename });
					var newFilePath = "{postPath}/{newFilename}.txt".WithVar(new { postPath, newFilename });

					//File.Delete(filePath);
					File.Move(oldFilePath, newFilePath);
					return newFilename;
				}
				catch (Exception ex)
				{
					Logger.Log(ex);
					return ex.Message;
				}
#else
				return "wrong";
#endif
			};

			Post["/blog/delete/{postname}"] = _ =>
			{
#if DEBUG
				BlogManager.UpdatePost(0);
				string filename = _.postname; // yyyyMMdd-name
				var post = BlogManager.PostList
					.Where(e => e.FilePath.Contains(filename))
					.FirstOrDefault();
				if (post == null)
					return "파일을 찾을 수 없습니다.";

				try
				{
					var postPath = Environment.CurrentDirectory + @"/jkw/blog/posts";
					var filePath = "{postPath}/{filename}.txt".WithVar(new { postPath, filename });
					filePath.Dump();

					File.Delete(filePath);
					return "파일을 삭제하였습니다.";
				}
				catch (Exception ex)
				{
					Logger.Log(ex);
					return ex.Message;
				}
#else
				return "wrong";
#endif
			};
		}
	}
}
