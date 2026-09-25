using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

public partial class AnalisiPage : ContentPage
{
	readonly AnalisiViewModel viewModel;

	public AnalisiPage(AnalisiViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = this.viewModel = viewModel;
	}

	// Ricalcola ogni volta che si apre la scheda, così include le spese appena aggiunte o modificate
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.CalcolaAsync();
	}
}
