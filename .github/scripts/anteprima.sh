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

# Coordinate del centro del controllo che mostra quel testo.
# Se ce ne sono più di uno (per esempio il titolo e la scheda "Cronologia") prende quello più in basso.
trova() {
  adb shell uiautomator dump /sdcard/ui.xml > /dev/null 2>&1
  adb shell cat /sdcard/ui.xml 2> /dev/null | python3 -c '
import re, sys
import xml.etree.ElementTree as ET
testo = sys.argv[1]
try:
    radice = ET.fromstring(sys.stdin.read())
except ET.ParseError:
    sys.exit()
punti = []
for nodo in radice.iter("node"):
    if testo in (nodo.get("text"), nodo.get("content-desc")):
        x1, y1, x2, y2 = map(int, re.findall(r"\d+", nodo.get("bounds")))
        if x2 > x1 and y2 > y1:
            punti.append(((x1 + x2) // 2, (y1 + y2) // 2))
if punti:
    x, y = max(punti, key=lambda p: p[1])
    print(x, y)
' "$1"
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
  punto=$(trova "$1")
  if [ -n "$punto" ]; then
    adb shell input tap $punto
    sleep 3
  else
    echo "::warning title=Anteprima::Non riesco a toccare \"$1\""
    ERRORI=$((ERRORI + 1))
  fi
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

tocca "Cronologia"
aspetta "Totale mensile, ultimi 12 mesi. Tocca un mese per vederne le spese."
foto 02-cronologia

MESE_SCORSO=$(python3 -c 'import datetime; m = "gen feb mar apr mag giu lug ago set ott nov dic".split(); print(m[(datetime.date.today().month - 2) % 12])')
tocca "$MESE_SCORSO"
foto 03-cronologia-mese-scorso

tocca "Analisi"
aspetta "Totale del periodo"
foto 04-analisi

tocca "Quest'anno"
foto 05-analisi-anno

tocca "Spese"
tocca "+ Nuova spesa"
aspetta "Salva"
foto 06-nuova-spesa
adb shell input keyevent KEYCODE_BACK
sleep 3

adb shell cmd uimode night yes
sleep 4
foto 07-spese-scuro
tocca "Cronologia"
foto 08-cronologia-scuro
tocca "Analisi"
foto 09-analisi-scuro

adb logcat -d > "$OUT/logcat.txt"
adb logcat -d -b crash > "$OUT/crash.txt"
if [ -s "$OUT/crash.txt" ]; then
  echo "::error title=Anteprima::L'app è andata in crash, vedi crash.txt nel ramo anteprima"
  ERRORI=$((ERRORI + 1))
fi

echo "Problemi riscontrati: $ERRORI"
exit $ERRORI
