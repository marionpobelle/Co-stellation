using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    private List<LineRenderer> _lineRenderers = new List<LineRenderer>();
    public GameObject StarsParent;

    public Segment PreviewSegment { 
        get => _previewSegment; 
        set
        {
            _previewSegment = value;
            RefreshRender();
        }
    }

    public void AddSegment(Star start, Star end)
    {
        AddSegment(new Segment(start, end));
    }

    public void AddSegment(Segment segment)
    {
        if( Segments.Find(x=>x.Equals(segment))!=null ) return;
        Segments.Add(segment);
        RefreshRender();
    }

    private void RefreshRender()
    {
        foreach (var lineRenderer in _lineRenderers)
        {
            Destroy(lineRenderer.gameObject);
        }
        _lineRenderers.Clear();
        foreach (var segment in Segments)
        {
            AddLineRenderer(segment,null,Color.white);
        }
        if (PreviewSegment == null) return;

        AddLineRenderer(PreviewSegment, null, Color.grey);
    }

    private void AddLineRenderer(Segment segment, Material material, Color color)
    {
        var lineRenderer = StarsParent.AddComponent<LineRenderer>();
        lineRenderer.transform.SetParent(transform);
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, segment._start.transform.position);
        lineRenderer.SetPosition(1, segment._end.transform.position);
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material.color = color;
        _lineRenderers.Add(lineRenderer);
    }
}
