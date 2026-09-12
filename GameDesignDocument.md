# Drone Delivery Simulator - Game Design Document (GDD)

## 🌍 1. Il Mondo di Gioco e la Lore
Il mondo in cui si muove il giocatore è una landa post-apocalittica controllata da uno spietato regime dittatoriale. La mappa ruota attorno a un immenso cratere: la maggior parte dei grattacieli, delle case e delle fabbriche sono ormai gigantesche rovine mezze distrutte e disabitate. 
In questo scenario desolante vivono pochi superstiti (umani nascosti nelle ombre e robot scartati che cercano di sopravvivere). Il giocatore, tramite il suo drone, è uno dei pochissimi mezzi di collegamento rimasti tra queste entità isolate.

### 📦 1.1. Missioni e Tipologie di Consegna
Le consegne non sono solo un espediente meccanico, ma il modo in cui viene raccontata la storia. A seconda del "pacco", cambia sia la narrativa che la fisica di volo:
- **Consegne di Sopravvivenza (Fisica Pura):** Cibo, acqua o medicine per famiglie umane nascoste tra le rovine. *Gameplay:* Sono carichi pesanti. Modificano drasticamente il baricentro del drone, prosciugano rapidamente la batteria e rendono difficilissimo combattere contro il vento.
- **Consegne Tecnologiche (Ricompensa Alta):** Pezzi di ricambio destinati a comunità di robot clandestini che si riparano a vicenda nella Zona Industriale. *Gameplay:* Carichi delicati, se si sbatte troppo contro i muri il pezzo si danneggia e fallisce la missione.
- **Incarichi Clandestini (Stealth/Time Attack):** Lettere d'amore segrete tra amanti distanti, o messaggi/dati criptati tra i capi dei ribelli. *Gameplay:* Pesano pochissimo, il drone è agilissimo, ma si attivano i "droni pattuglia" della dittatura. Bisogna volare bassi e nascondersi tra i cunicoli per evitare gli scanner.
- **Incarichi di Regime (Scelte Morali):** A volte sarai costretto a lavorare per la dittatura (es. consegnare sensori di sorveglianza o agenti chimici). Queste missioni ti forniscono "Codici di Sicurezza Ufficiali" per superare le No-Fly Zones senza fatica, ma fanno infuriare i Ribelli.

### ⚖️ 1.2. Le Fazioni e la Scelta del Giocatore
Il gioco prevede un delicato bilanciamento (Karma) tra due entità principali, che influenzerà pesantemente il gameplay:
- **Il Regime (La Corporazione dello Zenith):** Governano dall'alto della loro isola volante e dal grattacielo centrale, monopolizzando l'energia pulita. Sfruttano il giocatore come corriere "sacrificabile" per le zone più radioattive o pericolose. **Vantaggio in gioco:** Lavorare per loro abbassa il livello di allerta, disattiva temporaneamente i droni pattuglia e fornisce Lasciapassare Ufficiali.
- **Il Ribelli (La Sottocorrente):** Sopravvissuti, ex-ingegneri e robot rinnegati che vivono nascosti nelle Miniere e nella Zona Industriale. Il loro obiettivo finale è far crollare il potere dello Zenith. **Vantaggio in gioco:** Aiutarli aumenta la taglia sulla tua testa (più pattuglie nemiche), ma sblocca l'accesso a un **Mercato Nero di Upgrade Illegali**. Solo tramite i ribelli puoi ottenere moduli di hacking, scudi anti-EMP e rotori stealth, tutti strumenti indispensabili per affrontare l'assalto finale al grattacielo.

### 🔄 1.3. Game Flow e Struttura delle Missioni
Il giocatore ha totale libertà su come guadagnare Crediti ed Esperienza (EXP), grazie a un sistema ibrido tra incarichi procedurali e missioni di trama:
- **La Bacheca Incarichi:** All'interno dell'Hub, si accede a una lista dinamica di missioni generate proceduralmente.
- **Missioni Secondarie (Grinding e Scelte Morali):** Sono illimitate e sempre disponibili. Il giocatore sceglie attivamente la propria strategia di guadagno: incarichi legali e sicuri (pagati poco), consegne pericolose per il Regime (tanti crediti ma malus reputazione), o contrabbando rischiosissimo per i ribelli. Servono per farmare risorse.
- **Il Livello Licenza (Progressione):** L'EXP ottenuta fa salire il "Livello Licenza" del pilota. Salire di livello è fondamentale perché sblocca i nuovi droni nel Negozio.
- **Missioni Principali (Story/Golden Path):** Sono missioni uniche, scritte a mano (non procedurali). Compaiono nella bacheca solo quando il giocatore raggiunge un certo Livello Licenza. Sono obbligatorie per far avanzare la trama e disattivare in modo permanente le *No-Fly Zone*, sbloccando nuove macro-aree del cratere.

#### 📡 Esempio di Missione Principale: L'Assalto all'Antenna (Sblocco del Parco)
Questa è la prima grande missione di trama per sbloccare l'accesso permanente alla zona del Parco.
- **Fase 1 (Infiltrazione Stealth):** Il giocatore deve entrare nel Parco volando al di sotto della chioma degli alberi. Il radar dell'antenna spazza il cielo aperto: se si vola troppo in alto, scatta l'EMP. 
- **Fase 2 (L'Ascesa nel Vento e i Campi Elettrici):** Arrivati alla base dell'antenna, bisogna scalarla verticalmente con meccanica **Stamina (Surriscaldamento)**. Si naviga tra i cavi scoperti e *Campi Elettromagnetici* che ricaricano la barra.
- **Fase 3 (Il Rilascio):** Sgancio fisico del Drive Virale nel nucleo.

---

## 🎮 2. Sistema di Controllo e Filosofia (Gamepad-First)
L'intero gioco è bilanciato e ottimizzato per l'uso primario del **Gamepad (Joystick)**.
- **Volo:** Sfrutta la precisione degli stick analogici e dei grilletti per gestire propulsione e inclinazione.
- **Interfaccia:** La navigazione nei menu e nell'Hub è pensata per essere immediata tramite croce direzionale (D-Pad) e tasti dorsali (LB/RB). Mouse e Tastiera sono supportati, ma secondari.

---

## 🏙️ 3. Struttura della Mappa e Level Design
La mappa è suddivisa in 5 macro-aree principali, unite da infrastrutture e strutture sotterranee.

### 3.1. Le 5 Parti Principali e la Progressione di Gioco
1. **Quartiere Residenziale (Livello 1):** Un semicerchio sulla terra piatta in cima al cratere.
2. **Il Parco (Livello 2):** Inizialmente bloccato, offre le prime sfide ambientali.
3. **Zona Industriale (Livello 3):** Ambiente verticale scosceso e pericoloso.
4. **DownTown (Livello 4):** Il fondo oscuro del cratere. Altissima densità.
5. **Lo Zenith (Endgame):** L'enorme isola volante sospesa nel cielo.

### 3.2. Il Grattacielo Centrale (La Via per lo Zenith)
Edificio intatto, sterile e iper-tecnologico al centro della DownTown, da scalare verticalmente.

### 3.3. Le Barriere (No-Fly Zones)
Griglie di sicurezza (visibili da alert su HUD) protette da attacchi EMP fatali.

### 3.4. Strutture Sotterranee
- **La Miniera:** Caverne collegate e cunicoli profondi.
- **Le Grotte Naturali.**
- **Il Sistema Stradale a Spirale (La Discesa).**

---

## 🏠 4. Il Quartier Generale (Hub Principale)
Il gioco inizia in una **Stanzetta Segreta** fisicamente integrata nella mappa di gioco. L'Hub è un ambiente fluido 3D/2D.

### 4.1. Setup Architetturale e UI (Glassmorphism)
- **Illuminazione Intima:** Spotlight morbida sopra il banco da lavoro, *GarageCamera* ravvicinata (FOV 40-45).
- **Livello 2D (Advanced Glassmorphism):** Blur di fondo, Tint oscuro, Noise, e Edge Highlight.
- **Tipografia:** `Orbitron` per titoli, `Roboto Mono` per dati, con contrasto forzato costante.

### 4.2. Le 3 Tab (Regia Dinamica tramite LB/RB)
- **1. Officina:** Camera ruotabile, lista componenti su UI sinistra, Radar Chart stat su UI destra.
- **2. Negozio Droni:** Panning su piedistallo, ologrammi 3D per droni bloccati che si materializzano all'acquisto.
- **3. Impostazioni:** Telecamera retrocedente, uso massiccio di Depth of Field.

### 4.3. L'Uscita (Deploy System)
Premendo 'Y' la UI glitcha, la camera si aggancia in Chase, la porta si apre e inizia il gameplay in continuità fisica, senza caricamenti.

---

## 🖥️ 5. Interfaccia di Volo (HUD)
- **Top-Left:** Batteria, HP, Altitudine.
- **Bottom-Center:** Bussola direzionale (3D).
- **Right:** Minimappa tattica.
Tutti gli elementi UI usano Drop Shadow e stile coerente.
