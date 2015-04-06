using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using System.IO;
using System.Text.RegularExpressions;

namespace helloJkw
{
	public static class BlogManager
	{
		static DateTime _lastUpdateTime;
		public static string BlogRootPath = @"jkw/blog/";
		public static List<Post> PostList;
		public static List<DateTime> DateList;
		public static List<CategoryItem> CategoryList;
		public static List<string> TagList;

		static BlogManager()
		{
			_lastUpdateTime = DateTime.Now;
			UpdatePost(0);
		}

		public static void UpdatePost(int minute = 5)
		{
			if (minute > 0 && DateTime.Now.Subtract(_lastUpdateTime).TotalMinutes < minute)
				return ;
			var path = Path.Combine(Environment.CurrentDirectory, BlogRootPath, "posts");
			var pattern = @"\/\d{8}\s*\-\s*.+";
			PostList = Directory.GetFiles(path)
				.Select(filepath => filepath.Replace(@"\", "/"))
				.Where(filepath => Regex.IsMatch(filepath, pattern))
				.Select(filepath => new Post(filepath))
				.ToList();
			DateList = PostList.Select(post => post.Date).Distinct().ToList();
			CategoryList = PostList.Select(post => new { post.CategoryUrl, post.Category })
				.GroupBy(e => e)
				.Select(e => new CategoryItem { Url = e.Key.CategoryUrl, Name = e.Key.Category, Count = e.Count() })
				.ToList();
			TagList = PostList.SelectMany(post => post.Tags.ToList()).Distinct().OrderBy(e => e).ToList();
		}

		public static IEnumerable<Post> GetLastPosts(int postCount)
		{
			return PostList.OrderByDescending(e => e.Date).Take(postCount);
		}
	}

	public class CategoryItem
	{
		public string Url;
		public string Name;
		public int Count;
	}
}
