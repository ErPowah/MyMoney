using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

public partial class CronologiaPage : ContentPage
{
	readonly CronologiaViewModel viewModel;

	public CronologiaPage(CronologiaViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = this.viewModel = viewModel;

		// I colori della legenda del grafico a linee dipendono dal tema chiaro o scuro
		viewModel.ImpostaTema(Application.Current?.RequestedTheme == AppTheme.Dark);
		if (Application.Current is not null)
			Application.Current.RequestedThemeChanged += (_, e) => viewModel.ImpostaTema(e.RequestedTheme == AppTheme.Dark);
	}

	// Ricarica ogni volta che si apre la scheda, così include le spese appena aggiunte o modificate
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.CaricaAsync();
	}
}
