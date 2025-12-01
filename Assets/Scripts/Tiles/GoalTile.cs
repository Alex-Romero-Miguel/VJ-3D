using UnityEngine;
using System.Collections;

public class GoalTile : TileBase
{

    protected override void Activate(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MoveCube cube = other.GetComponent<MoveCube>();

            if (cube != null && cube.isStanding())
            {
                //Debug.Log("VICTORIA");
                cube.startGoalFalling();
                //StartCoroutine(LevelManager.Instance.CompleteLevel());
            }
            //else
            //{
            //    Debug.Log("Estás en la meta, pero TUMBADO. ¡Ponte de pie!");
            //}
        }
    }
}