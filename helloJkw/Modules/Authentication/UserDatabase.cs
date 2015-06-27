using Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public static class UserDatabase
	{
		static string filepath = @"jkw/db/users.txt";

		/* 회원가입한 모든 유저 정보 */
		static ConcurrentDictionary<string /* google id number */, User> _userDic = new ConcurrentDictionary<string, User>();

		static UserDatabase()
		{
			try
			{
				var json = File.ReadAllText(filepath, Encoding.UTF8);
				List<User> userList = JsonConvert.DeserializeObject<List<User>>(json);
				foreach (var user in userList)
				{
					_userDic.TryAdd(user.Id, user);
				}
			}
			catch (FileNotFoundException)
			{
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
			}
		}

		public static bool Save()
		{
			try
			{
				var json = JsonConvert.SerializeObject(_userDic.Select(e => e.Value));
#if DEBUG
#else
				File.WriteAllText(filepath, json, Encoding.UTF8);
#endif
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
				return false;
			}
			return true;
		}

		public static bool IsRegister(string id)
		{
			return _userDic.ContainsKey(id);
		}

		public static User Register(string id, string userName, string imageUrl)
		{
			if (IsRegister(id))
				throw new AlreadyRegisterdException();

			User user = null;
			lock (_userDic)
			{
				int no = 1;
				if (_userDic.Count() > 0)
					no = _userDic.Select(e => e.Value).Max(t => t.No) + 1;
				user = new User(no, id, regDate: DateTime.Now) { Name = userName, ImageUrl = imageUrl };
				if (!_userDic.TryAdd(id, user))
					throw new RegistrationFailException();
			}
			Save();
			return user;
		}

		//public static bool Update(User updateUser)
		//{
		//	User user;
		//	if (_userDic.TryGetValue(updateUser.Id, out user))
		//	{
		//		_userDic.TryUpdate(updateUser.Id, updateUser, user);
		//		Save();
		//		return true;
		//	}
		//	return false;
		//}

		public static User GetUser(string id)
		{
			if (IsRegister(id))
				return _userDic[id];
			return null;
		}
	}
}
