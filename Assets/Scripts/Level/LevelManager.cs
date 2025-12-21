using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    
    public float tileAnimationDelay;
    public float levelAnimationDelay;
    public GameObject playerReference;
    public TextAsset[] maps;

    private bool transitioning;
    private int currentLevel;
    private int animationsRunning;
    private MapCreator mapCreator;
    private MenuManager menuManager;
    private MoveCube player;
    private InputAction numberAction;
    private List<GameObject> tiles;

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

        numberAction = InputSystem.actions.FindAction("Go To Level");
    }

    public void ResetToFirstLevel()
    {
        currentLevel = 0;
        if (HudManager.Instance != null) HudManager.Instance.UpdateLevel();
        BeginGame();
    }

    public int GetCurrentLevel()
    { 
        return currentLevel+1; 
    }

    private void Start()
    {
        animationsRunning = 0;
        tiles = new List<GameObject>();
        mapCreator = MapCreator.Instance;
        menuManager = MenuManager.Instance;
        player = playerReference.GetComponent<MoveCube>();

        transitioning = false;
        currentLevel = 0;
        playerReference.SetActive(false);
    }

    // Atajo: teclas numéricas para saltar niveles (del 1 al 9, y el 0 como el 10)
    private void Update()
    {
        if (transitioning || player.isMoving()) return;
        if (Keyboard.current == null) return;

        float n = numberAction.ReadValue<float>();
        if (0 <= n && n < 10) 
        {
            int targetLevel = (int) n;
            if (targetLevel == currentLevel) StartCoroutine(RestartLevel());
            else StartCoroutine(ChangeLevel(targetLevel));
        }
    }

    public void BeginGame() 
    {
        playerReference.SetActive(false);
        StartCoroutine(StartLevel());
    }

    // Inicia el nivel actual
    public IEnumerator StartLevel()
    {
        if(transitioning) yield break;

        LoadLevel(currentLevel);
        yield return StartCoroutine(MapRiseAnimation());

        playerReference.SetActive(true);
        player.Reset();
    }

    // Reinicia el nivel con caída y reaparecer
    public IEnumerator RestartLevel()
    {
        if(transitioning) yield break;
        playerReference.SetActive(false);

        yield return StartCoroutine(MapFallAnimation());

        UnloadLevel();

        yield return new WaitForSeconds(levelAnimationDelay);

        if(transitioning) yield break;

        LoadLevel(currentLevel);

        yield return StartCoroutine(MapRiseAnimation());

        playerReference.SetActive(true);
        player.Reset();
    }

    public void Restart()
    {
        StartCoroutine(RestartLevel());
    }

    public void CompleteLevel()
    {
        StartCoroutine(NextLevel());
    }

    // Completa el nivel con animación en espiral y destruye el mapa
    private IEnumerator NextLevel()
    {
        if(transitioning) yield break;

        yield return StartCoroutine(PlayerSlideAnimation());
        playerReference.SetActive(false);

        // Animación de victoria
        yield return StartCoroutine(MapSpiralAnimation());
        UnloadLevel();
        
        yield return new WaitForSeconds(levelAnimationDelay);

        currentLevel++;
        if (currentLevel < maps.Length)
        {
            HudManager.Instance.UpdateLevel();
            // Cargar siguiente nivel
            yield return StartCoroutine(StartLevel());
        }
        else
        {
            // Terminar juego
            currentLevel = 0;
            HudManager.Instance.UpdateLevel();
            yield return StartCoroutine(CompleteGame());
        }
    }

    // Función para saltar a un nivel específico
    private IEnumerator ChangeLevel(int levelIndex)
    {
        if(transitioning) yield break;
        playerReference.SetActive(false);

        yield return StartCoroutine(MapFallAnimation());
        UnloadLevel();

        yield return new WaitForSeconds(levelAnimationDelay);

        currentLevel = levelIndex;
        HudManager.Instance.UpdateLevel();
        yield return StartCoroutine(StartLevel());
    }

    // Carga un nivel y anima la aparición de los tiles
    private void LoadLevel(int levelIndex)
    {
        if(transitioning) return;

        if (levelIndex < 0 || levelIndex >= maps.Length)
        {
            Debug.LogWarning($"LevelManager: Nivel {levelIndex} fuera de rango.");
            return;
        }

        TextAsset mapFile = maps[currentLevel];

        Vector3 origin = Vector3.zero;
        tiles = mapCreator.CreateMap(mapFile, origin);
        Vector3 posPlayer = mapCreator.PlayerStartWorldPos;

        player.SetInitPos(posPlayer);
    }

    private void UnloadLevel()
    {
        if(transitioning) return;

        // Destruir mapa
        mapCreator.DestroyMap();
        tiles.Clear();
    }

    // Función de marcador para el final del juego
    private IEnumerator CompleteGame()
    {
        if (transitioning) yield break;

        yield return StartCoroutine(menuManager.RunCredits());

        menuManager.GoToMainMenu();
    }

    // Wrapper que incrementa el contador, ejecuta la corutina y lo decrementa al terminar
    private IEnumerator RunTracked(IEnumerator routine)
    {
        animationsRunning++;
        yield return StartCoroutine(routine);
        animationsRunning--;
    }

    private IEnumerator MapRiseAnimation()
    {
        transitioning = true;

        // Clona la lista para no modificar tiles
        List<GameObject> order = new List<GameObject>(tiles);

        // Mezcla el orden
        Shuffle(order);

        foreach (var tile in order)
        {
            TileAnimator anim = tile.GetComponent<TileAnimator>();
            if (anim != null)
            {
                // Lanzamos la animación sin esperar a que termine
                StartCoroutine(RunTracked(anim.AnimateAppear()));
            }

            // Pequeño retardo entre tiles
            yield return new WaitForSeconds(tileAnimationDelay);
        }

        // Esperar a que todas las animaciones de caída finalicen
        yield return new WaitUntil(() => animationsRunning == 0);
        
        transitioning = false;
    }

    private IEnumerator MapFallAnimation()
    {
        transitioning = true;

        // Clona la lista para no modificar tiles
        List<GameObject> order = new List<GameObject>(tiles);

        // Mezcla el orden
        Shuffle(order);

        foreach (var tile in order)
        {
            TileAnimator anim = tile.GetComponent<TileAnimator>();
            if (anim != null)
            {
                // Lanzamos la animación sin esperar a que termine
                StartCoroutine(RunTracked(anim.AnimateDisappearFall()));
            }
            
            // Pequeño retardo entre tiles
            yield return new WaitForSeconds(tileAnimationDelay);
        }

        // Esperar a que todas las animaciones de caída finalicen
        yield return new WaitUntil(() => animationsRunning == 0);
        
        transitioning = false;
    }

    private IEnumerator MapSpiralAnimation()
    {
        transitioning = true;

        // Clona la lista para no modificar tiles
        List<GameObject> order = new List<GameObject>(tiles);

        // Mezcla el orden
        Shuffle(order);
        
        foreach (var tile in order)
        {
            TileAnimator anim = tile.GetComponent<TileAnimator>();
            if (anim != null)
            {
                // Lanzamos la animación sin esperar a que termine
                StartCoroutine(RunTracked(anim.AnimateDisappearVictory()));
            }

            // Pequeño retardo entre tiles
            yield return new WaitForSeconds(tileAnimationDelay);
        }

        // Esperar a que todas las animaciones de caída finalicen
        yield return new WaitUntil(() => animationsRunning == 0);
        
        transitioning = false;
    }

    private void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    private IEnumerator PlayerSlideAnimation()
    {
        transitioning = true;
        yield return StartCoroutine(player.AnimateSlide());
        transitioning = false;
    }

}
