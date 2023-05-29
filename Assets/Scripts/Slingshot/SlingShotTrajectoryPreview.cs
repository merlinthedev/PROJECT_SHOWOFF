using UnityEngine;
using System.Collections.Generic;

public class SlingShotTrajectoryPreview : MonoBehaviour {

    public void DrawPredictionLine(Vector2 direction) {
        this.simulateArc(direction);
    }

    private List<Vector2> simulateArc(Vector2 velocity) {
        List<Vector2> lineRendererPoints = new List<Vector2>();

        float maxDuration = 5f;
        float timeStepInterval = 0.1f;
        int maxSteps = (int)(maxDuration / timeStepInterval);
        
        Vector2 direction = Vector2.up;
        Vector2 launchPosition = transform.position;

        for (int i = 0; i < maxSteps; ++i) {
            Vector2 calculatedPosition = launchPosition + direction * velocity * timeStepInterval * i;
            calculatedPosition.y += Physics2D.gravity.y / 2 * Mathf.Pow(i * timeStepInterval, 2);
            
            lineRendererPoints.Add(calculatedPosition);
            
            if (CheckForCollision(calculatedPosition)) {
                break;
            }
        }
        

        return lineRendererPoints;
    }

    private bool CheckForCollision(Vector2 positionToCheck) {
        RaycastHit2D hit = Physics2D.Raycast(positionToCheck, Vector2.zero);

        if (hit.collider != null) {
            return true;
        }
        
        return false;
    }
}
