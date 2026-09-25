using DiarioSpese.Models;
using SQLite;

namespace DiarioSpese.Services;

// Legge e scrive le spese in un database SQLite salvato nella cartella privata dell'app.
public class SpeseDatabase
{
	SQLiteAsyncConnection? connessione;

	async Task<SQLiteAsyncConnection> ApriAsync()
	{
		if (connessione is not null)
			return connessione;

		var percorso = Path.Combine(FileSystem.AppDataDirectory, "spese.db3");
		connessione = new SQLiteAsyncConnection(percorso);
		await connessione.CreateTableAsync<Spesa>(); // crea la tabella solo se non esiste
		return connessione;
	}

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
}
