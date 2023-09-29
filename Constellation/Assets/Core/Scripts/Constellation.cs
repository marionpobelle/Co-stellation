using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Segment
{
    public Star _start;
    public Star _end;

    public Segment(Star start, Star end)
    {
        _start = start;
        _end = end;
    }

    public override bool Equals(object obj)
    {
        return obj is Segment segment &&
            (
                (
                    EqualityComparer<Star>.Default.Equals(_start, segment._start) &&
                    EqualityComparer<Star>.Default.Equals(_end, segment._end)
                )
                ||
                (
                    EqualityComparer<Star>.Default.Equals(_start, segment._end) &&
                    EqualityComparer<Star>.Default.Equals(_end, segment._start)
                )
            );
    }
}

public class Constellation : MonoBehaviour
{
    public List<Segment> Segments = new List<Segment>();

    private Segment _previewSegment;

    public List<LineRenderer> LineRenderers = new List<LineRenderer>();
    private LineRenderer _previewLineRenderer;

    public GameObject StarsParent;
    public float SegmentLineWidth = 0.6f;
    public float PreviewLineWidth = 0.3f;
    public Color SegmentColor = Color.blue;
    public Color PreviewSegmentColor = Color.gray;

    public Material PreviewSegmentMaterial;

    public Material CurrentSegmentMaterial;

    [Tooltip("Distance between the star and the start of the segment, except if the stars are too close")]
    public float DistanceBetweenSegmentAndStar = 0.5f;

    [Header("Limitations")]
    public int MaxSegments = 16;
    public float MaxDistance = 50;

    private bool _previewSegmentInErrorMode = false;

    [Header("Error segments attributes")]
    [SerializeField] private Color _errorSegmentColor = Color.red;
    [Tooltip("When you try to confirm  a wrong segment it will flash from normal color to this one, then back to normal color, following this (supposedly) bell curve")]
    [SerializeField] private AnimationCurve _errorAnimationCurve;
    [SerializeField] private float _errorAnimationDuration = 0.5f;

    [Header("Saved constellations attributes")]
    public Color SavedSegmentColor = Color.white;
    public float SavedSegmentLineWidth = 0.5f;
    public Material SavedSegmentMaterial;
    public float minDefaultAlpha = .1f;
    public float maxDefaultAlpha = .3f;
    public float minSelectedAlpha = .5f;
    public float maxSelectedAlpha = .8f;
    public float fadingSpeed = .5f;
    float random;
    bool isTweeningConstellation = false;

    [Header("Callbacks")]
    public UnityEvent ErrorOnSegment = new UnityEvent();


    private void Start()
    {
        _previewLineRenderer = CreateLineRenderer(new Segment(null, null), PreviewSegmentMaterial, PreviewSegmentColor, PreviewLineWidth);
        HidePreviewSegment();
        random = UnityEngine.Random.Range(0f, 100f);

        
    }

    private void Update()
    {
        if (StarIsInConstellation(CursorManager.Instance.CurrentStar) || isTweeningConstellation)
        {
            foreach (var item in LineRenderers)
            {
                item.material.SetFloat("_Alpha", Mathf.Lerp(minSelectedAlpha, maxSelectedAlpha, Mathf.InverseLerp(-1, 1, MathF.Cos(Time.time * fadingSpeed + random))));
            }
        }
        else
        {
            foreach (var item in LineRenderers)
            {
                item.material.SetFloat("_Alpha", Mathf.Lerp(minDefaultAlpha, maxDefaultAlpha, Mathf.InverseLerp(-1, 1, MathF.Cos(Time.time * fadingSpeed + random))));
            }
        }
    }

    public bool AddSegment(Star start, Star end)
    {

        return AddSegment(new Segment(start, end));
    }

    public bool AddSegment(Segment segment)
    {
        //We return true because this segment is valid, it's just that it already exists
        if (Segments.Find(x => x.Equals(segment)) != null) return true;

        if (segment._end == null || segment._start == null)
        {
            return false;
        }

        bool tooLong = (segment._end.transform.position - segment._start.transform.position).sqrMagnitude > MaxDistance * MaxDistance;
        if (tooLong)
        {
            ErrorOnSegment?.Invoke();
            return false;
        }

        HidePreviewSegment();
        Segments.Add(segment);
        AddLineRenderer(segment, CurrentSegmentMaterial, SegmentColor, SegmentLineWidth);
        TweenLineRenderer(LineRenderers.Last());

        return true;
    }

    public bool HasTooManySegments(bool playAnimIfTooMany = false)
    {
        if (Segments.Count < MaxSegments) return false;

        if (!playAnimIfTooMany) return true;

        PlayErrorFlashAnimation();

        return true;
    }

    public void PlayErrorFlashAnimation()
    {
        foreach (var lineRenderer in LineRenderers)
        {
            DOTween.Kill(lineRenderer.material);
            lineRenderer.material.color = SegmentColor;
            lineRenderer.material.DOColor(_errorSegmentColor, _errorAnimationDuration).SetEase(_errorAnimationCurve).OnComplete(
                () =>
                {
                    lineRenderer.material.color = SegmentColor;
                }
                );
        }
        ErrorOnSegment?.Invoke();
    }

    public bool StarIsInConstellation(Star starToFind, bool playAnimIfIsntIn = false)
    {
        bool isIn = Segments.Find(x => x._start == starToFind || x._end == starToFind) != null;
        if (!isIn && playAnimIfIsntIn)
            PlayErrorFlashAnimation();
        return isIn;
    }

    //Returns the start point of the last segment
    public Star RemoveLastSegment()
    {
        if (Segments.Count <= 0) return null;
        Star startPointOfLastSegment = Segments.Last()._start;
        Segments.RemoveAt(Segments.Count - 1);
        var lastLineRenderer = LineRenderers.Last();

        LineRenderers.Remove(lastLineRenderer);
        TweenLineRenderer(lastLineRenderer, true, true);

        return startPointOfLastSegment;
    }

    private void RefreshRender()
    {
        foreach (var lineRenderer in LineRenderers)
        {
            Destroy(lineRenderer.gameObject);
        }
        LineRenderers.Clear();
        foreach (var segment in Segments)
        {
            AddLineRenderer(segment, CurrentSegmentMaterial, SegmentColor, SegmentLineWidth);
        }
    }

    private void TweenLineRenderer(LineRenderer lineRenderer, bool tweeningOut = false, bool deleteAtEnd = false, float duration=0.25f, bool tweenNeighbours=false, bool mustReenableLineRenderer=false)
    {
        //The start position of the line renderer, basically the starting star 
        Vector3 startPosition = lineRenderer.GetPosition(0);
        //We calculate the final length of the segment
        float length = (lineRenderer.GetPosition(0) - lineRenderer.GetPosition(1)).magnitude;
        //The direction from line start to line end
        Vector3 direction = (lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0)).normalized;

        List<LineRenderer> neighborLineRenderers = new List<LineRenderer>();

        if(tweenNeighbours)
        {
            isTweeningConstellation = true;
            foreach(var otherLineRenderer in LineRenderers)
            {
                //Currently the tweenNEighbor thing is only used to display the constellation. Before that, every line renderer is disabled. If it aint, it means the line renderer has already been tweened in. Avoid glitches on circular constellation parts. (infinite loop)
                if (otherLineRenderer.enabled) continue;
                if(otherLineRenderer.GetPosition(0) == lineRenderer.GetPosition(1))
                {
                    neighborLineRenderers.Add(otherLineRenderer);
                }                  
            }  
        }

        if(mustReenableLineRenderer)
        {
            lineRenderer.SetPosition(1, startPosition);
            lineRenderer.enabled = true;
        }

        //Tween between 0 and full length, at each frame, the x will be somewhere between 0 and full length, and we'll get the point in between
        var anim=DOTween.To(
            x =>
            {
                lineRenderer.SetPosition(1, startPosition + direction * x);
            },
            (tweeningOut) ? length : 0,
            (tweeningOut) ? 0 : length,
            duration
            ).OnComplete(
            () =>
            {
                if (deleteAtEnd) 
                {
                    Destroy(lineRenderer);
                }
                if(tweenNeighbours)
                {
                    foreach(var nlr in neighborLineRenderers)
                    {
                        TweenLineRenderer(nlr, false, false, duration, true,true);
                    }
                    if (neighborLineRenderers.Count <= 0) 
                    {
                        isTweeningConstellation = false;
                    }
                }
            }
            );

        if (tweenNeighbours)
        {
            anim.SetEase(Ease.Linear);
        }
    }

    public void PreviewSegment(Vector3 start, Vector3 end)
    {
        _previewLineRenderer.enabled = true;
        _previewLineRenderer.SetPosition(0, start);
        _previewLineRenderer.SetPosition(1, end);

        bool tooLong = (end - start).sqrMagnitude > MaxDistance * MaxDistance;

        if (tooLong && !_previewSegmentInErrorMode)
        {
            _previewSegmentInErrorMode = true;
            DOTween.Kill(_previewLineRenderer.material);
            _previewLineRenderer.material.DOColor(_errorSegmentColor, 0.2f);
            return;
        }

        if (tooLong || !_previewSegmentInErrorMode) return;

        _previewSegmentInErrorMode = false;
        DOTween.Kill(_previewLineRenderer);
        _previewLineRenderer.material.DOColor(PreviewSegmentColor, 0.2f);
    }

    public void HidePreviewSegment()
    {
        _previewLineRenderer.enabled = false;
        DOTween.Kill(_previewLineRenderer);
        _previewSegmentInErrorMode = false;
        _previewLineRenderer.material.color = PreviewSegmentColor;
    }

    private void AddLineRenderer(Segment segment, Material material, Color color, float width)
    {
        var lineRenderer = CreateLineRenderer(segment, material, color, width);
        LineRenderers.Add(lineRenderer);
    }

    private LineRenderer CreateLineRenderer(Segment segment, Material material, Color color, float width)
    {
        var lineRenderer = new GameObject().AddComponent<LineRenderer>();
        lineRenderer.gameObject.name = "LineRenderer";
        lineRenderer.transform.SetParent(transform);
        lineRenderer.positionCount = 2;

        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        //lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material = material;
        lineRenderer.material.color = color;
        lineRenderer.textureMode = LineTextureMode.Tile;

        if (segment._start != null && segment._end != null)
        {
            var direction = segment._end.transform.position - segment._start.transform.position;
            var length = direction.magnitude;
            direction = direction.normalized;

            float offsetLength = (length > 3 * DistanceBetweenSegmentAndStar) ? DistanceBetweenSegmentAndStar : 0.33f * length;

            lineRenderer.SetPosition(0, segment._start.transform.position + offsetLength * direction);
            lineRenderer.SetPosition(1, segment._end.transform.position - offsetLength * direction);
        }

        return lineRenderer;
    }

    public bool SaveConstellation()
    {
        HidePreviewSegment();
        if (Segments.Count <= 0) return false;
        var consCopy = new GameObject().AddComponent<Constellation>();
        consCopy.gameObject.name = "Constellation";
        consCopy.Segments = new List<Segment>(Segments);
        consCopy.SegmentColor = SavedSegmentColor;
        consCopy.SegmentLineWidth = SavedSegmentLineWidth;
        consCopy.StarsParent = StarsParent;
        consCopy.DistanceBetweenSegmentAndStar = DistanceBetweenSegmentAndStar;
        consCopy.CurrentSegmentMaterial = SavedSegmentMaterial;

        consCopy.RefreshRender();

        foreach(var lineRenderer in consCopy.LineRenderers)
        {
            lineRenderer.enabled = false;
        }
        consCopy.TweenLineRenderer(consCopy.LineRenderers[0], false, false, 0.25f, true, true);

        ClearConstellation();
        return true;
    }

    public void ClearConstellation()
    {
        Segments.Clear();
        HidePreviewSegment();
        RefreshRender();
    }
}
