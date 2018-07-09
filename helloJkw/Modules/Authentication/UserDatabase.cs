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
		static UserDatabase()
		{
		}

		public static bool IsRegister(string id)
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
				var user = new User(no, id, regDate: DateTime.Now) { Name = userName, Email = email, ImageUrl = imageUrl, LastLogin = DateTime.Now, Grade = UserGrade.Friend };
				string query = @"insert into users (id, no, name, email, grade, regdate, lastdate, imageurl) 
										values(@id, @no, @name, @email, @grade, @regdate, @lastdate, @imageurl);";
				var cmd = query.CreateCommand();
				#region Setting Prams
				cmd.Parameters.AddWithValue("@id", user.Id);
				cmd.Parameters.AddWithValue("@no", user.No);
				cmd.Parameters.AddWithValue("@name", user.Name);
				cmd.Parameters.AddWithValue("@email", user.Email);
				cmd.Parameters.AddWithValue("@grade", user.Grade.ToString());
				cmd.Parameters.AddWithValue("@regdate", user.RegDate);
				cmd.Parameters.AddWithValue("@lastdate", user.LastLogin);
				cmd.Parameters.AddWithValue("@imageurl", user.ImageUrl);
				#endregion

				if (cmd.ExecuteNonQuery() == 1)
				{
					return user;
				}
				else
				{
					throw new RegistrationFailException();
				}
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
				var query = "select * from users where id = @id;";
				using (var cmd = query.CreateCommand())
				{
					cmd.Parameters.AddWithValue("@id", id);
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							return new User(
								no: reader.GetInt32("no"),
								id: reader.GetString("id"),
								regDate: reader.GetDateTime("regDate")
							)
							{
								Name = reader.GetString("name"),
								Grade = (UserGrade)Enum.Parse(typeof(UserGrade), reader.GetString("grade")),
								LastLogin = reader.GetDateTime("lastdate"),
								ImageUrl = reader.GetString("imageurl"),
								Email = reader.GetString("email"),
							};
						}
						else
						{
							return null;
						}
					}
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
            var userList = new List<User>();
            try
            {
                var query = "select * from users where email is not null;";
                using (var cmd = query.CreateCommand())
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new User(
                                no: reader.GetInt32("no"),
                                id: reader.GetString("id"),
                                regDate: reader.GetDateTime("regDate")
                            )
                            {
                                Name = reader.GetString("name"),
                                Grade = (UserGrade)Enum.Parse(typeof(UserGrade), reader.GetString("grade")),
                                LastLogin = reader.GetDateTime("lastdate"),
                                ImageUrl = reader.GetString("imageurl"),
                                Email = reader.GetString("email"),
                            };

                            userList.Add(user);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
            return userList;
        }

		private static int GetLastNo()
		{
			string query = "select max(no) as maxno from users;";
			using (var reader = DB.ExecuteReader(query))
			{
				if (reader.Read())
				{
					return (int)reader["maxno"];
				}
				else
				{
					return 0;
				}
			}
			throw new Exception();
		}

		public static void SaveLastLogin(this User user)
		{
			Logger.Log("save user last login: {0}, {1}".With(user.Id, user.LastLogin));
			string query = @"update users set lastdate=@lastdate where id=@id;";
			using (var cmd = query.CreateCommand())
			{
				cmd.Parameters.AddWithValue("@id", user.Id);
				cmd.Parameters.AddWithValue("@lastdate", user.LastLogin);
				cmd.ExecuteNonQuery();
			}
		}

		public static void SaveUserName(this User user)
		{
			Logger.Log("save user name: {0}, {1}".With(user.Id, user.Name));
			string query = @"update users set name=@name where id=@id;";
			using (var cmd = query.CreateCommand())
			{
				cmd.Parameters.AddWithValue("@id", user.Id);
				cmd.Parameters.AddWithValue("@name", user.Name);
				cmd.ExecuteNonQuery();
			}
		}

		public static void SaveUserImage(this User user, string imageUrl = null)
		{
			if (imageUrl != null) user.ImageUrl = imageUrl;
			Logger.Log("save user image: {0}, {1}".With(user.Id, user.ImageUrl));
			string query = @"update users set imageurl=@imageurl where id=@id;";
			using (var cmd = query.CreateCommand())
			{
				cmd.Parameters.AddWithValue("@id", user.Id);
				cmd.Parameters.AddWithValue("@imageurl", user.ImageUrl);
				cmd.ExecuteNonQuery();
			}
		}
	}
}
