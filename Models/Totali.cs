namespace DiarioSpese.Models;

// Risultati dei calcoli in Services/Statistiche.cs
public record TotaleCategoria(string Categoria, decimal Totale);

public record TotaleMese(DateTime Mese, decimal Totale);
