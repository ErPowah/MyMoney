using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiarioSpese.Models;
using DiarioSpese.Services;
using DiarioSpese.Views;

namespace DiarioSpese.ViewModels;

// Scheda "Cronologia": gli ultimi 12 mesi in due modi, a scelta.
// - Colonne: il totale di ogni mese, con un filtro per categoria.
// - Linee: una linea per categoria (più quella di tutte le categorie insieme), ognuna da mostrare o nascondere.
// In entrambi i modi, toccando un mese sotto compaiono il suo totale e le sue spese.
public partial class CronologiaViewModel(SpeseDatabase database) : ObservableObject
{
	public const string TutteLeCategorie = "Tutte le categorie";
	const int NumeroMesi = 12;

	List<Spesa> spese = [];
	DateTime meseScelto = Statistiche.PrimoDelMese(DateTime.Today);

	public string[] Filtri { get; } = [TutteLeCategorie, .. CategorieSpesa.Tutte];

	public ObservableCollection<ColonnaMese> Colonne { get; } = [];

	public ObservableCollection<Spesa> SpeseDelMese { get; } = [];

	// Le linee: prima il totale (nascosto all'inizio), poi una per categoria, ognuna col suo colore fisso
	public IReadOnlyList<SerieMensile> Serie { get; } =
	[
		new SerieMensile(TutteLeCategorie, PaletteCategorie.TotaleChiaro, PaletteCategorie.TotaleScuro) { Visibile = false },
		.. CategorieSpesa.Tutte.Select((categoria, i) => new SerieMensile(categoria, PaletteCategorie.Chiaro(i), PaletteCategorie.Scuro(i)))
	];

	SerieMensile SerieTotale => Serie[0];

	[ObservableProperty]
	public partial string Filtro { get; set; } = TutteLeCategorie;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(ModalitaColonne))]
	public partial bool ModalitaLinee { get; set; }

	public bool ModalitaColonne => !ModalitaLinee;

	// Una nuova lista a ogni cambiamento, così il grafico a linee sa di doversi ridisegnare
	[ObservableProperty]
	public partial IReadOnlyList<SerieMensile> SerieVisibili { get; set; } = [];

	[ObservableProperty]
	public partial IReadOnlyList<string> EtichetteMesi { get; set; } = [];

	[ObservableProperty]
	public partial int IndiceMeseScelto { get; set; } = -1;

	[ObservableProperty]
	public partial bool NessunaSerie { get; set; }

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

	// Colori della legenda: la pagina li aggiorna quando cambia il tema chiaro/scuro
	public void ImpostaTema(bool scuro)
	{
		foreach (var s in Serie)
			s.Colore = scuro ? s.ColoreScuro : s.ColoreChiaro;
	}

	// Cambiando categoria si aggiornano sia il grafico a colonne sia l'elenco
	partial void OnFiltroChanged(string value) => Aggiorna();

	[RelayCommand]
	void Modalita(string modalita)
	{
		ModalitaLinee = modalita == "linee";
		AggiornaElenco();
	}

	[RelayCommand]
	void AlternaSerie(SerieMensile serie)
	{
		serie.Visibile = !serie.Visibile;
		AggiornaLinee();
		AggiornaElenco();
	}

	[RelayCommand]
	void SelezionaMese(ColonnaMese colonna)
	{
		meseScelto = colonna.Mese;
		foreach (var c in Colonne)
			c.Selezionata = c == colonna;
		AggiornaElenco();
	}

	// Il grafico a linee indica il mese toccato con la sua posizione (da 0 a 11)
	[RelayCommand]
	void SelezionaIndice(int indice)
	{
		if (indice >= 0 && indice < Colonne.Count)
			SelezionaMese(Colonne[indice]);
	}

	[RelayCommand]
	Task ModificaAsync(Spesa spesa) => SpesaPage.ApriAsync(spesa);

	IEnumerable<Spesa> SpeseFiltrate() =>
		Filtro == TutteLeCategorie ? spese : spese.Where(s => s.Categoria == Filtro);

	// Le spese che contano nel modo attuale: il filtro delle colonne, oppure le linee visibili
	IEnumerable<Spesa> SpeseDelModo()
	{
		if (!ModalitaLinee)
			return SpeseFiltrate();
		if (SerieTotale.Visibile)
			return spese;

		var visibili = Serie.Where(s => s.Visibile).Select(s => s.Nome).ToHashSet();
		return spese.Where(s => visibili.Contains(s.Categoria));
	}

	void Aggiorna()
	{
		var oggi = DateTime.Today;

		// Colonne: totale di ogni mese per il filtro scelto
		var mesi = Statistiche.PerMese(SpeseFiltrate(), oggi, NumeroMesi);
		var massimo = mesi.Max(m => m.Totale);
		Colonne.Clear();
		for (int i = 0; i < mesi.Count; i++)
		{
			var mese = mesi[i];
			// anche la spesa più piccola resta visibile come una colonnina
			var quota = mese.Totale > 0 ? Math.Max((double)(mese.Totale / massimo), 0.02) : 0;
			Colonne.Add(new ColonnaMese(mese.Mese, mese.Totale, quota, i) { Selezionata = mese.Mese == meseScelto });
		}
		EtichetteMesi = Colonne.Select(c => c.Etichetta).ToArray();

		// Linee: gli stessi 12 mesi, per tutte le categorie insieme e per ciascuna categoria.
		// Prima del mese della prima spesa il diario non esisteva ancora: lì niente valore (null), invece di 0.
		var inizioDiario = spese.Count == 0 ? DateTime.MaxValue : Statistiche.PrimoDelMese(spese.Min(s => s.Data));
		decimal?[] ValoriMensili(IEnumerable<Spesa> quali) =>
			Statistiche.PerMese(quali, oggi, NumeroMesi).Select(m => m.Mese < inizioDiario ? (decimal?)null : m.Totale).ToArray();

		SerieTotale.Valori = ValoriMensili(spese);
		foreach (var s in Serie.Skip(1))
			s.Valori = ValoriMensili(spese.Where(x => x.Categoria == s.Nome));

		AggiornaLinee();
		AggiornaElenco();
	}

	void AggiornaLinee()
	{
		SerieVisibili = Serie.Where(s => s.Visibile).ToArray();
		NessunaSerie = SerieVisibili.Count == 0;
	}

	void AggiornaElenco()
	{
		IndiceMeseScelto = Colonne.ToList().FindIndex(c => c.Mese == meseScelto);
		foreach (var s in Serie)
			s.ValoreMese = IndiceMeseScelto >= 0 && IndiceMeseScelto < s.Valori.Length ? s.Valori[IndiceMeseScelto] ?? 0 : 0;

		var delMese = SpeseDelModo().Where(s => Statistiche.PrimoDelMese(s.Data) == meseScelto).ToList();

		SpeseDelMese.Clear();
		foreach (var spesa in delMese)
			SpeseDelMese.Add(spesa);

		var titolo = meseScelto.ToString("MMMM yyyy");
		TitoloMese = char.ToUpper(titolo[0]) + titolo[1..];
		TotaleMese = delMese.Sum(s => s.Importo);
		MeseVuoto = delMese.Count == 0;
	}
}
