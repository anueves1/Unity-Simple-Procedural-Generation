using UnityEngine;

namespace SurvivalKit.ProceduralGeneration
{
    public class Water : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 1000)]
        private int m_Scale;

        [SerializeField]
        [Range(0, 100)]
        private int m_DetailLevel;

        [SerializeField]
        private Material m_Material;

        [Header("Water")]

        [SerializeField]
        private bool m_UseWaves;

        [SerializeField]
        private Vector2 m_Displacement;

        [SerializeField]
        private Vector2 m_Speed;

        private Vector2 m_Offset;

        private MeshFilter m_Filter;
        private MeshRenderer m_Renderer;

        private void Start() { Reset(); }

        private void Setup()
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

            //Add a renderer if we don't have one.
            if (m_Renderer == null)
                m_Renderer = gameObject.AddComponent<MeshRenderer>();

            //Add a filter if we don't have one.
            if (m_Filter == null)
                m_Filter = gameObject.AddComponent<MeshFilter>();
        }

        public void Update()
        {
            //Only use waves if needed.
            if (m_UseWaves == false || HasMesh() == false)
                return;

            //Update time.
            m_Offset.x += Time.deltaTime * m_Speed.x;
            m_Offset.y += Time.deltaTime * m_Speed.y;

            //Generate the noise grid.
            m_Filter.GenerateSimpleNoiseGrid(true, m_Displacement, m_Offset);
        }

        public bool HasMesh()
        {
            Setup();

            //Check if there is a mesh assigned.
            var hasMesh = m_Filter.sharedMesh != null;
            //Check if that mesh has something in it.
            var hasVertices = hasMesh && m_Filter.sharedMesh.vertices.Length > 0;

            return hasVertices;
        }

        public void Reset() { m_Offset = Vector2.zero; }

        public void Generate()
        {
            Setup();

            //Assign the material.
            m_Renderer.sharedMaterial = m_Material;

            //Get the scale setup.
            transform.localScale = Vector3.one * m_Scale;

            //Assign the mesh and the material.
            m_Filter.GenerateGrid(m_DetailLevel, true);
        }

        public void Eliminate() { m_Filter.sharedMesh.Clear(); }
    }
}