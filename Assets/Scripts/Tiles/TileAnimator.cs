using UnityEngine;
using System.Collections;

public class TileAnimator : MonoBehaviour
{
    private Vector3 initialPos;
    private Vector3 hiddenPos;

    void Awake()
    {
        initialPos = transform.position;
        hiddenPos = initialPos + Vector3.down * 3f;
        gameObject.SetActive(false);
    }

    public IEnumerator AnimateAppear(float duration = 0.4f)
    {
        gameObject.SetActive(true);
        float t = 0;
        transform.position = hiddenPos;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(hiddenPos, initialPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
    }

    public IEnumerator AnimateDisappearFall(float duration = 0.4f)
    {
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(initialPos, hiddenPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public IEnumerator AnimateDisappearVictory(float duration = 0.8f)
    {
        float t = 0;
        Vector3 start = transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float angle = t * 6f * Mathf.PI;
            float height = Mathf.Lerp(0, 5f, t);
            float radius = Mathf.Lerp(0, 10f, Mathf.Pow(t, 1.5f));

            transform.position = new Vector3(
                start.x + Mathf.Cos(angle) * radius,
                start.y + height,
                start.z + Mathf.Sin(angle) * radius
            );

            yield return null;
        }
        
        gameObject.SetActive(false);
    }

    // Para tile Fragil
    public IEnumerator AnimateBreak(float fallDuration = 0.2f)
    {
        float t = 0;
        Vector3 start = transform.position;
        Vector3 targetPos = start + Vector3.down * 5f; 

        while (t < 1f)
        {
            t += Time.deltaTime / fallDuration;
            transform.position = Vector3.Lerp(start, targetPos, t * t);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
