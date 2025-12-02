using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using System.Collections;

public class BridgeTile : MonoBehaviour
{
    public Transform hinge;
    public int channelID;   // Asignado por MapCreator

    private float rotationTime = 0.4f;

    private bool isActive = false;

    private Quaternion closedRot; // rotación sin abrir
    private Quaternion openRot; // rotación abierta

    private void Awake()
    {
        openRot = hinge.localRotation;
        closedRot = openRot * Quaternion.Euler(0, 0, -90);

        InitializeState(false);
    }

    // Llamado por MapCreator
    public void Configure(int id)
    {
        this.channelID = id;
    }

    // Configura el estado inicial sin animación (para el Start del nivel)
    public void InitializeState(bool startActive)
    {
        isActive = startActive;
        hinge.localRotation = isActive ? openRot : closedRot;
    }

    // Cambia el estado (de abierto a cerrado o viceversa)
    public void ToggleState()
    {
        isActive = !isActive;
        StopAllCoroutines();
        StartCoroutine(RotateBridge(isActive ? openRot : closedRot));
    }

    private IEnumerator RotateBridge(Quaternion targetRot)
    {
        float time = 0f;
        Quaternion startRot = hinge.localRotation;

        while (time < rotationTime)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, time / rotationTime);
            hinge.localRotation = Quaternion.Lerp(startRot, targetRot, t);
            yield return null;
        }

        hinge.localRotation = targetRot;
    }
}