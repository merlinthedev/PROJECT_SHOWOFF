using System;
using UnityEngine;
using System.Collections.Generic;

public class SlingShotTrajectoryPreview : MonoBehaviour {
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lineWidth = 0.2f;

    private void Start() {
        lineRenderer.widthMultiplier = lineWidth;
    }

    public void DrawPredictionLine(Vector2 direction, Vector2 startPoint) {
        List<Vector3> lineRendererPoints = simulateArc(direction, startPoint);

        lineRenderer.positionCount = lineRendererPoints.Count;
        lineRenderer.SetPositions(lineRendererPoints.ToArray());
    }

    private List<Vector3> simulateArc(Vector2 velocity, Vector2 startPoint) {
        List<Vector3> lineRendererPoints = new();

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
}