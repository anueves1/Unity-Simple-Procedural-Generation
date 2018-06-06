using System.Collections.Generic;
using UnityEngine;

namespace SurvivalKit.ProceduralGeneration
{
    public class VoxelMeshCreator
    {
        public static List<Chunk> GenerateVoxelTerrain(Biome info, TerrainInfo tInfo)
        {
            //Generate the terrain.
            List<Chunk> chunks = GenerateVoxelGrid(info, tInfo, true);

            //Go trough each chunk.
            for (int v = 0; v < chunks.Count; v++)
            {
                //Get the current chunk.
                Chunk currentChunk = chunks[v];

                //Get the vertices.
                List<Vector3> vertices = currentChunk.Vertices;

                //Loop trough them.
                for (int i = 0; i < vertices.Count; i++)
                {
                    //Get the current vertice.
                    Vector3 currentVert = vertices[i];

                    //Get the x coordinate.
                    float xCoord = currentVert.x + (tInfo.DetailLevel * currentChunk.Position.x);
                    //Get the y coordinate.
                    float zCoord = currentVert.z + (tInfo.DetailLevel * currentChunk.Position.y);

                    //Calculate the x position variation.
                    float x = xCoord + tInfo.Seed.x;
                    //Calculate the y position variation.
                    float z = zCoord + tInfo.Seed.y;

                    //Assign the needed perlin value.
                    currentVert.y = GetFractal(x, z, info, tInfo);

                    //Set back the vert.
                    vertices[i] = currentVert;
                }

                //Set the new vertices.
                chunks[v].Set(vertices);
            }

            return chunks;
        }

        private static float GetFractal(float x, float z, Biome info, TerrainInfo tInfo)
        {
            float perlinValue = 0f;

            float maxAmplitude = 0f;
            float amplitude = 1f;

            float frequency = info.Lacunarity;

            //Go trough every octave.
            for (int o = 0; o < info.Octaves; o++)
            {
                //Get the final noise coordinates.
                float xFreq = x * frequency;
                float zFreq = z * frequency;

                //Get the needed noise value.
                float nValue = tInfo.Noise.Get(xFreq, zFreq);

                //Get the value after redistribution.
                nValue = info.Redistribution.Get(nValue);

                //Make terraces.
                nValue = Mathf.Round(nValue * info.TerraceValue) / info.TerraceValue;
     
                //Add the noise.
                perlinValue += nValue * amplitude;

                //Add the current amplitude to the max.
                maxAmplitude += amplitude;

                //Lower the amplitude as octaves get higher.
                amplitude *= (-info.Persistance);

                //Double the frequency so the spaces between high points are further apart.
                frequency *= info.Lacunarity;
            }

            //Average the perlin value by the amplitude.
            perlinValue /= maxAmplitude;

            return perlinValue;
        }

        public static List<Chunk> GenerateVoxelGrid(Biome info, TerrainInfo tInfo, bool recalculateNormals = false)
        {
            //Make a list to hold the chunks.
            List<Chunk> chunks = new List<Chunk>();

            //Loop for every chunk.
            for (int y = 0; y < tInfo.ChunkCount; y++)
            {
                for (int x = 0; x < tInfo.ChunkCount; x++)
                {
                    //Create an object for the chunk
                    GameObject chunkObject = new GameObject("Chunk " + "(" + x + ", " + y + ")");

                    //Parent it to this object.
                    chunkObject.transform.SetParent(tInfo.ChunkParent);

                    //Calculate the new chunk's position,
                    chunkObject.transform.localPosition = new Vector3(x * tInfo.DetailLevel, 0f, y * tInfo.DetailLevel);

                    //Fix the scale.
                    chunkObject.transform.localScale = Vector3.one;

                    //Add the chunk component.
                    Chunk c = chunkObject.AddComponent<Chunk>();

                    //Make sure the chunk knows its position.
                    c.Position = new Vector2(x, y);

                    //Setup the chunk.
                    c.Setup(info.ChunkMaterial);

                    //Make lists for the chunk's vertices and triangles.
                    List<Vector3> vertices = new List<Vector3>();
                    List<int> triangles = new List<int>();

                    //Go trough both x and y detail.
                    for (int dX = 0; dX < tInfo.DetailLevel; dX++)
                    {
                        //Generate one quad for each place we want it to be at.
                        for (int dY = 0; dY < tInfo.DetailLevel; dY++)
                            MeshCreator.GetQuadValues(new Vector3(dX, 0, dY), ref vertices, ref triangles);
                    }

                    //Give the chunk the data it needs.
                    c.Set(vertices, triangles);

                    //Add the chunk to the list.
                    chunks.Add(c);
                }
            }

            //Return the chunks.
            return chunks;
        }
    }
}