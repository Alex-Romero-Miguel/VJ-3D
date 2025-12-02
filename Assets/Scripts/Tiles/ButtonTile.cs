using UnityEngine;
using System.Collections.Generic;

public class ButtonTile : TileBase
{
    // Marca esto como TRUE solo en el prefab del botón Cruz (Strict)
    public bool isStrictButton;

    public int channelID; // Asignado por MapCreator
    private bool isPressed = false;
    private List<BridgeTile> connectedBridges = new List<BridgeTile>();

    // Esta función la llama MapCreator al crear el mapa
    public void Configure(int id)
    {
        this.channelID = id;
    }

    private void Start()
    {
        BridgeTile[] allBridges = FindObjectsByType<BridgeTile>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (BridgeTile bridge in allBridges)
        {
            if (bridge.channelID == this.channelID)
            {
                connectedBridges.Add(bridge);

            }
        }
        //Debug.Log($"Botón ID {channelID} encontró {connectedBridges.Count} puentes.");
    }

    protected override void Activate(Collider other)
    {
        MoveCube player = other.GetComponent<MoveCube>();
        if (player == null) return;

        // Botón CRUZ
        if (isStrictButton)
        {
            //Debug.Log("cruz");

            if (player.isStanding())
                PressButton();
        }
        else // Boton Rodondo
        {
            PressButton();
        }
    }

    private void PressButton()
    {
        isPressed = true;

        // Animación visual del botón bajando
        TileAnimator animator = GetComponent<TileAnimator>();

        // Activar todos los puentes conectados
        foreach (BridgeTile bridge in connectedBridges)
        {
            if (bridge != null)
            {
                bridge.ToggleState();
                //Debug.Log("activate");
            }
        }
    }

    // Dibuja una línea en el editor para ver a qué puentes está conectado
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        foreach (var bridge in connectedBridges)
        {
            if (bridge != null)
                Gizmos.DrawLine(transform.position, bridge.transform.position);
        }
    }
}