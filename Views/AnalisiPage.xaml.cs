using CommunityToolkit.Maui.Extensions;
using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

public partial class AnalisiPage : ContentPage
{
	readonly AnalisiViewModel viewModel;
	readonly ImpostazioniPopup impostazioni;

	public AnalisiPage(AnalisiViewModel viewModel, ImpostazioniPopup impostazioni)
	{
		InitializeComponent();
		BindingContext = this.viewModel = viewModel;
		this.impostazioni = impostazioni;
	}

	// Ricalcola ogni volta che si apre la scheda, così include le spese appena aggiunte o modificate
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.CalcolaAsync();
	}

	async void ImpostazioniClic(object? sender, EventArgs e) => await this.ShowPopupAsync(impostazioni);
}
