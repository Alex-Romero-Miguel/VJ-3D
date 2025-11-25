using UnityEngine;

public abstract class TileBase : MonoBehaviour
{
    protected abstract void Activate(Collider other);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Activate(other);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}