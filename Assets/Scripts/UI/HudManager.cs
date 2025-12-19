using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudManager : MonoBehaviour
{
    public static HudManager Instance;

    public TextMeshProUGUI movementText;
    public TextMeshProUGUI levelText;

    private int movements = 0;
    private int lastLevel = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddMovement()
    {
        movements++;
        UpdateUI();
    }

    public void ResetCounter()
    {
        movements = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        movementText.text = movements.ToString("D6");
    }

    public void UpdateLevel()
    {
        lastLevel = LevelManager.Instance.GetCurrentLevel();
        levelText.text = $"Stage {lastLevel}";
    }


}