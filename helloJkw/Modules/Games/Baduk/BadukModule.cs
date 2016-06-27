using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace helloJkw
{
	public class BadukModule : JkwModule
	{
		string pathMemo = @"jkw/games/Baduk/memo.txt";
		public BadukModule()
		{
			Get["/games/Baduk"] = _ =>
			{
				Model.Memo = JsonConvert.DeserializeObject<string>(File.ReadAllText(pathMemo, Encoding.UTF8));
				return View["Games/Baduk/badukMain.cshtml", Model];
			};

			Post["/games/Baduk/save-memo"] = _ =>
			{
				if (!session.IsLogin)
					return "로그인하여야 합니다.";
				string memo = Request.Form["memo"];
				File.WriteAllText(pathMemo, JsonConvert.SerializeObject(memo), Encoding.UTF8);
				return "저장되었습니다.";
			};
		}
	}
}
