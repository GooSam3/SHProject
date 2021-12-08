using UnityEngine;

public class ZGizmos
{
    public static void DrawCircleGizmo(Vector3 startPos, float meleeRadius, Color circleColor)
    {
        Color colorOld = Gizmos.color;
        Gizmos.color = circleColor;
        float theta = 0;
        float stepTheta = Mathf.PI / 20;
        float x = meleeRadius * Mathf.Cos(theta);
        float y = meleeRadius * Mathf.Sin(theta);
        Vector3 pos = startPos + new Vector3(x, 0, y);
        Vector3 newPos = pos;
        //Vector3 lastPos = pos;
        for (theta = 0.1f; theta < Mathf.PI * 2; theta += stepTheta)
        {
            x = meleeRadius * Mathf.Cos(theta);
            y = meleeRadius * Mathf.Sin(theta);
            newPos = startPos + new Vector3(x, 0, y);
            Gizmos.DrawLine(pos, newPos);
            pos = newPos;
        }
        //Gizmos.DrawLine(pos, lastPos);
        Gizmos.color = colorOld;
    }
}
