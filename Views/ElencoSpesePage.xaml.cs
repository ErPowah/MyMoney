using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

public partial class ElencoSpesePage : ContentPage
{
	readonly ElencoSpeseViewModel viewModel;

	// Il ViewModel viene passato dalla dependency injection (registrato in MauiProgram.cs)
	public ElencoSpesePage(ElencoSpeseViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = this.viewModel = viewModel;
	}

	// Ricarica l'elenco ogni volta che la pagina torna visibile, ad esempio dopo aver salvato una spesa
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.CaricaCommand.ExecuteAsync(null);
	}
}
