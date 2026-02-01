using UnityEngine;

public class ShovelSand : MonoBehaviour
{
    public float shovelRange = 2f;
    public LayerMask sandLayer;
    public ParticleSystem sandParticlePrefab;   // prefab in Project window

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left click
        {
            Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, shovelRange, sandLayer))
            {
                // Spawn particle at hit point
                if (sandParticlePrefab != null)
                {
                    ParticleSystem ps = Instantiate(
                        sandParticlePrefab,
                        hit.point + Vector3.up * 0.1f,   // small lift above sand
                        Quaternion.identity
                    );

                    // Optional: auto-destroy the particle object after it finishes
                    var main = ps.main;
                    Destroy(ps.gameObject, main.duration + main.startLifetimeMultiplier);
                }

                // Remove the sand object
                Destroy(hit.collider.gameObject);
            }
        }
    }
}
