using UnityEngine;
//[RequireComponent(typeof(MeshFilter))]

public class WaveManager : MonoBehaviour
{
    public float waveHeight = 0.5f;     // how tall the waves are
    public float waveFrequency = 1f;    // how many waves fit in space
    public float waveSpeed = 1f;        // how fast waves move

    private Mesh mesh;
    private Vector3[] baseVertices;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        baseVertices = mesh.vertices;
    }

    void Update()
    {
        Vector3[] vertices = new Vector3[baseVertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = baseVertices[i];
            // Wave formula: sine based on position and time
            v.y += Mathf.Sin(Time.time * waveSpeed + v.x * waveFrequency + v.z * waveFrequency) * waveHeight;
            vertices[i] = v;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals(); // keeps lighting correct
    }
}
