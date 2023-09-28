using DG.Tweening;
using DG.Tweening.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public enum BuildingState { ChoosingStartStar, ChoosingEndStar }


    Vector2 _currentSnapInput = Vector2.zero;
    Vector2 _currentMoveInput = Vector2.zero;

    public Star CurrentStar;
    private Star _previewStar;

    [Header("External objects")]
    [SerializeField] private Image _previewCursor;
    [SerializeField] private Image _cursor;
    [SerializeField] private Constellation _previewConstellation;

    [Header("Technical settings aobut the raycasting")]
    [SerializeField] private float _boxCastWidth = 10;
    [SerializeField] private LayerMask _starsLayerMask;

    [Header("Snapping animation")]
    [SerializeField] private float _snapAnimationDuration = 0.2f;
    [SerializeField] private AnimationCurve _snapAnimationCurve;

    [Header("Control settings")]
    [Tooltip("The speed at which the cursor moves when the player is holding the stick")]
    [SerializeField] private float _freeMovementCursorSpeed = 300;

    
    [Serializable]
    internal struct Borders
    {
        public Transform topLeft;
        public Transform bottomRight;
    }

    [Header("Borders")]
    [SerializeField] private Borders _borders;

    [Header("Callbacks")]
    public UnityEvent<int> PlacedASegment = new UnityEvent<int>();
    public UnityEvent PlacedAConstellation = new UnityEvent();
    public UnityEvent CancelledASegment = new UnityEvent();
    public UnityEvent CancelledAConstellation=new UnityEvent();
    public UnityEvent Snapped=new UnityEvent();


    private Star _startStar;
    private Star _endStar;
    public BuildingState _buildingState = BuildingState.ChoosingStartStar;

    private void Start()
    {
        InputManager.Instance.OnMove += OnMove;
        InputManager.Instance.OnSnap += OnSnap;
        InputManager.Instance.OnConfirm += OnConfirm;
        InputManager.Instance.OnSave += OnSave;
        InputManager.Instance.OnCancelStep += OnCancelStep;
        InputManager.Instance.OnCancelBuild += OnCancelBuild;

        SnapToStar(GetClosestStar(_cursor.transform));
        _previewCursor.enabled = false;
    }

    private void OnCancelStep()
    {
        switch (_buildingState)
        {
            case BuildingState.ChoosingStartStar:
                _buildingState = BuildingState.ChoosingEndStar;
                var startStarOfLastSegment=_previewConstellation.RemoveLastSegment();
                if (startStarOfLastSegment == null) break;
                _startStar = startStarOfLastSegment;
                RefreshPreviewSegment();
                break;
            case BuildingState.ChoosingEndStar:
                _buildingState = BuildingState.ChoosingStartStar;
                _startStar = null;
                _previewConstellation.HidePreviewSegment();
                break;
        }
        CancelledASegment?.Invoke();
    }

    private void OnCancelBuild()
    {
        _previewConstellation.ClearConstellation();
        _startStar = null;
        _endStar = null;
        CancelledAConstellation?.Invoke();
        _buildingState = BuildingState.ChoosingStartStar;
        OnCancelStep();
    }

    private void OnConfirm()
    {
        switch (_buildingState)
        {
            case BuildingState.ChoosingStartStar:
                if(_previewConstellation.HasTooManySegments(true)) break;
                //If we're currently building a constellation, the start star must be in the constellation
                if(_previewConstellation.Segments.Count>0 && !_previewConstellation.StarIsInConstellation(CurrentStar,true)) break;

                _buildingState = BuildingState.ChoosingEndStar;
                _startStar = CurrentStar;
                break;
            case BuildingState.ChoosingEndStar:
                if (_startStar == CurrentStar)
                {
                    break;
                }

                if (_startStar == null)
                {
                    _buildingState = BuildingState.ChoosingStartStar;
                    break;
                }

                _endStar = CurrentStar;

                if(!_previewConstellation.AddSegment(_startStar, _endStar)) return;

                _buildingState = BuildingState.ChoosingEndStar;
                _startStar = CurrentStar;

                if(_previewConstellation.HasTooManySegments())
                {
                    _previewConstellation.HidePreviewSegment();
                    _startStar = null;
                    _buildingState = BuildingState.ChoosingStartStar;
                }

                PlacedASegment?.Invoke(_previewConstellation.Segments.Count);
                break;
        }
        
    }

    private void OnSave()
    {
        _previewConstellation.SaveConstellation();
        _startStar = null;
        _endStar = null;
        _buildingState = BuildingState.ChoosingStartStar;
        PlacedAConstellation?.Invoke();
    }

    private const float INPUT_THRESHOLD = 0.8f;
    private void OnSnap(Vector2 value)
    {
        var previousInput = _currentSnapInput;
        _currentSnapInput = value;

        if (_currentSnapInput.sqrMagnitude < INPUT_THRESHOLD * INPUT_THRESHOLD)
        {
            _currentSnapInput = Vector2.zero;
        }

        if (_currentSnapInput != Vector2.zero)
        {
            DOTween.Kill(_cursor.transform);
        }

        if (_currentSnapInput != previousInput && _currentSnapInput != Vector2.zero)
        {
            var previewStar = FindNextStar(CurrentStar, _currentSnapInput);
            SnapPreviewToStar(previewStar);
        }

        //Snap to the next star once the player has released the stick
        if (_currentSnapInput == Vector2.zero && previousInput != Vector2.zero)
        {
            var nextStar = _previewStar;
            SnapToStar(nextStar);
        }
    }

    private void OnMove(Vector2 value)
    {
        _currentMoveInput = value;

        if (_currentMoveInput == Vector2.zero)
        {
            SnapToStar(GetClosestStar(_cursor.transform));
        }
        else
        {
            DOTween.Kill(_cursor.transform);
        }
    }

    private Star FindNextStar(Star startPoint, Vector2 direction)
    {
        float angleToward = Vector2.SignedAngle(Vector2.up, direction);
        float height = 0.1f;
        var results = Physics2D.BoxCastAll((Vector2)startPoint.transform.position + (height * direction), new Vector2(_boxCastWidth, height), angleToward, direction, Mathf.Infinity, _starsLayerMask);
        Vector2 perpendicularToDirection = new Vector2(direction.y, direction.x);

        RaycastHit2D? result = null;

        foreach (var hit in results)
        {
            if (hit.collider.transform.position == startPoint.transform.position) continue;
            if (Mathf.Abs(Vector3.Dot(hit.collider.transform.position - startPoint.transform.position, direction)) < Mathf.Abs(Vector3.Dot((hit.collider.transform.position - startPoint.transform.position), perpendicularToDirection))) continue;

            result = hit;
            break;
        }

        if (result == null) return null;

        return result.Value.transform.GetComponent<Star>();
    }

    public void SnapToStar(Star nextStar)
    {
        if (nextStar == null) return;
        CurrentStar = nextStar;
        _previewCursor.enabled = false;

        Vector3 position = nextStar.transform.position;

        Snapped?.Invoke();

        var targetPosition = RectTransformUtility.WorldToScreenPoint(Camera.main, position);
        _cursor.transform.DOMove(targetPosition, _snapAnimationDuration).SetEase(_snapAnimationCurve).OnUpdate(() =>
        {   
            RefreshPreviewSegment();
        });
    }

    public void RefreshPreviewSegment()
    {
        if (_startStar == null) return;
        _previewConstellation.PreviewSegment(_startStar.transform.position, GetCursorWorldPosition(_cursor.transform));
    }

    public void SnapPreviewToStar(Star nextStar)
    {
        _previewCursor.enabled = true;
        _previewCursor.transform.position = RectTransformUtility.WorldToScreenPoint(Camera.main, nextStar.transform.position);
        _previewStar = nextStar;
    }

    public Star GetClosestStar(Transform cursor)
    {


        float radiusStep = 0.2f;
        int maxTries = 10000;
        Star closestStar = null;
        for (int i = 0; i < maxTries; i++)
        {
            var hit = Physics2D.CircleCast(GetCursorWorldPosition(cursor), i * radiusStep, Vector2.zero);
            if (hit == null) continue;
            if (hit.collider == null) continue;
            if (hit.collider.transform.GetComponent<Star>() == null) continue;
            closestStar = hit.transform.GetComponent<Star>();
            break;
        }

        return closestStar;
    }

    public Vector3 GetCursorWorldPosition(Transform cursor)
    {
        var screenPosition = cursor.gameObject.transform.position;
        screenPosition.z = -Camera.main.transform.position.z; //Maybe Camera.main.nearClipPlane; has something to do with it ? I don't understand this cursed function

        var worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        return worldPosition;
    }

    public void Update()
    {
        if (_currentMoveInput == Vector2.zero)
        {
            return;
        }
        Vector3 newPosition= _cursor.transform.position + (Vector3)(_currentMoveInput * Time.deltaTime * _freeMovementCursorSpeed);

        newPosition.x = Mathf.Clamp(newPosition.x, _borders.topLeft.position.x, _borders.bottomRight.position.x);
        newPosition.y = Mathf.Clamp(newPosition.y, _borders.bottomRight.position.y, _borders.topLeft.position.y);

        _cursor.transform.position = newPosition;

        SnapPreviewToStar(GetClosestStar(_cursor.transform));
        if (_startStar == null) return;
        RefreshPreviewSegment();
    }
}
