# 🚁 Project Roadmap - Drone Delivery Simulator

Questo documento funge da tracker operativo per le varie fasi di sviluppo del progetto, suddivise in Milestones. Ogni task indica il reparto (Agente) di competenza e i criteri di accettazione diretti dal GDD.

## Milestone 1: Core Flight & Secret Garage Hub
*Status: [IN PROGRESS]*

- **[TODO] TASK-101: Ottimizzazione Input Gamepad**
  - **Agent:** @[02_FlightPhysics.md]
  - **GDD Ref:** 2. Sistema di Controllo e Filosofia
  - **Acceptance Criteria:** Volo stabile e responsivo mappato nativamente sugli stick analogici del controller, per rendere il Controller il sistema di input principale (Gamepad-first).

- **[TODO] TASK-102: Setup Hub Garage e Regia 3D**
  - **Agent:** @[06_WorldLevelDesign.md] & @[03_UIShader.md]
  - **GDD Ref:** 4.1. Setup Architetturale e UI
  - **Acceptance Criteria:** Stanza segreta integrata fisicamente nel mondo di gioco (non a coordinate strane). Setup della spotlight sul banco da lavoro, configurazione `GarageCamera` (FOV 40-45). Shader UI Glassmorphism attivo a 4 layer.

- **[TODO] TASK-103: Navigazione UI a Schede (Gamepad)**
  - **Agent:** @[03_UIShader.md]
  - **GDD Ref:** 4.2. Le 3 Tab
  - **Acceptance Criteria:** Navigazione fluida tramite D-Pad e LB/RB tra Officina, Negozio e Impostazioni. Lerp dinamico della camera (Camera Director) e Post-Processing dedicato per le varie schede.

- **[TODO] TASK-104: Sistema di Deploy (Uscita in Volo)**
  - **Agent:** @[02_FlightPhysics.md] & @[03_UIShader.md]
  - **GDD Ref:** 4.3. L'Uscita (Deploy System)
  - **Acceptance Criteria:** Tenendo premuto 'Y', la UI scompare in glitch, la telecamera si aggancia per il volo esterno senza caricamenti e la porta si apre, passando fluidamente dalla scena Hub al gameplay mondo aperto.

---

## Milestone 2: Delivery & Cargo Physics
*Status: [TODO]*

- **[TODO] TASK-201: Sistema di Trasporto Pacco**
  - **Agent:** @[02_FlightPhysics.md]
  - **GDD Ref:** 1.1. Missioni e Tipologie di Consegna
  - **Acceptance Criteria:** Il drone può agganciare un carico fisico (es. tramite ConfigurableJoint), il cui peso e offset modificano realisticamente il baricentro, l'inerzia e il consumo batteria.

- **[TODO] TASK-202: Dinamiche del Vento e Surriscaldamento**
  - **Agent:** @[02_FlightPhysics.md]
  - **GDD Ref:** 1.3. Esempio Missione - Meccanica Stamina
  - **Acceptance Criteria:** `WindManager` implementato per simulare raffiche. Spingere i rotori al 100% controvento attiva il surriscaldamento fino allo stallo completo dei motori (drop del drone).

- **[TODO] TASK-203: Telemetria Volo e HUD**
  - **Agent:** @[03_UIShader.md]
  - **GDD Ref:** 5. Interfaccia di Volo (HUD)
  - **Acceptance Criteria:** Barre vitali (Batteria, Punti Vita) implementate, Bussola funzionante, Minimappa abbozzata, forte leggibilità (Font Orbitron/RobotoMono, ombra forzata per stacco visivo).

---

## Milestone 3: No-Fly Zones & Antenna Mission
*Status: [TODO]*

- **[TODO] TASK-301: Sistema di No-Fly Zone e EMP**
  - **Agent:** @[04_SystemsEconomy.md]
  - **GDD Ref:** 3.3. Le Barriere
  - **Acceptance Criteria:** Entrare in una zona interdetta mostra alert rosso su HUD. Se ignorato, scatta un attacco EMP che invalida i controlli causando game-over.

- **[TODO] TASK-302: AI Droni Pattuglia (Stealth)**
  - **Agent:** @[02_FlightPhysics.md] & @[04_SystemsEconomy.md]
  - **GDD Ref:** 1.1. Incarichi Clandestini
  - **Acceptance Criteria:** Pattuglie nemiche base con scanner a cono che reagiscono al giocatore attivando rinforzi o provocando fallimento stealth.

- **[TODO] TASK-303: Scalata dell'Antenna (Missione Core)**
  - **Agent:** @[06_WorldLevelDesign.md] & @[02_FlightPhysics.md]
  - **GDD Ref:** 1.3. Assalto all'Antenna
  - **Acceptance Criteria:** Creazione della boss fight ambientale: level design con vegetazione, campi elettromagnetici attraversabili per boost stamina, cutscene finale disattivazione scudo.

---

## Milestone 4: Economy & Metagame
*Status: [TODO]*

- **[TODO] TASK-401: Bacheca Missioni Procedurale**
  - **Agent:** @[04_SystemsEconomy.md]
  - **GDD Ref:** 1.3. Struttura delle Missioni
  - **Acceptance Criteria:** Sistema dati dinamico che genera varianti di consegne base (Sopravvivenza, Tecnologiche, Clandestine) offrendo Crediti/EXP differenti.

- **[TODO] TASK-402: Fazioni e Karma (Zenith/Ribelli)**
  - **Agent:** @[04_SystemsEconomy.md]
  - **GDD Ref:** 1.2. Le Fazioni e la Scelta
  - **Acceptance Criteria:** Logica Karma persistente nei salvataggi JSON. Modificatori di gameplay sbloccati per ciascuna fazione (Es: accessi Ufficiali o Mercato Nero).

- **[TODO] TASK-403: Negozio ed Equipaggiamento**
  - **Agent:** @[03_UIShader.md] & @[04_SystemsEconomy.md]
  - **GDD Ref:** 4.2. Tab Negozio Droni
  - **Acceptance Criteria:** Sistema ScriptableObject per gli sblocchi. Ologrammi 3D che si "riempiono" al momento dell'acquisto (VFX), bilanciamento stat e UI di comparazione.
