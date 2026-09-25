using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

// Il disegno del grafico a linee con Microsoft.Maui.Graphics, separato dal controllo GraficoLinee
// così si può provare anche fuori dall'app. Le misure sono in punti, come quelle dei controlli MAUI.
public static class DisegnoLinee
{
	const float Sinistra = 52; // spazio per i valori dell'asse verticale
	const float Destra = 8;
	const float Alto = 10;
	const float Basso = 24; // spazio per i nomi dei mesi

	record Inchiostro(Color Sfondo, Color Testo, Color TestoForte, Color Griglia, Color Base, Color Mirino);

	static readonly Inchiostro Chiaro = new(Colors.White, Color.FromArgb("#6E6E6E"), Colors.Black,
		Color.FromArgb("#E1E1E1"), Color.FromArgb("#C8C8C8"), Color.FromArgb("#919191"));

	static readonly Inchiostro Scuro = new(Color.FromArgb("#1F1F1F"), Color.FromArgb("#ACACAC"), Colors.White,
		Color.FromArgb("#333333"), Color.FromArgb("#404040"), Color.FromArgb("#6E6E6E"));

	// Centro orizzontale del mese "indice": come le colonne, ogni mese occupa una fascia uguale
	public static float XMese(int indice, int mesi, float larghezza) =>
		Sinistra + (larghezza - Sinistra - Destra) / mesi * (indice + 0.5f);

	// Il mese della fascia in cui cade una posizione orizzontale (serve per il tocco)
	public static int IndiceMese(double x, double larghezza, int mesi)
	{
		var fascia = (larghezza - Sinistra - Destra) / mesi;
		return Math.Clamp((int)Math.Floor((x - Sinistra) / fascia), 0, mesi - 1);
	}

	// Fondo scala e distanza tra le righe della griglia, arrotondati a numeri tondi (1, 2, 2,5 o 5 per una potenza di 10)
	public static (double FondoScala, double Passo) Scala(double massimo)
	{
		if (massimo <= 0)
			return (100, 25);

		var grezzo = massimo / 5; // al massimo 5 intervalli
		var potenza = Math.Pow(10, Math.Floor(Math.Log10(grezzo)));
		var passo = new[] { 1, 2, 2.5, 5, 10 }.Select(p => p * potenza).First(p => p >= grezzo);
		return (Math.Ceiling(massimo / passo) * passo, passo);
	}

	public static void Disegna(ICanvas canvas, RectF area, IReadOnlyList<SerieMensile> serie,
		IReadOnlyList<string> etichette, int selezionato, bool scuro)
	{
		int mesi = etichette.Count;
		if (mesi == 0 || area.Width <= Sinistra + Destra)
			return;

		var inchiostro = scuro ? Scuro : Chiaro;
		float fondo = area.Height - Basso;
		double massimo = (double)serie.SelectMany(s => s.Valori).Max().GetValueOrDefault();
		var (fondoScala, passo) = Scala(massimo);
		float Y(double valore) => fondo - (float)(valore / fondoScala) * (fondo - Alto);

		canvas.FontSize = 11;
		canvas.StrokeSize = 1;

		// Righe orizzontali sottili e, a sinistra, i loro valori
		for (int i = 0; i * passo <= fondoScala + passo / 2; i++)
		{
			float y = Y(i * passo);
			canvas.StrokeColor = i == 0 ? inchiostro.Base : inchiostro.Griglia;
			canvas.DrawLine(Sinistra, y, area.Width - Destra, y);
			canvas.FontColor = inchiostro.Testo;
			canvas.DrawString($"{i * passo:N0} €", 0, y - 8, Sinistra - 6, 16, HorizontalAlignment.Right, VerticalAlignment.Center);
		}

		// Nomi dei mesi sotto il grafico; quello scelto in nero (in bianco col tema scuro)
		for (int i = 0; i < mesi; i++)
		{
			float x = XMese(i, mesi, area.Width);
			canvas.FontColor = i == selezionato ? inchiostro.TestoForte : inchiostro.Testo;
			canvas.DrawString(etichette[i], x - 20, fondo + 6, 40, 16, HorizontalAlignment.Center, VerticalAlignment.Top);
		}

		// Linea verticale sul mese scelto
		bool haSelezione = selezionato >= 0 && selezionato < mesi;
		if (haSelezione)
		{
			float x = XMese(selezionato, mesi, area.Width);
			canvas.StrokeColor = inchiostro.Mirino;
			canvas.DrawLine(x, Alto, x, fondo);
		}

		// Una linea per serie, spessa 2 punti con giunture arrotondate
		canvas.StrokeSize = 2;
		canvas.StrokeLineCap = LineCap.Round;
		canvas.StrokeLineJoin = LineJoin.Round;
		foreach (var s in serie)
		{
			var percorso = new PathF();
			bool iniziata = false;
			for (int i = 0; i < mesi && i < s.Valori.Length; i++)
			{
				if (s.Valori[i] is not decimal valore)
					continue; // mese prima dell'inizio del diario: niente linea
				var punto = new PointF(XMese(i, mesi, area.Width), Y((double)valore));
				if (iniziata)
					percorso.LineTo(punto);
				else
					percorso.MoveTo(punto);
				iniziata = true;
			}
			canvas.StrokeColor = scuro ? s.ColoreScuro : s.ColoreChiaro;
			canvas.DrawPath(percorso);
		}

		// Sul mese scelto, un pallino per serie con un anello del colore dello sfondo
		if (haSelezione)
		{
			float x = XMese(selezionato, mesi, area.Width);
			foreach (var s in serie.Where(s => selezionato < s.Valori.Length && s.Valori[selezionato] is not null))
			{
				float y = Y((double)s.Valori[selezionato]!.Value);
				canvas.FillColor = inchiostro.Sfondo;
				canvas.FillCircle(x, y, 6);
				canvas.FillColor = scuro ? s.ColoreScuro : s.ColoreChiaro;
				canvas.FillCircle(x, y, 4);
			}
		}
	}
}
