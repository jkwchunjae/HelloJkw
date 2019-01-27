using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using helloJkw.Utils;
using System.Dynamic;

namespace helloJkw
{
	public class JkwModule : NancyModule
	{
		public dynamic Model = new ExpandoObject();
		public string sessionId = null;
		public Session session = null;
        protected bool IsDebug { get; set; } = false;

		public JkwModule()
		{
#if (DEBUG)
			Model.isDebug = true;
            IsDebug = true;
#else
			Model.isDebug = false;
#endif
			Before += ctx =>
			{
				Model.SiteBase = Request.Url.SiteBase;
				SetSession();
				return null;
			};

			After += ctx =>
			{
				ctx.Response.WithCookie("session_id", session.SessionId);
			};
		}

		public void SetSession()
		{
			sessionId = Request.GetSessionId();
			session = SessionManager.GetSession(sessionId);

			if (session.IsExpired)
			{
				session.Logout();
			}
			else
			{
				session.RefreshExpire();
			}
			Model.isLogin = session.IsLogin;
			if (session.IsLogin)
			{
				Model.user = session.User;
			}
		}
	}

	public class JkwHomeModule : JkwModule
	{
		public JkwHomeModule()
		{
			Get["/"] = _ =>
			{
				//HitCounter.Hit("hellojkw home");

				Model.BackGroundFileName = Directory.GetFiles(@"Static/Agency/img/bg/", "*").GetRandom();
				Model.games = GameCenter.GetGameList().Select(e => e.ToExpando());
				return View["jkwHome", Model];
			};

            Get["/games/worldcup"] = _ => Response.AsRedirect("/worldcup");

#if (DEBUG)
			Get["/playground"] = _ =>
			{
				return View["playground", Model];
			};
#endif

            Get["/wedding/{preview?}"] = _ =>
            {
                //string previewImage = _.preview;
                //Model.PreviewImage = previewImage ?? "preview2";
                //return View["wedding/weddingHome.cshtml", Model];

                return Response.AsRedirect("/kyungwon-taehee");
            };

            Get["/kyungwon-taehee/{option?}"] = _ =>
            {
                Model.PreviewImage = "preview2";
                string option = _.option;
                option = option?.ToLower() ?? "";
                Model.IsCatholic = option.Contains("catholic");

                Model.LetterTaehee = "";
                Model.LetterKyungwon = "";

                if (session.IsLogin)
                {
                    Action<string, string> SetLetter = (to, from) =>
                    {
                        var dirPath = $"jkw/project/wedding/letters/{to}";
                        var filePath = $"{dirPath}/{from}.txt";

                        if (File.Exists(filePath))
                        {
                            if (to == "taehee")
                                Model.LetterTaehee = File.ReadAllText(filePath, Encoding.UTF8);
                            if (to == "kyungwon")
                                Model.LetterKyungwon = File.ReadAllText(filePath, Encoding.UTF8);
                        }
                    };

                    string fromm = session.User.Name + "." + session.User.Email;
                    SetLetter("taehee", fromm);
                    SetLetter("kyungwon", fromm);
                }
                return View["wedding/weddingHome.cshtml", Model];
            };

            Get["/happy-wedding"] = _ =>
            {
                Model.PreviewImage = "jsg-preview";
                string option = _.option;
                option = option?.ToLower() ?? "";
                Model.IsCatholic = true;

                Model.LetterJsg = "";
                Model.LetterKck = "";

                if (session.IsLogin)
                {
                    Action<string, string> SetLetter = (to, from) =>
                    {
                        var dirPath = $"jkw/project/wedding/letters/{to}";
                        var filePath = $"{dirPath}/{from}.txt";

                        if (File.Exists(filePath))
                        {
                            if (to == "jsg")
                                Model.LetterJsg = File.ReadAllText(filePath, Encoding.UTF8);
                            if (to == "kck")
                                Model.LetterKck = File.ReadAllText(filePath, Encoding.UTF8);
                        }
                    };

                    string fromm = session.User.Name + "." + session.User.Email;
                    SetLetter("jsg", fromm);
                    SetLetter("kck", fromm);
                }
                return View["wedding/wedding-kck-jsg.cshtml", Model];
            };

            Post["/wedding/letters"] = _ =>
            {
                if (!session.IsLogin)
                    return HttpStatusCode.Forbidden;

                string from = session.User.Name + "." + session.User.Email;
                string to = Request.Form["to"];
                string letter = Request.Form["letter"];

                if (letter.Length > 40000)
                    return HttpStatusCode.BadRequest;

                var dirPath = $"jkw/project/wedding/letters/{to}";
                var filePath = $"{dirPath}/{from}.txt";
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);

                File.WriteAllText(filePath, letter, Encoding.UTF8);

                var logDirPath = $"jkw/project/wedding/letters/{to}/log";
                var logPath = $"{logDirPath}/{from}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
                if (!Directory.Exists(logDirPath))
                    Directory.CreateDirectory(logDirPath);

                File.WriteAllText(logPath, letter, Encoding.UTF8);

                return HttpStatusCode.OK;
            };

			Get["/error"] = _ =>
			{
				string type = Request.Query.type;
				if (type == "already-registered")
				{
					Model.ErrorTitle = "회원가입 에러";
					Model.ErrorMessage = @"
이미 회원가입 되어 있습니다.
로그인을 해주세요.
";
				}
				else if (type == "not-registered-user")
				{
					Model.ErrorTitle = "로그인 에러";
					Model.ErrorMessage = @"
회원가입되지 않은 사용자의 로그인 시도였습니다.
<a href=""/register"">회원가입</a> 먼저 해주세요.
";
				}
				else if (type == "invalid-accountid")
				{
					Model.ErrorTitle = "로그인 에러";
					Model.ErrorMessage = @"
로그인 과정에서 ID 를 받아오지 못했습니다.
<br/>
에러가 계속된다면 뭔가 문제가 있는것입니다.
";
				}
				else if (type == "access-denied")
				{
					Model.ErrorTitle = "계정 접근 거부";
					Model.ErrorMessage = @"
계정 사용에 동의하지 않았습니다.
<br />
이름, 사진만 사용할건데, 싫으시면 할 수 없지요.
";
				}
				else
				{
					Model.ErrorTitle = "Error!";
					Model.ErrorMessage = "정의되지 않은 에러입니다.";
				}
				return View["error", Model];
			};
		}
	}
}
