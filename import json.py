import json
import random

# 1. Definizione dei template per le 4 azioni CRUD (formali e colloquiali)
templates = {
    "Create": [
        "Crea {subject} per {name}",
        "Aggiungi {subject} con nome {name}",
        "Inserisci {subject} per l'utente {name}",
        "creami {subject} chiamato {name}",
        "genera un nuovo {subject} per {name}"
    ],
    "Read": [
        "Mostrami i dettagli di {subject} {id}",
        "Cerca {subject} con id {id}",
        "Fammi vedere {subject} numero {id}",
        "leggi i dati di {subject} {id}",
        "trova {subject} {id}"
    ],
    "Update": [
        "Aggiorna {subject} {id} a {value}",
        "Modifica {subject} {id} impostando {value}",
        "Cambia lo stato di {subject} {id} in {value}",
        "updata {subject} {id} a {value}",
        "setta {subject} {id} a {value}"
    ],
    "Delete": [
        "Cancella {subject} {id}",
        "Elimina {subject} {name}",
        "Rimuovi {subject} numero {id}",
        "droppa {subject} {name}",
        "fai fuori {subject} {id} per favore"
    ]
}

# 2. Dizionari di parole per riempire i template (personalizzabili)
subjects = ["ticket", "utente", "database", "server", "fattura", "prodotto", "ordine", "report"]
names = ["Mario Rossi", "Giulia", "Acme Corp", "test", "admin", "backup_2024", "reparto IT"]
ids = ["102", "45", "99", "555", "77", "A-123", "001"]
values = ["spedito", "12 euro", "alta", "inattivo", "123456", "completato", "bassa"]

dataset = []

# 3. Ciclo per generare N esempi (es. 500)
for i in range(500):
    action = random.choice(list(templates.keys()))
    template = random.choice(templates[action])
    
    subject = random.choice(subjects)
    name = random.choice(names)
    item_id = random.choice(ids)
    value = random.choice(values)
    
    # Costruisci la frase utente e il payload JSON a seconda dell'azione
    if action == "Create":
        phrase = template.format(subject=subject, name=name)
        payload = {"name": name}
    elif action == "Read":
        phrase = template.format(subject=subject, id=item_id)
        payload = {"id": item_id}
    elif action == "Update":
        phrase = template.format(subject=subject, id=item_id, value=value)
        payload = {"id": item_id, "value": value}
    elif action == "Delete":
        if "{id}" in template:
            phrase = template.format(subject=subject, id=item_id)
            payload = {"id": item_id}
        else:
            phrase = template.format(subject=subject, name=name)
            payload = {"name": name}
            
    # Formato standard richiesto da HuggingFace / Unsloth
    entry = {
        "messages": [
            {"role": "system", "content": "Sei un estrattore CRUD. Rispondi solo in JSON con campi: action, subject, payload."},
            {"role": "user", "content": phrase},
            {"role": "assistant", "content": json.dumps({"action": action, "subject": subject.capitalize(), "payload": payload})}
        ]
    }
    dataset.append(entry)

# 4. Salva tutto nel file JSONL
file_path = "dataset_ampliato.jsonl"
with open(file_path, "w", encoding="utf-8") as f:
    for entry in dataset:
        f.write(json.dumps(entry, ensure_ascii=False) + "\n")

print(f"Dataset salvato con successo: {file_path}")