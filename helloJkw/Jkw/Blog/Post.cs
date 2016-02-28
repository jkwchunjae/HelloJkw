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
	public class Post
	{
		public string FilePath { get; private set; }
		public string FileName { get; private set; }
		public string Name { get; private set; }
		public string Title { get; private set; }
		public string Category { get; private set; }
		public string CategoryUrl { get; private set; }
		public List<TagItem> Tags { get; private set; }
		public DateTime CreateDate { get; private set; }
		public DateTime PublishDate { get; private set; }
		public DateTime Date { get { return PublishDate; } }
		public string Content { get; private set; }
		public string Html { get; private set; }
		public string HtmlCut { get; private set; }
		public bool IsPublish { get; private set; }

		public string Raw { get; private set; }

		/// <summary>
		/// text원본을 받아 파싱한다.
		/// </summary>
		/// <param name="text">directory에 저장되어 있는 text원본</param>
		public Post(string filepath)
		{
			FilePath = filepath;
			//var text = File.ReadAllText(filepath, Encoding.UTF8);
			string text = string.Empty;
			using (var inStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (var reader = new StreamReader(inStream))
				{
					Raw = text = reader.ReadToEnd();
				}
			}
			FileName = Path.GetFileNameWithoutExtension(filepath);
			Parse(FileName, text);
		}

		void Parse(string filename, string text)
		{
			var indexContent = text.IndexOf("@content");

			var textList = text.Substring(0, indexContent)
				.Split('\n')
				.Select(e => e.Trim())
				.ToList();

			Name = filename.Substring(9).Trim().Replace(" ", "");
			CreateDate = filename.Substring(0, 8).ToDate();
			Func<string, DateTime, DateTime> GetDate = (string strDate, DateTime defulatDate) =>
			{
				if (string.IsNullOrWhiteSpace(strDate))
					return defulatDate;
				return strDate.Replace(".", "").Replace("/", "").ToDate(defulatDate);
			};
			PublishDate = GetDate(textList.GetValue("@publishDate"), CreateDate);
			Content = text.Substring(indexContent + 8).Trim();
			Html = Content.ToHtml();
			HtmlCut = Html.CutParagraph();
			Title = textList.GetValue("@title");
			IsPublish = textList.GetValue("@isPublish").ToBoolean();
			Tags = textList.GetValue("@tags").Split(',')
				.Select(e => e.Trim().SplitUrl())
				.Select(e => new TagItem { Name = e.Item1.ToLower(), Url = e.Item2.ToLower() })
				.ToList();
			var categoryTuple = textList.GetValue("@category").Trim().SplitUrl();
			Category = categoryTuple.Item1;
			CategoryUrl = categoryTuple.Item2;

			var categoryPattern = @"(.*)\((.*)\)";
			if (Regex.IsMatch(Category, categoryPattern))
			{
				var m = Regex.Match(Category, categoryPattern);
				Category = m.Groups[1].Captures[0].Value.Trim();
				CategoryUrl = m.Groups[2].Captures[0].Value.Trim();
			}
		}
	}

	public class TagItem
	{
		public string Name;
		public string Url;
		public int Count;
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

		public static Tuple<string, string> SplitUrl(this string text)
		{
			var pattern = @"(.*)\((.*)\)";
			var m = Regex.Match(text, pattern);
			if (!m.Success)
				return Tuple.Create(text, text);
			var v1 = m.Groups[1].Captures[0].Value.Trim();
			var v2 = m.Groups[2].Captures[0].Value.Trim();
			return Tuple.Create(v1, v2);
		}

		static MarkdownSharp.Markdown _markdown = new MarkdownSharp.Markdown();
		public static string ToHtml(this string text)
		{
			return _markdown.Transform(text);
		}

		public static string CutParagraph(this string html)
		{
			if (html.Length <= 300)
				return html;
			var firstP = html.IndexOf(@"</p>");
			return html.Substring(0, firstP + 4);
		}
	}
}
