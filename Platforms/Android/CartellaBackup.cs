using Android.Content;
using Android.Provider;
using AndroidUri = Android.Net.Uri;

namespace DiarioSpese.Services;

// Scrive la copia di sicurezza in Download/DiarioSpese con MediaStore: da Android 10 un'app può creare file lì
// senza chiedere permessi, e i file restano anche quando l'app viene disinstallata.
static class CartellaBackup
{
	public static bool Disponibile => OperatingSystem.IsAndroidVersionAtLeast(29);

	// L'indirizzo del file creato da questa installazione, per sovrascriverlo ai backup successivi. Sta nella
	// cartella esclusa dal backup di Google: dopo una reinstallazione si crea un file nuovo, e quello vecchio,
	// con le spese da ripristinare, resta intatto.
	static string FileIndirizzo =>
		Path.Combine(Android.App.Application.Context.NoBackupFilesDir!.AbsolutePath, "backup_uri.txt");

	public static void Scrivi(string copia)
	{
		if (!OperatingSystem.IsAndroidVersionAtLeast(29))
			return;

		var resolver = Android.App.Application.Context.ContentResolver!;
		if (File.Exists(FileIndirizzo) && ProvaASovrascrivere(resolver, AndroidUri.Parse(File.ReadAllText(FileIndirizzo))!, copia))
			return;

		var valori = new ContentValues();
		valori.Put(MediaStore.IMediaColumns.DisplayName, BackupService.NomeFile);
		valori.Put(MediaStore.IMediaColumns.MimeType, "application/octet-stream");
		valori.Put(MediaStore.IMediaColumns.RelativePath, BackupService.Cartella);
		var uri = resolver.Insert(MediaStore.Downloads.ExternalContentUri!, valori)
			?? throw new IOException("Android non ha creato il file di backup.");
		CopiaIn(resolver, uri, copia);
		File.WriteAllText(FileIndirizzo, uri.ToString());
	}

	// Falso se il file non si può più scrivere, per esempio perché è stato cancellato dall'app File
	static bool ProvaASovrascrivere(ContentResolver resolver, AndroidUri uri, string copia)
	{
		try
		{
			CopiaIn(resolver, uri, copia);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	static void CopiaIn(ContentResolver resolver, AndroidUri uri, string copia)
	{
		using var uscita = resolver.OpenOutputStream(uri, "wt") // "wt": sovrascrive tutto il file, non solo l'inizio
			?? throw new IOException("Android non ha aperto il file di backup.");
		using var sorgente = File.OpenRead(copia);
		sorgente.CopyTo(uscita);
	}
}
