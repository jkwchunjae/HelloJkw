using System;
using System.IO;
using System.Linq;
using Nancy;
using Extensions;

public class LuciaHomeModule : NancyModule
{
	public LuciaHomeModule()
	{
		Get["/lucia"] = _ =>
		{
			// 이미지를 손으로 하나하나 추가하기 힘들어서 폴더 안에 있는 모든 파일을 순서대로 불러오게 하였다.
			var files = Directory.GetFiles(@"Static/image/lucia/main/", "*").OrderBy(e => e);
			var model = new
			{
				ImageList = files.Select(e => Path.GetFileName(e)).ToList()
			};
			return View["luciaHome", model];
		};
	}
}