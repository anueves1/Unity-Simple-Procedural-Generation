using UnityEngine;
using UnityEditor;
using SurvivalKit.ProceduralGeneration;

namespace SurvivalKit.EditorScripts
{
    [CustomEditor(typeof(Water), true)]
    public class WaterEditor : Editor
    {
        private bool m_Previewing = false;
        private Water m_Water;

        public void OnEnable() { EditorApplication.update += Update; }

        public void OnDisable()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                m_Previewing = false;

                if (m_Water != null)
                    m_Water.Reset();
            }

            EditorApplication.update -= Update;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            m_Water = (Water)target;

            EditorGUILayout.BeginHorizontal();

            if(!m_Water.HasMesh())
            {
                if (GUILayout.Button("Generate"))
                {
                    m_Water.Generate();
                    m_Previewing = false;

                    m_Water.Reset();
                }
            }
            else
            {
                string text = m_Previewing ? "Stop Preview" : "Preview";
                if (GUILayout.Button(text))
                {
                    m_Previewing = !m_Previewing;
                    m_Water.Reset();
                }

                if (GUI.changed)
                    m_Water.Generate();

                if (GUILayout.Button("Delete"))
                {
                    m_Water.Eliminate();
                    m_Water.Reset();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void Update()
        {
            if (m_Previewing && m_Water != null)
            {
                if (Application.isEditor && !Application.isPlaying)
                    m_Water.Update();
            }
        }
    }
}