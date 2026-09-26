using CommunityToolkit.Maui.Extensions;
using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

public partial class ElencoSpesePage : ContentPage
{
	readonly ElencoSpeseViewModel viewModel;
	readonly ImpostazioniPopup impostazioni;

	// Il ViewModel e il popup Impostazioni vengono passati dalla dependency injection (MauiProgram.cs)
	public ElencoSpesePage(ElencoSpeseViewModel viewModel, ImpostazioniPopup impostazioni)
	{
		InitializeComponent();
		BindingContext = this.viewModel = viewModel;
		this.impostazioni = impostazioni;
	}

	// Ricarica l'elenco ogni volta che la pagina torna visibile, ad esempio dopo aver salvato una spesa
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.CaricaCommand.ExecuteAsync(null);
	}

	async void ImpostazioniClic(object? sender, EventArgs e) => await this.ShowPopupAsync(impostazioni);
}
