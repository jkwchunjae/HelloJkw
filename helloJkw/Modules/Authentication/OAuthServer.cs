using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Collections.Specialized;
using Extensions;
using System.IO;
using Newtonsoft.Json;
using System.Threading;

namespace helloJkw
{
	public static class OAuthServer
	{
		class OAuthInfo
		{
			public class OAuthInfoData
			{
				public string ClientId { get; set; } = "";
				public string ApiKey { get; set; } = "";
				public string RedirectUri { get; set; } = "";
				public string ClientSecret { get; set; } = "";
			}

			public OAuthInfoData Google { get; set; } = new OAuthInfoData();
			public OAuthInfoData Kakao { get; set; } = new OAuthInfoData();
		}

		static OAuthInfo _oauthInfo = null;

		static OAuthServer()
		{
			var filepath = @"jkw/db/helloJkwOAuthInfo.txt";
			var json = File.ReadAllText(filepath, Encoding.UTF8);

			_oauthInfo = JsonConvert.DeserializeObject<OAuthInfo>(json);
		}

		#region Google
		public static async Task<string> GetGoogleAccessTokenAsync(string code, string siteBase)
		{
			string json = string.Empty;
			try
			{
				var url = "https://www.googleapis.com/oauth2/v3/token";
				var data = new Dictionary<string, string>();
				data["code"] = code;
				data["client_id"] = _oauthInfo.Google.ClientId;
				data["client_secret"] = _oauthInfo.Google.ClientSecret;
				data["redirect_uri"] = siteBase + _oauthInfo.Google.RedirectUri;
				data["grant_type"] = "authorization_code";
				var param = data.Select(e => "{0}={1}".With(e.Key, e.Value)).StringJoin("&");
				var paramBytes = Encoding.ASCII.GetBytes(param);

				var request = WebRequest.Create(url);

				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = paramBytes.Length;

				using (var stream = await request.GetRequestStreamAsync())
				{
					stream.Write(paramBytes, 0, paramBytes.Length);
				}

				var response = (HttpWebResponse)await request.GetResponseAsync();

				var responseString = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
				json = responseString;
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
			}
			return json;
		}

		public static async Task<string> GetGoogleAccountInfoAsync(string access_token)
		{
			string json = string.Empty;
			try
			{
				var url = "https://www.googleapis.com/plus/v1/people/me";

				var request = WebRequest.Create(url);
				request.Method = "GET";
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = 0;
				request.Headers.Add("Authorization", "Bearer " + access_token);
				request.Headers.Add("key", _oauthInfo.Google.ApiKey);

				var response = (HttpWebResponse) await request.GetResponseAsync();
				json = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
				Logger.Log(json);
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				return json;
			}
			return json;
		}
		#endregion

		#region Kakao
		public static async Task<string> GetKakaoAccessTokenAsync(string code, string siteBase)
		{
			string json = string.Empty;
			try
			{
				var url = "https://kauth.kakao.com/oauth/token";
				var data = new Dictionary<string, string>();
				data["code"] = code;
				data["client_id"] = _oauthInfo.Kakao.ClientId;
				data["client_secret"] = _oauthInfo.Kakao.ClientSecret;
				data["redirect_uri"] = siteBase + _oauthInfo.Kakao.RedirectUri;
				data["grant_type"] = "authorization_code";
				var param = data.Select(e => "{0}={1}".With(e.Key, e.Value)).StringJoin("&");
				var paramBytes = Encoding.ASCII.GetBytes(param);

				var request = WebRequest.Create(url);

				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.ContentLength = paramBytes.Length;

				using (var stream = await request.GetRequestStreamAsync())
				{
					stream.Write(paramBytes, 0, paramBytes.Length);
				}

				var response = (HttpWebResponse)await request.GetResponseAsync();

				var responseString = await new StreamReader(response.GetResponseStream()).ReadToEndAsync();
				json = responseString;
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
			}
			return json;
		}



		#endregion
	}
}
