using CommunityToolkit.Mvvm.ComponentModel;

namespace DiarioSpese.ViewModels;

// I grafici sono fatti con normali controlli XAML: ogni barra è un Border dentro un AbsoluteLayout,
// e "Barra" è il rettangolo che la posiziona. I valori proporzionali vanno da 0 a 1.

// Una riga del grafico "Spese per categoria": barra alta 12, lunga in proporzione alla categoria più alta
public record BarraCategoria(string Categoria, decimal Totale, double Percentuale, double Quota)
{
	public Rect Barra => new(0, 0, Quota, 12);

	public string Descrizione => $"{Categoria}: {Totale:C}, {Percentuale:P0} del totale";
}

// Una colonna del grafico mensile: larga 16, centrata, alta in proporzione al mese più alto.
// Selezionata cambia il colore della colonna quando la tocchi.
public partial class ColonnaMese(DateTime mese, decimal totale, double quota, int indice) : ObservableObject
{
	public DateTime Mese { get; } = mese;

	public decimal Totale { get; } = totale;

	public int Indice { get; } = indice;

	public string Etichetta { get; } = mese.ToString("MMM");

	public Rect Barra { get; } = new(0.5, 1, 16, quota);

	public string Descrizione => $"{Mese:MMMM yyyy}: {Totale:C}";

	[ObservableProperty]
	public partial bool Selezionata { get; set; }
}

// Una linea del grafico a linee: una categoria (o il totale) con i suoi 12 totali mensili.
// Ogni serie ha il suo colore fisso, che non cambia quando altre linee vengono nascoste.
public partial class SerieMensile(string nome, Color coloreChiaro, Color coloreScuro) : ObservableObject
{
	public string Nome { get; } = nome;

	public Color ColoreChiaro { get; } = coloreChiaro;

	public Color ColoreScuro { get; } = coloreScuro;

	// Un valore per mese; null nei mesi prima della prima spesa del diario (lì la linea non c'è)
	public decimal?[] Valori { get; set; } = [];

	[ObservableProperty]
	public partial bool Visibile { get; set; } = true;

	// Colore per il tema attuale, usato dalla legenda (la pagina lo aggiorna quando cambia il tema)
	[ObservableProperty]
	public partial Color Colore { get; set; } = coloreChiaro;

	// Totale nel mese scelto, scritto sotto il grafico
	[ObservableProperty]
	public partial decimal ValoreMese { get; set; }
}

// Colori delle linee, uno per posizione nell'elenco CategorieSpesa.Tutte, sempre nello stesso ordine.
// È una palette verificata per le varie forme di daltonismo, con tonalità diverse per il tema chiaro e scuro.
public static class PaletteCategorie
{
	static readonly (string Chiaro, string Scuro)[] colori =
	[
		("#2A78D6", "#3987E5"), // blu
		("#EB6834", "#D95926"), // arancione
		("#1BAF7A", "#199E70"), // verde acqua
		("#EDA100", "#C98500"), // giallo
		("#E87BA4", "#D55181"), // rosa
		("#008300", "#008300"), // verde
		("#4A3AA7", "#9085E9"), // viola
		("#E34948", "#E66767"), // rosso
	];

	// Oltre l'ottava categoria i colori non si ripetono (si confonderebbero): le eventuali altre restano grigie
	public static Color Chiaro(int posizione) =>
		posizione < colori.Length ? Color.FromArgb(colori[posizione].Chiaro) : Color.FromArgb("#919191");

	public static Color Scuro(int posizione) =>
		posizione < colori.Length ? Color.FromArgb(colori[posizione].Scuro) : Color.FromArgb("#919191");

	// Il totale di tutte le categorie è grigio, come il testo, per distinguerlo dalle categorie
	public static readonly Color TotaleChiaro = Color.FromArgb("#404040");

	public static readonly Color TotaleScuro = Color.FromArgb("#C8C8C8");
}
