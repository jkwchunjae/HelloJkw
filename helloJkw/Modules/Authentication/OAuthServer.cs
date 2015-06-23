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
		static string clientId = "CLIENT_ID";
		static string clientSecret = "CLIENT_SECRET";
		static string apiKey = "API_KEY";

		static OAuthServer()
		{
			var filepath = @"jkw/db/googleHelloJkwOAuthInfo.txt";
			var json = File.ReadAllText(filepath, Encoding.UTF8);

			dynamic OAuthInfo = JsonConvert.DeserializeObject(json);
			clientId = OAuthInfo.clientId;
			clientSecret = OAuthInfo.clientSecret;
			apiKey = OAuthInfo.apiKey;

			//clientId.Dump();
			//clientSecret.Dump();
			//apiKey.Dump();
		}

		public static async Task<string> GetAccessTokenAsync(string code)
		{
			string json = string.Empty;
			try
			{
				var url = "https://www.googleapis.com/oauth2/v3/token";
				var data = new Dictionary<string, string>();
				data["code"] = code;
				data["client_id"] = clientId;
				data["client_secret"] = clientSecret;
#if DEBUG
				data["redirect_uri"] = "http://localhost/login";
#else
				data["redirect_uri"] = "http://hellojkw.com/login";
#endif
				data["grant_type"] = "authorization_code";
				var param = data.Select(e => "{0}={1}".With(e.Key, e.Value)).StringJoin("&");
				var paramBytes = Encoding.ASCII.GetBytes(param);

				var request = WebRequest.CreateHttp(url);

				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";
				request.Host = "www.googleapis.com";
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

		public static async Task<string> GetAccountInfoAsync(string access_token)
		{
			string json = string.Empty;
			try
			{
				var url = "https://www.googleapis.com/plus/v1/people/me";

				var request = WebRequest.CreateHttp(url);
				request.Method = "GET";
				request.ContentType = "application/x-www-form-urlencoded";
				request.Host = "www.googleapis.com";
				request.ContentLength = 0;
				request.Headers.Add("Authorization", "Bearer " + access_token);
				request.Headers.Add("key", apiKey);

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
	}
}
