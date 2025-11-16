using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private void Awake()
    {
        // Singleton básico
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Cargar una escena por nombre
    public void LoadLevel(string sceneName)
    {
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"La escena '{sceneName}' no existe en Build Settings.");
        }
    }

    // Cargar siguiente nivel según el índice
    public void LoadNextLevel()
    {
        int nextIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else
        {
            Debug.Log("No quedan más niveles. Volviendo al menú.");
            SceneManager.LoadScene("Menu");
        }
    }

    // Reiniciar el nivel actual
    public void RestartLevel()
    {
        Scene actual = SceneManager.GetActiveScene();
        SceneManager.LoadScene(actual.buildIndex);
    }

    // Cargar menú principal
    public void LoadMenu()
    {
        if (Application.CanStreamedLevelBeLoaded("Menu"))
            SceneManager.LoadScene("Menu");
        else
            Debug.LogError("No existe una escena llamada 'Menu' en Build Settings.");
    }

    // Atajo: teclas numéricas para saltar niveles (0–9)
    private void Update()
    {
        if (Keyboard.current == null) return;

        // Comprobar teclas individualmente
        if (Keyboard.current.digit0Key.wasPressedThisFrame) LoadSceneSafe(1);
        if (Keyboard.current.digit1Key.wasPressedThisFrame) LoadSceneSafe(2);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) LoadSceneSafe(3);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) LoadSceneSafe(4);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) LoadSceneSafe(5);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) LoadSceneSafe(6);
        if (Keyboard.current.digit6Key.wasPressedThisFrame) LoadSceneSafe(7);
        if (Keyboard.current.digit7Key.wasPressedThisFrame) LoadSceneSafe(8);
        if (Keyboard.current.digit8Key.wasPressedThisFrame) LoadSceneSafe(9);
        if (Keyboard.current.digit9Key.wasPressedThisFrame) LoadSceneSafe(10);
    }

    // Función auxiliar para evitar errores si el índice no existe
    private void LoadSceneSafe(int index)
    {
        if (index < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(index);
        }
        else
        {
            Debug.LogWarning($"El nivel {index} no está añadido en Build Settings.");
        }
    }
}