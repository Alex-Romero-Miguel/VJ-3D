using UnityEngine;
using System.Collections.Generic;

public class BridgeButton : TileBase, ITileConfigurable
{
    // Marca esto como TRUE solo en el prefab del bot�n Cruz (Strict)
    public bool isStrictButton;

    public int channelID; // Asignado por MapCreator
    private List<BridgeTile> connectedBridges = new List<BridgeTile>();

    // Esta funci�n la llama MapCreator al crear el mapa
    public void Configure(int id, string extra)
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
        //Debug.Log($"Bot�n ID {channelID} encontr� {connectedBridges.Count} puentes.");
    }

    protected override void Activate(Collider other)
    {
        MoveCube player = other.GetComponent<MoveCube>();
        if (player == null) return;

        // Bot�n CRUZ
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
        // Animaci�n visual del bot�n bajando
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

    // Dibuja una l�nea en el editor para ver a qu� puentes est� conectado
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