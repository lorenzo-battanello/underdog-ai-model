---
name: allinea-docs-da-codice
description: "Usa questa skill quando l'utente chiede di allineare la documentazione alle modifiche applicative, aggiornare docs/, AGENTS.md, README.md, sincronizzare architettura, flussi, repository, API, cache, audit, metriche e versioning in base al codice reale."
user-invocable: true
---

# Skill: Allinea Docs Da Codice

## Obiettivo
Aggiornare in modo coerente la documentazione del progetto a partire dalle modifiche nel codice applicativo.

File target principali:
- docs/**
- AGENTS.md
- README.md

## Quando usarla
- Dopo refactor applicativi importanti.
- Quando cambia l'architettura (DI, CQRS, Mediator, Repository Pattern).
- Quando cambiano modelli dati, query, script DB o endpoint API.
- Quando l'utente chiede esplicitamente di sincronizzare documentazione e codice.

## Input atteso
- Branch o working tree corrente con modifiche codice.
- Eventuale focus area indicata dall'utente (es. cache, API, audit, performance).

## Procedura operativa
1. Raccogli differenze codice rispetto allo stato attuale del branch.
2. Identifica impatti documentali: architettura, componenti, flussi, configurazione, database, endpoint.
3. Aggiorna docs/ privilegiando documenti tecnici dedicati.
4. Aggiorna AGENTS.md con pattern architetturali e responsabilita dei servizi.
5. Aggiorna README.md con overview, workflow e output atteso.
6. Verifica consistenza terminologica tra i tre livelli: README, AGENTS, docs tecnici.
7. Evita di inventare feature non presenti nel codice.

## Regole di allineamento
- La documentazione deve riflettere il codice effettivo, non il design ideale.
- Se un comportamento non e implementato, indicarlo come "non implementato" o non documentarlo.
- Mantenere naming coerente con classi e file reali.
- Preferire sezioni brevi e verificabili.
- Non duplicare dettagli ultra-tecnici nel README: spostali nei file docs specifici.

## Checklist minima
- Architettura aggiornata (CQRS, Mediator, DI, Repository Pattern).
- Flusso end-to-end request -> cache -> model -> response.
- Configurazioni runtime documentate (appsettings).
- Schema DB documentato e coerente con script SQL.
- Nuovi moduli (audit, metriche, storico versioni) documentati.
- Rimosse dalla doc le parti obsolete.

## Output richiesto
- Modifiche ai file docs/, AGENTS.md, README.md.
- Breve changelog finale con:
  - file toccati
  - sezioni aggiornate
  - eventuali gap tra codice e documentazione

## Vincoli
- Non cambiare codice applicativo, salvo richiesta esplicita.
- Non introdurre convenzioni non richieste dal progetto.
- Non rimuovere contenuti utili senza sostituzione equivalente.
