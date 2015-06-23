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
			Get["/login", runAsync: true] = async (_, ct) =>
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
					var jsonStr = await OAuthServer.GetAccessTokenAsync(code);
					//jsonStr.Dump();
					dynamic json = JsonConvert.DeserializeObject(jsonStr);
					var accessToken = (string)json.access_token;
					//accessToken.Dump();
					var accountInfoJson = await OAuthServer.GetAccountInfoAsync(accessToken);
					//accountInfoJson.Dump();
					dynamic accountInfo = JsonConvert.DeserializeObject(accountInfoJson);
					#endregion

					#region user login
					User user;
					if (UserManager.Login(accountInfo, out user))
					{
						session.Login(user);
					}
					else
					{
						session.Logout();
					}
					#endregion
				}
				catch
				{
					session.Logout();
				}
				
				return View["beforeHome", Model];
			};

			Get["/logout"] = _ =>
			{
				if (session.IsLogin)
				{
					UserManager.Logout(session.User);
					session.Logout();
				}
				return View["beforeHome", Model];
			};
		}
	}
}
