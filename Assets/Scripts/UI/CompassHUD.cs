using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CompassHUD : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Il Transform del drone da tracciare per la direzione.")]
    [SerializeField] private Transform drone;

    [Tooltip("Il RectTransform del nastro della bussola da muovere.")]
    [SerializeField] private RectTransform compassTape;

    [Header("Configuration")]
    [Tooltip("Numero di pixel per ciascun grado di rotazione (aumenta per rallentare lo scorrimento e distanziare i punti).")]
    [SerializeField] private float pixelsPerDegree = 6f;

    [Tooltip("Intervallo di gradi dopo il quale la bussola si ripete (tipicamente 360).")]
    [SerializeField] private float wrapDegrees = 360f;

    [Tooltip("Offset angolare per allineare correttamente il nastro della bussola.")]
    [SerializeField] private float headingOffset = 0f;

    [Header("Aesthetic Settings")]
    [SerializeField] private Color cardinalColor = new Color(0.1f, 0.85f, 1f, 1f); // Neon Cyan
    [SerializeField] private Color numericColor = new Color(0.8f, 0.8f, 0.8f, 1f);  // Light Grey
    [SerializeField] private Color tickColor = new Color(0.6f, 0.6f, 0.6f, 0.8f);     // Semi-transparent grey

    private float tapeWidth;

    private void Start()
    {
        UpdateTapeWidth();
        GenerateTape();
    }

    private void OnValidate()
    {
        UpdateTapeWidth();
    }

    [ContextMenu("Rigenera Nastro Bussola")]
    public void RegenerateTapeMenu()
    {
        UpdateTapeWidth();
        GenerateTape();
    }

    private void UpdateTapeWidth()
    {
        tapeWidth = wrapDegrees * pixelsPerDegree;
    }

    private void Update()
    {
        if (drone == null || compassTape == null || tapeWidth <= 0f) return;

        // Ottiene la rotazione Y del drone (heading) in gradi
        float heading = drone.eulerAngles.y;

        // Applica l'offset di allineamento
        heading += headingOffset;

        // Esegue il wrapping matematico tra 0 e wrapDegrees
        float wrappedHeading = ((heading % wrapDegrees) + wrapDegrees) % wrapDegrees;

        // Calcola la posizione target sull'asse X
        float targetX = -wrappedHeading * pixelsPerDegree;

        // Aggiorna la posizione del RectTransform mantendo invariata l'altezza (Y)
        Vector2 currentPosition = compassTape.anchoredPosition;
        compassTape.anchoredPosition = new Vector2(targetX, currentPosition.y);
    }

    /// <summary>
    /// Genera proceduralmente i punti cardinali, i numeri e le tacchette del nastro della bussola
    /// </summary>
    private void GenerateTape()
    {
        if (compassTape == null) return;

        // 1. Pulisce i vecchi figli (ma conserva eventuali target marker gestiti da altri script)
        List<GameObject> childrenToDestroy = new List<GameObject>();
        foreach (Transform child in compassTape)
        {
            // Distrugge solo le grafiche autogenerate (hanno il tag o il nome specifico)
            if (child.name.StartsWith("Gen_"))
            {
                childrenToDestroy.Add(child.gameObject);
            }
        }
        
        foreach (var child in childrenToDestroy)
        {
            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }

        // 2. Genera il nastro da -180 a 540 gradi per coprire perfettamente lo scorrimento infinito
        int startAngle = -180;
        int endAngle = 540;
        int step = 5;

        for (int angle = startAngle; angle <= endAngle; angle += step)
        {
            float localX = angle * pixelsPerDegree;
            int normalizedAngle = ((angle % 360) + 360) % 360;

            // Determina la categoria dell'angolo
            bool isCardinal = (normalizedAngle % 90 == 0);
            bool isSubCardinal = (normalizedAngle % 45 == 0) && !isCardinal;
            bool isNumeric = (normalizedAngle % 30 == 0) && !isCardinal && !isSubCardinal;
            bool isMajorTick = (normalizedAngle % 10 == 0) && !isNumeric && !isCardinal && !isSubCardinal;

            // Altezze e spessori delle tacchette
            float tickHeight = 6f;
            float tickWidth = 1.5f;
            Color currentTickColor = tickColor;

            string labelText = "";
            Color labelColor = numericColor;
            float labelFontSize = 12f;
            bool labelBold = false;

            if (isCardinal)
            {
                tickHeight = 16f;
                tickWidth = 2.5f;
                currentTickColor = cardinalColor;
                labelColor = cardinalColor;
                labelFontSize = 18f;
                labelBold = true;

                switch (normalizedAngle)
                {
                    case 0: labelText = "N"; break;
                    case 90: labelText = "E"; break;
                    case 180: labelText = "S"; break;
                    case 270: labelText = "W"; break;
                }
            }
            else if (isSubCardinal)
            {
                tickHeight = 12f;
                tickWidth = 2f;
                labelFontSize = 13f;

                switch (normalizedAngle)
                {
                    case 45: labelText = "NE"; break;
                    case 135: labelText = "SE"; break;
                    case 225: labelText = "SW"; break;
                    case 315: labelText = "NW"; break;
                }
            }
            else if (isNumeric)
            {
                tickHeight = 10f;
                tickWidth = 2f;
                // I simulatori mostrano le decine di gradi (es. 03 = 30°, 12 = 120°) per pulizia visiva
                labelText = (normalizedAngle / 10).ToString("D2"); 
                labelFontSize = 12f;
            }
            else if (isMajorTick)
            {
                tickHeight = 8f;
                tickWidth = 1.5f;
            }

            // A. Crea la tacchetta (Tick Line)
            GameObject tickObj = new GameObject("Gen_Tick_" + angle, typeof(RectTransform), typeof(CanvasRenderer), typeof(UnityEngine.UI.Image));
            tickObj.transform.SetParent(compassTape, false);
            
            RectTransform tickRect = tickObj.GetComponent<RectTransform>();
            tickRect.anchorMin = new Vector2(0.5f, 0f);
            tickRect.anchorMax = new Vector2(0.5f, 0f);
            tickRect.pivot = new Vector2(0.5f, 0f);
            tickRect.anchoredPosition = new Vector2(localX, 0f);
            tickRect.sizeDelta = new Vector2(tickWidth, tickHeight);

            UnityEngine.UI.Image tickImg = tickObj.GetComponent<UnityEngine.UI.Image>();
            tickImg.color = currentTickColor;

            // B. Crea il testo (Label) se necessario
            if (!string.IsNullOrEmpty(labelText))
            {
                GameObject labelObj = new GameObject("Gen_Label_" + angle, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                labelObj.transform.SetParent(compassTape, false);

                RectTransform labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0.5f, 0f);
                labelRect.anchorMax = new Vector2(0.5f, 0f);
                labelRect.pivot = new Vector2(0.5f, 0f);
                // Posiziona il testo sopra la tacchetta
                labelRect.anchoredPosition = new Vector2(localX, tickHeight + 4f);
                labelRect.sizeDelta = new Vector2(60f, 30f);

                TextMeshProUGUI labelTMP = labelObj.GetComponent<TextMeshProUGUI>();
                labelTMP.text = labelText;
                labelTMP.fontSize = labelFontSize;
                labelTMP.fontStyle = labelBold ? FontStyles.Bold : FontStyles.Normal;
                labelTMP.color = labelColor;
                labelTMP.alignment = TextAlignmentOptions.Bottom;
            }
        }
    }

    #region Public API
    public float PixelsPerDegree => pixelsPerDegree;
    public Transform Drone => drone;

    /// <summary>
    /// Permette di impostare dinamicamente il drone da tracciare.
    /// </summary>
    public void SetDroneTransform(Transform newDrone)
    {
        drone = newDrone;
    }
    #endregion
}
