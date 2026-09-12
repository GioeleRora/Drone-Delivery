using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class HubUIManager : MonoBehaviour
{
    public enum HubTab { Officina = 0, Negozio = 1, Impostazioni = 2 }

    [Header("Dependencies")]
    public HubCameraDirector cameraDirector;
    
    [Header("Top Bar Testi")]
    public TextMeshProUGUI navText;

    [Header("UI Panels")]
    public GameObject officinaPanel;
    public GameObject negozioPanel;
    public GameObject impostazioniPanel;

    private int currentTabIndex = 0;

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        Gamepad gamepad = Gamepad.current;
        Keyboard kb = Keyboard.current;

        bool moveLeft = false;
        bool moveRight = false;

        // Input Gamepad PlayStation (L1 / R1)
        if (gamepad != null)
        {
            if (gamepad.leftShoulder.wasPressedThisFrame) moveLeft = true;
            if (gamepad.rightShoulder.wasPressedThisFrame) moveRight = true;
        }

        // Input Tastiera (Q / E) - Fallback
        if (kb != null)
        {
            if (kb.qKey.wasPressedThisFrame) moveLeft = true;
            if (kb.eKey.wasPressedThisFrame) moveRight = true;
        }

        if (moveRight)
        {
            currentTabIndex++;
            if (currentTabIndex > 2) currentTabIndex = 2; // Non cicla, si ferma al limite (stile console)
            else UpdateUI();
        }
        else if (moveLeft)
        {
            currentTabIndex--;
            if (currentTabIndex < 0) currentTabIndex = 0;
            else UpdateUI();
        }
    }

    private void UpdateUI()
    {
        // 1. Attiva/Disattiva Pannelli
        if (officinaPanel != null) officinaPanel.SetActive(currentTabIndex == 0);
        if (negozioPanel != null) negozioPanel.SetActive(currentTabIndex == 1);
        if (impostazioniPanel != null) impostazioniPanel.SetActive(currentTabIndex == 2);

        // 2. Aggiorna Regia 3D (Cerca automaticamente lo script generato dal GarageBuilder se manca)
        if (cameraDirector == null)
        {
            cameraDirector = Object.FindFirstObjectByType<HubCameraDirector>();
        }

        if (cameraDirector != null)
        {
            cameraDirector.ChangeTab((HubTab)currentTabIndex);
        }

        // 3. Aggiorna testo barra di navigazione
        if (navText != null)
        {
            string strOfficina = currentTabIndex == 0 ? "<color=#2CF><b>[ OFFICINA ]</b></color>" : "<color=#888>OFFICINA</color>";
            string strNegozio = currentTabIndex == 1 ? "<color=#2CF><b>[ NEGOZIO ]</b></color>" : "<color=#888>NEGOZIO</color>";
            string strImpostazioni = currentTabIndex == 2 ? "<color=#2CF><b>[ IMPOSTAZIONI ]</b></color>" : "<color=#888>IMPOSTAZIONI</color>";

            navText.text = $"<color=#555>[L1]</color>   {strOfficina}   |   {strNegozio}   |   {strImpostazioni}   <color=#555>[R1]</color>";
        }
    }
}
