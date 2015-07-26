using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;
using System.Collections.Concurrent;
using System.IO;

namespace helloJkw
{
	public static class UserManager
	{
		/* 로그인되어있는 유저를 관리한다. */
		static ConcurrentDictionary<string /* google id number */, User> _userDic = new ConcurrentDictionary<string, User>();

		public static User GetUser(string id)
		{
			if (_userDic.ContainsKey(id))
				return _userDic[id];
			return null;
		}

		public static User Login(dynamic accountInfo)
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
			string userName = accountInfo.nickname != null ? accountInfo.nickname : accountInfo.displayname;
			
			string imageUrl;
			try { imageUrl = ((string)accountInfo.image.url).RegexReplace(@"\?.*", ""); }
			catch { imageUrl = null; }
			#endregion
			#endregion

			#region add user
			if (!_userDic.ContainsKey(id))
				_userDic.TryAdd(id, UserDatabase.GetUser(id));
			User user = GetUser(id);
			#endregion

			#region update user info
			if (user.IsUseGoogleImage && imageUrl != null)
			{
				user.ImageUrl = imageUrl;
			}
			#endregion

			user.LastLogin = DateTime.Now;
			user.SaveLastLogin();
			return user;
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

			string imageUrl;
			try { imageUrl = ((string)accountInfo.image.url).RegexReplace(@"\?.*", ""); }
			catch { imageUrl = null; }
			#endregion
			#endregion

			Logger.Log("Register: {0}".With(id));

			#region register
			User user = UserDatabase.Register(id, userName, imageUrl);
			#endregion

			#region update user info
			if (user.IsUseGoogleImage && imageUrl != user.ImageUrl)
			{
				user.SaveUserImage(imageUrl);
			}
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
