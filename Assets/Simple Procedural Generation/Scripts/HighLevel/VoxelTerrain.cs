using System.Collections.Generic;
using UnityEngine;

namespace SurvivalKit.ProceduralGeneration
{
    public class VoxelTerrain : MonoBehaviour
    {
        [SerializeField]
        [Range(0, 1000)]
        protected int m_Scale;

        [SerializeField]
        private TerrainInfo m_TerrainInfo;

        [SerializeField]
        public Biome[] m_Biomes;

        [SerializeField]
        private bool m_DrawChunks;  

        private List<Chunk> m_Chunks = new List<Chunk>();

        private void Start()
        {
            //If we want the seed randomized.
            if(m_TerrainInfo.RandomizeAtStart)
            {
                //Get a random seed.
                m_TerrainInfo.Seed = Vector2.one * Random.insideUnitSphere * 100;

                //Generate the terrain with that seed.
                Generate();
            }
        }

        private void Setup()
        {
            //Go back if we already have a parent for the chunks.
            if (m_TerrainInfo.ChunkParent != null)
                return;

            //Get the amount of children.
            var cCount = transform.childCount;

            //If we already have a chunk parent, assign it.
            if (cCount > 0)
                m_TerrainInfo.ChunkParent = transform.GetChild(0);
            else
            {
                //Create a new gameobject to store chunks.
                m_TerrainInfo.ChunkParent = new GameObject("Chunks").transform;
                m_TerrainInfo.ChunkParent.SetParent(transform);
            }

            //Set the appropiate position.
            m_TerrainInfo.ChunkParent.localPosition = Vector3.zero;
            //Set the appropiate scale.
            m_TerrainInfo.ChunkParent.localScale = Vector3.one;
        }

        public void Update()
        {
            //Don't run this while playing.
            if (Application.isPlaying)
                return;

            //Get the current seed.
            var seed = m_TerrainInfo.Seed;

            //Update the x position by time.
            seed.x += Time.deltaTime * 10f;

            //Return the seed.
            m_TerrainInfo.Seed = seed;

            Generate();
        }

        public void Reset() { m_TerrainInfo.Seed = new Vector2(); }

        public void Generate()
        {
            //Get the scale setup.
            transform.localScale = Vector3.one * m_Scale;

            //Delete the chunks.
            Eliminate();

            //Create the parent again.
            Setup();

            //If we're just tweaking values, override the mesh.
            var overrideMesh = !Application.isPlaying && Application.isEditor;

            //Go trough the biomes.
            for (int i = 0; i < m_Biomes.Length; i++)
            {
                //Fix octaves editor.
                if (m_Biomes[i].Octaves % 2 != 0)
                    m_Biomes[i].Octaves++;
            }

            //Generate the actual terrain.
            m_Chunks = VoxelMeshCreator.GenerateVoxelTerrain(m_Biomes[0], m_TerrainInfo);
        }

        public void Eliminate()
        {
            Setup();

            //Destroy all the chunks.
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        private void OnDrawGizmos()
        {
            if (m_DrawChunks == false)
                return;

            for (var i = 0; i < m_Chunks.Count; i++)
            {
                //Get the current chunk.
                var c = m_Chunks[i];

                if (c != null)
                {
                    var cVect = c.transform.position;
                    cVect.x += m_TerrainInfo.DetailLevel / 2 * m_Scale;
                    cVect.z += m_TerrainInfo.DetailLevel / 2 * m_Scale;

                    Gizmos.DrawWireCube(cVect, c.transform.lossyScale * m_TerrainInfo.DetailLevel);
                }
            }
        }
    }
}