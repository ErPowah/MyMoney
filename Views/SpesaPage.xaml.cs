using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

public partial class SpesaPage : ContentPage
{
	public SpesaPage(SpesaViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
