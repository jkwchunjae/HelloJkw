using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using Newtonsoft.Json.Linq;

namespace helloJkw
{
	public class BadukModule : JkwModule
	{
		string pathMemo = @"jkw/games/Baduk/memo.txt";
		string pathData = @"jkw/games/Baduk/savedata";

		public BadukModule()
		{
			Get["/games/Baduk"] = _ =>
			{
				Model.Memo = JsonConvert.DeserializeObject<string>(File.ReadAllText(pathMemo, Encoding.UTF8));

				return View["Games/Baduk/badukMain.cshtml", Model];
			};

			Post["/games/Baduk/save-memo"] = _ =>
			{
#if !DEBUG
				if (!session.IsLogin)
					return "로그인하여야 합니다.";
#endif
				string memo = Request.Form["memo"];
				File.WriteAllText(pathMemo, JsonConvert.SerializeObject(memo), Encoding.UTF8);
				return "저장되었습니다.";
			};

			Post["/games/Baduk/loadsavelist"] = _ =>
			{
#if !DEBUG
				if (!session.IsLogin)
				{
					return "[]";
				}
				var email = session.User.Email;
#else
				var email = "test@hellojkw.com";
#endif
				var dirPath = "{pathData}/{email}".WithVar(new { pathData, email });

				if (Directory.Exists(dirPath))
				{
					var list = Directory.GetFiles(dirPath)
						.Where(x => !x.Contains("__delete__"))
						.Select(x => new { Path = x, Text = File.ReadAllText(x) })
						.Select(x => JsonConvert.DeserializeObject<BadukData>(x.Text))
						.Select(x => new { x.Subject, x.Favorite })
						.OrderByDescending(x => x.Favorite)
						.ToList();
					return JsonConvert.SerializeObject(list);
				}
				else
				{
					return "[]";
				}
			};

			Post["/games/Baduk/save"] = _ =>
			{
				dynamic result = new ExpandoObject();
#if !DEBUG
				if (!session.IsLogin)
				{
					result.Status = "fail";
					result.Message = "로그인하여야 합니다.";
					return JsonConvert.SerializeObject(result);
				}
				var email = session.User.Email;
#else
				var email = "test@hellojkw.com";
#endif
				string subject = Request.Form["subject"];
				subject = subject.Trim();
				string filepath = "{pathData}/{email}/{subject}.txt".WithVar(new { pathData, email, subject });
				int size = Request.Form["size"];
				int currentIndex = Request.Form["currentIndex"];
				int indexMaximum = Request.Form["indexMaximum"];
				string memo = Request.Form["memo"];
				string stoneLogJson = Request.Form["stoneLog"];
				StoneChangeMode changeMode = (StoneChangeMode)((int)Request.Form["stoneChangeMode"]);
				StoneColor currentColor = (StoneColor)((int)Request.Form["currentColor"]);
				int confirm = Request.Form["confirm"];

#region Validation
				if (string.IsNullOrEmpty(subject))
				{
					result.Status = "fail";
					result.Message = @"제목을 적어주세요.";
					return JsonConvert.SerializeObject(result);
				}
				if (!subject.IsValidFileName())
				{
					result.Status = "fail";
					result.Message = @"파일명에 \ / : * ? "" < > | 를 사용할 수 없습니다.";
					return JsonConvert.SerializeObject(result);
				}
				if (subject.ToLower().Contains("__delete__"))
				{
					result.Status = "fail";
					result.Message = "파일명에 __delete__ 라는 단어를 포함할 수 없습니다.";
					return JsonConvert.SerializeObject(result);
				}
				if (stoneLogJson.Length > 50000)
				{
					result.Status = "fail";
					result.Message = @"기록이 너무 깁니다.";
					return JsonConvert.SerializeObject(result);
				}
				if (memo.Length > 99999)
				{
					result.Status = "fail";
					result.Message = @"메모가 너무 깁니다.";
					return JsonConvert.SerializeObject(result);
				}
				if (confirm == 0)
				{
					if (File.Exists(filepath))
					{
						result.Status = "confirm";
						result.Message = @"같은 이름이 존재합니다. 덮어쓰시겠습니까?";
						return JsonConvert.SerializeObject(result);
					}
				}
#endregion

#region Parsing StoneLogJson
				var stoneLog = ((JArray)JsonConvert.DeserializeObject(stoneLogJson))
					.Where(x => x.HasValues)
					.Select(x => (dynamic)x)
					.Select(x => new StoneData
					{
						Row = x.row,
						Column = x.column,
						Action = x.stoneAction,
						Color = x.color,
					})
					.ToList();
#endregion

				var newData = new BadukData();
				newData.OwnerEmail = email;

				// 이미 파일이 있는지 본다. 유지해야할 정보가 있다. (만든날짜, 에디터목록)
				if (File.Exists(filepath))
				{
					newData = JsonConvert.DeserializeObject<BadukData>(File.ReadAllText(filepath, Encoding.UTF8));
				}

				newData.Subject = subject;
				newData.LastModifyDate = DateTime.Now;
				newData.Size = size;
				newData.ChangeMode = changeMode;
				newData.CurrentColor = currentColor;
				newData.CurrentIndex = currentIndex;
				newData.IndexMaximum = indexMaximum;
				newData.Memo = memo;
				newData.StoneLog = stoneLog;

				var newDataJson = JsonConvert.SerializeObject(newData, Formatting.Indented);

				(new FileInfo(filepath)).Directory.Create();
				File.WriteAllText(filepath, newDataJson, Encoding.UTF8);

				result.Status = "success";
				result.Message = @"저장하였습니다.";
				return JsonConvert.SerializeObject(result);
			};

			Post["/games/Baduk/favorite"] = _ =>
			{
				dynamic result = new ExpandoObject();
#if !DEBUG
				if (!session.IsLogin)
				{
					result.Status = "fail";
					result.Message = "로그인이 필요합니다.";
					return JsonConvert.SerializeObject(result);
				}
				var email = session.User.Email;
#else
				var email = "test@hellojkw.com";
#endif
				string subject = Request.Form["subject"];
				bool favorite = Request.Form["favorite"];
				subject = subject.Trim();
				string filepath = "{pathData}/{email}/{subject}.txt".WithVar(new { pathData, email, subject });

				if (!File.Exists(filepath))
				{
					result.Status = "fail";
					result.Message = "파일을 찾을 수 없습니다.";
					return JsonConvert.SerializeObject(result);
				}

				var jsonText = File.ReadAllText(filepath, Encoding.UTF8);
				var badukData = JsonConvert.DeserializeObject<BadukData>(jsonText);

				if (badukData.OwnerEmail != email && !badukData.EditorEmail.Contains(email))
				{
					result.Status = "fail";
					result.Message = "권한이 없습니다.";
					return JsonConvert.SerializeObject(result);
				}

				badukData.Favorite = favorite;
				jsonText = JsonConvert.SerializeObject(badukData, Formatting.Indented);
				File.WriteAllText(filepath, jsonText, Encoding.UTF8);

				result.Status = "success";
				return JsonConvert.SerializeObject(result);
			};

			Post["/games/Baduk/loaddata"] = _ =>
			{
				dynamic result = new ExpandoObject();
#if !DEBUG
				if (!session.IsLogin)
				{
					result.Status = "fail";
					result.Message = "로그인이 필요합니다.";
					return JsonConvert.SerializeObject(result);
				}
				var email = session.User.Email;
#else
				var email = "test@hellojkw.com";
#endif
				string subject = Request.Form["subject"];
				subject = subject.Trim();
				string filepath = "{pathData}/{email}/{subject}.txt".WithVar(new { pathData, email, subject });

				if (!File.Exists(filepath))
				{
					result.Status = "fail";
					result.Message = "파일을 찾을 수 없습니다.";
					return JsonConvert.SerializeObject(result);
				}

				var jsonText = File.ReadAllText(filepath, Encoding.UTF8);
				var badukData = JsonConvert.DeserializeObject<BadukData>(jsonText);

				if (badukData.OwnerEmail != email && !badukData.EditorEmail.Contains(email))
				{
					result.Status = "fail";
					result.Message = "권한이 없습니다.";
					return JsonConvert.SerializeObject(result);
				}

				result.Status = "success";
				result.JsonText = JsonConvert.SerializeObject(badukData);
				return JsonConvert.SerializeObject(result);
			};

			Post["/games/Baduk/delete"] = _ =>
			{
				dynamic result = new ExpandoObject();
#if !DEBUG
				if (!session.IsLogin)
				{
					result.Status = "fail";
					result.Message = "로그인이 필요합니다.";
					return JsonConvert.SerializeObject(result);
				}
				var email = session.User.Email;
#else
				var email = "test@hellojkw.com";
#endif
				string subject = Request.Form["subject"];
				subject = subject.Trim();
				string filepath = "{pathData}/{email}/{subject}.txt".WithVar(new { pathData, email, subject });
				string deleteFilepath = "{pathData}/{email}/__delete__{subject}.txt".WithVar(new { pathData, email, subject });

				if (!File.Exists(filepath))
				{
					result.Status = "fail";
					result.Message = "파일을 찾을 수 없습니다.";
					return JsonConvert.SerializeObject(result);
				}

				var jsonText = File.ReadAllText(filepath, Encoding.UTF8);
				var badukData = JsonConvert.DeserializeObject<BadukData>(jsonText);

				if (badukData.OwnerEmail != email)
				{
					result.Status = "fail";
					result.Message = "권한이 없습니다.";
					return JsonConvert.SerializeObject(result);
				}

				File.Move(filepath, deleteFilepath);

				result.Status = "success";
				return JsonConvert.SerializeObject(result);
			};
		}
	}

	public enum StoneColor
	{
		None, Black, White
	}

	public enum StoneAction
	{
		Set, Remove
	}

	public enum StoneChangeMode
	{
		Auto, Menual
	}

	public class BadukData
	{
		public string Subject { get; set; }
		public bool Favorite { get; set; }
		public DateTime CreateDate { get; set; } = DateTime.Now;
		public DateTime LastModifyDate { get; set; }
		public string OwnerEmail { get; set; }
		public List<string> EditorEmail { get; set; }
		public int Size { get; set; }
		public StoneChangeMode ChangeMode { get; set; }
		public StoneColor CurrentColor { get; set; }
		public int CurrentIndex { get; set; }
		public int IndexMaximum { get; set; }
		public string Memo { get; set; }
		public List<StoneData> StoneLog { get; set; }
	}

	public class StoneData
	{
		public int Row { get; set; }
		public int Column { get; set; }
		public StoneAction Action { get; set; }
		public StoneColor Color { get; set; }
	}
}
