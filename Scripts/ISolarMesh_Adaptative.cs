using UnityEngine;

namespace SolarMesh
{
    public class ISolarMesh_Adaptative : ISolarMesh
    {
        public void Generate(SolarMesh solarMesh)
        {
            if (!solarMesh.ValidateRayCount()) 
                return;

            solarMesh.mesh = new Mesh();

            int totalVertices = solarMesh.rayCount * 2 * solarMesh.windows.Length;
            int totalTriangles = solarMesh.rayCount * 6 * solarMesh.windows.Length;

            Vector3[] vertices = new Vector3[totalVertices];
            int[] triangles = new int[totalTriangles];
            Vector2[] uvs = new Vector2[totalVertices];

            int vertexIndex = 0;
            int triangleIndex = 0;

            for (int i = 0; i < solarMesh.windows.Length; i++)
            {
                solarMesh.ProcessWindow(solarMesh.windows[i], ref vertexIndex, ref triangleIndex, vertices, triangles, uvs);
            }

            solarMesh.FinalizeMesh(vertices, triangles, uvs);
        }
    }
}