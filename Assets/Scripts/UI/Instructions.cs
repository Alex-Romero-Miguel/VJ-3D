using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InstructionsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text pageCounterText;
    [SerializeField] private TMP_Text bodyText;

    private int currentPage;

    private string[] pages =
    {
        "Controls:\n" +
        "Use the Arrow Keys or WASD to roll the block.",

        "Goal:\n" +
        "Get the block to fall into the square hole at the end of each stage."
    };

    private void OnEnable()
    {
        currentPage = 0;
        Refresh();
    }

    public void Next()
    {
        currentPage = Mathf.Min(currentPage + 1, pages.Length - 1);
        Refresh();
    }

    public void Prev()
    {
        currentPage = Mathf.Max(currentPage - 1, 0);
        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    private void Refresh()
    {
        pageCounterText.text = $"{currentPage + 1}/{pages.Length}";
        bodyText.text = pages[currentPage];
    }
}
