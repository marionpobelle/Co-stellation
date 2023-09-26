using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cursor : MonoBehaviour
{
    RectTransform rectTransform;

    public enum CursorMovementTypes { Snapping, FreeMovement }
    [SerializeField] CursorMovementTypes _cursorMovementType = CursorMovementTypes.Snapping;

    Vector2 _currentInput = Vector2.zero;

    private Star _currentStar;

    [SerializeField] private float _boxCastWidth = 10;
    [SerializeField] private LayerMask _starsLayerMask;
    [SerializeField] private Image _previewCursor;
    [SerializeField] private Transform debugThing;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        InputManager.Instance.OnMove += OnMove;

        SnapToStar(GetClosestStar());
        _previewCursor.enabled = false;
    }

    private void OnMove(Vector2 value)
    {
        var previousInput = _currentInput;
        _currentInput = value;

        if(_currentInput!= previousInput && _currentInput!=Vector2.zero && _cursorMovementType == CursorMovementTypes.Snapping) 
        {
            var previewStar = FindNextStar(_currentStar, _currentInput);
            SnapPreviewToStar(previewStar);
        }

        //Snap to the next star once the player has released the stick
        if (_currentInput == Vector2.zero && previousInput != Vector2.zero && _cursorMovementType == CursorMovementTypes.Snapping)
        {
            var nextStar = FindNextStar(_currentStar, previousInput);
            SnapToStar(nextStar);
        }
    }

    private Star FindNextStar(Star startPoint, Vector2 direction)
    {
        float angleToward = Vector2.SignedAngle(Vector2.up, direction);
        //float height = 1f;
        //var result = Physics2D.BoxCast((Vector2)startPoint.transform.position + ( height * direction), new Vector2(_boxCastWidth, height),angleToward, direction, Mathf.Infinity, _starsLayerMask);

        var result = Physics2D.CircleCast((Vector2)startPoint.transform.position + (_boxCastWidth * direction*1.5f), _boxCastWidth, direction, Mathf.Infinity, _starsLayerMask);

        debugThing.transform.eulerAngles=new Vector3(0, 0, angleToward);
        //Debug.Log(result.transform.position);
        return result.transform.GetComponent<Star>();
    }

    public void SnapToStar(Star nextStar)
    {
        _currentStar = nextStar;
        _previewCursor.enabled = false;

        Vector3 position = nextStar.transform.position;

        transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
    }

    public void SnapPreviewToStar(Star nextStar)
    {
        _previewCursor.enabled = true;
        _previewCursor.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, nextStar.transform.position);
    }

    public Star GetClosestStar()
    {
        var screenPosition = gameObject.transform.position;
        screenPosition.z = Camera.main.transform.position.z + Camera.main.nearClipPlane;


        var worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        worldPosition += Camera.main.transform.position;

        float radiusStep = 0.2f;
        int maxTries = 10000;
        Star closestStar = null;
        for (int i = 0; i < maxTries; i++)
        {
            var hit = Physics2D.CircleCast(worldPosition, i * radiusStep, Vector2.zero);
            if (hit == null) continue;
            if (hit.collider == null) continue;
            if (hit.collider.transform.GetComponent<Star>() == null) continue;
            closestStar = hit.transform.GetComponent<Star>();
            break;
        }

        return closestStar;
    }


}
