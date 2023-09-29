using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    #region Singleton pattern
    public static InputManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        } else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public Action<Vector2> OnMove;
    public Action<Vector2> OnSnap;    
    public Action OnConfirm;
    public Action OnSave;
    public Action OnCancelStep;
    public Action OnCancelBuild;

    bool isAxisInverted = false;

    public void OnInMove(InputValue value)
    {
        Vector2 vector2 = value.Get<Vector2>();
        OnMove?.Invoke(new Vector2(vector2.x * (isAxisInverted ? -1 : 1), vector2.y));
    }

    public void OnInSnap(InputValue value)
    {
        Vector2 vector2 = value.Get<Vector2>();
        OnSnap?.Invoke(new Vector2(vector2.x * (isAxisInverted ? -1 : 1), vector2.y));
    }

    public void OnInConfirm()
    {
        OnConfirm?.Invoke();
    }

    public void OnInSave()
    {
        OnSave?.Invoke();
    }

    public void OnInCancelStep()
    {
        OnCancelStep?.Invoke();
    }

    public void OnInCancelBuild()
    {
        OnCancelBuild?.Invoke();
    }

    public void OnToggleInvertXAxis()
    {
        isAxisInverted = !isAxisInverted;
    }
}
