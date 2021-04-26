using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Line{
    const float verticalLineSlope = 1e5f;
    private float slope;
    private float yIntercept;
    private float slopePerpendicular;
    private Vector2 pointOnLine_1;
    private Vector2 pointOnLine_2;
    private bool approachSide;

    public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine){
        float dx = pointOnLine.x - pointPerpendicularToLine.x;
        float dy = pointOnLine.y - pointPerpendicularToLine.y;

        if(dx == 0){
            slopePerpendicular = verticalLineSlope;
        }else{
            slopePerpendicular = dy/dx;
        }

        if(slopePerpendicular == 0){
            slope = verticalLineSlope;
        }else{
            // the product of two perpendicular line slopes equals -1
            slope = -1 / slopePerpendicular;
        }

        yIntercept = pointOnLine.y - slope * pointOnLine.x;
        pointOnLine_1 = pointOnLine;
        pointOnLine_2 = pointOnLine + new Vector2(1, slope);

        approachSide = false;
        approachSide = GetSide(pointPerpendicularToLine);
    }

    private bool GetSide(Vector2 p){
        // returns true if p is on the right side of the line, and false if p is on the left side
        return (p.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) > (p.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
    }

    public bool HasCrossedLine(Vector2 p){
        return GetSide(p) != approachSide;
    }

    public float DistanceFromPoint(Vector2 p){
        float yInterceptPerpendicular = p.y - slopePerpendicular * -p.x;
        float intersectX = (yInterceptPerpendicular - yIntercept) / (slope - slopePerpendicular);
        float intersectY = slope * intersectX + yIntercept;
        return Vector2.Distance(p, new Vector2(intersectX, intersectY));
    }

    public void DrawWithGizmos(float lenght){
        Vector3 lineDir = new Vector3(1, 0, slope).normalized;
        Vector3 lineCentre = new Vector3(pointOnLine_1.x, 0, pointOnLine_1.y) + Vector3.up;
        Gizmos.DrawLine(lineCentre - lineDir*lenght/2f, lineCentre + lineDir*lenght/2f);
    }
}
