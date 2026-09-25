using DiarioSpese.Views;

namespace DiarioSpese;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Rende la pagina raggiungibile con Shell.Current.GoToAsync(nameof(SpesaPage))
		Routing.RegisterRoute(nameof(SpesaPage), typeof(SpesaPage));
	}
}
