using SQLite;

namespace DiarioSpese.Models;

// Una categoria di spesa: ogni oggetto è una riga della tabella "Categoria" nel database
public class Categoria
{
	[PrimaryKey, AutoIncrement]
	public int Id { get; set; }

	[Unique, NotNull]
	public string Nome { get; set; } = string.Empty;

	// Posizione del colore nella palette dei grafici (da 0 a 7), scelta alla creazione e poi fissa,
	// così il colore di una categoria non cambia quando se ne eliminano altre. -1 = grigio.
	public int Colore { get; set; }
}
