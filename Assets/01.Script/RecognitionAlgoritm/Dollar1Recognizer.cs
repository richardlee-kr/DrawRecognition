using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UIElements;

public class Dollar1Recognizer : Recognizer, IRecognizer
{
    private float m_size = 250;

    public Dollar1Recognizer(string name) : base(name)
    {
    }

    public string GetName() => name;

    public (string, float) DoRecognition(DollarPoint[] points, int n, List<RecognitionManager.GestureTemplate> gestureTemplates)
    {
        DollarPoint[] preparedPoints = Normalize(points, n);
        return Recognize(preparedPoints, gestureTemplates);
    }

    public DollarPoint[] Normalize(DollarPoint[] points, int n)
    {
        DollarPoint[] copyPoints = new DollarPoint[points.Length];
        points.CopyTo(copyPoints, 0);

        DollarPoint[] resampled = ResamplePoints(copyPoints, n);
        DollarPoint[] rotated = RotateToZero(resampled);
        DollarPoint[] scaled = SclaeToSquare(rotated, m_size);
        DollarPoint[] translated = TranslateToOrigin(scaled);

        return translated;
    }

    private DollarPoint[] ResamplePoints(DollarPoint[] points, int n)
    {
        List<DollarPoint> _points = points.ToList();

        float increament = PathLength(points) / (n - 1);
        float proceedDistance = 0;
        DollarPoint[] newPoints = new DollarPoint[n];
        newPoints[0] = points[0];
        int _index = 1;

        for (int i = 1; i < _points.Count; i++)
        {
            DollarPoint prevPoint = _points[i - 1];
            DollarPoint currPoint = _points[i];
            float distance = Vector2.Distance(prevPoint.point, currPoint.point);

            if (proceedDistance + distance >= increament)
            {
                float t = (increament - proceedDistance) / distance;

                float approximatedX = prevPoint.point.x + t * (currPoint.point.x - prevPoint.point.x);
                float approximatedY = prevPoint.point.y + t * (currPoint.point.y - prevPoint.point.y);

                DollarPoint approximatedPoint = new DollarPoint(approximatedX, approximatedY);
                newPoints[_index] = approximatedPoint;
                _index++;

                _points.Insert(i, approximatedPoint);

                proceedDistance = 0;
            }
            else
            {
                proceedDistance += distance;
            }
        }

        if(proceedDistance > 0.1f)
        {
            newPoints[newPoints.Length - 1] = _points[_points.Count - 1];
            _index++;
        }

        return newPoints;
    }

    private float PathLength(DollarPoint[] points)
    {
        float length = 0f;
        for(int i = 1; i < points.Length; i++)
        {
            length += Vector2.Distance(points[i - 1].point, points[i].point);
        }

        return length;
    }

    private DollarPoint[] RotateToZero(DollarPoint[] points)
    {
        Vector2 centeroid = GetCenteroid(points);
        float angle = Mathf.Atan2(centeroid.y - points[0].point.y, centeroid.x - points[0].point.x);
        DollarPoint[] newPoints = RotateBy(points, -angle);

        return newPoints;
    }

    private DollarPoint[] RotateBy(DollarPoint[] points, float angle)
    {
        DollarPoint[] newPoints = new DollarPoint[points.Length];
        int _index = 0;

        Vector2 centeroid = GetCenteroid(points);
        foreach(DollarPoint point in points)
        {
            float rotatedX = (point.point.x - centeroid.x) * Mathf.Cos(angle)
                           - (point.point.y - centeroid.y) * Mathf.Sin(angle)
                           + centeroid.x;
            float rotatedY = (point.point.x - centeroid.x) * Mathf.Sin(angle)
                           + (point.point.y - centeroid.y) * Mathf.Cos(angle)
                           + centeroid.x;
            newPoints[_index] = new DollarPoint(rotatedX, rotatedY);
            _index++;
        }

        return newPoints;
    }

    private Vector2 GetCenteroid(DollarPoint[] points)
    {
        float centerX = points.Sum(point => point.point.x) / points.Length;
        float centerY = points.Sum(point => point.point.y) / points.Length;

        return new Vector2(centerX, centerY);
    }

    private DollarPoint[] SclaeToSquare(DollarPoint[] points, float size)
    {
        DollarPoint[] newPoints = new DollarPoint[points.Length];
        int _index = 0;

        Rect bbox = GetBoundingBox(points);
        foreach(DollarPoint point in points)
        {
            float scaledX = point.point.x * size / bbox.width;
            float scaledY = point.point.y * size / bbox.height;
            newPoints[_index] = new DollarPoint(scaledX, scaledY);
            _index++;
        }

        return newPoints;
    }

    private DollarPoint[] TranslateToOrigin(DollarPoint[] points)
    {
        DollarPoint[] newPoints = new DollarPoint[points.Length];
        int _index = 0;

        Vector2 centeroid = GetCenteroid(points);

        foreach(DollarPoint point in points)
        {
            float translatedX = point.point.x - centeroid.x;
            float translatedY = point.point.y - centeroid.y;
            newPoints[_index] = new DollarPoint(translatedX, translatedY);
            _index++;
        }

        return newPoints;
    }

    private Rect GetBoundingBox(DollarPoint[] points)
    {
        float minX = points.Select(point => point.point.x).Min();
        float maxX = points.Select(point => point.point.x).Max();
        float minY = points.Select(point => point.point.y).Min();
        float maxY = points.Select(point => point.point.y).Max();
        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private (string, float) Recognize(DollarPoint[] points, List<RecognitionManager.GestureTemplate> templates)
    {
        float theta = 45;
        float deltaTheta = 2;
        float angle = 0.5f * (-1 + Mathf.Sqrt(5));

        float best = float.PositiveInfinity;

        RecognitionManager.GestureTemplate bestTemplate = new RecognitionManager.GestureTemplate();

        foreach(RecognitionManager.GestureTemplate template in templates)
        {
            float distance = DistanceAtBestAngle(points, template, -theta, theta, deltaTheta, angle);

            if(distance < best)
            {
                best = distance;
                bestTemplate = template;
            }
        }

        double score = 1 - (best / (0.5f * Math.Sqrt(2 * m_size * m_size)));
        return ((string, float))(bestTemplate.name, score);
    }

    private float DistanceAtBestAngle(DollarPoint[] points, RecognitionManager.GestureTemplate template,
        float thetaA, float thetaB, float deltaTheta, float angle)
    {
        float x1 = angle * thetaA + (1 - angle) * thetaB;
        float x2 = (1 - angle) * thetaA + angle * thetaB;

        float d1 = DistanceAtAngle(points, template, x1);
        float d2 = DistanceAtAngle(points, template, x2);

        while(Mathf.Abs(thetaB - thetaA) > deltaTheta)
        {
            if(d1 < d2)
            {
                thetaB = x2;
                x2 = x1;
                d2 = d1;
                x1 = angle * thetaA + (1 - angle) * thetaB;
                d1 = DistanceAtAngle(points, template, x1);
            }
            else
            {
                thetaA = x1;
                x1 = x2;
                d1 = d2;
                x2 = (1 - angle) * thetaA + angle * thetaB;
                d2 = DistanceAtAngle(points, template, x2);
            }
        }

        return Mathf.Min(d1, d2);
    }

    private float DistanceAtAngle(DollarPoint[] points, RecognitionManager.GestureTemplate template, float angle)
    {
        DollarPoint[] newPoints = RotateBy(points, angle);
        float distance = PathDistance(newPoints, Normalize(template.points, 64));

        return distance;
    }

    private float PathDistance(DollarPoint[] A, DollarPoint[] B)
    {
        float distance = 0f;

        for(int i = 0; i < A.Length; i++)
        {
            distance += Vector2.Distance(A[i].point, B[i].point);
        }

        return distance / A.Length;
    }
}
