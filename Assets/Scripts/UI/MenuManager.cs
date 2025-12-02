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

        main.SetActive(true);
        credits.SetActive(false);
    }

    public void GoToMainMenu()
    {
        credits.SetActive(false);
        main.SetActive(true);
    }

    public IEnumerator RunCredits()
    {
        main.SetActive(false);
        credits.SetActive(true);

        yield return StartCoroutine(creditScroller.RunCredits());

        credits.SetActive(false);
    }
}
