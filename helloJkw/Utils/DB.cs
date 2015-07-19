using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Extensions;

namespace helloJkw
{
	class MySqlConn
	{
		MySqlConnection _conn;
		string _connStr;

		public MySqlConn()
		{
			var connStrBuilder = new MySqlConnectionStringBuilder();
			connStrBuilder.Port = 3306;
			connStrBuilder.Server = "";
			connStrBuilder.Database = "";
			connStrBuilder.UserID = "";
			connStrBuilder.Password = "";
			connStrBuilder.CharacterSet = "";

			_connStr = connStrBuilder.ToString();

			_conn = new MySqlConnection(_connStr);
			_conn.Open();
			Logger.Log("open connection");
		}

		~MySqlConn()
		{
			Logger.Log("close connection");
			_conn.Close();
		}

		public MySqlConnection Connection
		{
			get
			{
				return _conn;
			}
		}
	}

	public static class DB
	{
		static DB()
		{
		}

		public static MySqlConnection Connection
		{
			get
			{
				return (new MySqlConn()).Connection;
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
