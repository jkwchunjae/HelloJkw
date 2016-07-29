using Extensions;
using Nancy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public static class SessionManager
	{
		static ConcurrentDictionary<string /* sessionId = GUID */, Session> _sessionDic = new ConcurrentDictionary<string, Session>();

		public static Session NewSession()
		{
			var session = new Session();
			_sessionDic.TryAdd(session.SessionId, session);
			return session;
		}

		public static void Remove(string sessionId)
		{
			Session session;
			_sessionDic.TryRemove(sessionId, out session);
		}

		public static bool IsValid(string sessionId)
		{
			return _sessionDic.ContainsKey(sessionId) && _sessionDic[sessionId].IsAlive;
		}

		public static Session GetSession(string sessionId)
		{
			if (_sessionDic.ContainsKey(sessionId))
			{
				var session = _sessionDic[sessionId];
				return session;
			}
			else
			{
				var session = new Session(sessionId);
				_sessionDic.TryAdd(sessionId, session);
			}

			if (StaticRandom.Next(1, 10) == 1) ///< 10% 확률로 Session을 정리한다.
				RemoveExpiredSession();
			return null;
		}

		public static bool ChangeSessionId(Session session, string newSessionId)
		{
			Session tmpSession;
			if (_sessionDic.Where(x => x.Value == session).Any())
			{
				var sessionIdList = _sessionDic.Where(x => x.Value == session).Select(x => x.Key).ToList();
				foreach (var sessionId in sessionIdList)
				{
					_sessionDic.TryRemove(sessionId, out tmpSession);
				}
			}
			if (_sessionDic.TryAdd(newSessionId, session))
			{
				return true;
			}
			return false;
		}

		public static void RemoveExpiredSession()
		{
			lock (_sessionDic)
			{
				foreach (var session in _sessionDic.Select(e => e.Value))
				{
					if (session.IsExpired)
					{
						UserManager.Logout(session.User);
						Remove(session.SessionId);
					}
				}
			}
		}

		/// <summary>
		/// Request's Cookie에서 session_id를 구한다.
		/// 없으면 새 session을 부여한다.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		public static string GetSessionId(this Request request)
		{
			if (request.Cookies.ContainsKey("session_id"))
			{
				string sessionId = request.Cookies["session_id"];
				if (!_sessionDic.ContainsKey(sessionId))
					_sessionDic.TryAdd(sessionId, new Session(sessionId));
				return sessionId;
			}
			else
			{
				return NewSession().SessionId;
			}
		}
	}
}
