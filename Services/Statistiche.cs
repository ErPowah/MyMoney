using DiarioSpese.Models;

namespace DiarioSpese.Services;

// Calcoli sulle spese già lette dal database, fatti in memoria con LINQ
public static class Statistiche
{
	// Totale di ogni categoria, dalla più alta alla più bassa
	public static List<TotaleCategoria> PerCategoria(IEnumerable<Spesa> spese) =>
		spese.GroupBy(s => s.Categoria)
			.Select(g => new TotaleCategoria(g.Key, g.Sum(s => s.Importo)))
			.OrderByDescending(t => t.Totale)
			.ToList();

	// Totale di ciascuno degli ultimi "quanti" mesi fino a "ultimoMese" compreso, dal più vecchio al più recente.
	// I mesi senza spese valgono 0, così nel grafico si vedono anche i mesi vuoti.
	public static List<TotaleMese> PerMese(IEnumerable<Spesa> spese, DateTime ultimoMese, int quanti)
	{
		var totali = spese
			.GroupBy(s => PrimoDelMese(s.Data))
			.ToDictionary(g => g.Key, g => g.Sum(s => s.Importo));

		var ultimo = PrimoDelMese(ultimoMese);
		return Enumerable.Range(0, quanti)
			.Select(i => ultimo.AddMonths(i - quanti + 1))
			.Select(mese => new TotaleMese(mese, totali.GetValueOrDefault(mese)))
			.ToList();
	}

	public static DateTime PrimoDelMese(DateTime data) => new(data.Year, data.Month, 1);
}
