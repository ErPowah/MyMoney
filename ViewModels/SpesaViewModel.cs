using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiarioSpese.Models;
using DiarioSpese.Services;

namespace DiarioSpese.ViewModels;

// Logica della pagina "Nuova spesa" / "Modifica spesa".
public partial class SpesaViewModel(SpeseDatabase database) : ObservableObject, IQueryAttributable
{
	Spesa spesa = new();

	public string[] Categorie { get; } = ["Alimentari", "Casa", "Trasporti", "Salute", "Svago", "Altro"];

	[ObservableProperty]
	public partial string Titolo { get; set; } = "Nuova spesa";

	[ObservableProperty]
	public partial string Descrizione { get; set; } = string.Empty;

	// L'importo resta testo finché non premi Salva, così puoi scrivere "12,5" senza che venga corretto mentre digiti
	[ObservableProperty]
	public partial string Importo { get; set; } = string.Empty;

	[ObservableProperty]
	public partial DateTime Data { get; set; } = DateTime.Today;

	[ObservableProperty]
	public partial string Categoria { get; set; } = "Alimentari";

	[ObservableProperty]
	public partial bool InModifica { get; set; }

	// Shell lo chiama quando si arriva qui toccando una spesa dell'elenco (vedi ElencoSpeseViewModel.ModificaAsync)
	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue("Spesa", out var valore) && valore is Spesa esistente)
		{
			spesa = esistente;
			Titolo = "Modifica spesa";
			Descrizione = esistente.Descrizione;
			Importo = esistente.Importo.ToString("0.00");
			Data = esistente.Data;
			Categoria = esistente.Categoria;
			InModifica = true;
		}
	}

	[RelayCommand]
	async Task SalvaAsync()
	{
		if (string.IsNullOrWhiteSpace(Descrizione))
		{
			await Shell.Current.DisplayAlertAsync("Manca la descrizione", "Scrivi per che cosa hai speso.", "OK");
			return;
		}

		// Accetta sia "12,50" sia "12.50"
		var testo = Importo.Trim().Replace(',', '.');
		if (!decimal.TryParse(testo, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var importo) || importo <= 0)
		{
			await Shell.Current.DisplayAlertAsync("Importo non valido", "Scrivi un importo, ad esempio 12,50.", "OK");
			return;
		}

		spesa.Descrizione = Descrizione.Trim();
		spesa.Importo = Math.Round(importo, 2);
		spesa.Data = Data;
		spesa.Categoria = Categoria;

		await database.SalvaAsync(spesa);
		await Shell.Current.GoToAsync(".."); // torna all'elenco
	}

	[RelayCommand]
	async Task EliminaAsync()
	{
		bool conferma = await Shell.Current.DisplayAlertAsync("Eliminare questa spesa?", spesa.Descrizione, "Elimina", "Annulla");
		if (!conferma)
			return;

		await database.EliminaAsync(spesa);
		await Shell.Current.GoToAsync("..");
	}
}
