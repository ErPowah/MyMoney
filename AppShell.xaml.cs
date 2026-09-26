using DiarioSpese.Views;

namespace DiarioSpese;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Rende le pagine raggiungibili con Shell.Current.GoToAsync(nameof(...))
		Routing.RegisterRoute(nameof(SpesaPage), typeof(SpesaPage));
		Routing.RegisterRoute(nameof(CategoriePage), typeof(CategoriePage));
	}
}
