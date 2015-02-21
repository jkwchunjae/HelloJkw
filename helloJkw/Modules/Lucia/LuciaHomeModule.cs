using Nancy;

public class LuciaHomeModule : NancyModule
{
	public LuciaHomeModule()
	{
		Get["/"] = _ =>
		{
			return View["luciaHome"];
		};
	}
}