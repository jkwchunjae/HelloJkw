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

		public JkwModule()
		{
#if (DEBUG)
			Model.isDebug = true;
#else
			Model.isDebug = false;
#endif
			Before += ctx =>
			{
				SetSession(ctx);
				return null;
			};

			After += ctx =>
			{
				SetSession(ctx);
				ctx.Response.WithCookie("session_id", session.SessionId);
			};
		}

		public void SetSession(NancyContext context)
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
				var files = Directory.GetFiles(@"Static/Agency/img/bg/", "*");
				var gameRoot = @"jkw/games";
				var games = Directory.GetDirectories(gameRoot)
					.Select(path => new
					{
						engName = Path.GetFileName(path),
						korName = File.ReadAllText(Path.Combine(path, "info.txt")),
						thumbnail = Path.GetFileName(Directory.GetFiles(path, "*thumbnail*").FirstOrDefault()),
					}.ToExpando());

				Model.BackGroundFileName = Path.GetFileName(files.GetRandom());
				Model.games = games;
				return View["jkwHome", Model];
			};

			Get["/error"] = _ =>
			{
				string type = Request.Query.type;
				if (type == "not-registered-user")
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
구글 로그인 과정에서 ID 를 받아오지 못했습니다.
<br/>
에러가 계속된다면 뭔가 문제가 있는것입니다.
";
				}
				else if (type == "access-denied")
				{
					Model.ErrorTitle = "구글계정 접근 거부";
					Model.ErrorMessage = @"
구글 계정 사용에 동의하지 않았습니다.
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
