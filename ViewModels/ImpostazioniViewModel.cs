using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiarioSpese.Models;
using DiarioSpese.Services;

namespace DiarioSpese.ViewModels;

// Un tema preimpostato con già calcolato il colore che avrebbe: lo usa la fila di pallini nel popup
public record VoceTema(string Nome, double Tonalita, Color Colore);

// Logica del popup "Impostazioni". Per ora l'unica impostazione è il colore del tema, scelto tra un
// elenco di preset oppure "creato" spostando lo slider della tonalità: il tema si applica subito,
// mentre lo trascini, così vedi il risultato dal vivo anche dietro al popup.
public partial class ImpostazioniViewModel(TemaService temaService) : ObservableObject
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
}
