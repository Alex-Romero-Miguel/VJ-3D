using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;

    public GameObject main;
    public GameObject credits;
    public GameObject hud;
    public GameObject pausePanel;
    public GameObject instructionsPanel;
    public GameObject settingsPanel;

    private CreditScroller creditScroller;

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

    private void Start()
    {
        creditScroller = credits.GetComponent<CreditScroller>();
        GoToMainMenu();
    }

    public void StartNewGame()
    {
        // Reiniciar el contador de movimientos
        HudManager hudManager = hud.GetComponent<HudManager>();
        if (hudManager != null) hudManager.ResetCounter();

        main.SetActive(false);
        hud.SetActive(true);
        Time.timeScale = 1f;
    }

    public void ResumeGame() // Volver al juego
    {
        main.SetActive(false);
        credits.SetActive(false);
        pausePanel.SetActive(false);
        hud.SetActive(true);
        Time.timeScale = 1f;
    }

    public void OpenInstructions()
    {
        main.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
    }

    public void OpenSettings()
    {
        main.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void ExitGame()
    {
        //Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void GoToMainMenu()
    {
        pausePanel.SetActive(false);
        credits.SetActive(false);
        hud.SetActive(false);
        main.SetActive(true);
        settingsPanel.SetActive(false);
        instructionsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void OpenPause() // abrir panel de pausa
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void PlayCredits()
    {
        StartCoroutine(RunCredits());
    }

    public IEnumerator RunCredits()
    {
        main.SetActive(false);
        hud.SetActive(false);
        credits.SetActive(true);

        yield return StartCoroutine(creditScroller.RunCredits());

        credits.SetActive(false);
        main.SetActive(true);
    }
}
