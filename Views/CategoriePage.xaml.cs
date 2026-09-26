using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

public partial class CategoriePage : ContentPage
{
	readonly CategorieViewModel viewModel;

	public CategoriePage(CategorieViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = this.viewModel = viewModel;

		// I colori dipendono dal tema: un metodo della pagina, perché MAUI tiene questo evento con un riferimento debole
		if (Application.Current is not null)
			Application.Current.RequestedThemeChanged += TemaCambiato;
	}

	async void TemaCambiato(object? sender, AppThemeChangedEventArgs e) =>
		await viewModel.CaricaAsync(e.RequestedTheme == AppTheme.Dark);

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.CaricaAsync(Application.Current?.RequestedTheme == AppTheme.Dark);
	}
}
