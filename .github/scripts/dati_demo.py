#!/usr/bin/env python3
# Crea un database SQLite con spese di esempio per le schermate di prova sull'emulatore.
# La tabella è identica a quella che sqlite-net crea dalla classe Models/Spesa.cs.
import calendar
import datetime
import sqlite3
import sys

OGGI = datetime.date.today()

# (mesi fa, giorno del mese, descrizione, importo, categoria)
SPESE = [
    (0, 1, "Affitto", 650.00, "Casa"),
    (0, 3, "Spesa al supermercato", 86.40, "Alimentari"),
    (0, 5, "Benzina", 60.00, "Trasporti"),
    (0, 8, "Farmacia", 23.90, "Salute"),
    (0, 12, "Cinema", 18.00, "Svago"),
    (0, 14, "Pizza con amici", 32.00, "Svago"),
    (0, 18, "Spesa al supermercato", 54.20, "Alimentari"),
    (0, 20, "Abbonamento autobus", 35.00, "Trasporti"),
    (0, 22, "Bolletta della luce", 78.30, "Casa"),
    (0, 25, "Caffè e brioche", 3.50, "Alimentari"),
    (1, 1, "Affitto", 650.00, "Casa"),
    (1, 6, "Spesa al supermercato", 92.10, "Alimentari"),
    (1, 9, "Benzina", 55.00, "Trasporti"),
    (1, 16, "Concerto", 45.00, "Svago"),
    (1, 21, "Dentista", 120.00, "Salute"),
    (1, 27, "Mercato", 27.50, "Alimentari"),
    (2, 1, "Affitto", 650.00, "Casa"),
    (2, 5, "Spesa al supermercato", 74.80, "Alimentari"),
    (2, 12, "Treno per Milano", 39.90, "Trasporti"),
    (2, 19, "Regalo di compleanno", 40.00, "Altro"),
    (3, 1, "Affitto", 650.00, "Casa"),
    (3, 10, "Vacanza al mare", 420.00, "Svago"),
    (3, 15, "Spesa al supermercato", 88.00, "Alimentari"),
    (4, 1, "Affitto", 650.00, "Casa"),
    (4, 11, "Visita medica", 80.00, "Salute"),
    (4, 20, "Spesa al supermercato", 70.40, "Alimentari"),
    (5, 1, "Affitto", 650.00, "Casa"),
    (5, 8, "Assicurazione auto", 380.00, "Trasporti"),
    (5, 17, "Bolletta del gas", 110.00, "Casa"),
]


def data(mesi_fa, giorno):
    anno, mese = OGGI.year, OGGI.month - mesi_fa
    while mese <= 0:
        mese += 12
        anno -= 1
    ultimo = OGGI.day if mesi_fa == 0 else calendar.monthrange(anno, mese)[1]
    return datetime.date(anno, mese, min(giorno, ultimo))


def ticks(giorno):
    # DateTime.Ticks di .NET: intervalli di 100 nanosecondi dal 1° gennaio dell'anno 1
    return (giorno - datetime.date(1, 1, 1)).days * 864_000_000_000


db = sqlite3.connect(sys.argv[1])
db.execute('CREATE TABLE "Spesa" ("Id" integer primary key autoincrement not null, '
           '"Descrizione" varchar, "Importo" float, "Data" bigint, "Categoria" varchar)')
db.executemany('INSERT INTO "Spesa" ("Descrizione", "Importo", "Data", "Categoria") VALUES (?, ?, ?, ?)',
               [(d, i, ticks(data(m, g)), c) for m, g, d, i, c in SPESE])
db.commit()
print(f"{len(SPESE)} spese di esempio scritte in {sys.argv[1]}")
