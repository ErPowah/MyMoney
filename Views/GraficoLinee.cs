using System.Windows.Input;
using DiarioSpese.ViewModels;

namespace DiarioSpese.Views;

// Controllo riutilizzabile: un GraphicsView che disegna il grafico a linee (vedi DisegnoLinee)
// e, quando lo tocchi, passa a ToccoCommand l'indice del mese più vicino.
public class GraficoLinee : GraphicsView, IDrawable
{
	public static readonly BindableProperty SerieProperty = BindableProperty.Create(
		nameof(Serie), typeof(IReadOnlyList<SerieMensile>), typeof(GraficoLinee), Array.Empty<SerieMensile>(), propertyChanged: Ridisegna);

	public static readonly BindableProperty EtichetteProperty = BindableProperty.Create(
		nameof(Etichette), typeof(IReadOnlyList<string>), typeof(GraficoLinee), Array.Empty<string>(), propertyChanged: Ridisegna);

	public static readonly BindableProperty IndiceSelezionatoProperty = BindableProperty.Create(
		nameof(IndiceSelezionato), typeof(int), typeof(GraficoLinee), -1, propertyChanged: Ridisegna);

	public static readonly BindableProperty ToccoCommandProperty = BindableProperty.Create(
		nameof(ToccoCommand), typeof(ICommand), typeof(GraficoLinee));

	// Le linee da disegnare
	public IReadOnlyList<SerieMensile> Serie
	{
		get => (IReadOnlyList<SerieMensile>)GetValue(SerieProperty);
		set => SetValue(SerieProperty, value);
	}

	// I nomi dei mesi sotto il grafico
	public IReadOnlyList<string> Etichette
	{
		get => (IReadOnlyList<string>)GetValue(EtichetteProperty);
		set => SetValue(EtichetteProperty, value);
	}

	public int IndiceSelezionato
	{
		get => (int)GetValue(IndiceSelezionatoProperty);
		set => SetValue(IndiceSelezionatoProperty, value);
	}

	public ICommand? ToccoCommand
	{
		get => (ICommand?)GetValue(ToccoCommandProperty);
		set => SetValue(ToccoCommandProperty, value);
	}

	public GraficoLinee()
	{
		Drawable = this;

		var tocco = new TapGestureRecognizer();
		tocco.Tapped += (_, e) =>
		{
			if (Etichette.Count > 0 && e.GetPosition(this) is Point punto)
				ToccoCommand?.Execute(DisegnoLinee.IndiceMese(punto.X, Width, Etichette.Count));
		};
		GestureRecognizers.Add(tocco);

		// I colori cambiano tra tema chiaro e scuro: ridisegna quando cambia il tema
		// (un metodo del controllo, perché MAUI tiene questo evento con un riferimento debole)
		if (Application.Current is not null)
			Application.Current.RequestedThemeChanged += TemaCambiato;
	}

	void TemaCambiato(object? sender, AppThemeChangedEventArgs e) => Invalidate();

	public void Draw(ICanvas canvas, RectF area) =>
		DisegnoLinee.Disegna(canvas, area, Serie, Etichette, IndiceSelezionato,
			Application.Current?.RequestedTheme == AppTheme.Dark);

	static void Ridisegna(BindableObject controllo, object vecchio, object nuovo) =>
		((GraficoLinee)controllo).Invalidate();
}
