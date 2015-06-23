using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace helloJkw
{
	public class Session
	{
		public string SessionId { get { return _sessionId; } }
		public User User { get { return _user; } }

		string _sessionId;
		DateTime _expire;
		User _user = null;

		public bool IsAlive { get { return DateTime.Now <= _expire; } }
		public bool IsExpired { get { return !IsAlive; } }
		public bool IsLogin { get { return _user != null; } }

		public Session()
		{
			_sessionId = Guid.NewGuid().ToString();
			RefreshExpire();
		}
		
		public Session(string sessionId)
		{
			_sessionId = sessionId;
			RefreshExpire();
		}

		public Session(User user)
		{
			_sessionId = Guid.NewGuid().ToString();
			RefreshExpire();
			_user = user;
		}
		
		public void Login(User user)
		{
			_user = user;
			RefreshExpire();
		}

		public void Logout()
		{
			_user = null;
		}

		public void RefreshExpire()
		{
			_expire = DateTime.Now.AddMinutes(60);
			//_expire = DateTime.Now.AddSeconds(10);
		}
	}
}
