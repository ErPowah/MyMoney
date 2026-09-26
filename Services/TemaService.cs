using DiarioSpese.Models;

namespace DiarioSpese.Services;

// Costruisce la palette dell'app (Primary, PrimaryDark, Secondary, ecc.) da una sola tonalità e la
// applica cambiando le risorse dinamiche di App.Resources: le pagine usano {DynamicResource ...}
// invece di {StaticResource ...} per questi colori, quindi si aggiornano da sole, senza bisogno di
// ricreare le pagine o riavviare l'app. La tonalità scelta resta salvata sul telefono (Preferences)
// e viene applicata di nuovo al prossimo avvio.
//
// Le saturazioni sono quelle originali dell'app (calcolate dal viola #512BD4 e dagli altri colori del
// tema): cambia solo la tonalità. La luminosità invece è "adattiva" per Primary e Tertiary, i due
// colori usati come sfondo di un testo bianco o come testo scuro su sfondo chiaro: senza adattarla,
// tonalità chiare come il giallo risulterebbero illeggibili (contrasto vicino a 1,5:1 invece di 4,5:1).
public class TemaService
{
	const string ChiaveTonalita = "tema_tonalita";
	const double ContrastoMinimo = 4.5; // come il testo normale secondo le linee guida WCAG AA

	public double Tonalita { get; private set; } = TemiPreimpostati.TonalitaPredefinita;

	// Da chiamare una sola volta all'avvio, dopo che le risorse XAML sono state caricate
	public void Avvia()
	{
		Tonalita = Preferences.Default.Get(ChiaveTonalita, TemiPreimpostati.TonalitaPredefinita);
		Applica(Tonalita);
	}

	public void Imposta(double tonalita)
	{
		Tonalita = tonalita;
		Preferences.Default.Set(ChiaveTonalita, tonalita);
		Applica(tonalita);
	}

	// Il colore "Primary" per una tonalità, con la stessa luminosità adattiva usata da Applica:
	// la usano le Impostazioni per disegnare l'anteprima di ogni tema senza doverlo applicare.
	public static Color Anteprima(double tonalita) => Primario(tonalita);

	static Color Primario(double h) => ColoreHsl.DaHsl(h, 0.663,
		ColoreHsl.TrovaLuminosita(h, 0.663, Colors.White, ContrastoMinimo, partenza: 0.55, minima: 0.15));

	void Applica(double h)
	{
		var risorse = Application.Current?.Resources;
		if (risorse is null)
			return;

		var primario = Primario(h);
		var secondario = ColoreHsl.DaHsl(h, 0.660, 0.908); // chiarissimo: la luminosità fissa va bene su ogni tonalità
		var terziario = ColoreHsl.DaHsl(h, 0.865,
			ColoreHsl.TrovaLuminosita(h, 0.865, secondario, ContrastoMinimo, partenza: 0.40, minima: 0.08));
		var primarioScuro = ColoreHsl.DaHsl(h, 0.659, 0.759);
		var secondarioTestoScuro = ColoreHsl.DaHsl(h, 0.660, 0.700);
		var graficoScuro = ColoreHsl.DaHsl(h, 0.702, 0.671);

		risorse["Primary"] = primario;
		risorse["PrimaryDark"] = primarioScuro;
		risorse["Secondary"] = secondario;
		risorse["SecondaryDarkText"] = secondarioTestoScuro;
		risorse["Tertiary"] = terziario;
		risorse["GraficoScuro"] = graficoScuro;

		risorse["PrimaryBrush"] = new SolidColorBrush(primario);
		risorse["SecondaryBrush"] = new SolidColorBrush(secondario);
		risorse["TertiaryBrush"] = new SolidColorBrush(terziario);
	}
}
