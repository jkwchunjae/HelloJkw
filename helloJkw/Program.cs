using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Hosting.Self;
using Nancy.Conventions;
using Extensions;
using Nancy.TinyIoc;
using Nancy.Bootstrapper;
using helloJkw.Game.Worldcup;

namespace helloJkw
{
	class Program
	{
		static void Main(string[] args)
		{
			#region Setting
			var argsDic = args.ToCommandArgumentsDictionary().ToDefaultDictionary();

			#region Port
			string port = argsDic["port"];
			if (port == null)
			{
				Console.Write("Port: ");
				port = Console.ReadLine();
			}
			Logger.Log("Port: {port}", new { port });
			#endregion

			#endregion

			#region Check Database Connection
			try
			{
#if DEBUG
				var conn = DB.Connection;
				conn.Close();
#else
				var query = @"insert into server_start values (now());";
				var affectedRow = query.ExecuteNonQuery();
				if (affectedRow != 1)
				{
					throw new Exception(query + " 쿼리 실행에 실패하였습니다.");
				}
#endif
				Logger.Log("Database연결에 성공하였습니다.");
			}
			catch (Exception ex)
			{
				Logger.Log("Database에 접속할 수 없습니다.");
				Logger.Log(ex);
				return;
			}
			#endregion

			#region Load something
#if !DEBUG
			Logger.Log("Load LuciaShop");
			LuciaStatic.LuciaDir = LuciaStatic.RootPath.CreateDirInfo();
			LuciaStatic.UpdateLuciaDir(0);

			Logger.Log("Load KboCenter");
			KboCenter.Load();
#endif
            WorldcupBettingManager.Load();
			#endregion

			using (var host = new NancyHost(new Uri("http://localhost:" + port)))
			{
				host.Start();
				Logger.Log("Start HelloJkw");

				while (true)
				{
					Console.ReadLine();
					Console.ReadLine();
					Console.Write("Command : ");
					var ans = Console.ReadLine();
					if (ans == "quit" || ans == "exit")
					{
						Console.Write("Really? (Y/n): ");
						ans = Console.ReadLine();
						if (ans == "Y")
							break;
					}
				}
			}
		}
	}

	public class Bootstrapper : DefaultNancyBootstrapper
	{
		protected override void ConfigureConventions(NancyConventions nancyConventions)
		{
			base.ConfigureConventions(nancyConventions);
			nancyConventions.StaticContentsConventions.Clear();

			nancyConventions.StaticContentsConventions.AddDirectory("Static", "Static");
			nancyConventions.StaticContentsConventions.AddDirectory("jkw", "jkw/static");
			nancyConventions.StaticContentsConventions.AddDirectory("lucia", "lucia");
			nancyConventions.StaticContentsConventions.AddDirectory("lucia-web", "lucia-web");
			nancyConventions.StaticContentsConventions.AddDirectory("lucia-mobile", "lucia-mobile");
		}

		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			base.ApplicationStartup(container, pipelines);
			this.Conventions.ViewLocationConventions.Add((viewName, model, context) =>
				{ return string.Concat("Views/Lucia/", viewName); });
			this.Conventions.ViewLocationConventions.Add((viewName, model, context) =>
				{ return string.Concat("Views/Jkw/", viewName); });
			this.Conventions.ViewLocationConventions.Add((viewName, model, context) =>
				{ return string.Concat("Views/Games/", viewName); });
		}
	}
}
