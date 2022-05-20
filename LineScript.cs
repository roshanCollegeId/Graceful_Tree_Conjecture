using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LineScript
{
    public static bool Intersect(Elements startingGameElement, Elements nextGameElement, IEnumerable<List<Elements>> branches)
    {
        return startingGameElement != null && nextGameElement != null && (from list in branches
            from elements in list
            where elements.NextAttachedElement != null
            where elements != startingGameElement && elements.NextAttachedElement != startingGameElement &&
                  elements != nextGameElement && elements.NextAttachedElement != nextGameElement
            select elements).Any(elements => IntersectingLines(startingGameElement.Position, nextGameElement.Position,
            elements.Position, elements.NextAttachedElement.Position));
    }
    public static bool OverlapWithNodes(Elements startingGameObject, Elements nextElement, IEnumerable<Elements> blanksList, float radii)
    {
        return blanksList.Where(element => element != startingGameObject && element != nextElement)
            .Where(element => !OppositeQuadrant(startingGameObject.Position, nextElement.Position, element.Position))
            .Any(element =>
                PerpendicularDistanceOverlap(startingGameObject.Position, nextElement.Position, element.Position,
                    radii));
    }
    private static bool CounterClockWise(Vector2 a, Vector2 b, Vector2 c) { return (c.y - a.y) * (b.x - a.x) > (b.y - a.y) * (c.x - a.x); }
    public static bool PerpendicularDistanceOverlap(Vector2 a, Vector2 b, Vector2 c, float radii)
    {
        if (ImpossibleOverlappingCase(a, b, c)) { return false; }
        
        Vector2 ab = new Vector2(b[0] - a[0], b[1] - a[1]).normalized;
        Vector2 ac = new Vector2(c[0] - a[0], c[1] - a[1]);
        Vector2 n = new Vector2(-ab[1], ab[0]);
        
        Vector2 endPoint = c - (ac[0] * n[0] + ac[1] * n[1]) * n.normalized;
        
        return Vector3.Distance(c, endPoint) < radii;
    }
    public static bool IntersectingLines(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return CounterClockWise(a, c, d) != CounterClockWise(b, c, d) &&
               CounterClockWise(a, b, c) != CounterClockWise(a, b, d);
    }
    private static bool ImpossibleOverlappingCase(Vector2 a, Vector2 b, Vector2 c)
    {
        float x0 = a.x;
        float y0 = a.y;
        float x1 = b.x;
        float y1 = b.y;

        float x = c.x;
        float y = c.y;

        float m = (y1 - y0) / (x1 - x0);
        
        float midX = (x0 + x1) * 0.5f;
        float midY = (y0 + y1) * 0.5f;
        
        bool case1 = (y - y1 + (x - x1) / m) * (midY - y1 + (midX - x1) / m) > 0; 
        bool case2 = (y - y0 + (x - x0) / m) * (midY - y0 + (midX - x0) / m) > 0;

        return !case1 || !case2;
    }
    private static bool OppositeQuadrant(Vector2 a, Vector2 b, Vector2 c)
    {
        if (!(a.x * b.x > 0) || !(a.y * b.y > 0)) return false;
        return a.x * c.x < 0 && a.y * c.y < 0;
    }
}