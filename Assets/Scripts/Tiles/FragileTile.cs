using UnityEngine;
using System.Collections;

public class FragileTile : TileBase
{
    private bool broken = false;
    protected override void Activate(Collider other)
    {
        if (broken) return;

        MoveCube cube = other.GetComponentInParent<MoveCube>();

        if (cube != null && cube.isStanding() && !cube.isDivided())
        {
            broken = true;

            StartCoroutine(BreakSequence());

            //Debug.Log("break");

            //StartCoroutine(LevelManager.Instance.RestartLevel());
        }
    }

    public override void Reset()
    {
        broken = false;
        //gameObject.SetActive(true);
    }

    private IEnumerator BreakSequence()
    {
        broken = true;

        TileAnimator animator = GetComponent<TileAnimator>();
        if (animator != null)
        {
            yield return StartCoroutine(animator.AnimateBreak(0.5f));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
