using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public float JumpInput { get; private set; }
    public float ShootInput { get; private set; }

    public bool JumpQueued { get; set; }

    public bool ShootQueued { get; set; }

    public void OnMoveInputTest(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    public void OnLookInputTest(InputAction.CallbackContext context)
    {
        LookInput = context.ReadValue<Vector2>();
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if(context.started)
            JumpQueued = true;
    }

    public void OnShootInput(InputAction.CallbackContext context)
    {
        if (context.started)
            ShootQueued = true;
    }
}
