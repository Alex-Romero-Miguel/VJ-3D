using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Processors;

//
// Processor que convierte una tecla en un número fijo.
// Ejemplo: en un binding asignas "KeyToNumber(number=5)" y al pulsarlo
// ReadValue<int>() devolverá 5.
//
#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
#endif
public class KeyToNumberProcessor : InputProcessor<float>
{
    #if UNITY_EDITOR
    static KeyToNumberProcessor()
    {
        Initialize();
    }
    #endif

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<KeyToNumberProcessor>();
    }

    public float number;

    public override float Process(float value, InputControl control)
    {
        // Cuando la tecla está presionada, value normalmente es 1.
        // Ignoramos ese valor y devolvemos el número configurado.
        if (control != null && control.IsPressed()) return number;
        return -1f;
    }
}