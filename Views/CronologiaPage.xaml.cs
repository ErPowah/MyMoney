using CommunityToolkit.Maui.Extensions;
using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

public partial class CronologiaPage : ContentPage
{
	readonly CronologiaViewModel viewModel;
	readonly ImpostazioniPopup impostazioni;

	public CronologiaPage(CronologiaViewModel viewModel, ImpostazioniPopup impostazioni)
	{
		InitializeComponent();
		BindingContext = this.viewModel = viewModel;
		this.impostazioni = impostazioni;

		// I colori della legenda dipendono dal tema chiaro o scuro.
		// Serve un metodo della pagina e non una lambda: MAUI tiene questo evento con un riferimento
		// "debole", e una lambda che non appartiene a nessun oggetto vivo verrebbe eliminata dalla memoria.
		if (Application.Current is not null)
			Application.Current.RequestedThemeChanged += TemaCambiato;
	}

	void TemaCambiato(object? sender, AppThemeChangedEventArgs e) =>
		viewModel.ImpostaTema(e.RequestedTheme == AppTheme.Dark);

	// Ricarica ogni volta che si apre la scheda, così include le spese appena aggiunte o modificate
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		viewModel.ImpostaTema(Application.Current?.RequestedTheme == AppTheme.Dark);
		await viewModel.CaricaAsync();
	}

	async void ImpostazioniClic(object? sender, EventArgs e) => await this.ShowPopupAsync(impostazioni);
}
