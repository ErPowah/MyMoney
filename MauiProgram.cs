using System.Globalization;
using CommunityToolkit.Maui;
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

#pragma warning disable CA1416 // il progetto compila solo per le piattaforme che UseMauiCommunityToolkit supporta
		builder.UseMauiCommunityToolkit(); // per la finestra delle Impostazioni (CommunityToolkit.Maui.Views.Popup)
#pragma warning restore CA1416

		// Dependency injection: qui si dichiara come creare database, ViewModel e pagine
		builder.Services.AddSingleton<SpeseDatabase>();
		builder.Services.AddSingleton<TemaService>();
		builder.Services.AddSingleton<ElencoSpeseViewModel>();
		builder.Services.AddSingleton<ElencoSpesePage>();
		builder.Services.AddTransient<SpesaViewModel>();
		builder.Services.AddTransient<SpesaPage>();
		builder.Services.AddSingleton<CronologiaViewModel>();
		builder.Services.AddSingleton<CronologiaPage>();
		builder.Services.AddSingleton<AnalisiViewModel>();
		builder.Services.AddSingleton<AnalisiPage>();
		builder.Services.AddTransient<CategorieViewModel>();
		builder.Services.AddTransient<CategoriePage>();
		builder.Services.AddSingleton<ImpostazioniViewModel>();
		builder.Services.AddSingleton<ImpostazioniPopup>(); // condiviso dalle tre schede: sono le stesse impostazioni ovunque

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
