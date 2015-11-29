using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace helloJkw
{
	public static class UserManager
	{
		/* 로그인되어있는 유저를 관리한다. */
		static ConcurrentDictionary<string /* google id number */, User> _userDic = new ConcurrentDictionary<string, User>();
		static string _jsonPath = @"jkw/db/userInfoJson.txt";
		static ConcurrentDictionary<string /* google id number */, UserInfoJson> _userInfoJsonDic = new ConcurrentDictionary<string, UserInfoJson>();

		public static void SaveJson()
		{
			SaveJson(_jsonPath);
		}

		public static void SaveJson(string path)
		{
			// write lock
			var json = _userDic.Select(x => new UserInfoJson(x.Value))
				.Select(x => JsonConvert.SerializeObject(x))
				.StringJoin("[\n\t", ",\n\t", "\n]");
			File.WriteAllText(path, json, Encoding.UTF8);
		}

		public static ConcurrentDictionary<string /* google id number */, UserInfoJson> LoadJson(bool reload = false)
		{
			return LoadJson(_jsonPath, reload);
		}

		public static ConcurrentDictionary<string /* google id number */, UserInfoJson> LoadJson(string path, bool reload = false)
		{
			// write lock
			var userInfoJsonList = JsonConvert.DeserializeObject<List<UserInfoJson>>(File.ReadAllText(path));
			_userInfoJsonDic.Clear();
			foreach (var userInfoJson in userInfoJsonList)
			{
				_userInfoJsonDic.TryAdd(userInfoJson.Id, userInfoJson);
			}
			return _userInfoJsonDic;
		}

		static UserInfoJson GetJsonInfo(User user)
		{
			if (user == null) return new UserInfoJson();

			var userInfoJsonDic = LoadJson();
			UserInfoJson userInfoJson;
			userInfoJsonDic.TryGetValue(user.Id, out userInfoJson);
			return userInfoJson;
		}

		public static User GetUser(string id)
		{
			User user;
			_userDic.TryGetValue(id, out user);
			if (user == null)
				user = UserDatabase.GetUser(id);
			if (user == null)
				return null;
			var userInfoJson = GetJsonInfo(user);
			return user.UpdateJsonInfo(userInfoJson);
		}

		public static User Login(User user)
		{
			Logger.Log("login: {0}, {1}".With(user.Id, user.Name));
			if (!_userDic.ContainsKey(user.Id))
				_userDic.TryAdd(user.Id, user);

			user.LastLogin = DateTime.Now;
			user.SaveLastLogin();
			return user;
		}

		public static User Register(dynamic accountInfo)
		{
			#region Get Id and user info
			#region get Id, cutting id error
			string id = accountInfo.id;
			if (id == null)
			{
				// id는 꼭 있어야 하는 정보이다.
				// 없으면 실패로 처리하자.
				throw new InValidAccountIdException();
			}
			#endregion

			#region parsing user info
			// nickName 이 없으면 displayName 이라도..
			string userName = accountInfo.nickname != null ? accountInfo.nickname : accountInfo.displayName;

			var emails = (JArray)accountInfo.emails;
			string email = emails
				.Select(x => (dynamic)x)
				.Where(x => x.type == "account")
				.Select(x => x.value)
				.FirstOrDefault();

			string imageUrl;
			try { imageUrl = ((string)accountInfo.image.url).RegexReplace(@"\?.*", ""); }
			catch { imageUrl = null; }
			#endregion
			#endregion

			Logger.Log("Register: {0}".With(id));

			#region register
			User user = UserDatabase.Register(id, userName, email, imageUrl);
			#endregion

			#region update user info
			if (user.IsUseGoogleImage && imageUrl != user.ImageUrl)
			{
				user.SaveUserImage(imageUrl);
			}
			var userInfoJson = GetJsonInfo(user);
			user.UpdateJsonInfo(userInfoJson);
			#endregion

			Login(user);

			return user;
		}

		public static void Logout(User user)
		{
			if (user == null)
				return;
			_userDic.TryRemove(user.Id, out user);
		}
	}
}
