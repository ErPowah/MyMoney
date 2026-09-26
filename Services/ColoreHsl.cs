namespace DiarioSpese.Services;

// Calcoli su colori HSL (tonalità/saturazione/luminosità) e sul contrasto, usati da TemaService
// per costruire l'intera palette dell'app da una sola tonalità scelta dall'utente.
public static class ColoreHsl
{
	// Tonalita in gradi (0-360, i valori fuori range si "avvolgono"), saturazione e luminosita da 0 a 1
	public static Color DaHsl(double tonalita, double saturazione, double luminosita)
	{
		double h = (tonalita % 360 + 360) % 360 / 360.0;
		saturazione = Math.Clamp(saturazione, 0, 1);
		luminosita = Math.Clamp(luminosita, 0, 1);

		if (saturazione == 0)
			return new Color((float)luminosita, (float)luminosita, (float)luminosita);

		double q = luminosita < 0.5 ? luminosita * (1 + saturazione) : luminosita + saturazione - luminosita * saturazione;
		double p = 2 * luminosita - q;
		return new Color((float)Canale(p, q, h + 1.0 / 3), (float)Canale(p, q, h), (float)Canale(p, q, h - 1.0 / 3));
	}

	static double Canale(double p, double q, double t)
	{
		if (t < 0) t += 1;
		if (t > 1) t -= 1;
		if (t < 1.0 / 6) return p + (q - p) * 6 * t;
		if (t < 1.0 / 2) return q;
		if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
		return p;
	}

	// Luminanza relativa di un colore, formula WCAG (0 = nero, 1 = bianco)
	public static double LuminanzaRelativa(Color colore)
	{
		static double Lineare(float c) => c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
		return 0.2126 * Lineare(colore.Red) + 0.7152 * Lineare(colore.Green) + 0.0722 * Lineare(colore.Blue);
	}

	// Rapporto di contrasto WCAG tra due colori: va da 1 (identici) a 21 (bianco puro su nero puro)
	public static double Contrasto(Color a, Color b)
	{
		var (chiara, scura) = LuminanzaRelativa(a) >= LuminanzaRelativa(b) ? (a, b) : (b, a);
		return (LuminanzaRelativa(chiara) + 0.05) / (LuminanzaRelativa(scura) + 0.05);
	}

	// Cerca, scendendo da "partenza" verso "minima", la luminosità più alta (quindi il colore più chiaro e
	// vivace) il cui contrasto con "sfondo" raggiunge "contrastoRichiesto". Se nessuna la raggiunge,
	// restituisce quella con il contrasto migliore trovato nell'intervallo: il testo resta il più leggibile
	// possibile anche nel caso peggiore, invece di restare bloccato su un valore che non la raggiunge affatto.
	public static double TrovaLuminosita(double tonalita, double saturazione, Color sfondo,
		double contrastoRichiesto, double partenza, double minima, double passo = 0.002)
	{
		double miglioreL = minima, miglioreContrasto = -1;
		for (double l = partenza; l >= minima; l -= passo)
		{
			double contrasto = Contrasto(DaHsl(tonalita, saturazione, l), sfondo);
			if (contrasto > miglioreContrasto)
			{
				miglioreContrasto = contrasto;
				miglioreL = l;
			}
			if (contrasto >= contrastoRichiesto)
				return l;
		}
		return miglioreL;
	}
}
