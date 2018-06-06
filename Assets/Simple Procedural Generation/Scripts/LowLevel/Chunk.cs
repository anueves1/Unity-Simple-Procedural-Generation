using System.Collections.Generic;
using UnityEngine;

namespace SurvivalKit.ProceduralGeneration
{
    public class Chunk : MonoBehaviour
    {
        public Vector2 Position { get { return m_Position; } set { m_Position = value; } }
        public List<Vector3> Vertices { get { return m_Vertices; } }

        private Vector2 m_Position;

        private List<Vector3> m_Vertices = new List<Vector3>();
        private List<int> m_Triangles = new List<int>();

        private MeshFilter m_Filter;
        private MeshRenderer m_Renderer;
        private MeshCollider m_Collider;

        public Mesh Set(List<Vector3> verts, bool recalculateNormals = true)
        {
            //Clear the shared mesh.
            m_Filter.sharedMesh = null;

            //Setup the vert data.
            m_Vertices = verts;

            //Update the mesh.
            return Generate(recalculateNormals);
        }

        public Mesh Set(List<Vector3> verts, List<int> tris, bool recalculateNormals = true)
        {
            //Clear the shared mesh.
            m_Filter.sharedMesh = null;

            //Setup the vert data.
            m_Vertices = verts;
            //Setup the triangle data.
            m_Triangles = tris;

            //Update the mesh.
            return Generate(recalculateNormals);
        }

        private Mesh Generate(bool recalculateNormals)
        {
            //Create a new mesh.
            Mesh m = new Mesh();
            m.name = "Chunk";

            //Assign the vertices.
            m.vertices = m_Vertices.ToArray();
            //Assign the triangles.
            m.triangles = m_Triangles.ToArray();

            //Add the mesh.
            m_Filter.sharedMesh = m;

            //Recalculate normals if needed.
            if (recalculateNormals)
                m_Filter.sharedMesh.RecalculateNormals();

            //Update the collision.
            m_Collider.sharedMesh = m;

            return m_Filter.sharedMesh;
        }

        public void Setup(Material mat)
        {
            //Get a renderer if we don't have one.
            if (m_Renderer == null)
            {
                //Get the renderer.
                m_Renderer = GetComponent<MeshRenderer>();
            }

            //Add a filter if we don't have one.
            if (m_Filter == null)
            {
                //Get the filter.
                m_Filter = GetComponent<MeshFilter>();
            }

            //Add a collider if we don't have one.
            if (m_Collider == null)
            {
                //Get the collider.
                m_Collider = GetComponent<MeshCollider>();
            }

            //Add a renderer if we don't have one.
            if (m_Renderer == null)
                m_Renderer = gameObject.AddComponent<MeshRenderer>();

            //Add a filter if we don't have one.
            if (m_Filter == null)
                m_Filter = gameObject.AddComponent<MeshFilter>();

            //Add a collider if we don't have one.
            if (m_Collider == null)
                m_Collider = gameObject.AddComponent<MeshCollider>();

            //Assign a material.
            m_Renderer.sharedMaterial = mat;
        }
    }
}