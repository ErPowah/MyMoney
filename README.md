# Diario Spese

Piccola app per annotare le spese, fatta con **.NET MAUI 10** (C# + XAML). Funziona su Android, iOS, Windows e macOS.

- **+ Nuova spesa**: aggiungi una spesa (descrizione, importo, data, categoria)
- **Tocca una riga**: la modifichi
- **Scorri una riga verso sinistra**: la elimini (solo su schermi touch; su Windows usa il pulsante *Elimina* nella pagina di modifica)
- Il riquadro in alto mostra il totale del mese corrente
- I dati restano sul dispositivo, in un database SQLite locale

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

L'APK è firmato con una chiave di prova conservata nella cache di GitHub, così le versioni nuove si installano sopra le vecchie senza perdere i dati. Se passano più di 7 giorni senza nuove compilazioni, la cache scade e viene creata una chiave nuova. In quel caso disinstalla la vecchia app prima di installare la nuova.

## Come è organizzato il codice (MVVM)

```
Models/Spesa.cs                   il dato: una spesa = una riga del database
Services/SpeseDatabase.cs         legge e scrive su SQLite (pacchetto sqlite-net-pcl)
ViewModels/ElencoSpeseViewModel   logica dell'elenco: caricamento, totale del mese, eliminazione
ViewModels/SpesaViewModel         logica del modulo: controlli e salvataggio
Views/ElencoSpesePage.xaml        pagina principale (interfaccia in XAML)
Views/SpesaPage.xaml              pagina nuova/modifica spesa
AppShell.xaml                     navigazione tra le pagine
MauiProgram.cs                    avvio: dependency injection e formati italiani (€, date)
Platforms/                        codice specifico per ogni piattaforma (di solito non si tocca)
Resources/                        icona, schermata di avvio, font, colori e stili
```

Le pagine XAML non contengono logica: si collegano al ViewModel con `{Binding ...}`. `[ObservableProperty]` e `[RelayCommand]` (dal pacchetto CommunityToolkit.Mvvm) generano il codice che tiene aggiornata l'interfaccia.

## Idee per continuare

- Filtro per mese (mese precedente/successivo)
- Totali per categoria, magari con un grafico
- Esportazione CSV da condividere via email o WhatsApp (`Share.Default.RequestAsync`)
- Un'icona tutta tua: sostituisci `Resources/AppIcon/appicon.svg`
