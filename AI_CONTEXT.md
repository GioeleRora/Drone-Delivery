# AI_CONTEXT - Drone Delivery Simulator (Mobile)

## 📌 Visione del Gioco
Siamo sviluppando un videogioco 3D per dispositivi mobile (iOS e Android) utilizzando Unity 6.5. 
Il giocatore controlla un drone che deve consegnare pacchi in una città post-apocalittica, navigando tra ostacoli, gestendo la batteria e affrontando le raffiche di vento.

## 🚀 Stato Attuale del Progetto (Fase: Greyboxing)
Ci stiamo concentrando sul Core Game Loop e sulla prototipazione. Tutti gli asset 3D esterni sono stati rimossi per alleggerire il progetto: stiamo utilizzando esclusivamente forme primitive di Unity (cubi/cilindri) per il level design procedurale.

## 🏗️ Architettura e Script Principali

### 1. Sistema di Volo (`DroneMovement.cs`)
- **Fisica Realistica:** Gestita rigorosamente tramite `Rigidbody`. Qualsiasi movimento, alterazione di altitudine o forza esterna (vento) usa `rb.AddForce()` e `Torque` all'interno del `FixedUpdate()`.
- **Input Ibrido (Configurabile):** Centralizzato in questo script con un toggle (`useKeyboardInput`) per passare in tempo reale dai controlli Touch (Twin-Stick virtuali) a quelli PC (WASD + Frecce + Spazio/Shift). *(Nota: il vecchio script `DroneInputHandler.cs` è stato dismesso).*
- **Separazione Visiva/Fisica:** Per evitare compenetrazioni anomale dei collider, le rotazioni visive (il "tilt" del drone in accelerazione) vanno applicate a un `Transform` figlio (Visual Model), mantenendo il root fisico del drone sempre dritto.

### 2. Sistema di Telecamera (`CameraFollow.cs`)
- **Comportamento:** Telecamera orbitale in terza persona (Chase Camera). 
- **Logica:** Si aggiorna in `LateUpdate()` usando `Vector3.Lerp` e `transform.LookAt` per mantenere un offset alle spalle del drone ed evitare tremolii derivati dal motore fisico.

### 3. Generazione Mappa Procedurale (`ProceduralMapBuilder.cs`)
- **Struttura:** Assegnato al GameObject `MapManager`. L'esecuzione avviene rigorosamente tramite `[ContextMenu("Genera Mappa Procedurale")]` dall'Inspector.
- **Motore Procedurale:** - Usa mappe `Mathf.PerlinNoise` per gestire l'elevazione (colline) e i biomi.
  - Vengono istanziati 4 prefab basici (primitive 3D): `CityBuilding` (cubi allungati fino a 40m), `Ruin` (cubi bassi), `ParkTree` e `Road`.
  - **Sinking:** Applica un offset negativo sull'asse Y basato su `Terrain.SampleHeight` per adattare gli edifici alle pendenze senza farli fluttuare.

### 4. UI e HUD (`UIManager.cs`, `CompassHUD.cs`, `VirtualJoystick.cs`)
- Interfaccia autogenerante basata su Canvas. Include feedback visivi, joystick capacitivi, un interruttore animato (toggle) e una bussola.

## ⚠️ Pilastri e Standard di Codice (Le tue regole d'oro)
1. **Performance Mobile-First:** Ottimizzazione estrema. Evitare categoricamente l'uso di `GetComponent()`, allocazioni pesanti, o `FindObjectOfType()` in metodi continui come `Update()` o `FixedUpdate()`.
2. **Decoupling (Approccio SOLID):** Gli script devono avere una singola responsabilità. Il motore fisico non deve conoscere la UI. Usare metodi pubblici per comunicare tra componenti.
3. **API Moderne:** Non utilizzare metodi deprecati di Unity. Usa le sintassi aggiornate (es. `FindObjectsByType<T>(FindObjectsInactive.Exclude)`).
4. **Niente Hardcoding:** Qualsiasi parametro di bilanciamento (velocità, scale, dimensioni) DEVE essere esposto nell'Inspector tramite `[SerializeField]` o `public` e categorizzato con `[Header("...")]`.
5. **No UnityEditor in Runtime:** Il codice del gioco non deve mai dipendere dal namespace `UnityEditor`.
6. **Filosofia Greybox:** Fino a nuovo ordine, non suggerire l'integrazione di pacchetti 3D o materiali IA. Usa `GameObject.CreatePrimitive` o manipolazioni di scala matematica.

## 🛑 Regole di Ingaggio per la CLI (Workflow Autorizzativo)
L'agente AI DEVE rispettare questo flusso di lavoro per ogni singola interazione:
1. **Contesto:** Leggi SEMPRE prima questo file e i file `.cs` attuali sul disco prima di proporre modifiche, per capire lo stato di implementazione.
2. **Analisi e Proposta:** Individua la soluzione, fornisci correzioni mirate (senza riscrivere interi script se non strettamente necessario) e SCRIVI la tua proposta strategica o gli snippet di codice nella chat.
3. **Attesa di Conferma:** FERMATI. Non modificare, non creare e non sovrascrivere ALCUN file sul disco in questa fase. Attendi la mia revisione.
4. **Esecuzione:** Modificherai i file sul disco SOLO E SOLTANTO dopo che ti avrò dato un esplicito comando di conferma (es. "Applica le modifiche", "Procedi", "Scrivi i file"). Se non ricevi questo comando, limitati a discutere le soluzioni.