using Assets.Scripts.MapGenerator.Maps;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MapGenerator.Generators
{
    public class GrassGenerator : MonoBehaviour, IGenerator
    {
        public int Octaves = 4;
        public float Scale = 40;
        public float Lacunarity = 2f;
        [Range(0, 1)]
        public float Persistance = 0.5f;
        public float Offset = 100f;
        public float MinLevel = 0;
        public float MaxLevel = 100;
        [Range(0, 90)]
        public float MaxSteepness = 70;
        [Range(-1, 1)]
        public float IslandsSize = 0;
        [Range(1, 100)]
        public int Density = 10;
        public bool Randomize;
        public bool AutoUpdate;

        public List<Texture2D> GrassTextures;

        public void Generate()
        {
            if (Randomize)
            {
                Offset = Random.Range(0f, 9999f);
            }

            List<DetailPrototype> detailPrototypes = new List<DetailPrototype>();
            foreach (var g in GrassTextures)
            {
                
                detailPrototypes.Add( new DetailPrototype() { prototypeTexture = g });
            }

            TerrainData terrainData = Terrain.activeTerrain.terrainData;
            terrainData.wavingGrassTint = Color.white;
            terrainData.detailPrototypes = detailPrototypes.ToArray();

            float[,] noiseMap = new PerlinMap()
            {
                Size = terrainData.detailWidth,
                Octaves = Octaves,
                Scale = Scale,
                Offset = Offset,
                Persistance = Persistance,
                Lacunarity = Lacunarity
            }.Generate();

            for (int i = 0; i < terrainData.detailPrototypes.Length; i++)
            {
                int[,] detailLayer = terrainData.GetDetailLayer(0, 0, terrainData.detailWidth, terrainData.detailHeight, i);

                for (int x = 0; x < terrainData.alphamapWidth; x++)
                {
                    for (int y = 0; y < terrainData.alphamapHeight; y++)
                    {
                        float height = terrainData.GetHeight(x, y);
                        float xScaled = (x + Random.Range(-1f, 1f)) / terrainData.alphamapWidth;
                        float yScaled = (y + Random.Range(-1f, 1f)) / terrainData.alphamapHeight;
                        float steepness = terrainData.GetSteepness(xScaled, yScaled);

                        if (noiseMap[x, y] < IslandsSize && steepness < MaxSteepness && height > MinLevel && height < MaxLevel)
                        {
                            detailLayer[x, y] = Density;
                        }
                        else
                        {
                            detailLayer[x, y] = 0;
                        }
                    }
                }

                terrainData.SetDetailLayer(0, 0, i, detailLayer);
            }
            
        }

        public void Clear()
        {
            Terrain.activeTerrain.terrainData.detailPrototypes = null;
        }
    }
}
