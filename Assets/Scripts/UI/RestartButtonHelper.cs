using UnityEngine;

public class RestartButtonHelper : MonoBehaviour
{
    /// <summary>
    /// Metodo chiamato al click del pulsante di restart per ricaricare la partita.
    /// </summary>
    public void ClickRestart()
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.RestartGame();
        }
        else
        {
            // Fallback nel caso in cui non sia presente il GameStateManager in scena
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
    }
}
