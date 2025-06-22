using UnityEngine;
using System.Collections.Generic;

public class LidarScanner : MonoBehaviour
{
    [Header("Scan Settings")]
    public float scanRange = 5f;
    public int raysPerScan = 360;
    public LayerMask wallLayer;
    public LayerMask fogLayer;
    public bool showDebugRays = true;

    [Header("Timing")]
    private float scanInterval = 0.2f;
    private float scanTimer = 0f;

    [Header("Offset")]
    public Vector3 lidarOffset = new Vector3(0f, 0f, -0.2f); // Slightly behind robot

    private HashSet<Transform> scannedTiles = new HashSet<Transform>();
    private HashSet<Transform> clearedFog = new HashSet<Transform>();

    void Update()
    {
        scanTimer += Time.deltaTime;
        if (scanTimer >= scanInterval)
        {
            scanTimer = 0f;
            Scan();
        }
    }

    void Scan()
    {
        float angleStep = 360f / raysPerScan;
        Vector3 scanOrigin = transform.position + transform.TransformVector(lidarOffset);

        for (int i = 0; i < raysPerScan; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;

            // Hit both wall and fog using bitwise OR of layers
            if (Physics.Raycast(scanOrigin, direction, out RaycastHit hit, scanRange, wallLayer | fogLayer))
            {
                if (showDebugRays)
                    Debug.DrawRay(scanOrigin, direction * hit.distance, Color.green);

                Transform hitObject = hit.transform;

                // Reveal wall tile
                if (((1 << hitObject.gameObject.layer) & wallLayer) != 0)
                {
                    if (!scannedTiles.Contains(hitObject))
                    {
                        scannedTiles.Add(hitObject);
                        MeshRenderer rend = hitObject.GetComponent<MeshRenderer>();
                        if (rend != null)
                            rend.enabled = true;
                    }
                }

                // Disable fog
                if (((1 << hitObject.gameObject.layer) & fogLayer) != 0)
                {
                    if (!clearedFog.Contains(hitObject))
                    {
                        clearedFog.Add(hitObject);
                        hitObject.gameObject.SetActive(false); // Or disable renderer if preferred
                    }
                }
            }
            else if (showDebugRays)
            {
                Debug.DrawRay(scanOrigin, direction * scanRange, Color.red);
            }
        }
    }
}

