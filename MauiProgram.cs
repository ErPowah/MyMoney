using System.Globalization;
using DiarioSpese.Services;
using DiarioSpese.ViewModels;
using DiarioSpese.Views;
using Microsoft.Extensions.Logging;

namespace DiarioSpese;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		// Euro, date e nomi dei mesi in italiano anche se il telefono (o l'emulatore) è in inglese
		var italiano = new CultureInfo("it-IT");
		CultureInfo.DefaultThreadCurrentCulture = italiano;
		CultureInfo.DefaultThreadCurrentUICulture = italiano;
		CultureInfo.CurrentCulture = italiano;
		CultureInfo.CurrentUICulture = italiano;

		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Dependency injection: qui si dichiara come creare database, ViewModel e pagine
		builder.Services.AddSingleton<SpeseDatabase>();
		builder.Services.AddSingleton<ElencoSpeseViewModel>();
		builder.Services.AddSingleton<ElencoSpesePage>();
		builder.Services.AddTransient<SpesaViewModel>();
		builder.Services.AddTransient<SpesaPage>();
		builder.Services.AddSingleton<CronologiaViewModel>();
		builder.Services.AddSingleton<CronologiaPage>();
		builder.Services.AddSingleton<AnalisiViewModel>();
		builder.Services.AddSingleton<AnalisiPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
