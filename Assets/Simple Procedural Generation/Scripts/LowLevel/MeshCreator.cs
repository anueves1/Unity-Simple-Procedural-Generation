using System.Collections.Generic;
using UnityEngine;

namespace SurvivalKit.ProceduralGeneration
{
    public static class MeshCreator
    {
        public static Mesh GenerateTerrain(this MeshFilter filter, bool overrideMesh, int octaves, float persistance,
            float detailLevel, float lacunarity, Vector3 seed, Vector2 clamp)
        {
            //Create a new mesh.
            Mesh m = new Mesh();

            //Only generate grid if needed.
            if(overrideMesh || (filter.sharedMesh == null || filter.sharedMesh.vertices.Length == 0))
                m = GenerateGrid(filter, detailLevel, true);
            else if (filter.sharedMesh != null && filter.sharedMesh.vertices.Length > 0)
                m = filter.sharedMesh;

            //Get the vertices.
            Vector3[] vertices = m.vertices;

            //Loop trough them.
            for (int i = 0; i < vertices.Length; i++)
            {
                //Get the x coordinate.
                float xCoord = vertices[i].x;
                //Get the y coordinate.
                float zCoord = vertices[i].z;

                //Calculate the x position variation.
                float x = xCoord + seed.x;
                //Calculate the y position variation.
                float z = zCoord + seed.y;

                //Assign the needed perlin value.
                vertices[i].y = CalculatePerlinWithOctaves(x, z, octaves, persistance, lacunarity, clamp);
            }

            //Assign back the vertices.
            m.vertices = vertices;
            //Assign back the mesh.
            filter.sharedMesh = m;

            //Recalculate the normals.
            filter.sharedMesh.RecalculateNormals();

            return m;
        }

        public static Mesh GenerateSimpleNoiseGrid(this MeshFilter filter, bool recalculateNormals = true, Vector2 displacement = new Vector2(),
            Vector2 offset = new Vector2(), float heightMultiplier = 1f)
        {
            //Create a new mesh.
            Mesh m = new Mesh();

            //Generate the grid if needed.
            if(filter.sharedMesh != null && filter.sharedMesh.vertices.Length > 0)
                m = filter.sharedMesh;
            else
                m = GenerateGrid(filter);

            //Get the vertices.
            Vector3[] vertices = m.vertices;

            //Loop trough them.
            for (int i = 0; i < vertices.Length; i++)
            {
                //Calculate the x position variation.
                float x = (vertices[i].x * (displacement.x * 0.1f)) + offset.x;
                //Calculate the y position variation.
                float z = (vertices[i].z * (displacement.y * 0.1f)) + offset.y;

                //Calculate what the new Y is.
                float newY = Mathf.PerlinNoise(x, z) * heightMultiplier;

                //Assign it.
                vertices[i].y = newY;
            }

            //Assign back the vertices.
            m.vertices = vertices;
            //Assign back the mesh.
            filter.sharedMesh = m;

            //Recalculate normals if needed.
            if (recalculateNormals)
                m.RecalculateNormals();

            return m;
        }

        private static float CalculatePerlinWithOctaves(float x, float z, float octaves, float persistance, float lacunarity, Vector2 clamp)
        {
            float perlinValue = 0f;

            float maxAmplitude = 0f;
            float amplitude = 1f;

            float frequency = lacunarity;

            //Go trough every octave.
            for (int o = 0; o < octaves; o++)
            {
                //Add the perlin value.
                perlinValue += Mathf.PerlinNoise(x * frequency, z * frequency) * amplitude;

                //Add the current amplitude to the max.
                maxAmplitude += amplitude;

                //Lower the amplitude as octaves get higher.
                amplitude *= (-persistance);

                //Double the frequency so the spaces between high points are further apart.
                frequency *= lacunarity;
            }

            //Average the perlin value by the amplitude.
            perlinValue /= maxAmplitude;

            //Clamp the perlin value.
            perlinValue = Mathf.Clamp(perlinValue, clamp.x, clamp.y);

            return perlinValue;
        }

        public static Mesh GenerateGrid(this MeshFilter filter, float detailLevel = 10, bool recalculateNormals = false)
        {
            //If it has a mesh.
            if(filter.sharedMesh != null)
            {
                //Clear it.
                filter.sharedMesh.Clear();
            }

            //Create a new mesh and name it.
            Mesh gridMesh = new Mesh();
            gridMesh.name = "Grid";

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            for (int x = 0; x < detailLevel; x++)
            {
                //Generate one quad for each place we want it to be at.
                for (int y = 0; y < detailLevel; y++)
                    GetQuadValues(new Vector3(x, 0, y), ref vertices, ref triangles);
            }

            gridMesh.vertices = vertices.ToArray();
            gridMesh.triangles = triangles.ToArray();

            //If needed, calculate normals.
            if (recalculateNormals)
                gridMesh.RecalculateNormals();

            //Assign the mesh.
            filter.sharedMesh = gridMesh;

            return gridMesh;
        }

        public static Mesh GenerateQuad(this MeshFilter filter, Vector3 position = new Vector3(), bool recalculateNormals = false)
        {
            //If it has a mesh.
            if (filter.sharedMesh != null)
            {
                //Clear it.
                filter.sharedMesh.Clear();
            }

            //Create a new mesh and name it.
            Mesh quadMesh = new Mesh();
            quadMesh.name = "Quad";

            List<Vector3> verts = new List<Vector3>();
            List<int> triangles = new List<int>();
            GetQuadValues(position, ref verts, ref triangles);

            quadMesh.vertices = verts.ToArray();
            quadMesh.triangles = triangles.ToArray();

            //If needed, calculate normals.
            if (recalculateNormals)
                quadMesh.RecalculateNormals();

            //Assign the mesh.
            filter.sharedMesh = quadMesh;

            return filter.sharedMesh;
        }

        public static void GetCubeValues(Vector3 position, ref List<Vector3> vertices, ref List<int> triangles)
        {
            //Spawn the first quad.
            GetQuadValues(position, ref vertices, ref triangles, true);
            GetQuadValues(position + Vector3.down, ref vertices, ref triangles, false);

            Vector3[] q1verts = new Vector3[6];

            q1verts[0] = Vector3.right + position;
            q1verts[1] = Vector3.right + Vector3.down + position;
            q1verts[2] = Vector3.right + Vector3.down + Vector3.forward + position;

            q1verts[3] = Vector3.right + Vector3.forward + position;
            q1verts[4] = Vector3.right + position;
            q1verts[5] = Vector3.right + Vector3.down + Vector3.forward + position;

            int[] tris = new int[6];

            tris[0] = 2 + triangles.Count;
            tris[1] = 1 + triangles.Count;
            tris[2] = 0 + triangles.Count;

            tris[3] = 5 + triangles.Count;
            tris[4] = 4 + triangles.Count;
            tris[5] = 3 + triangles.Count;

            //Add all the verts.
            for (int x = 0; x < q1verts.Length; x++)
                vertices.Add(q1verts[x]);

            for (int y = 0; y < tris.Length; y++)
                triangles.Add(tris[y]);

            Vector3[] q2Verts = new Vector3[6];

            q2Verts[0] = Vector3.zero + position;
            q2Verts[1] = Vector3.zero + Vector3.down + position;
            q2Verts[2] = Vector3.zero + Vector3.down + Vector3.forward + position;

            q2Verts[3] = Vector3.zero + Vector3.forward + position;
            q2Verts[4] = Vector3.zero + position;
            q2Verts[5] = Vector3.zero + Vector3.down + Vector3.forward + position;

            int[] tris2 = new int[6];

            tris2[0] = 0 + triangles.Count;
            tris2[1] = 1 + triangles.Count;
            tris2[2] = 2 + triangles.Count;

            tris2[3] = 3 + triangles.Count;
            tris2[4] = 4 + triangles.Count;
            tris2[5] = 5 + triangles.Count;

            //Add all the verts.
            for (int x = 0; x < q2Verts.Length; x++)
                vertices.Add(q2Verts[x]);

            for (int y = 0; y < tris2.Length; y++)
                triangles.Add(tris2[y]);

            Vector3[] q3Verts = new Vector3[6];

            q3Verts[0] = Vector3.zero + position;
            q3Verts[1] = Vector3.zero + Vector3.down + position;
            q3Verts[2] = Vector3.right + Vector3.down + position;

            q3Verts[3] = Vector3.right + position;
            q3Verts[4] = Vector3.zero + position;
            q3Verts[5] = Vector3.right + Vector3.down + position;

            int[] tris3 = new int[6];

            tris3[0] = 2 + triangles.Count;
            tris3[1] = 1 + triangles.Count;
            tris3[2] = 0 + triangles.Count;

            tris3[3] = 5 + triangles.Count;
            tris3[4] = 4 + triangles.Count;
            tris3[5] = 3 + triangles.Count;

            //Add all the verts.
            for (int x = 0; x < q3Verts.Length; x++)
                vertices.Add(q3Verts[x]);

            for (int y = 0; y < tris3.Length; y++)
                triangles.Add(tris3[y]);

            Vector3[] q4Verts = new Vector3[6];

            q4Verts[0] = Vector3.forward + position;
            q4Verts[1] = Vector3.forward + Vector3.down + position;
            q4Verts[2] = Vector3.forward + Vector3.right + Vector3.down + position;

            q4Verts[3] = Vector3.forward + Vector3.right + position;
            q4Verts[4] = Vector3.forward + position;
            q4Verts[5] = Vector3.forward + Vector3.right + Vector3.down + position;

            int[] tris4 = new int[6];

            tris4[0] = 0 + triangles.Count;
            tris4[1] = 1 + triangles.Count;
            tris4[2] = 2 + triangles.Count;

            tris4[3] = 3 + triangles.Count;
            tris4[4] = 4 + triangles.Count;
            tris4[5] = 5 + triangles.Count;

            //Add all the verts.
            for (int x = 0; x < q4Verts.Length; x++)
                vertices.Add(q4Verts[x]);

            for (int y = 0; y < tris4.Length; y++)
                triangles.Add(tris4[y]);
        }

        public static void GetQuadValues(Vector3 position, ref List<Vector3> vertices, ref List<int> triangles, bool clockwise = true)
        {
            Vector3[] verts = new Vector3[6];

            verts[0] = Vector3.zero + position;
            verts[1] = Vector3.right + position;
            verts[2] = Vector3.right + Vector3.forward + position;

            verts[3] = Vector3.forward + position;
            verts[4] = Vector3.zero + position;
            verts[5] = Vector3.right + Vector3.forward + position;

            int[] tris = new int[6];

            if(clockwise)
            {
                tris[0] = 2 + triangles.Count;
                tris[1] = 1 + triangles.Count;
                tris[2] = 0 + triangles.Count;

                tris[3] = 5 + triangles.Count;
                tris[4] = 4 + triangles.Count;
                tris[5] = 3 + triangles.Count;
            }
            else
            {
                tris[0] = 0 + triangles.Count;
                tris[1] = 1 + triangles.Count;
                tris[2] = 2 + triangles.Count;

                tris[3] = 3 + triangles.Count;
                tris[4] = 4 + triangles.Count;
                tris[5] = 5 + triangles.Count;
            }

            for (int x = 0; x < verts.Length; x++)
                vertices.Add(verts[x]);

            for (int y = 0; y < tris.Length; y++)
                triangles.Add(tris[y]);
        }
    }
}