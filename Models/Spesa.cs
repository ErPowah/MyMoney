using SQLite;

namespace DiarioSpese.Models;

// Una spesa del diario: ogni oggetto diventa una riga della tabella "Spesa" nel database SQLite.
public class Spesa
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }

	public string Descrizione { get; set; } = string.Empty;

	public decimal Importo { get; set; }

	public DateTime Data { get; set; } = DateTime.Today;

	public string Categoria { get; set; } = "Altro";
}
