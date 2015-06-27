using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public class LoginModule : JkwModule
	{
		public LoginModule()
		{
			Get["/register"] = _ =>
			{
				return View["register", Model];
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
					User user = UserManager.Login(accountInfo);
					session.Login(user);
					#endregion
				}
				#region Exceptions
				catch (InValidAccountIdException)
				{
				}
				catch (NotRegisteredUserException)
				{
					Model.RedirectUrl = "/error?type=not-registered-user";
					return View["redirect", Model];
				}
				catch
				{
					session.Logout();
				}
				#endregion

				Model.RedirectUrl = "/";
				return View["redirect", Model];
			};

			Get["/oauth/register", runAsync: true] = async (_, ct) =>
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
						throw new AccessDeniedException();
					}
					#endregion

					#region get account info Async
					var jsonStr = await OAuthServer.GetAccessTokenAsync(code, "register");
					//jsonStr.Dump();
					dynamic json = JsonConvert.DeserializeObject(jsonStr);
					var accessToken = (string)json.access_token;
					//accessToken.Dump();
					var accountInfoJson = await OAuthServer.GetAccountInfoAsync(accessToken);
					//accountInfoJson.Dump();
					dynamic accountInfo = JsonConvert.DeserializeObject(accountInfoJson);
					#endregion

					#region user login
					User user = UserManager.Register(accountInfo);
					session.Login(user);
					#endregion
				}
				#region Exceptions
				catch (AccessDeniedException)
				{
					Model.RedirectUrl = "/error?type=access-denied";
					return View["redirect", Model];
				}
				catch (InValidAccountIdException)
				{
					Model.RedirectUrl = "/error?type=invalid-accountid";
					return View["redirect", Model];
				}
				catch (AlreadyRegisterdException)
				{
					Model.RedirectUrl = "/error?type=already-registered";
					return View["redirect", Model];
				}
				catch (RegistrationFailException)
				{
					Model.RedirectUrl = "/error?type=registration-fail";
					return View["redirect", Model];
				}
				catch
				{
					session.Logout();
				}
				#endregion

				Model.RedirectUrl = "/";
				return View["redirect", Model];
			};

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
		}
	}
}
