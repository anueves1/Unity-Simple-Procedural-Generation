using UnityEngine;
using UnityEditor;
using SurvivalKit.ProceduralGeneration;

namespace SurvivalKit.EditorScripts
{
    [CustomEditor(typeof(VoxelTerrain), true)]
    public class VoxelTerrainEditor : Editor
    {
        private VoxelTerrain m_Terrain;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            m_Terrain = (VoxelTerrain)target;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Generate"))
            {
                m_Terrain.Generate();

                m_Terrain.Reset();
            }

            if (GUILayout.Button("Delete"))
            {
                m_Terrain.Eliminate();
                m_Terrain.Reset();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}