using UnityEngine;

public abstract class TileBase : MonoBehaviour
{
    protected abstract void Activate(Collider other);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Activate(other);
    }

    public abstract void Reset();
}