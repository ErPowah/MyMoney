using DiarioSpese.Models;
using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

public partial class SpesaPage : ContentPage
{
	readonly SpesaViewModel viewModel;

	public SpesaPage(SpesaViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = this.viewModel = viewModel;
	}

	// Le categorie arrivano dal database: si caricano quando la pagina compare
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.CaricaCategorieAsync();
	}

	// Apre questa pagina: vuota per una nuova spesa, già compilata per modificarne una esistente
	public static Task ApriAsync(Spesa? spesa = null) =>
		spesa is null
			? Shell.Current.GoToAsync(nameof(SpesaPage))
			: Shell.Current.GoToAsync(nameof(SpesaPage), new ShellNavigationQueryParameters { ["Spesa"] = spesa });
}
