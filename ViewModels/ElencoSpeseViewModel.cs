using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiarioSpese.Models;
using DiarioSpese.Services;
using DiarioSpese.Views;

namespace DiarioSpese.ViewModels;

// Logica della pagina principale. Non conosce i controlli grafici: la pagina XAML si "aggancia"
// alle sue proprietà e ai suoi comandi tramite i {Binding}.
public partial class ElencoSpeseViewModel(SpeseDatabase database) : ObservableObject
{
	// ObservableCollection avvisa da sola la CollectionView quando aggiungi o togli elementi
	public ObservableCollection<Spesa> Spese { get; } = [];

	public string MeseCorrente => DateTime.Today.ToString("MMMM yyyy");

	// [ObservableProperty] genera il codice che aggiorna la grafica quando il valore cambia
	[ObservableProperty]
	public partial decimal TotaleMese { get; set; }

	// [RelayCommand] genera CaricaCommand, collegabile a pulsanti e gesti nello XAML
	[RelayCommand]
	async Task CaricaAsync()
	{
		var spese = await database.LeggiSpeseAsync();

		Spese.Clear();
		foreach (var spesa in spese)
			Spese.Add(spesa);

		var oggi = DateTime.Today;
		TotaleMese = spese
			.Where(s => s.Data.Year == oggi.Year && s.Data.Month == oggi.Month)
			.Sum(s => s.Importo);
	}

	[RelayCommand]
	Task NuovaSpesaAsync() => Shell.Current.GoToAsync(nameof(SpesaPage));

	[RelayCommand]
	Task ModificaAsync(Spesa spesa) =>
		Shell.Current.GoToAsync(nameof(SpesaPage), new ShellNavigationQueryParameters { ["Spesa"] = spesa });

	[RelayCommand]
	async Task EliminaAsync(Spesa spesa)
	{
		await database.EliminaAsync(spesa);
		await CaricaAsync();
	}
}
