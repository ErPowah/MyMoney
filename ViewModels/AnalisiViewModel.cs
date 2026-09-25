using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiarioSpese.Services;

namespace DiarioSpese.ViewModels;

// Scheda "Analisi": totale speso tra due date e grafico delle spese per categoria nello stesso periodo
public partial class AnalisiViewModel(SpeseDatabase database) : ObservableObject
{
	int ultimoCalcolo;
	bool impostandoPeriodo;

	[ObservableProperty]
	public partial DateTime Dal { get; set; } = Statistiche.PrimoDelMese(DateTime.Today);

	[ObservableProperty]
	public partial DateTime Al { get; set; } = DateTime.Today;

	[ObservableProperty]
	public partial decimal Totale { get; set; }

	[ObservableProperty]
	public partial int NumeroSpese { get; set; }

	[ObservableProperty]
	public partial decimal MediaGiornaliera { get; set; }

	[ObservableProperty]
	public partial bool PeriodoValido { get; set; } = true;

	// Messaggio da mostrare al posto del grafico (periodo non valido o senza spese)
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(HaAvviso), nameof(HaRisultati))]
	public partial string Avviso { get; set; } = string.Empty;

	public bool HaAvviso => Avviso.Length > 0;

	public bool HaRisultati => Avviso.Length == 0;

	public ObservableCollection<BarraCategoria> Categorie { get; } = [];

	// Ogni volta che cambi una delle due date, il calcolo riparte da solo
	partial void OnDalChanged(DateTime value) => RicalcolaDopoModifica();

	partial void OnAlChanged(DateTime value) => RicalcolaDopoModifica();

	void RicalcolaDopoModifica()
	{
		if (!impostandoPeriodo)
			_ = CalcolaAsync();
	}

	// Scorciatoie per i periodi più usati
	[RelayCommand]
	Task PeriodoAsync(string periodo)
	{
		var oggi = DateTime.Today;
		var inizioMese = Statistiche.PrimoDelMese(oggi);
		var (dal, al) = periodo switch
		{
			"mese" => (inizioMese, oggi),
			"mese-scorso" => (inizioMese.AddMonths(-1), inizioMese.AddDays(-1)),
			"30-giorni" => (oggi.AddDays(-29), oggi),
			"anno" => (new DateTime(oggi.Year, 1, 1), oggi),
			_ => (Dal, Al)
		};

		impostandoPeriodo = true;
		Dal = dal;
		Al = al;
		impostandoPeriodo = false;
		return CalcolaAsync();
	}

	public async Task CalcolaAsync()
	{
		int questoCalcolo = ++ultimoCalcolo;

		if (Dal.Date > Al.Date)
		{
			Svuota("La data di inizio è successiva alla data di fine.");
			PeriodoValido = false;
			return;
		}

		var spese = await database.LeggiSpeseAsync(Dal, Al);
		if (questoCalcolo != ultimoCalcolo)
			return; // nel frattempo è stato scelto un altro periodo

		PeriodoValido = true;
		if (spese.Count == 0)
		{
			Svuota("Nessuna spesa in questo periodo.");
			return;
		}

		int giorni = (Al.Date - Dal.Date).Days + 1;
		Totale = spese.Sum(s => s.Importo);
		NumeroSpese = spese.Count;
		MediaGiornaliera = Math.Round(Totale / giorni, 2);

		var perCategoria = Statistiche.PerCategoria(spese);
		var massimo = perCategoria[0].Totale;
		Categorie.Clear();
		foreach (var t in perCategoria)
		{
			var quota = Math.Max((double)(t.Totale / massimo), 0.02);
			Categorie.Add(new BarraCategoria(t.Categoria, t.Totale, (double)(t.Totale / Totale), quota));
		}
		Avviso = string.Empty;
	}

	void Svuota(string avviso)
	{
		Totale = 0;
		NumeroSpese = 0;
		MediaGiornaliera = 0;
		Categorie.Clear();
		Avviso = avviso;
	}
}
