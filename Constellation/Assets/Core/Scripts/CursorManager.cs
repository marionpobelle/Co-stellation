using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public enum CursorMovementTypes { Snapping, FreeMovement }
    public enum BuildingState { ChoosingStartStar, ChoosingEndStar }
    

    Vector2 _currentInput = Vector2.zero;

    public Star _currentStar;

    [Header("External objects")]
    [SerializeField] private Image _previewCursor;
    [SerializeField] private Image _cursor;
    [SerializeField] private Constellation _previewConstellation;

    [Header("Technical settings aobut the raycasting")]
    [SerializeField] private float _boxCastWidth = 10;
    [SerializeField] private LayerMask _starsLayerMask;

    [Header("Control settings")]
    [Tooltip("The speed at which the cursor moves when the player is holding the stick")]
    [SerializeField] private float _freeMovementCursorSpeed = 300;
    [Tooltip("The duration the player has to hold the stick in a direction to switch the cursor  movement style to free movement")]
    [SerializeField] private float _holdToMoveToFreeMovementDuration = 0.5f;
    [SerializeField] CursorMovementTypes _cursorMovementType = CursorMovementTypes.Snapping;
 
    [Header("Debug objects")]
    [SerializeField] private Transform debugThing;

    private Star _startStar;
    private Star _endStar;
    private BuildingState _buildingState = BuildingState.ChoosingStartStar;

    private void Start()
    {
        InputManager.Instance.OnMove += OnMove;
        InputManager.Instance.OnHoldMove += OnHoldMove;
        InputManager.Instance.OnConfirm += OnConfirm;

        InputManager.Instance.HoldMoveDuration = _holdToMoveToFreeMovementDuration;

        SnapToStar(GetClosestStar(_cursor.transform));
        _previewCursor.enabled = false;
    }

    private void OnConfirm()
    {
        switch(_buildingState)
        {
            case BuildingState.ChoosingStartStar:      
                _buildingState = BuildingState.ChoosingEndStar;
                _startStar = _currentStar;
                break;
            case BuildingState.ChoosingEndStar:
                if (_startStar == _currentStar)
                {
                    Debug.Log("Saving");
                    _previewConstellation.SaveConstellation();
                    _buildingState = BuildingState.ChoosingStartStar;
                    break;
                }
                _buildingState = BuildingState.ChoosingEndStar;                
                _endStar = _currentStar;
                _previewConstellation.AddSegment(_startStar, _endStar);
                _startStar = _currentStar;
                break;
        }
    }

    private void OnHoldMove()
    {
        _cursorMovementType = CursorMovementTypes.FreeMovement;
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

        if(_currentInput == Vector2.zero && _cursorMovementType == CursorMovementTypes.FreeMovement)
        {
            _cursorMovementType = CursorMovementTypes.Snapping;
            SnapToStar(GetClosestStar(_cursor.transform));
        }
    }

    private Star FindNextStar(Star startPoint, Vector2 direction)
    {
        float angleToward = Vector2.SignedAngle(Vector2.up, direction);
        float height = 0.1f;
        var results = Physics2D.BoxCastAll((Vector2)startPoint.transform.position + ( height * direction), new Vector2(_boxCastWidth, height),angleToward, direction, Mathf.Infinity, _starsLayerMask);

        RaycastHit2D? result = null;

        foreach (var hit in results)
        {
            if (hit.collider.transform.position == startPoint.transform.position) continue;
            result = hit;
            break;
        }
       
        if(result==null) return null;

        //debugThing.transform.eulerAngles=new Vector3(0, 0, angleToward);

        return result.Value.transform.GetComponent<Star>();
    }

    public void SnapToStar(Star nextStar)
    {
        _currentStar = nextStar;
        _previewCursor.enabled = false;

        Vector3 position = nextStar.transform.position;

        _cursor.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, position);

        if (_buildingState != BuildingState.ChoosingEndStar) return;
        if (_currentStar == _startStar) return;
        _previewConstellation.PreviewSegment = new Segment(_startStar, _currentStar);
    }

    public void SnapPreviewToStar(Star nextStar)
    {
        _previewCursor.enabled = true;
        _previewCursor.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, nextStar.transform.position);
    }

    public Star GetClosestStar(Transform cursor)
    {
        var screenPosition = cursor.gameObject.transform.position;
        screenPosition.z = -Camera.main.transform.position.z; //Maybe Camera.main.nearClipPlane; has something to do with it ? I don't understand this cursed function

        var worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);

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

    public void Update()
    {
        if (_cursorMovementType != CursorMovementTypes.FreeMovement)
        {
            return;
        }
        _cursor.transform.position += (Vector3)(_currentInput * Time.deltaTime * _freeMovementCursorSpeed);
        SnapPreviewToStar(GetClosestStar(_cursor.transform));
    }
}
