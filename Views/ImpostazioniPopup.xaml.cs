using CommunityToolkit.Maui.Extensions;
using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

public partial class ImpostazioniPopup : ContentView
{
	public ImpostazioniPopup(ImpostazioniViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
		PulsanteChiudi.Clicked += async (_, _) => await Shell.Current.ClosePopupAsync();
	}
}
