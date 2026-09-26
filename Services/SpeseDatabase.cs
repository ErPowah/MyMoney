using DiarioSpese.Models;
using SQLite;

namespace DiarioSpese.Services;

// Legge e scrive spese e categorie in un database SQLite salvato nella cartella privata dell'app.
public class SpeseDatabase
{
	const int ColoriDisponibili = 8; // come la palette dei grafici (ViewModels/ElementiGrafico.cs)

	readonly string? percorsoFile;
	Task<SQLiteAsyncConnection>? apertura;

	// Nell'app: il file sta nella cartella dati dell'app
	public SpeseDatabase()
	{
	}

	// Nei test: un file a scelta
	public SpeseDatabase(string percorso) => percorsoFile = percorso;

	// La prima chiamata apre il database e lo prepara; le successive riusano la stessa apertura
	Task<SQLiteAsyncConnection> ApriAsync() => apertura ??= PreparaAsync();

	async Task<SQLiteAsyncConnection> PreparaAsync()
	{
		var db = new SQLiteAsyncConnection(percorsoFile ?? Path.Combine(FileSystem.AppDataDirectory, "spese.db3"));
		await db.CreateTableAsync<Spesa>(); // crea le tabelle solo se non esistono
		await db.CreateTableAsync<Categoria>();

		// Alla prima apertura (anche dopo l'aggiornamento da una versione senza categorie personalizzate)
		// la tabella è vuota: si parte dalle categorie predefinite, ognuna col suo colore
		var categorie = await db.Table<Categoria>().ToListAsync();
		if (categorie.Count == 0)
		{
			categorie = CategorieSpesa.Predefinite.Select((nome, i) => new Categoria { Nome = nome, Colore = i }).ToList();
			await db.InsertAllAsync(categorie);
		}

		// Per sicurezza: una categoria usata da qualche spesa ma assente dall'elenco viene aggiunta
		var usate = await db.QueryScalarsAsync<string>("SELECT DISTINCT Categoria FROM Spesa");
		foreach (var nome in usate.Where(n => !string.IsNullOrWhiteSpace(n) && categorie.All(c => c.Nome != n)))
		{
			var nuova = new Categoria { Nome = nome, Colore = PrimoColoreLibero(categorie) };
			await db.InsertAsync(nuova);
			categorie.Add(nuova);
		}

		return db;
	}

	// ----- Spese -----

	public async Task<List<Spesa>> LeggiSpeseAsync()
	{
		var db = await ApriAsync();
		return await db.Table<Spesa>()
			.OrderByDescending(s => s.Data)
			.ThenByDescending(s => s.Id)
			.ToListAsync();
	}

	// Spese dal giorno "dal" al giorno "al", entrambi compresi
	public async Task<List<Spesa>> LeggiSpeseAsync(DateTime dal, DateTime al)
	{
		var db = await ApriAsync();
		var inizio = dal.Date;
		var fine = al.Date.AddDays(1);
		return await db.Table<Spesa>()
			.Where(s => s.Data >= inizio && s.Data < fine)
			.OrderByDescending(s => s.Data)
			.ThenByDescending(s => s.Id)
			.ToListAsync();
	}

	public async Task SalvaAsync(Spesa spesa)
	{
		var db = await ApriAsync();
		if (spesa.Id == 0)
			await db.InsertAsync(spesa); // nuova spesa: l'Id lo assegna SQLite
		else
			await db.UpdateAsync(spesa);
	}

	public async Task EliminaAsync(Spesa spesa)
	{
		var db = await ApriAsync();
		await db.DeleteAsync(spesa);
	}

	// ----- Categorie -----

	// Nell'ordine di creazione, con "Altro" sempre in fondo
	public async Task<List<Categoria>> LeggiCategorieAsync()
	{
		var db = await ApriAsync();
		var categorie = await db.Table<Categoria>().ToListAsync();
		return categorie.OrderBy(c => c.Nome == CategorieSpesa.Altro).ThenBy(c => c.Id).ToList();
	}

	// Il nome va controllato prima con CategorieSpesa.ErroreNome
	public async Task<Categoria> AggiungiCategoriaAsync(string nome)
	{
		var db = await ApriAsync();
		var esistenti = await db.Table<Categoria>().ToListAsync();
		var categoria = new Categoria { Nome = nome.Trim(), Colore = PrimoColoreLibero(esistenti) };
		await db.InsertAsync(categoria);
		return categoria;
	}

	// Cambia il nome della categoria e di tutte le spese che la usano, in un'unica operazione
	public async Task RinominaCategoriaAsync(Categoria categoria, string nuovoNome)
	{
		if (categoria.Nome == CategorieSpesa.Altro)
			throw new InvalidOperationException("La categoria «Altro» non si rinomina.");

		var db = await ApriAsync();
		var vecchioNome = categoria.Nome;
		nuovoNome = nuovoNome.Trim();
		await db.RunInTransactionAsync(t =>
		{
			t.Execute("UPDATE Spesa SET Categoria = ? WHERE Categoria = ?", nuovoNome, vecchioNome);
			categoria.Nome = nuovoNome;
			t.Update(categoria);
		});
	}

	// Elimina la categoria; le sue spese passano in "Altro"
	public async Task EliminaCategoriaAsync(Categoria categoria)
	{
		if (categoria.Nome == CategorieSpesa.Altro)
			throw new InvalidOperationException("La categoria «Altro» non si elimina.");

		var db = await ApriAsync();
		await db.RunInTransactionAsync(t =>
		{
			t.Execute("UPDATE Spesa SET Categoria = ? WHERE Categoria = ?", CategorieSpesa.Altro, categoria.Nome);
			t.Delete(categoria);
		});
	}

	// Il primo colore della palette non ancora usato da nessuna categoria; -1 (grigio) se sono tutti presi
	static int PrimoColoreLibero(IEnumerable<Categoria> categorie)
	{
		var usati = categorie.Select(c => c.Colore).ToHashSet();
		for (int colore = 0; colore < ColoriDisponibili; colore++)
		{
			if (!usati.Contains(colore))
				return colore;
		}
		return -1;
	}
}
