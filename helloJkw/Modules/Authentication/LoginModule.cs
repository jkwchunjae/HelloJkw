using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using helloJkw.Utils;
using Extensions;

namespace helloJkw
{
	public class LoginModule : JkwModule
	{
		public LoginModule()
		{
			Get["/register"] = _ =>
			{
				HitCounter.Hit("register");
				return View["register", Model];
			};

			Get["/login"] = _ =>
			{
				HitCounter.Hit("login");
				return View["login", Model];
			};

			Get["/user"] = _ =>
			{
				if (session.IsExpired || !session.IsLogin)
				{
					Model.RedirectUrl = "/";
					return View["redirect", Model];
				}
				HitCounter.Hit("user-setting");
				Model.Title = "유저 정보 수정";
				return View["userSetting", Model];
			};

			Get["/oauth/login", runAsync: true] = async (_, ct) =>
			{
				try
				{
					#region Session setting
					if (session.IsLogin)
					{
						UserManager.Logout(session.User);
						session.Logout();
					}
					#endregion

					#region process access_denied
					string code = Request.Query["code"];
					string error = Request.Query["error"];
					if (error == "access_denied")
					{
						throw new Exception();
					}
					#endregion

					#region get account info Async
					var jsonStr = await OAuthServer.GetAccessTokenAsync(code, "login");
					//jsonStr.Dump();
					dynamic json = JsonConvert.DeserializeObject(jsonStr);
					var accessToken = (string)json.access_token;
					//accessToken.Dump();
					var accountInfoJson = await OAuthServer.GetAccountInfoAsync(accessToken);
					//accountInfoJson.Dump();
					dynamic accountInfo = JsonConvert.DeserializeObject(accountInfoJson);
					#endregion

					#region user login
					// 로그인한다는 뜻은 무조건 가입한다는 뜻으로 처리하자.
					// 분리는 불필요하다고 판단함.
					User user = UserManager.Register(accountInfo);
					Logger.Log("login: {0}".With(user.Name));
					Logger.Log("login / {0}".With(session.SessionId));
					session.Login(user);
					#endregion
				}
				#region Exceptions
				catch (InValidAccountIdException ex)
				{
					Logger.Log(ex);
				}
				catch (Exception ex)
				{
					//session.Logout();
					Logger.Log(ex);
				}
				#endregion

				return View["redirectLogin", Model];
			};

			#region Get /oauth/register
			//Get["/oauth/register", runAsync: true] = async (_, ct) =>
			//{
			//	try
			//	{
			//		#region Session setting
			//		if (session.IsLogin)
			//		{
			//			UserManager.Logout(session.User);
			//			session.Logout();
			//		}
			//		#endregion

			//		#region process access_denied
			//		string code = Request.Query["code"];
			//		string error = Request.Query["error"];
			//		if (error == "access_denied")
			//		{
			//			throw new AccessDeniedException();
			//		}
			//		#endregion

			//		#region get account info Async
			//		var jsonStr = await OAuthServer.GetAccessTokenAsync(code, "register");
			//		//jsonStr.Dump();
			//		dynamic json = JsonConvert.DeserializeObject(jsonStr);
			//		var accessToken = (string)json.access_token;
			//		//accessToken.Dump();
			//		var accountInfoJson = await OAuthServer.GetAccountInfoAsync(accessToken);
			//		//accountInfoJson.Dump();
			//		dynamic accountInfo = JsonConvert.DeserializeObject(accountInfoJson);
			//		#endregion

			//		#region user login
			//		User user = UserManager.Register(accountInfo);
			//		session.Login(user);
			//		#endregion
			//	}
			//	#region Exceptions
			//	catch (AccessDeniedException)
			//	{
			//		Model.RedirectUrl = "/error?type=access-denied";
			//		return View["redirect", Model];
			//	}
			//	catch (InValidAccountIdException)
			//	{
			//		Model.RedirectUrl = "/error?type=invalid-accountid";
			//		return View["redirect", Model];
			//	}
			//	catch (RegistrationFailException)
			//	{
			//		Model.RedirectUrl = "/error?type=registration-fail";
			//		return View["redirect", Model];
			//	}
			//	catch (Exception ex)
			//	{
			//		session.Logout();
			//		Logger.Log(ex);
			//	}
			//	#endregion

			//	Model.RedirectUrl = "/";
			//	return View["redirect", Model];
			//};
			#endregion

			Get["/logout"] = _ =>
			{
				if (session.IsLogin)
				{
					UserManager.Logout(session.User);
					session.Logout();
				}
				Model.RedirectUrl = "/";
				return View["redirect", Model];
			};

			Post["/user-setting", runAsync: true] = async (ctx, ct) =>
			{
				try
				{
					if (session.IsExpired || !session.IsLogin)
						throw new Exception();
					var user = session.User;

					var bytes = new byte[Request.Body.Length];
					int l = await Request.Body.ReadAsync(bytes, 0, (int)Request.Body.Length);
					string infostr = Encoding.UTF8.GetString(bytes);
					//string infostr = bytes.JQueryAjaxEncoding();
					dynamic accountInfo = JsonConvert.DeserializeObject(infostr);

					if (accountInfo.name == null || accountInfo.name == "")
						throw new Exception();

					user.Name = accountInfo.name;
					user.SaveUserName();
				}
				catch (Exception ex)
				{
					Logger.Log(ex);
					return "fail";
				}
				return "success";
			};
		}
	}
}
