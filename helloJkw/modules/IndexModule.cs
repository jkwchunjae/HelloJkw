using Nancy;

public class IndexModule : NancyModule
{
	public IndexModule()
	{
		Get["/"] = _ =>
		{
			return View["index"];
		};
		Get["/index2"] = _ =>
		{
			return View["index2"];
		};
	}
}