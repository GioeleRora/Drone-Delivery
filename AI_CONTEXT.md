# PROGETTO: Drone Post-Apocalittico (Mobile)

## 📌 Visione del Gioco
Siamo sviluppando un videogioco 3D per dispositivi mobile (iOS e Android) utilizzando Unity 6.5. 
Il giocatore controlla un drone che deve consegnare pacchi in una città post-apocalittica, navigando tra ostacoli, gestendo la batteria e affrontando le raffiche di vento.

## 🏗️ Pilastri Architetturali (Le tue regole d'oro)
1. **Performance Mobile-First:** Il codice deve essere altamente ottimizzato. Evitare categoricamente l'uso di `GetComponent()`, `FindObjectOfType()` o allocazioni pesanti all'interno di metodi continui come `Update()` o `FixedUpdate()`.
2. **Decoupling (Disaccoppiamento):** Gli script devono avere una singola responsabilità (approccio SOLID). Il motore fisico non deve conoscere la UI, e la gestione dell'input non deve applicare direttamente le forze. Usare metodi pubblici per comunicare tra componenti.
3. **Fisica Realistica:** Il movimento principale del drone è gestito rigorosamente tramite un componente `Rigidbody`. Qualsiasi movimento, alterazione di altitudine o interferenza esterna (es. Vento) deve essere applicato tramite `rb.AddForce()` all'interno del `FixedUpdate()`.
4. **Separazione Visiva/Fisica:** Per evitare compenetrazioni anomale dei collider, le rotazioni visive (il "tilt" del drone quando accelera) devono essere applicate a un `Transform` figlio (Visual Model), mantenendo il root fisico del drone sempre dritto.

## 🚀 Stato Attuale del Progetto (Fase 1)
Ci stiamo concentrando sul Core Game Loop di base:
- **DroneMovement.cs:** Gestisce il volo in 3D. Riceve vettori di input dall'esterno.
- **DroneInputHandler.cs:** Fa da ponte tra l'input del giocatore (touch UI / joystick virtuale o fallback per PC) e lo script di movimento.

## 🛠️ Istruzioni per l'Assistente AI (CLI)
Ogni volta che ti viene chiesto di generare, analizzare o modificare del codice:
1. Leggi SEMPRE prima questo file per allinearti al contesto.
2. Leggi i file `.cs` attuali sul disco prima di proporre modifiche, per capire lo stato di implementazione.
3. Fornisci solo le correzioni mirate, senza riscrivere interi script se non strettamente necessario.

## 🛑 Regole di Ingaggio (Workflow Autorizzativo)
L'agente AI DEVE rispettare questo flusso di lavoro per ogni singola interazione:
1. **Analisi e Proposta:** Quando ricevi una richiesta, analizza il codice, individua la soluzione e SCRIVI la tua proposta strategica o gli snippet di codice nella chat.
2. **Attesa di Conferma:** FERMATI. Non modificare, non creare e non sovrascrivere ALCUN file sul disco in questa fase. Attendi la mia revisione.
3. **Esecuzione:** Modificherai i file sul disco SOLO E SOLTANTO dopo che ti avrò dato un esplicito comando di conferma (es. "Applica le modifiche", "Procedi", "Scrivi i file"). Se non ricevi questo comando, limitati a discutere le soluzioni.