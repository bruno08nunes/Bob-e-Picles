using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public InputManager Instance { get; private set; }
    static InputAction move, attack;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        move = InputSystem.actions.FindAction("Move");
        attack = InputSystem.actions.FindAction("Attack");
    }

    public static Vector2 GetMove()
    {
        return move.ReadValue<Vector2>();
    }

    public static bool WasAttackPressed()
    {
        return attack.WasPressedThisFrame();
    }
}
