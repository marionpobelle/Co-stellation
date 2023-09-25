using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    [SerializeField] RectTransform cursor;
    [SerializeField] float cursorSpeed = 1f;

    Vector2 currentInput = Vector2.zero;

    void Update()
    {
        //TODO Smoothing cursor speed => animation curve ? DoTween ?
        cursor.position += new Vector3(currentInput.x, currentInput.y, 0) * cursorSpeed * Time.deltaTime;

        //TODO cursor snapping

        //TODO cursor selecting star
    }

    public void SetNewMovementValue(Vector2 newInput)
    {
        currentInput = newInput.normalized;
    }
}
