using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    public Camera Cam;
    public float SmoothSpeed = 6f;

    private float minX, maxX, minY, maxY;
    private bool hasBounds;

    public void SetBounds(float x0, float x1, float y0, float y1)
    {
        minX = x0; maxX = x1; minY = y0; maxY = y1;
        hasBounds = true;
    }

    void LateUpdate()
    {
        if (Target == null || Cam == null) return;

        Vector3 desired = new Vector3(Target.position.x, Target.position.y, -10f);

        if (hasBounds)
        {
            float hw = Cam.orthographicSize * Cam.aspect;
            float hh = Cam.orthographicSize;
            desired.x = Mathf.Clamp(desired.x, minX + hw, maxX - hw);
            desired.y = Mathf.Clamp(desired.y, minY + hh, maxY - hh);
        }

        transform.position = Vector3.Lerp(transform.position, desired, SmoothSpeed * Time.deltaTime);
    }
}
