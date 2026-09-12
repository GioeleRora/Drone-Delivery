# 🏗️ ARCHITECTURE.md - Drone Delivery Simulator

Questo documento detta gli standard tecnici, le pipeline architetturali e le regole di base per tutti gli agenti e sviluppatori coinvolti nella programmazione di "Drone Delivery Simulator". Le seguenti regole sono ferree e servono a mantenere le performance PC/Console stabili.

## 1. Engine & Pipeline
- **Versione Unity:** Unity 6.5 LTS
- **Render Pipeline:** Universal Render Pipeline (URP). Qualsiasi shader o materiale generato deve usare gli standard URP, sfruttando al massimo Shader Graph.

## 2. Input & Sistema di Controllo
- **Nuovo Input System:** Obbligatorio usare l'Input System Package di Unity (niente legacy `Input.GetAxis`).
- **Target Gamepad-First:** Tutti i controlli, il volo (stick analogici) e l'interfaccia UI devono essere progettati *prima* per Xbox/PlayStation Controller. Il supporto mouse/tastiera è un ripiego e non deve governare le scelte di design base.

## 3. Pattern Architetturali
- **Decoupling ed Event-Driven:** Evitare rigide dipendenze a catena (Spaghetti Code). I manager comunicano tramite Eventi `System.Action` in C# o Eventi basati su `ScriptableObject`.
- **Zero Allocazioni nei Loop:** All'interno dei metodi `Update()`, `FixedUpdate()` e `LateUpdate()` è *severamente vietato* l'utilizzo di chiamate costose come `GetComponent<T>()`, `GetComponentInChildren<T>()` o allocazioni di nuove istanze pesanti. 
- **Caching Obbligatorio:** Tutte le referenze necessarie vanno recuperate in `Awake()` o `Start()`.

## 4. Coding Standards & API
- **Convenzioni Nomi:**
  - `PascalCase` per Nomi delle Classi, Metodi e Proprietà.
  - `camelCase` per parametri locali.
  - `_camelCase` per i campi/variabili private (es. `private int _currentAmmo;`).
- **Nessun Hardcoding:** Tutti i parametri bilanciabili, le costanti e le curve di design devono essere serializzati in inspector (`[SerializeField]`) per permetterne la rapida modifica dal Game Designer.
- **Raggruppamento Inspector:** Qualsiasi gruppo di variabili in un componente script deve essere abbellito tramite `[Header("Nome")]` e adeguatamente spiegato con attributi `[Tooltip]`.
- **API Moderne (Unity 6.5):** È vietato usare API deprecate (es. evitare `FindObjectOfType<T>()`, usare `FindFirstObjectByType<T>(FindObjectsInactive.Exclude)`).

## 5. Script di Editor & Workflow Prefab
- **Isolamento Editor/Runtime:** Nessuno script che importa `UnityEditor` può coesistere nel codice di gameplay. Utilizzare i blocchi `#if UNITY_EDITOR` o salvare il codice nella cartella speciale `Assets/Editor/`.
- **Intoccabilità Prefab IA:** L'AI *non deve mai* manipolare o alterare a mano file testuali binari o YAML (come i file `.prefab` e `.unity`). Questo corrompe regolarmente i file di progetto.
- **Tools Procedurali in C#:** Per generare prefab o manipolare GameObjects nell'Editor, scrivere appositi script di editor dotati dell'attributo `[MenuItem]`. Gli oggetti si assemblano da codice prima del play.
- **Script Auto-Configuranti:** Fare largo utilizzo di `[RequireComponent(typeof(T))]` per prevenire errori di dimenticanza componenti nell'Inspector e popolare automaticamente i riferimenti di base implementando l'API `Reset()`.

## 6. Filosofia Greybox
- **Design Iterativo:** Costruire e testare le meccaniche usando primitive semplici (Cubi, Sfere, Capsule). Non bloccare lo sviluppo in attesa di mesh 3D finali.
- **Struttura Separata:** Mantenere i modelli visivi in un Object "Art" separati dalla Root logica e del collider (per evitare che i pivot scalati sballino la fisica rigidbody).
