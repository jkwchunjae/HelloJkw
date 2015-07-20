using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Extensions;
using Newtonsoft.Json;
using System.IO;

namespace helloJkw
{
	public static class DB
	{
		static string _connStr;
		static MySqlConnection _conn = null;

		static DB()
		{
			#region Read DB Info
			var filepath = @"jkw/db/mysqlHelloJkwInfo.txt";
			var jsonStr = File.ReadAllText(filepath, Encoding.UTF8);
			dynamic dbinfo = JsonConvert.DeserializeObject(jsonStr);
			#endregion

			#region Set Connection String Builder
			var connStrBuilder = new MySqlConnectionStringBuilder();
			connStrBuilder.Port = 3306;
			connStrBuilder.Server = dbinfo.server;
			connStrBuilder.Database = dbinfo.database;
			connStrBuilder.UserID = dbinfo.userid;
			connStrBuilder.Password = dbinfo.password;
			connStrBuilder.CharacterSet = dbinfo.characterset;
			connStrBuilder.ConnectionTimeout = 1;
			#endregion

			_connStr = connStrBuilder.ToString();
		}

		static MySqlConnection GetConnection(string connectionString)
		{
			if (_conn == null)
			{
				_conn = new MySqlConnection(connectionString);
				_conn.Open();
			}
			return _conn;
		}

		public static void Reset()
		{
			try
			{
				_conn.Close();
				_conn.Dispose();
			}
			catch { }
			_conn = null;
		}

		public static MySqlConnection Connection
		{
			get
			{
				return GetConnection(_connStr);
			}
		}

		public static MySqlCommand CreateCommand(this string query)
		{
			return Connection.CreateCommand(query);
		}

		public static MySqlCommand CreateCommand(this MySqlConnection conn, string query)
		{
			return new MySqlCommand(query, conn);
		}

		public static MySqlDataReader ExecuteReader(this string query, params object[] paramArray)
		{
			var cmd = CreateCommand(query);
			int cnt = 0;
			foreach (var param in paramArray)
				cmd.Parameters.AddWithValue("param" + (++cnt), param);
			return cmd.ExecuteReader();
		}

		public static int ExecuteNonQuery(this string query, params object[] paramArray)
		{
			var cmd = CreateCommand(query);
			int cnt = 0;
			foreach (var param in paramArray)
				cmd.Parameters.AddWithValue("param" + (++cnt), param);
			return cmd.ExecuteNonQuery();
		}
	}
}
