# Antigravity Global Orchestrator - Drone Delivery Simulator

## 📌 Specifiche Tecniche Globali (Unity 6.5)
- **Piattaforma:** PC (Windows / Steam).
- **Target di Controllo Assoluto:** **GAMEPAD-FIRST**. L'esperienza è bilanciata per controller (Stick analogici per il volo, D-Pad/Dorsali per UI). Mouse e Tastiera sono supportati solo come ripiego secondario.
- **Engine & Pipeline:** Unity 6.5 LTS, Universal Render Pipeline (URP).
- **Stile di Progetto:** Tripla-A Indie, architettura modulare, decoupled e orientata alle performance PC.

---

## ⚠️ Le 7 Regole d'Oro di Codice e Performance
Ogni agente che genera codice C# deve rispettare tassativamente:
1. **Zero Allocazioni e Zero Ricerche nei Loop:** Divieto assoluto di `GetComponent()`, `GetComponentInChildren()` o `FindObjectsByType()` all'interno di `Update()`, `FixedUpdate()` o `LateUpdate()`. La cache dei riferimenti si fa in `Awake()` o `Start()`.
2. **API Moderne (Unity 6.5):** Non usare metodi obsoleti/deprecati (es. vietato `FindObjectOfType<T>()`, usare `FindFirstObjectByType<T>(FindObjectsInactive.Exclude)`).
3. **Niente Hardcoding:** Qualsiasi valore numerico o curva di bilanciamento deve essere serializzato tramite `[SerializeField]` privato, raggruppato con `[Header("...")]` e dotato di `[Tooltip("...")]`.
4. **Isolamento Runtime/Editor:** Nessun namespace `UnityEditor` nei file di runtime. Tutto il codice editoriale va racchiuso tra `#if UNITY_EDITOR` o isolato nella cartella `Assets/Editor/`.
5. **Decoupling (SOLID & Events):** Comunicazione tra sistemi basata su eventi C# (`System.Action`) o ScriptableObject Events. Nessuna dipendenza circolare diretta tra moduli distinti.
6. **Filosofia Greybox:** Sviluppare e validare le meccaniche usando primitive 3D (cubi, sfere, capsule) prima di richiedere o integrare mesh finali.
7. **Automazione:** Ridurre al minimo le operazioni manuali dell'utente nell'Inspector. Fornire script che si autoconfigurano (`RequireComponent`, `Reset()`) o tool C# `[MenuItem]` per generare prefabs pronti.

---

## 📂 Documenti Guida nella Root
Tutti gli agenti fanno riferimento a questi tre file nella directory principale:
- `GameDesignDocument.md`: Lore, meccaniche, layout del cratere, garage e parametri di gameplay.
- `ProjectRoadMap.md`: Registro di avanzamento task (`[TODO]`, `[IN PROGRESS]`, `[TESTING]`, `[DONE]`).
- `ARCHITECTURE.md`: Convenzioni C#, pattern strutturali, layer e naming conventions.

---

## 🤖 Registro Ruoli Agenti (`.agents/`)
Richiama l'agente specializzato anteponendo `@` al file corrispondente:
- **`01_LeadArchitect.md`**: Project Management, roadmap, consulenza critica di Game Design e specifiche task.
- **`02_FlightPhysics.md`**: Controller di volo, `FixedUpdate`, fisica del carico, vento, stamina e surriscaldamento.
- **`03_UIShader.md`**: Interfaccia Gamepad-First (HUD, Hub), Camera Director e URP Shaders (Glassmorphism).
- **`04_SystemsEconomy.md`**: ScriptableObjects, bacheca missioni, logica Karma (Zenith vs Sottocorrente) e salvataggi JSON.
- **`05_UnityTooling.md`**: Script in `Assets/Editor/`, generatori di Prefab automatici e validatori di scena.
- **`06_WorldLevelDesign.md`**: Greybox del cratere, No-Fly Zones, illuminazione Hub/Diorama e pipeline prompt AI 3D (DALL-E + Meshy.ai).