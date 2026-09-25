using DiarioSpese.Models;
using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

public partial class SpesaPage : ContentPage
{
	public SpesaPage(SpesaViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	// Apre questa pagina: vuota per una nuova spesa, già compilata per modificarne una esistente
	public static Task ApriAsync(Spesa? spesa = null) =>
		spesa is null
			? Shell.Current.GoToAsync(nameof(SpesaPage))
			: Shell.Current.GoToAsync(nameof(SpesaPage), new ShellNavigationQueryParameters { ["Spesa"] = spesa });
}
