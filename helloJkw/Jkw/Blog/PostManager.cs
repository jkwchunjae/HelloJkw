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
	public static class PostManager
	{
		public static string BlogRootPath = @"jkw/blog/";
		public static List<Post> PostList;
		public static List<DateTime> DateList;
		public static List<string> CategoryList;
		public static List<string> TagList;

		public static void UpdatePost()
		{
			var path = Path.Combine(Environment.CurrentDirectory, BlogRootPath, "posts");
			var pattern = @"\/\d{8}\s*\-\s*.+";
			PostList = Directory.GetFiles(path)
				.Select(filepath => filepath.Replace(@"\", "/"))
				.Where(filepath => Regex.IsMatch(filepath, pattern))
				.Select(filepath => new Post(filepath))
				.ToList();
			DateList = PostList.Select(post => post.Date).Distinct().ToList();
			CategoryList = PostList.Select(post => post.Category).Distinct().ToList();
			TagList = PostList.SelectMany(post => post.Tags.ToList()).ToList();
		}

		public static IEnumerable<Post> GetLastDatePost()
		{
			var lastDate = DateList.Max();
			return PostList.Where(post => post.Date == lastDate);
		}
	}
}
