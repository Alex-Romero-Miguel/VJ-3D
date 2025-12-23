using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;

public class BridgeTile : MonoBehaviour, ITileConfigurable
{
    public int channelID;   // Asignado por MapCreator
    public float rotationTime;
    public Direction hingeDirection;

    public enum Direction { Left, Right, Up, Down }
    public Transform hinge;


    public bool startsActive = false;
    private bool isActive;

    private Quaternion closedRot; // rotación sin abrir
    private Quaternion openRot; // rotación abierta

    private void Awake()
    {
        //openRot = hinge.localRotation;
        //closedRot = openRot * Quaternion.Euler(0, 0, -90);

        //InitializeState(false);
        isActive = startsActive;
    }

    // Llamado por MapCreator
    public void Configure(int id, string extra)
    {
        this.channelID = id;

        //hingeDirection = dir switch
        //{
        //    "L" => Direction.Left,
        //    "R" => Direction.Right,
        //    "U" => Direction.Up,
        //    "D" => Direction.Down,
        //    _ => Direction.Left  // default si no viene extra
        //};

        hingeDirection = Direction.Left;
        bool startActive = false;

        if (!string.IsNullOrEmpty(extra))
        {
            var parts = extra.Split(':');

            // parts[0] = dirección (U/D/L/R)
            if (TryParseDirection(parts[0], out var dirParsed))
                hingeDirection = dirParsed;

            // parts[1] opcional = "1" o "0"
            if (parts.Length > 1 && TryParseBool01(parts[1], out bool b))
                startActive = b;
        }

        ApplyRotation();
        CacheRotations();
        InitializeState(startActive);
    }

    private bool TryParseDirection(string s, out Direction d)
    {
        d = Direction.Left;
        if (string.IsNullOrEmpty(s)) return false;

        switch (s.Trim().ToUpperInvariant())
        {
            case "L": d = Direction.Left; return true;
            case "R": d = Direction.Right; return true;
            case "U": d = Direction.Up; return true;
            case "D": d = Direction.Down; return true;
            default: return false;
        }
    }

    private bool TryParseBool01(string s, out bool b)
    {
        b = false;
        if (string.IsNullOrEmpty(s)) return false;
        s = s.Trim();
        if (s == "1") { b = true; return true; }
        if (s == "0") { b = false; return true; }
        return false;
    }

    private void CacheRotations()
    {
        openRot = hinge.localRotation;
        closedRot = openRot * Quaternion.Euler(0, 0, 180);
    }

    private void ApplyRotation()
    {
        switch (hingeDirection)
        {
            case Direction.Left:
                transform.rotation = Quaternion.Euler(0, 0, 0);
                break;

            case Direction.Right:
                transform.rotation = Quaternion.Euler(0, 180, 0);
                break;

            case Direction.Up:
                transform.rotation = Quaternion.Euler(0, 90, 0);
                break;

            case Direction.Down:
                transform.rotation = Quaternion.Euler(0, -90, 0);
                break;
        }
    }


    // Configura el estado inicial sin animación (para el Start del nivel)
    public void InitializeState(bool startActive)
    {
        startsActive = startActive;
        Reset();
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

    public void Reset()
    {
        isActive = startsActive;
        hinge.localRotation = isActive ? openRot : closedRot;
    }

}