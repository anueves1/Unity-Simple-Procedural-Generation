using SurvivalKit.PCG;
using UnityEngine;

namespace SurvivalKit.ProceduralGeneration
{
    [System.Serializable]
    public class Biome
    {
        public int BiomePossibility { get { return m_BiomePossibility; } }

        public Material ChunkMaterial { get { return m_ChunkMaterial; }  }
        public int Octaves { get { return m_Octaves; } set { m_Octaves = value; } }

        public float Persistance { get { return m_Persistance; } }
        public float Lacunarity { get { return m_Lacunarity; } }
        public float TerraceValue { get { return m_TerraceValue; } }

        public Redistribution Redistribution { get { return m_Redistribution; } }

        [SerializeField]
        private string m_BiomeName;

        [SerializeField]
        [Range(0, 100)]
        private int m_BiomePossibility;

        [SerializeField]
        private Material m_ChunkMaterial;

        [Header("Fractal")]

        [SerializeField]
        [Range(2, 16)]
        private int m_Octaves = 2;

        [SerializeField]
        [Range(0.01f, 0.99f)]
        private float m_Persistance = 0.01f;

        [SerializeField]
        [Range(0.01f, 1)]
        private float m_Lacunarity = 0.8f;

        [SerializeField]
        private Redistribution m_Redistribution;

        [Header("Additional")]

        [SerializeField]
        private int m_TerraceValue = 0;
    }
}