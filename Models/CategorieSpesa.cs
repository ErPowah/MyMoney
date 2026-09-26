namespace DiarioSpese.Models;

// Nomi fissi e regole delle categorie. L'elenco completo, con quelle che crei tu, è nel database (vedi SpeseDatabase).
public static class CategorieSpesa
{
	// Le categorie di partenza, create alla prima apertura dell'app
	public static readonly string[] Predefinite = ["Alimentari", "Casa", "Trasporti", "Salute", "Svago", "Altro"];

	// La categoria di riserva: raccoglie le spese delle categorie eliminate, quindi non si elimina né si rinomina
	public const string Altro = "Altro";

	// Nome usato dai filtri per dire "tutte insieme": non può diventare il nome di una categoria
	public const string TutteLeCategorie = "Tutte le categorie";

	public const int LunghezzaMassima = 30;

	// Controlla il nome di una categoria nuova o rinominata: restituisce il problema, oppure null se va bene
	public static string? ErroreNome(string? nome, IEnumerable<string> esistenti, string? nomeAttuale = null)
	{
		var pulito = nome?.Trim() ?? string.Empty;
		if (pulito.Length == 0)
			return "Scrivi un nome.";
		if (pulito.Length > LunghezzaMassima)
			return $"Il nome può avere al massimo {LunghezzaMassima} caratteri.";
		if (string.Equals(pulito, TutteLeCategorie, StringComparison.CurrentCultureIgnoreCase))
			return "Questo nome è già usato dai filtri.";
		if (esistenti.Any(e => e != nomeAttuale && string.Equals(e, pulito, StringComparison.CurrentCultureIgnoreCase)))
			return $"Esiste già una categoria «{pulito}».";
		return null;
	}
}
