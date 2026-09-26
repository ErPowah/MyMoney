using DiarioSpese.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DiarioSpese;

public partial class App : Application
{
	// TemaService viene passato dalla dependency injection: MAUI risolve App dallo stesso contenitore
	// con cui risolve pagine e ViewModel (MauiProgram.cs)
	public App(TemaService temaService)
	{
		InitializeComponent();

		// Applica il colore del tema scelto in precedenza (o quello di partenza, se è la prima apertura).
		// Va fatto qui, dopo InitializeComponent: prima di allora le risorse di Colors.xaml non sono ancora caricate.
		temaService.Avvia();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
