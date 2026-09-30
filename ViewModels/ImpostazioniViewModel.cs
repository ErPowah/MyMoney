using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiarioSpese.Models;
using DiarioSpese.Services;

namespace DiarioSpese.ViewModels;

// Un tema preimpostato con già calcolato il colore che avrebbe: lo usa la fila di pallini nel popup
public record VoceTema(string Nome, double Tonalita, Color Colore);

// Logica del popup "Impostazioni".
// - Colore del tema, scelto tra un elenco di preset oppure "creato" spostando lo slider della tonalità: il tema
//   si applica subito, mentre lo trascini, così vedi il risultato dal vivo anche dietro al popup.
// - Backup: quando è stato fatto l'ultimo, condividerne una copia, ripristinarne uno.
public partial class ImpostazioniViewModel(TemaService temaService, BackupService backupService) : ObservableObject
{
	public IReadOnlyList<VoceTema> Preset { get; } =
		[.. TemiPreimpostati.Tutti.Select(t => new VoceTema(t.Nome, t.Tonalita, TemaService.Anteprima(t.Tonalita)))];

	[ObservableProperty]
	public partial double Tonalita { get; set; } = temaService.Tonalita;

	// Il colore che avrà "Primary" con la tonalità scelta: lo mostra l'anteprima sopra lo slider
	public Color ColoreAnteprima => TemaService.Anteprima(Tonalita);

	partial void OnTonalitaChanged(double value)
	{
		OnPropertyChanged(nameof(ColoreAnteprima));
		temaService.Imposta(value);
	}

	[RelayCommand]
	void SelezionaPreset(VoceTema tema) => Tonalita = tema.Tonalita;

	public string StatoBackup
	{
		get
		{
			if (!BackupService.Automatico)
				return "Su questo dispositivo la copia automatica non è disponibile: usa «Condividi backup».";
			if (backupService.UltimoErrore is { } errore)
				return $"L'ultimo backup non è riuscito: {errore}";
			if (backupService.UltimoBackup is not { } quando)
				return $"Il primo backup si crea da solo alla prima spesa, in {BackupService.Cartella}.";

			var giorno = quando.Date == DateTime.Today ? "oggi"
				: quando.Date == DateTime.Today.AddDays(-1) ? "ieri"
				: $"il {quando:d MMMM}";
			return $"Ultimo backup {giorno} alle {quando:HH:mm}, in {BackupService.Cartella}. Si aggiorna da solo a ogni modifica.";
		}
	}

	// Il popup la chiama ogni volta che si apre
	public void AggiornaStatoBackup() => OnPropertyChanged(nameof(StatoBackup));

	[RelayCommand]
	async Task CondividiBackupAsync()
	{
		try
		{
			var file = await backupService.CopiaDaCondividereAsync();
			await Share.Default.RequestAsync(new ShareFileRequest
			{
				Title = "Backup del Diario Spese",
				File = new ShareFile(file, "application/octet-stream")
			});
		}
		catch (Exception ex)
		{
			await Shell.Current.DisplayAlertAsync("Backup non riuscito", ex.Message, "OK");
		}
	}

	[RelayCommand]
	async Task RipristinaBackupAsync()
	{
		try
		{
			var scelto = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Scegli il backup da ripristinare" });
			if (scelto is null)
				return;

			var file = await BackupService.CopiaInCacheAsync(scelto);
			if (SpeseDatabase.SpeseNelFile(file) is not { } numero)
			{
				await Shell.Current.DisplayAlertAsync("File non valido", "Questo file non è un backup del Diario Spese.", "OK");
				return;
			}

			var spese = numero == 1 ? "una spesa" : $"{numero} spese";
			if (!await Shell.Current.DisplayAlertAsync("Ripristinare il backup?",
				$"Contiene {spese}: prenderanno il posto di tutte quelle che ci sono ora nell'app.", "Ripristina", "Annulla"))
				return;

			await backupService.RipristinaAsync(file);
			AggiornaStatoBackup();
			await Shell.Current.DisplayAlertAsync("Backup ripristinato",
				numero == 1 ? "È tornata una spesa." : $"Sono tornate {numero} spese.", "OK");
		}
		catch (Exception ex)
		{
			await Shell.Current.DisplayAlertAsync("Ripristino non riuscito", ex.Message, "OK");
		}
	}
}
