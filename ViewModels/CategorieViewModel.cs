using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiarioSpese.Models;
using DiarioSpese.Services;

namespace DiarioSpese.ViewModels;

// Una riga della pagina Categorie: nome, colore nei grafici e quante spese la usano
public record VoceCategoria(Categoria Categoria, int NumeroSpese, Color Colore)
{
	public string Nome => Categoria.Nome;

	public string Dettaglio =>
		Nome == CategorieSpesa.Altro ? "Raccoglie anche le spese delle categorie eliminate"
		: NumeroSpese == 1 ? "1 spesa"
		: $"{NumeroSpese} spese";

	// "Altro" non si rinomina e non si elimina
	public bool Modificabile => Nome != CategorieSpesa.Altro;
}

// Pagina "Categorie": crea, rinomina ed elimina le categorie
public partial class CategorieViewModel(SpeseDatabase database) : ObservableObject
{
	bool temaScuro;

	public ObservableCollection<VoceCategoria> Categorie { get; } = [];

	public async Task CaricaAsync(bool scuro)
	{
		temaScuro = scuro;
		var categorie = await database.LeggiCategorieAsync();
		var spesePerCategoria = (await database.LeggiSpeseAsync())
			.GroupBy(s => s.Categoria)
			.ToDictionary(g => g.Key, g => g.Count());

		Categorie.Clear();
		foreach (var c in categorie)
		{
			var colore = scuro ? PaletteCategorie.Scuro(c.Colore) : PaletteCategorie.Chiaro(c.Colore);
			Categorie.Add(new VoceCategoria(c, spesePerCategoria.GetValueOrDefault(c.Nome), colore));
		}
	}

	[RelayCommand]
	async Task NuovaAsync()
	{
		if (await CreaConDialogoAsync(database) is not null)
			await CaricaAsync(temaScuro);
	}

	[RelayCommand]
	async Task RinominaAsync(VoceCategoria voce)
	{
		var nome = await Shell.Current.DisplayPromptAsync(
			"Rinomina categoria",
			$"Nuovo nome per «{voce.Nome}». Anche le sue spese passeranno al nuovo nome.",
			"Salva", "Annulla", maxLength: CategorieSpesa.LunghezzaMassima, initialValue: voce.Nome);
		if (nome is null || nome.Trim() == voce.Nome)
			return; // annullato o nome uguale

		var errore = CategorieSpesa.ErroreNome(nome, Categorie.Select(c => c.Nome), voce.Nome);
		if (errore is not null)
		{
			await Shell.Current.DisplayAlertAsync("Nome non valido", errore, "OK");
			return;
		}

		await database.RinominaCategoriaAsync(voce.Categoria, nome);
		await CaricaAsync(temaScuro);
	}

	[RelayCommand]
	async Task EliminaAsync(VoceCategoria voce)
	{
		var messaggio = voce.NumeroSpese switch
		{
			0 => "Nessuna spesa usa questa categoria.",
			1 => $"L'unica spesa di questa categoria passerà in «{CategorieSpesa.Altro}».",
			_ => $"Le {voce.NumeroSpese} spese di questa categoria passeranno in «{CategorieSpesa.Altro}».",
		};
		if (!await Shell.Current.DisplayAlertAsync($"Eliminare «{voce.Nome}»?", messaggio, "Elimina", "Annulla"))
			return;

		await database.EliminaCategoriaAsync(voce.Categoria);
		await CaricaAsync(temaScuro);
	}

	// Chiede il nome, lo controlla e crea la categoria. Restituisce il nome, oppure null se annullato o non valido.
	// La usa anche il modulo della spesa (pulsante "+ Nuova categoria").
	public static async Task<string?> CreaConDialogoAsync(SpeseDatabase database)
	{
		var nome = await Shell.Current.DisplayPromptAsync(
			"Nuova categoria", "Come vuoi chiamarla?",
			"Crea", "Annulla", placeholder: "Es. Regali", maxLength: CategorieSpesa.LunghezzaMassima);
		if (nome is null)
			return null; // annullato

		var esistenti = (await database.LeggiCategorieAsync()).Select(c => c.Nome);
		var errore = CategorieSpesa.ErroreNome(nome, esistenti);
		if (errore is not null)
		{
			await Shell.Current.DisplayAlertAsync("Nome non valido", errore, "OK");
			return null;
		}

		return (await database.AggiungiCategoriaAsync(nome)).Nome;
	}
}
