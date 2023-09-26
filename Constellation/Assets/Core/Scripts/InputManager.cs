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
    public Action OnHoldMove;
    public Action OnConfirm;
    public Action OnCancel;

    [HideInInspector] public float HoldMoveDuration = 0.5f; //If movement != 0 for this duration or longer, then it's considered a hold. Set by the cursor's script.

    private Coroutine CheckingHoldCoroutine=null;

    public void OnInMove(InputValue value)
    {
        Vector2 vector2 = value.Get<Vector2>();
        OnMove?.Invoke(vector2);

        _movementIsZero = (vector2 == Vector2.zero);
        if (!_holdingIsBeingChecked)
        {
            if (CheckingHoldCoroutine != null)
            {
                StopCoroutine(CheckingHoldCoroutine);
                CheckingHoldCoroutine = null;
            }
            CheckingHoldCoroutine=StartCoroutine(CheckForMoveHolding(HoldMoveDuration));
        }
    }

    public void OnInHoldMove()
    {
        OnHoldMove?.Invoke();
    }

    public void OnInConfirm()
    {
        OnConfirm?.Invoke();
    }

    public void OnInCancel()
    {
        OnCancel?.Invoke();
    }

    private bool _movementIsZero;
    private bool _holdingIsBeingChecked;
    private IEnumerator CheckForMoveHolding(float duration)
    {
        _holdingIsBeingChecked = true;
        yield return new WaitForSecondsRealtime(duration);
        if (!_movementIsZero)
        {
            OnInHoldMove();
        }
        _movementIsZero = true;
        _holdingIsBeingChecked = false;
    }
}
