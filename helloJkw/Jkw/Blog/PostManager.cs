using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using System.IO;

namespace helloJkw.Jkw.Blog
{
	public static class PostManager
	{
		public static string BlogRootPath = @"jkw/blog/";
		public static List<Post> Posts;

		public static void UpdatePost()
		{
			var path = Path.Combine(Environment.CurrentDirectory, 
			Directory.GetFiles()
		}
	}
}
