using UnityEngine;
using System.Collections;
using TMPro;

public class CreditScroller : MonoBehaviour
{
    public RectTransform textRect;
    public float speed;

    private RectTransform panelRect;
    private Vector2 startPos;
    private float endY;

    void Awake()
    {
        panelRect = GetComponent<RectTransform>();
    }

    void Start()
    {
        float panelH = panelRect.rect.height;
        float textH = textRect.rect.height;

        startPos = new Vector2(
            textRect.anchoredPosition.x,
            -panelH
        );

        endY = textH + panelH;
    }

    public IEnumerator RunCredits()
    {
        textRect.anchoredPosition = startPos;

        while (textRect.anchoredPosition.y < endY)
        {
            var p = textRect.anchoredPosition;
            p.y += speed * Time.deltaTime;
            textRect.anchoredPosition = p;
            yield return null;
        }
    }
}
