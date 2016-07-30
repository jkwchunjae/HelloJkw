using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using Newtonsoft.Json;
using System.IO;
using System.Dynamic;

namespace helloJkw
{
	public class WordBlankManager : JkwModule
	{
		public static string RootPath = "jkw/project/word-blank";
		public WordBlankManager()
		{
			Get["/wb"] = _ =>
			{
#if DEBUG
#else
				if (!session.IsLogin)
					return View["word-blank/wordBlankLogin.cshtml", Model];
#endif

				Model.PathList = "";
				Model.Type = "study";
				Model.Take = Request.Query["take"] == null ? 10 : ((string)Request.Query["take"]).ToInt();
				return View["word-blank/wordBlankMain.cshtml", Model];
			};

			Get["/wb/study/{pathList?}"] = _ =>
			{
#if DEBUG
#else
				if (!session.IsLogin)
					return View["word-blank/wordBlankLogin.cshtml", Model];
#endif

				Model.PathList = _.pathList;
				Model.Type = "study";
				Model.Take = Request.Query["take"] == null ? 10 : ((string)Request.Query["take"]).ToInt();
				return View["word-blank/wordBlankMain.cshtml", Model];
			};

			Get["/wb/exam/{pathList?}"] = _ =>
			{
#if DEBUG
#else
				if (!session.IsLogin)
					return View["word-blank/wordBlankLogin.cshtml", Model];
#endif

				Model.PathList = _.pathList;
				Model.Type = "exam";
				Model.Take = Request.Query["take"] == null ? 10 : ((string)Request.Query["take"]).ToInt();
				return View["word-blank/wordBlankMain.cshtml", Model];
			};

			Post["/wb/load"] = _ =>
			{
#if DEBUG
#else
				if (!session.IsLogin)
					return "로그인 해주세요.";
#endif

#if DEBUG
				string email = "jkwchunjae@gmail.com";
#else
				string email = session.User.Email;
#endif
				string json = Request.Form["data"];
				string takeString = Request.Form["take"];
				int takeCount = Request.Form["take"] == null ? 10 : ((string)Request.Form["take"]).ToInt();
				var pathList = JsonConvert.DeserializeObject<string>(json).Trim().Split(' ').Where(x => x.Length > 0);
				var textList = LoadText(email, pathList).Select(x => x.Item2);
				var nextPathList = textList.Where(x => x.Path.Count() > pathList.Count())
					.Select(x => x.Path[pathList.Count()])
					.Distinct().OrderBy(x => x).ToList();

				dynamic obj = new ExpandoObject();
				obj.PathList = pathList.Select((x, i) => new { PathName = x, PathPath = pathList.Select((e, j) => new { Path = e, Index = j }).Where(e => e.Index <= i).Select(e => e.Path).StringJoin(" ") }).ToList();
				obj.NextPathList = nextPathList;
				obj.TextList = textList.Take(takeCount);
				return JsonConvert.SerializeObject(obj);
			};

			Post["wb/word-blank"] = _ =>
			{
#if DEBUG
#else
				if (!session.IsLogin)
					return "로그인 해주세요.";
#endif

#if DEBUG
				string email = "jkwchunjae@gmail.com";
#else
				string email = session.User.Email;
#endif
				string name = Request.Form["name"];
				int index = (int)Request.Form["index"];
				int length = (int)Request.Form["length"];
				try
				{
					var tuple = FindText(email, name);
					var path = tuple.Item1;
					var text = tuple.Item2;
					var word = text.Content.Where(x => x.Index == index).First();
					if (word != null)
					{
						var blankStr = word.Str.Substring(0, length);
						word.BlankStr = word.BlankStr == blankStr ? "" : blankStr;
						File.WriteAllText(path, text.ToJson(), Encoding.UTF8);
					}
				}
				catch (Exception ex)
				{
					return ex.Message;
				}
				return "success";
			};

			Post["wb/add-editor"] = _ =>
			{
#if DEBUG
#else
				if (!session.IsLogin)
					return "로그인 해주세요.";
#endif

#if DEBUG
				string email = "jkwchunjae@gmail.com";
#else
				string email = session.User.Email;
#endif
				string json = Request.Form["pathList"];
				string editor = Request.Form["editor"];
				var pathList = JsonConvert.DeserializeObject<string>(json).Trim().Split(' ').Where(x => x.Length > 0);
				var textList = LoadText(email, pathList);
				foreach (var tuple in textList.Where(x => !x.Item2.Editor.Contains(editor)))
				{
					var path = tuple.Item1;
					var text = tuple.Item2;
					text.Editor.Add(editor);
					File.WriteAllText(path, text.ToJson(""), Encoding.UTF8);
				}
				return "";
			};

			Post["wb/remove-editor"] = _ =>
			{
#if DEBUG
#else
				if (!session.IsLogin)
					return "로그인 해주세요.";
#endif

#if DEBUG
				string email = "jkwchunjae@gmail.com";
#else
				string email = session.User.Email;
#endif
				string json = Request.Form["pathList"];
				string editor = Request.Form["editor"];
				var pathList = JsonConvert.DeserializeObject<string>(json).Trim().Split(' ').Where(x => x.Length > 0);
				var textList = LoadText(email, pathList);
				foreach (var tuple in textList.Where(x => x.Item2.Editor.Contains(editor)))
				{
					var path = tuple.Item1;
					var text = tuple.Item2;
					text.Editor.Remove(editor);
					File.WriteAllText(path, text.ToJson(""), Encoding.UTF8);
				}
				return "";
			};
		}

		public List<Tuple<string, Text>> LoadText(string email, IEnumerable<string> pathList, int takeCount = int.MaxValue)
		{
			var filenamePrefix = pathList.MakeFileName();
			return Directory.GetFiles(RootPath, filenamePrefix + "*.json", SearchOption.AllDirectories)
				.Select(x => new { Path = x, Text = JsonConvert.DeserializeObject<Text>(File.ReadAllText(x, Encoding.UTF8)) })
				.Where(x => x.Text.Owner == email || x.Text.Editor.Contains(email))
				.Take(takeCount)
				.Select(x => Tuple.Create(x.Path, x.Text))
				.ToList();
		}

		public Tuple<string, Text> FindText(string email, string name)
		{
			var result = Directory.GetFiles(RootPath, name + ".json", SearchOption.AllDirectories)
				.Select(x => new { Path = x, Text = JsonConvert.DeserializeObject<Text>(File.ReadAllText(x, Encoding.UTF8)) })
				.Where(x => x.Text.Owner == email || x.Text.Editor.Contains(email))
				.ToList();
			if (result.Count() == 0)
				throw new Exception("에러: 해당 파일이 없습니다.");
			if (result.Count() >= 2)
				throw new Exception("에러 : 중복된 파일이 있습니다.");

			return Tuple.Create(result.First().Path, result.First().Text);
		}
	}
	
	public class Text
	{
		public string Owner;
		public HashSet<string> Editor = new HashSet<string>();
		public List<string> Path = new List<string>();
		public List<Word> Content = new List<Word>();

		public string Name
		{
			get
			{
				return Path.StringJoin(" ");
			}
		}

		public void SetContent(string content)
		{
			Content = content.Replace("\r", "").Split('\n').SelectMany(x => x.Split(' ')).Where(x => x.Length > 0).Select((x, i) => new Word() { Index = i, Str = x, BlankStr = "" }).ToList();
		}
	}

	public class Word
	{
		public int Index;
		public string Str;
		public string BlankStr;

		public List<SplitStr> SplitStr
		{
			get
			{
				var result = Str.SplitStr().Select(x => new SplitStr() { IsBlank = false, Str = x }).ToList();
				int index = 0;
				foreach (var item in result)
				{
					if (BlankStr.Length >= index + item.Str.Length)
					{
						if (BlankStr.Substring(index, item.Str.Length) == item.Str)
						{
							item.IsBlank = true;
						}
					}
					index += item.Str.Length;
					item.Length = index;
				}
				return result;
			}
		}
	}

	public class SplitStr
	{
		public bool IsBlank;
		public string Str;
		public int Length;
	}

	public static class WordBlankExtensions
	{
		static HashSet<string> _exceptKeyword = "은 는 이 가 을 를 에게 와 과 의 하 으로 이나 에서 도 간에 부터 까지 또한 , . ? !".Split(' ').ToHashSet();
		static Dictionary<string, List<string>> _cacheSplitStr = new Dictionary<string, List<string>>();
		
		static WordBlankExtensions()
		{
			try
			{
				_exceptKeyword = File.ReadAllLines(Path.Combine(WordBlankManager.RootPath, "exceptKeyword.txt"), Encoding.UTF8)
					.SelectMany(x => x.Split(' '))
					.ToHashSet();
			}
			catch
			{
			}
		}

		public static List<string> SplitStr(this string str)
		{
			if (_cacheSplitStr.ContainsKey(str))
				return _cacheSplitStr[str];

			var result = new List<string>();
			if (string.IsNullOrWhiteSpace(str))
				return result;
			if (str.Length >= 2 && _exceptKeyword.Contains(str.Right(2)))
			{
				result.AddRange(SplitStr(str.Left(str.Length - 2)));
				result.Add(str.Right(2));
				_cacheSplitStr.Add(str, result);
				return result;
			}

			if (str.Length >= 1 && _exceptKeyword.Contains(str.Right(1)))
			{
				result.AddRange(SplitStr(str.Left(str.Length - 1)));
				result.Add(str.Right(1));
				_cacheSplitStr.Add(str, result);
				return result;
			}

			result.Add(str);
			_cacheSplitStr.Add(str, result);
			return result;
		}

		public static string MakeFileName(this IEnumerable<string> pathList)
		{
			return pathList.StringJoin(" ");
		}

		public static string ToJson(this Text text, string prefix = "")
		{
			var space = "  ";
			var nextPrefix = prefix + space;
			var sb = new StringBuilder();
			sb.AppendLine(prefix + "{");
			sb.AppendLine(prefix + space + @"""Owner"": " + text.Owner.ToJson(nextPrefix) + @",");
			sb.AppendLine(prefix + space + @"""Editor"": " + text.Editor.ToJson(nextPrefix) + @",");
			sb.AppendLine(prefix + space + @"""Path"": " + text.Path.ToJson(nextPrefix) + @",");
			sb.AppendLine(prefix + space + @"""Content"": " + text.Content.ToJson(nextPrefix) + @"");
			sb.Append(prefix + "}");
			return sb.ToString();
		}

		public static string ToJson(this Word word, string prefix = "")
		{
			return @"{""Str"": " + word.Str.ToJson(prefix) + @", ""BlankStr"": " + word.BlankStr.ToJson(prefix) + @"}";
		}

		public static string ToJson(this List<Word> list, string prefix)
		{
			var space = "  ";
			var sb = new StringBuilder();
			sb.AppendLine("[");
			foreach (var item in list.Select((x, i) => new { Order = i, Word = x }))
			{
				sb.Append(prefix + space + "{" + string.Format(@"""Index"": {0}, ""Str"": ""{1}"", ""BlankStr"": ""{2}""", item.Word.Index, item.Word.Str, item.Word.BlankStr) + "}");
				sb.AppendLine(item.Order == list.Count() - 1 ? "" : ",");
			}
			sb.Append(prefix + "]");
			return sb.ToString();
		}

		public static string ToJson(this Dictionary<int, Word> dic, string prefix = "")
		{
			var space = "  ";
			var sb = new StringBuilder();
			sb.AppendLine("{");
			var list = dic.Select(x => x).OrderBy(x => x.Key)
				.Select((x, i) => new { Index = i, WordIndex = x.Key, Word = x.Value });
			foreach (var item in list)
			{
				sb.Append(prefix + space + string.Format(@"""{0}"": {1}", item.WordIndex, item.Word.ToJson("")));
				sb.AppendLine(item.Index == dic.Count() - 1 ? "" : ",");
			}
			sb.Append(prefix + "}");
			return sb.ToString();
		}

		public static string ToJson(this string str, string prefix = "")
		{
			return string.Format(@"""{0}""", str);
		}

		public static string ToJson(this List<string> stringList, string prefix = "")
		{
			return "[" + string.Join(", ", stringList.Select(x => @"""" + x + @"""")) + "]";
		}

		public static string ToJson(this HashSet<string> stringSet, string prefix = "")
		{
			return "[" + string.Join(", ", stringSet.Select(x => @"""" + x + @"""")) + "]";
		}
	}
}
