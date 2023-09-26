using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CursorController : MonoBehaviour
{
    RectTransform rectTransform;

    public enum CursorMovementTypes { Snapping, FreeMovement }
    [SerializeField] CursorMovementTypes _cursorMovementType = CursorMovementTypes.Snapping;

    Vector2 _currentInput = Vector2.zero;

    private Star _currentStar;

    [SerializeField] float _boxCastWidth = 10;
    [SerializeField] LayerMask _starsLayerMask;

    private void Start()
    {
        rectTransform=GetComponent<RectTransform>();
        InputManager.Instance.OnMove += OnMove;

        SnapToStar(GetClosestStar());
    }

    private void OnMove(Vector2 value)
    {
        var previousInput = _currentInput;
        _currentInput = value;

        //Snap to the next star once the player has released the stick
        if (_currentInput==Vector2.zero && previousInput != Vector2.zero && _cursorMovementType==CursorMovementTypes.Snapping)
        {
            var nextStar=FindNextStar(_currentStar, _currentInput);
            SnapToStar(nextStar);
        }
    }

    private Star FindNextStar(Star startPoint, Vector2 direction)
    {
        float angleToward = Vector2.Angle(Vector2.up, direction);
        float height = 0.1f;
        var result=Physics2D.BoxCast((Vector2)startPoint.transform.position + (0.5f * height * direction), new Vector2(_boxCastWidth, height), angleToward, direction, Mathf.Infinity, _starsLayerMask);
        return result.transform.GetComponent<Star>();
    }

    public void SnapToStar(Star nextStar)
    {
        _currentStar=nextStar;

        Vector3 position=nextStar.transform.position;
        
        transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
    }

    public Star GetClosestStar()
    {
        var screenPosition = gameObject.transform.position;
        screenPosition.z = Camera.main.transform.position.z+Camera.main.nearClipPlane;   


        var worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        worldPosition += Camera.main.transform.position;

        float radiusStep = 0.2f;
        int maxTries = 10000;
        Star closestStar= null;
        for(int i = 0; i < maxTries; i++)
        {
            var hit = Physics2D.CircleCast(worldPosition, i * radiusStep, Vector2.zero);
            if (hit == null) continue;
            if(hit.collider==null) continue;
            if(hit.collider.transform.GetComponent<Star>()==null) continue;
            closestStar = hit.transform.GetComponent<Star>();
            break;
        }
        
        return closestStar;
    }

    
}
