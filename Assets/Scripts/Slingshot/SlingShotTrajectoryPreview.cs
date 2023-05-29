using UnityEngine;
using System.Collections.Generic;

public class SlingShotTrajectoryPreview : MonoBehaviour {

    [SerializeField] private LineRenderer lineRenderer;

    public void DrawPredictionLine(Vector2 direction, Vector2 startPoint) {
        // draw a line from vector2 points
        List<Vector3> lineRendererPoints = simulateArc(direction, startPoint);
        string lineRendererPointsString = "";
        foreach (Vector3 point in lineRendererPoints) {
            lineRendererPointsString += point.ToString() + "\n";
        }
        // Debug.Log(lineRendererPointsString);
        lineRenderer.positionCount = lineRendererPoints.Count;
        lineRenderer.SetPositions(lineRendererPoints.ToArray());
        
    }

    private List<Vector3> simulateArc(Vector2 velocity, Vector2 startPoint) {
        List<Vector3> lineRendererPoints = new List<Vector3>();

        Vector2 gravity = Physics2D.gravity;
        float timestep = Time.fixedDeltaTime;
        float simLength = 1.5f;
        
        Vector2 position = startPoint;
        
        // take the startpoint and the velocity and simulate the arc the projectile would take
        for (int i = 0; i < simLength / timestep; i++) {
            lineRendererPoints.Add(position);
            position += velocity * timestep;
            velocity += gravity * timestep;
        }

        return lineRendererPoints;

    }
    
    public void ClearPredictionLine() {
        lineRenderer.positionCount = 0;
    }

    private bool CheckForCollision(Vector2 positionToCheck) {
        RaycastHit2D hit = Physics2D.Raycast(positionToCheck, Vector2.zero);

        if (hit.collider != null) {
            return true;
        }

        return false;
    }
}