using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiarioSpese.Models;
using DiarioSpese.Services;
using DiarioSpese.Views;

namespace DiarioSpese.ViewModels;

// Scheda "Cronologia": totale di ognuno degli ultimi 12 mesi, filtrabile per categoria,
// e sotto l'elenco delle spese del mese scelto toccando una colonna del grafico.
public partial class CronologiaViewModel(SpeseDatabase database) : ObservableObject
{
	public const string TutteLeCategorie = "Tutte le categorie";
	const int NumeroMesi = 12;

	List<Spesa> spese = [];
	DateTime meseScelto = Statistiche.PrimoDelMese(DateTime.Today);

	public string[] Filtri { get; } = [TutteLeCategorie, .. CategorieSpesa.Tutte];

	public ObservableCollection<ColonnaMese> Colonne { get; } = [];

	public ObservableCollection<Spesa> SpeseDelMese { get; } = [];

	[ObservableProperty]
	public partial string Filtro { get; set; } = TutteLeCategorie;

	[ObservableProperty]
	public partial string TitoloMese { get; set; } = string.Empty;

	[ObservableProperty]
	public partial decimal TotaleMese { get; set; }

	[ObservableProperty]
	public partial bool MeseVuoto { get; set; }

	public async Task CaricaAsync()
	{
		spese = await database.LeggiSpeseAsync();
		Aggiorna();
	}

	// Cambiando categoria si aggiornano sia il grafico sia l'elenco
	partial void OnFiltroChanged(string value) => Aggiorna();

	[RelayCommand]
	void SelezionaMese(ColonnaMese colonna)
	{
		meseScelto = colonna.Mese;
		foreach (var c in Colonne)
			c.Selezionata = c == colonna;
		AggiornaElenco();
	}

	[RelayCommand]
	Task ModificaAsync(Spesa spesa) => SpesaPage.ApriAsync(spesa);

	IEnumerable<Spesa> SpeseFiltrate() =>
		Filtro == TutteLeCategorie ? spese : spese.Where(s => s.Categoria == Filtro);

	void Aggiorna()
	{
		var mesi = Statistiche.PerMese(SpeseFiltrate(), DateTime.Today, NumeroMesi);
		var massimo = mesi.Max(m => m.Totale);

		Colonne.Clear();
		for (int i = 0; i < mesi.Count; i++)
		{
			var mese = mesi[i];
			// anche la spesa più piccola resta visibile come una colonnina
			var quota = mese.Totale > 0 ? Math.Max((double)(mese.Totale / massimo), 0.02) : 0;
			Colonne.Add(new ColonnaMese(mese.Mese, mese.Totale, quota, i) { Selezionata = mese.Mese == meseScelto });
		}

		AggiornaElenco();
	}

	void AggiornaElenco()
	{
		var delMese = SpeseFiltrate().Where(s => Statistiche.PrimoDelMese(s.Data) == meseScelto).ToList();

		SpeseDelMese.Clear();
		foreach (var spesa in delMese)
			SpeseDelMese.Add(spesa);

		var titolo = meseScelto.ToString("MMMM yyyy");
		TitoloMese = char.ToUpper(titolo[0]) + titolo[1..];
		TotaleMese = delMese.Sum(s => s.Importo);
		MeseVuoto = delMese.Count == 0;
	}
}
