using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using System.IO;

namespace helloJkw
{
	public class Post
	{
		public string Filepath { get; set; }
		public string Name { get; set; }
		public string Title { get; set; }
		public string Category { get; set; }
		public string CategoryUrl { get; set; }
		public HashSet<string> Tags { get; set; }
		public DateTime Date { get; set; }
		public string Content;
		public string Html;

		/// <summary>
		/// text원본을 받아 파싱한다.
		/// </summary>
		/// <param name="text">directory에 저장되어 있는 text원본</param>
		public Post(string filepath)
		{
			Filepath = filepath;
			var text = File.ReadAllText(filepath);
			var filename = Path.GetFileNameWithoutExtension(filepath);
			Parse(filename, text);
		}

		void Parse(string filename, string text)
		{
			var indexContent = text.IndexOf("@content");

			var textList = text.Substring(0, indexContent)
				.Split('\n')
				.Select(e => e.Trim())
				.ToList();

			Name = filename.Substring(9).Trim().Replace(" ", "");
			Date = filename.Substring(0, 8).ToDate();
			Content = text.Substring(indexContent + 8).Trim();
			Html = Content.ToHtml();
			Title = textList.GetValue("@title");
			Category = textList.GetValue("@category");
			CategoryUrl = Category;
			Tags = textList.GetValue("@tags").Split(',').Select(e => e.Trim()).ToHashSet();
		}

		public bool ContainsTag(string tag)
		{
			return Tags.Contains(tag);
		}
	}

	static class PostUtil
	{
		public static string GetValue(this IEnumerable<string> textList, string key, string splitToken = ":")
		{
			var keyValue = textList.Where(e => e.Contains(key)).FirstOrDefault();
			if (keyValue == null || keyValue == string.Empty) keyValue = "";
			var tokenIndex = keyValue.IndexOf(splitToken);
			if (tokenIndex == -1) return "";
			return keyValue.Substring(tokenIndex + 1).Trim();
		}
	}
}
