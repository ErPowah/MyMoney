namespace DiarioSpese.Models;

// Un tema è solo una tonalità (la "H" di HSL, in gradi da 0 a 360): TemaService deriva da questa
// tutti i colori dell'app (vedi Services/TemaService.cs), tenendo fisse saturazione e luminosità di
// ogni ruolo (uguali a quelle del viola originale), così qualunque tonalità scelga chi usa l'app
// il testo sopra resta leggibile.
public record TemaApp(string Nome, double Tonalita);

public static class TemiPreimpostati
{
	public const double TonalitaPredefinita = 253; // il viola con cui è nata l'app

	public static readonly TemaApp[] Tutti =
	[
		new("Viola", TonalitaPredefinita),
		new("Blu", 213),
		new("Azzurro", 195),
		new("Verde acqua", 165),
		new("Verde", 140),
		new("Ambra", 40),
		new("Arancione", 20),
		new("Rosso", 355),
		new("Rosa", 330),
	];
}
