using UnityEngine;

namespace SurvivalKit.ProceduralGeneration
{
    [System.Serializable]
    public class TerrainInfo
    {
        public Noise Noise { get { return m_Noise; } }
        public Vector2 Seed { get { return m_Seed; } set { m_Seed = value; } }
        public bool RandomizeAtStart { get { return m_RandomizeAtStart; } }

        public int ChunkCount { get { return m_ChunkCount; } set { m_ChunkCount = value; } }
        public int DetailLevel { get { return m_DetailLevel; } set { m_DetailLevel = value; } }
        public Transform ChunkParent { get { return m_ChunkParent; } set { m_ChunkParent = value; } }

        [SerializeField]
        private Noise m_Noise;

        [SerializeField]
        private int m_DetailLevel;

        [SerializeField]
        private int m_ChunkCount;

        [Header("Seed")]

        [SerializeField]
        private Vector2 m_Seed;

        [SerializeField]
        private bool m_RandomizeAtStart;

        private Transform m_ChunkParent;
    }

    public enum NoiseType { Perlin, Simplex, Worley, Voronoi }
}
