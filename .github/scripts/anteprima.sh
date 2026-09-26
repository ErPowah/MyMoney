#!/bin/bash
# Installa l'app sull'emulatore con i dati di esempio e salva le schermate di ogni scheda,
# in modalità chiara e scura, nella cartella "anteprima".
# Lo esegue il job "anteprima" di .github/workflows/android.yml.
set -u
APK="$1"
PKG=it.diariospese.app
OUT=anteprima
ERRORI=0
mkdir -p "$OUT"

# Coordinate del centro del controllo che mostra quel testo (maiuscole e minuscole non contano;
# "@EditText" indica invece il primo campo di testo). Se ce ne sono più di uno (per esempio il titolo
# e la scheda "Cronologia") prende quello più in basso, oppure quello più in alto con "alto" come secondo
# argomento. Con "limiti" stampa x1 y1 x2 y2.
trova() {
  adb shell uiautomator dump /sdcard/ui.xml > /dev/null 2>&1
  adb shell cat /sdcard/ui.xml 2> /dev/null | python3 -c '
import re, sys
import xml.etree.ElementTree as ET
testo = sys.argv[1].casefold()
modo = sys.argv[2] if len(sys.argv) > 2 else ""
def corrisponde(nodo):
    if testo.startswith("@"):
        return nodo.get("class", "").casefold().endswith(testo[1:])
    return testo in ((nodo.get("text") or "").casefold(), (nodo.get("content-desc") or "").casefold())
try:
    radice = ET.fromstring(sys.stdin.read())
except ET.ParseError:
    sys.exit()
punti = []
for nodo in radice.iter("node"):
    if corrisponde(nodo):
        x1, y1, x2, y2 = map(int, re.findall(r"\d+", nodo.get("bounds")))
        if x2 > x1 and y2 > y1:
            punti.append(((x1 + x2) // 2, (y1 + y2) // 2, x1, y1, x2, y2))
if punti:
    p = (min if modo == "alto" else max)(punti, key=lambda p: p[1])
    print(*(p[2:] if modo == "limiti" else p[:2]))
' "$@"
}

aspetta() {
  for _ in $(seq 1 45); do
    [ -n "$(trova "$1")" ] && return 0
    sleep 2
  done
  echo "::warning title=Anteprima::Non trovo \"$1\" sullo schermo"
  ERRORI=$((ERRORI + 1))
  return 1
}

tocca() {
  local punto
  punto=$(trova "$@")
  if [ -n "$punto" ]; then
    adb shell input tap $punto
    sleep 3
  else
    echo "::warning title=Anteprima::Non riesco a toccare \"$1\""
    ERRORI=$((ERRORI + 1))
  fi
}

# Tocca nel grafico a linee il mese con quell'indice (0 = il più vecchio, 11 = il mese corrente)
tocca_grafico() {
  local limiti densita punto
  limiti=$(trova "Grafico a linee delle spese mensili" limiti)
  densita=$(adb shell wm density | grep -oE '[0-9]+' | tail -n 1)
  if [ -z "$limiti" ]; then
    echo "::warning title=Anteprima::Non trovo il grafico a linee"
    ERRORI=$((ERRORI + 1))
    return
  fi
  punto=$(python3 -c '
import sys
x1, y1, x2, y2 = map(int, sys.argv[1].split())
indice, scala = int(sys.argv[2]), int(sys.argv[3]) / 160
larghezza = (x2 - x1) / scala
x = 52 + (larghezza - 52 - 8) / 12 * (indice + 0.5)  # come DisegnoLinee.XMese
print(int(x1 + x * scala), (y1 + y2) // 2)
' "$limiti" "$1" "$densita")
  adb shell input tap $punto
  sleep 3
}

foto() {
  adb exec-out screencap -p > "$OUT/$1.png"
  echo "Salvata $1.png"
}

adb install -r "$APK" || { echo "::error title=Anteprima::Installazione dell'APK non riuscita"; exit 1; }

# Dati di esempio copiati nella cartella privata dell'app: si può fare solo con la versione Debug
python3 .github/scripts/dati_demo.py /tmp/spese.db3
adb push /tmp/spese.db3 /data/local/tmp/spese.db3
adb shell run-as $PKG mkdir -p files
adb shell "cat /data/local/tmp/spese.db3 | run-as $PKG sh -c 'cat > files/spese.db3'"
adb shell run-as $PKG ls -l files

adb logcat -c
adb shell cmd uimode night no
adb shell monkey -p $PKG -c android.intent.category.LAUNCHER 1

aspetta "+ Nuova spesa" && sleep 3
foto 01-spese

# Categorie: la pagina, il dialogo per crearne una e l'elenco con la nuova categoria
tocca "Categorie"
aspetta "+ Nuova categoria"
foto 02-categorie
tocca "+ Nuova categoria"
aspetta "Nuova categoria"
tocca "@EditText"
adb shell input text "Regali"
sleep 1
foto 03-nuova-categoria
tocca "Crea"
aspetta "Regali"
foto 04-categorie-regali
adb shell input keyevent KEYCODE_BACK
sleep 3

tocca "Cronologia"
aspetta "Totale mensile, ultimi 12 mesi. Tocca un mese per vederne le spese."
foto 05-cronologia

MESE_SCORSO=$(python3 -c 'import datetime; m = "gen feb mar apr mag giu lug ago set ott nov dic".split(); print(m[(datetime.date.today().month - 2) % 12])')
tocca "$MESE_SCORSO"
foto 06-cronologia-mese-scorso

# Grafico a linee: tutte le categorie, poi senza "Casa", poi con la linea di tutte le categorie insieme
tocca "Linee"
aspetta "Tocca una voce per mostrare o nascondere la sua linea. «Tutte le categorie» è la somma di tutte."
foto 07-cronologia-linee
# le voci della legenda sono sopra il grafico; sotto c'è la tabella con gli stessi nomi
tocca "Casa" alto
foto 08-cronologia-linee-senza-casa
tocca "Tutte le categorie" alto
foto 09-cronologia-linee-tutte
tocca_grafico 8
foto 10-cronologia-linee-giugno

tocca "Analisi"
aspetta "Totale del periodo"
foto 11-analisi

tocca "Quest'anno"
foto 12-analisi-anno

tocca "Spese"
tocca "+ Nuova spesa"
aspetta "Salva"
foto 13-nuova-spesa
tocca "+ Nuova categoria"
aspetta "Nuova categoria"
tocca "@EditText"
adb shell input text "Animali"
tocca "Crea"
aspetta "Animali"
foto 14-nuova-spesa-categoria-creata
adb shell input keyevent KEYCODE_BACK
sleep 3

adb shell cmd uimode night yes
sleep 4
foto 15-spese-scuro
tocca "Cronologia"
foto 16-cronologia-linee-scuro
tocca "Colonne"
foto 17-cronologia-colonne-scuro
tocca "Analisi"
foto 18-analisi-scuro
tocca "Spese"
tocca "Categorie"
aspetta "+ Nuova categoria"
foto 19-categorie-scuro

adb logcat -d > "$OUT/logcat.txt"
adb logcat -d -b crash > "$OUT/crash.txt"
if [ -s "$OUT/crash.txt" ]; then
  echo "::error title=Anteprima::L'app è andata in crash, vedi crash.txt nel ramo anteprima"
  ERRORI=$((ERRORI + 1))
fi

echo "Problemi riscontrati: $ERRORI"
exit $ERRORI
