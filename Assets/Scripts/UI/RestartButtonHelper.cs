using UnityEngine;

public class RestartButtonHelper : MonoBehaviour
{
    /// <summary>
    /// Metodo chiamato al click del pulsante di restart per ricaricare la partita.
    /// </summary>
    public void ClickRestart()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }
}
