using Microsoft.Extensions.Logging;

namespace CruiseControl;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif
		// Register AccountStorage as a singleton
    	string accountsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "accounts.json");
    	string loggedInFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "loggedIn.json");

    	builder.Services.AddSingleton(new CruiseControl.AccountStorage(accountsFilePath, loggedInFilePath));


		return builder.Build();
	}
}
