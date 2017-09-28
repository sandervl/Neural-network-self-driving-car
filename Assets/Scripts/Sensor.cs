using System;
using UnityEngine;

public class Sensor : MonoBehaviour
{
    public double ReportDistance;
    public float MaxDistance = 5;

    public SpriteRenderer HitPointSpriteRenderer;

    void Start()
    {
        HitPointSpriteRenderer.enabled = false;
    }

    void FixedUpdate()
    {
        //0 = good to go
        //1 = hit the surface
        var reportingValue = 0f;

        int obstaclesLayerMask = LayerMask.GetMask("Obstacles");
        var hit = Physics2D.Raycast(transform.position, transform.right, MaxDistance, obstaclesLayerMask);
        if (hit.collider != null)
        {
            if (hit.distance <= MaxDistance)
                reportingValue = (MaxDistance - hit.distance) / MaxDistance;
        }

        if (reportingValue > 0)
        {
            HitPointSpriteRenderer.enabled = true;
            HitPointSpriteRenderer.transform.position = hit.point;
        }
        else
        {
            HitPointSpriteRenderer.enabled = false;
        }

        ReportDistance = reportingValue;
    }
}
