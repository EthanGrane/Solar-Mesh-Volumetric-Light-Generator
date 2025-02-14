using System;
using System.Collections.Generic;
using UnityEngine;
using static SolarMesh.SolarMeshEditor;

namespace SolarMesh
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SolarMesh : MonoBehaviour
    {
        [Serializable]
        public struct SunLightWindow
        {
            public Transform center;
        }

        public Transform[] windows;
        public Transform lightPointTransform;
        public float lightRayLenght = 10;
        [Space]
        public int rayCount = 16;
        public const float rayLenght = 100;

        public EditorMode editorMode = EditorMode.Rectangle;
        public Mesh mesh;

        ISolarMesh currentGenerator;
        Dictionary<EditorMode, ISolarMesh> generators = new Dictionary<EditorMode, ISolarMesh>
        {
            { EditorMode.Rectangle, new ISolarMesh_Rectangle() },
            { EditorMode.Adaptative, new ISolarMesh_Adaptative() }
        };

        public void Generate()
        {
            if (generators.TryGetValue(editorMode, out currentGenerator))
            {
                currentGenerator.Generate(this);
            }
            else
            {
                Debug.LogError($"Modo {editorMode} no tiene un generador asignado.");
            }
        }


        public void ProcessWindow(Transform window, ref int vertexIndex, ref int triangleIndex, Vector3[] vertices, int[] triangles, Vector2[] uvs)
        {
            if (window == null)
                return;

            Transform centerTransform = window;
            Vector3 center = centerTransform.position;

            float angleStep = 360f / rayCount;

            for (int j = 0; j < rayCount; j++)
            {
                Vector3 firstHit = GetFirstHit(centerTransform, center, j, angleStep);
                Vector3 secondHit = GetSecondHit(firstHit);

                AssignVertices(vertices, vertexIndex, firstHit, secondHit);
                AssignTriangles(triangles, vertexIndex, triangleIndex, j);
                AssignUVs(uvs, vertexIndex, j);

                vertexIndex += 2;
                triangleIndex += 6;
            }
        }

        public Vector3 GetFirstHit(Transform centerTransform, Vector3 center, int rayIndex, float angleStep)
        {
            float angle = (rayIndex + 1) * angleStep + 45;
            Vector3 localDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 direction = centerTransform.TransformDirection(localDirection);

            if (Physics.Raycast(center, direction, out RaycastHit hit, rayLenght))
                return hit.point;

            return center + direction * rayLenght;
        }

        public Vector3 GetSecondHit(Vector3 firstHit)
        {
            Vector3 sunDir = lightPointTransform.forward.normalized;
            return firstHit + sunDir * lightRayLenght;
        }

        public bool ValidateRayCount()
        {
            if (rayCount % 4 != 0)
            {
                Debug.LogWarning("El n�mero de rayos debe ser divisible entre 4.");
                return false;
            }
            return true;
        }

        public void AssignVertices(Vector3[] vertices, int vertexIndex, Vector3 firstHit, Vector3 secondHit)
        {
            vertices[vertexIndex] = firstHit;
            vertices[vertexIndex + 1] = secondHit;
        }

        public void AssignTriangles(int[] triangles, int vertexIndex, int triangleIndex, int rayIndex)
        {
            if (rayIndex < rayCount - 1)
            {
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 1;
                triangles[triangleIndex + 2] = vertexIndex + 2;

                triangles[triangleIndex + 3] = vertexIndex + 2;
                triangles[triangleIndex + 4] = vertexIndex + 1;
                triangles[triangleIndex + 5] = vertexIndex + 3;
            }
            else
            {
                int firstVertexIndex = vertexIndex - ((rayCount - 1) * 2);
                
                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 1;
                triangles[triangleIndex + 2] = firstVertexIndex;

                triangles[triangleIndex + 3] = firstVertexIndex;
                triangles[triangleIndex + 4] = vertexIndex + 1;
                triangles[triangleIndex + 5] = firstVertexIndex + 1;
            }
        }

        public void AssignUVs(Vector2[] uvs, int vertexIndex, int rayIndex)
        {
            float u = rayIndex / rayCount;
            uvs[vertexIndex] = new Vector2(u, 0);
            uvs[vertexIndex + 1] = new Vector2(u, 1);
        }

        public void FinalizeMesh(Vector3[] vertices, int[] triangles, Vector2[] uvs)
        {
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;

            mesh.RecalculateBounds();
            //mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
        }

        public void OnDrawGizmos()
        {
            if (lightPointTransform == null)
                return;

            if (rayCount % 4 != 0)
            {
                Debug.LogWarning("El n�mero de rayos debe ser divisible entre 4.");
                return;
            }

            for (int i = 0; i < windows.Length; i++)
            {
                if (windows[i] == null)
                    continue;

                Transform centerTransform = windows[i];
                Vector3 center = centerTransform.position;

                Gizmos.DrawSphere(center, 0.1f);

                // Calcular �ngulo entre cada rayo
                float angleStep = 360f / rayCount;
                Vector3[] hits = new Vector3[rayCount];

                for (int j = 0; j < rayCount; j++)
                {
                    // Calcular direcci�n radial en espacio local y transformarla al espacio mundial
                    float angle = (j + 1) * angleStep + 45;
                    Vector3 localDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
                    Vector3 direction = centerTransform.TransformDirection(localDirection); // Relativo a la rotaci�n de center

                    Gizmos.color = Color.grey;
                    // Lanzar Raycast
                    if (Physics.Raycast(center, direction, out RaycastHit hit, rayLenght))
                    {
                        hits[j] = hit.point;
                    }
                    else
                    {
                        // Dibujar l�nea hasta el final del rayo si no hay colisi�n
                        Vector3 endPoint = center + direction * rayLenght;
                        Gizmos.DrawLine(center, endPoint);
                        hits[j] = endPoint;
                    }


                }

                Gizmos.color = Color.yellow;
                // Dibujar el rayo principal hacia el sol
                Vector3 sunDir = lightPointTransform.forward;
                Gizmos.DrawLine(center, center + (sunDir * 10));

                // Dibujar rayos desde los puntos impactados hacia la direcci�n del sol
                foreach (var hitPoint in hits)
                {
                    Gizmos.DrawRay(hitPoint, sunDir * lightRayLenght);
                }
            }
        }
    }
}
