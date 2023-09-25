using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] CursorController cursorController;

    public void OnMove(InputValue value)
    {
        cursorController.SetNewMovementValue(value.Get<Vector2>());
    }
}
