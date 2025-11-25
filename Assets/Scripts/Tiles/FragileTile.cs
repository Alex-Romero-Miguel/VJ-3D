using UnityEngine;

public class FragileTile : TileBase
{
    protected override void Activate(Collider other)
    {
        MoveCube cube = other.GetComponent<MoveCube>();

        if (cube == null)
            return;

        if (cube != null && cube.isStanding())
        {

        }
    }
}
