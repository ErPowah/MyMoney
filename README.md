# Diario Spese

Piccola app per annotare le spese, fatta con **.NET MAUI 10** (C# + XAML). Funziona su Android, iOS, Windows e macOS.

Tre schede in basso:

- **Spese**: l'elenco con il totale del mese corrente
  - **+ Nuova spesa**: aggiungi una spesa (descrizione, importo, data, categoria)
  - **Tocca una riga**: la modifichi
  - **Scorri una riga verso sinistra**: la elimini (solo su schermi touch; su Windows usa il pulsante *Elimina* nella pagina di modifica)
- **Cronologia**: gli ultimi 12 mesi in due modi, a scelta con i pulsanti *Colonne* e *Linee*.
  - *Colonne*: il totale di ogni mese, filtrabile per categoria.
  - *Linee*: una linea per categoria, ognuna col suo colore; tocca una voce della legenda per mostrarla o nasconderla. La voce *Tutte le categorie* aggiunge la linea della somma di tutte.
  - In entrambi i modi, tocca un mese per vederne il totale e l'elenco delle spese (nel modo *Linee*, anche il valore di ogni linea).
- **Analisi**: scegli un periodo (dal… al…, oppure *Questo mese*, *Mese scorso*, *Ultimi 30 giorni*, *Quest'anno*) e vedi il totale speso, il numero di spese, la media al giorno e il grafico delle spese per categoria.

**Categorie personalizzate**: dal pulsante *Categorie* in alto nella scheda Spese puoi crearne di nuove, rinominarle (le spese le seguono) ed eliminarle (le loro spese passano in *Altro*, che non si può eliminare). Nel modulo di una spesa c'è anche il pulsante *+ Nuova categoria*, per crearne una al volo. Ogni categoria riceve alla creazione un colore fisso per il grafico a linee; la palette ha 8 colori, e dalla nona categoria in poi le linee sono grigie per non confondersi.

I dati restano sul dispositivo, in un database SQLite locale.

## Cosa serve

| Dove | Strumenti |
|---|---|
| Windows | **Visual Studio 2026 Community** (gratuito) con il carico di lavoro *.NET Multi-platform App UI development*. Installa anche l'SDK e l'emulatore Android. |
| Mac | .NET 10 SDK, `sudo dotnet workload install maui`, Xcode, poi VS Code con l'estensione *.NET MAUI* (oppure JetBrains Rider) |
| iPhone | Apple richiede un **Mac con Xcode** per compilare. Da Windows, Visual Studio può collegarsi a un Mac in rete ("Pair to Mac"). |

## Avviare l'app

**Visual Studio:** apri `DiarioSpese.csproj`, scegli la destinazione dal pulsante verde ▶ (*Android Emulators*, *Windows Machine* oppure il tuo telefono collegato via USB) e premi **F5**.

**Riga di comando:**

```bash
dotnet build -t:Run -f net10.0-android     # emulatore o telefono Android
dotnet build -t:Run -f net10.0-ios         # simulatore iOS (solo su Mac)
```

Per installarla sul tuo telefono Android senza passare dal Play Store: attiva *Opzioni sviluppatore → Debug USB*, collega il telefono e avvia l'app da Visual Studio. L'app poi resta installata.

## Provarla sul telefono Android, senza PC

A ogni modifica del ramo `main`, GitHub compila l'app da solo (vedi `.github/workflows/android.yml`). Dopo circa 10 minuti:

1. Dal telefono apri la pagina **Releases** del repository.
2. Tocca `DiarioSpese.apk` per scaricarla.
3. Apri il file e conferma l'installazione. La prima volta Android ti chiede di consentire l'installazione di app da questa fonte (il browser o l'app File).

Nello stesso momento un secondo job avvia l'app su un emulatore Android con dati di esempio e salva le schermate di ogni scheda, chiare e scure, nel ramo `anteprima` del repository: servono a controllare la grafica senza un telefono.

L'APK è firmato con una chiave di prova conservata nella cache di GitHub, così le versioni nuove si installano sopra le vecchie senza perdere i dati. Se passano più di 7 giorni senza nuove compilazioni, la cache scade e viene creata una chiave nuova. In quel caso disinstalla la vecchia app prima di installare la nuova.

## Come è organizzato il codice (MVVM)

```
Models/Spesa.cs                   il dato: una spesa = una riga del database
Models/Categoria.cs               una categoria = una riga della tabella Categoria
Models/CategorieSpesa.cs          categorie iniziali e controllo dei nomi
Services/SpeseDatabase.cs         legge e scrive spese e categorie su SQLite (pacchetto sqlite-net-pcl)
Services/Statistiche.cs           totali per categoria e per mese
ViewModels/ElencoSpeseViewModel   logica dell'elenco: caricamento, totale del mese, eliminazione
ViewModels/SpesaViewModel         logica del modulo: controlli e salvataggio
ViewModels/CronologiaViewModel    grafici dei 12 mesi (colonne e linee), filtri, spese del mese scelto
ViewModels/AnalisiViewModel       periodo, totali e grafico per categoria
ViewModels/CategorieViewModel     creare, rinominare ed eliminare le categorie
ViewModels/ElementiGrafico.cs     colonne, barre e linee dei grafici; colori fissi delle categorie
Views/ElencoSpesePage.xaml        scheda Spese (interfaccia in XAML)
Views/CronologiaPage.xaml         scheda Cronologia
Views/AnalisiPage.xaml            scheda Analisi
Views/SpesaPage.xaml              pagina nuova/modifica spesa
Views/CategoriePage.xaml          pagina Categorie
Views/RigaSpesaView.xaml          una riga dell'elenco, usata da più pagine
Views/GraficoLinee.cs             controllo del grafico a linee (GraphicsView)
Views/DisegnoLinee.cs             il disegno del grafico a linee con Microsoft.Maui.Graphics
AppShell.xaml                     le tre schede in basso e la navigazione
MauiProgram.cs                    avvio: dependency injection e formati italiani (€, date)
Platforms/                        codice specifico per ogni piattaforma (di solito non si tocca)
Resources/                        icona, schermata di avvio, font, colori e stili
```

Le pagine XAML non contengono logica: si collegano al ViewModel con `{Binding ...}`. `[ObservableProperty]` e `[RelayCommand]` (dal pacchetto CommunityToolkit.Mvvm) generano il codice che tiene aggiornata l'interfaccia.

I grafici non usano librerie esterne: ogni colonna o barra è un `Border` dentro un `AbsoluteLayout`, con dimensioni proporzionali al valore. Il grafico a linee invece è disegnato con `Microsoft.Maui.Graphics` dentro un `GraphicsView`.

## Idee per continuare

- Un budget mensile, con un avviso quando lo superi
- Esportazione CSV da condividere via email o WhatsApp (`Share.Default.RequestAsync`)
- Un'icona tutta tua: sostituisci `Resources/AppIcon/appicon.svg`
