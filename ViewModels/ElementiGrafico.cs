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
