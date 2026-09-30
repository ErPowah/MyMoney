namespace DiarioSpese.Services;

// Copia di sicurezza del database fuori dalla cartella privata dell'app, che Android cancella quando l'app
// viene disinstallata. Da Android 10 la copia sta in Download/DiarioSpese e si aggiorna da sola all'avvio e
// dopo ogni modifica (Platforms/Android/CartellaBackup.cs); su ogni piattaforma si può anche condividere
// (Drive, email…) e ripristinare dalle Impostazioni.
public class BackupService
{
	public const string NomeFile = "Diario Spese - backup.db3";
	public const string Cartella = "Download/DiarioSpese";
	const string ChiaveUltimoBackup = "backup_ultimo";

	readonly SpeseDatabase database;
	readonly SemaphoreSlim occupato = new(1, 1);
	int inCoda; // 1 se un backup aspetta il suo turno: includerà anche le modifiche arrivate nel frattempo

	public BackupService(SpeseDatabase database)
	{
		this.database = database;
		database.Modificato += () => _ = BackupAutomaticoAsync();
	}

	public static bool Automatico =>
#if ANDROID
		CartellaBackup.Disponibile;
#else
		false;
#endif

	public DateTime? UltimoBackup => Preferences.Default.ContainsKey(ChiaveUltimoBackup)
		? Preferences.Default.Get(ChiaveUltimoBackup, DateTime.MinValue)
		: null;

	public string? UltimoErrore { get; private set; }

	// Da chiamare all'avvio: dopo un aggiornamento dell'app la copia esiste subito, senza aspettare una modifica
	public void Avvia() => _ = BackupAutomaticoAsync();

	async Task BackupAutomaticoAsync()
	{
		if (!Automatico || Interlocked.Exchange(ref inCoda, 1) == 1)
			return;

		await occupato.WaitAsync();
		try
		{
			Interlocked.Exchange(ref inCoda, 0);

			// Mai una copia vuota: appena reinstallata, l'app non deve rimpiazzare il backup con le spese vere
			if (await database.ContaSpeseAsync() == 0)
				return;

			var copia = Path.Combine(FileSystem.CacheDirectory, NomeFile);
			await database.CopiaInAsync(copia);
#if ANDROID
			await Task.Run(() => CartellaBackup.Scrivi(copia));
#endif
			Preferences.Default.Set(ChiaveUltimoBackup, DateTime.Now);
			UltimoErrore = null;
		}
		catch (Exception ex)
		{
			UltimoErrore = ex.Message; // un backup non riuscito non deve mai bloccare il salvataggio di una spesa
		}
		finally
		{
			occupato.Release();
		}
	}

	// Una copia con la data nel nome, da mandare dove si vuole con il menu Condividi
	public async Task<string> CopiaDaCondividereAsync()
	{
		var file = Path.Combine(FileSystem.CacheDirectory, $"Diario Spese - backup {DateTime.Now:yyyy-MM-dd}.db3");
		await database.CopiaInAsync(file);
		return file;
	}

	// Il selettore dei file dà solo un flusso da leggere: lo si copia in un file vero, da controllare e ripristinare
	public static async Task<string> CopiaInCacheAsync(FileResult scelto)
	{
		var file = Path.Combine(FileSystem.CacheDirectory, "ripristino.db3");
		await using (var sorgente = await scelto.OpenReadAsync())
		await using (var destinazione = File.Create(file))
			await sorgente.CopyToAsync(destinazione);
		return file;
	}

	public async Task RipristinaAsync(string file)
	{
		await occupato.WaitAsync();
		try
		{
			await database.SostituisciConAsync(file);
		}
		finally
		{
			occupato.Release();
		}
		await BackupAutomaticoAsync(); // ora la copia fuori dall'app contiene le spese ripristinate
	}
}
