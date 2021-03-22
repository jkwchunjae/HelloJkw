using Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace helloJkw
{
	public static class UserDatabase
	{
		private static readonly string _dbRoot = "jkw/db/users/";

		static UserDatabase()
		{
		}

		private static bool IsRegister(string id)
		{
			var user = GetUser(id);
			return user != null;
		}

		public static User Register(string id, string userName, string email, string imageUrl)
		{
			if (IsRegister(id))
			{
				return GetUser(id);
			}

			try
			{
				int no = GetLastNo() + 1;
				var user = new User(no, id, regDate: DateTime.Now)
				{
					Name = userName,
					Email = email,
					ImageUrl = imageUrl,
					LastLogin = DateTime.Now,
					Grade = UserGrade.Friend
				};

				SaveUser(user);
				return user;
			}
			catch(Exception ex)
			{
				Logger.Log(ex);
				throw new RegistrationFailException();
			}
		}

		public static User GetUser(string id)
		{
			#region GetUser from id
			try
			{
				var userFilePath = Path.Combine(_dbRoot, $"user.google.{id}.json");
				if (File.Exists(userFilePath))
				{
					var text = File.ReadAllText(userFilePath);
					var user = JsonConvert.DeserializeObject<User>(text);
					return user;
				}
				else
				{
					return null;
				}
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
			}
			finally
			{
			}
			return null;
			#endregion
		}

		public static List<User> GetAllUser()
		{
			var userIdList = Directory.GetFiles(_dbRoot)
				.Select(path => Path.GetFileNameWithoutExtension(path).Replace("user.google.", ""));

			var userList = userIdList
				.Select(id => GetUser(id))
				.ToList();

			return userList;
		}

		private static int GetLastNo()
		{
			var users = GetAllUser();
			if (users.Any())
			{
				return users.Max(x => x.No);
			}
			else
			{
				return 0;
			}
		}

		private static void SaveUser(User user)
		{
			var userFilePath = Path.Combine(_dbRoot, $"user.google.{user.Id}.json");
			var userJsonText = JsonConvert.SerializeObject(user, Formatting.Indented);
			File.WriteAllText(userFilePath, userJsonText, Encoding.UTF8);
		}

		public static void SaveLastLogin(this User user)
		{
			SaveUser(user);
		}

		public static void SaveUserName(this User user)
		{
			SaveUser(user);
		}

		public static void SaveUserImage(this User user, string imageUrl = null)
		{
			if (imageUrl != null)
			{
				user.ImageUrl = imageUrl;
			}
			SaveUser(user);
		}
	}
}
