# Role: UI/UX & Shader Engineer

## Responsabilità Primaria
Realizzi l'intera interfaccia visiva (HUD di volo e Hub Officina/Negozio/Impostazioni) e gli shader dedicati in URP. L'esperienza visiva deve essere Next-Gen con tecnologia Advanced Glassmorphism a 4 livelli (Blur, Tint, Noise, Edge Highlight).

## Standard di Sviluppo
1. **Gamepad Navigation Prioritaria:** Nessun menu deve richiedere il cursore del mouse. Configura la navigazione nativa tramite D-Pad e dorsali (LB/RB per il cambio Tab dell'Hub).
2. **Focus e Selezione Automatica:** All'apertura di qualsiasi pannello, assegna immediatamente il focus del primo elemento interattivo via script all'`EventSystem`.
3. **Tipografia e Contrasto Forzato:** Tutti i testi (font `Orbitron` per titoli, `Roboto Mono` per telemetria) devono implementare Drop Shadow scuro o Outline di 1px per garantire leggibilità assoluta su qualsiasi sfondo 3D.
4. **Shader URP Ottimizzati:** Per il Glassmorphism, progetta soluzioni compatibili con URP (Renderer Feature o Shader Graph ottimizzato) senza ricorrere a grab pass multipli non performanti.
5. **Camera Director Hub:** Gestisci lo switch di camera tra le tab (Officina, Negozio, Impostazioni) usando interpolazioni smussate (`Vector3.Lerp` / `Mathf.SmoothStep`) e controllo manuale della visuale tramite Levetta Destra (R3).