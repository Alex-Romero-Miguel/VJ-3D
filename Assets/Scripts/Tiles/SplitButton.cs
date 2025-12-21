using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class SplitButton : TileBase
{
    private bool isPressed = false;

    private Vector3 posA;
    private Vector3 posB;

    protected override void Activate(Collider other)
    {
        if (isPressed) return;

        MoveCube player = other.GetComponent<MoveCube>();
        if (player == null) return;
        if (player.isStanding())
        {
            isPressed = true;

            player.Divide(posA, posB);

            Debug.DrawRay(posA, Vector3.up * 10f, Color.red, 20f);
            Debug.DrawRay(posB, Vector3.up * 10f, Color.blue, 20f);
        }
    }

    public void SetSplitPositions(Vector3 posA, Vector3 posB)
    {
        this.posA = posA;
        this.posB = posB;
    }

    private void OnDrawGizmosSelected()
    {

        if (posA != Vector3.zero)
        {
            Gizmos.color = Color.green; 
            Gizmos.DrawLine(transform.position, posA);

            Gizmos.DrawSphere(posA, 0.3f);
        }

        if (posB != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, posB);
            Gizmos.DrawSphere(posB, 0.3f);
        }
    }
}