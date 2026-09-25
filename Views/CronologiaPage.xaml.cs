using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

public partial class CronologiaPage : ContentPage
{
	readonly CronologiaViewModel viewModel;

	public CronologiaPage(CronologiaViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = this.viewModel = viewModel;
	}

	// Ricarica ogni volta che si apre la scheda, così include le spese appena aggiunte o modificate
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.CaricaAsync();
	}
}
