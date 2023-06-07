using Assets.Scripts.MapGenerator.Maps;
using UnityEngine;

namespace Assets.Scripts.MapGenerator.Generators
{
    public class HeightsGenerator : MonoBehaviour, IGenerator
    {
        public int Width = 129;
        public int Height = 129;
        public int Depth = 10;
        public int Octaves = 4;
        public float Scale = 50f;
        public float Lacunarity = 2f;
        [Range(0, 1)]
        public float Persistance = 0.5f;
        public AnimationCurve HeightCurve;
        public float Offset = 100f;
        public float FalloffDirection = 3f;
        public float FalloffRange = 3f;
        public bool UseFalloffMap;
        public bool Randomize;
        public bool AutoUpdate;

        private void OnValidate()
        {
            if (Width < 1)
            {
                Width = 1;
            }
            if (Height < 1)
            {
                Height = 1;
            }
            if (Lacunarity < 1)
            {
                Lacunarity = 1;
            }
            if (Octaves < 0)
            {
                Octaves = 0;
            }
            if (Scale <= 0)
            {
                Scale = 0.0001f;
            }
        }

        public void Generate()
        {
            if (Randomize)
            {
                Offset = Random.Range(0f, 9999f);
            }

            TerrainData terrainData = Terrain.activeTerrain.terrainData;

            terrainData.heightmapResolution = Width + 1;
            terrainData.alphamapResolution = Width;
            terrainData.SetDetailResolution(Width, 8);

            terrainData.size = new Vector3(Width, Depth, Height);

            float[,] falloff = null;
            if (UseFalloffMap)
            {
                falloff = new FalloffMap
                {
                    FalloffDirection = FalloffDirection,
                    FalloffRange = FalloffRange,
                    Size = Width
                }.Generate();
            }

            float[,] noiseMap = GenerateNoise(falloff);
            terrainData.SetHeights(0, 0, noiseMap);
        }

        float[,] GenerateNoise(float[,] falloffMap = null)
        {
            AnimationCurve heightCurve = new AnimationCurve(HeightCurve.keys);

            float maxLocalNoiseHeight;
            float minLocalNoiseHeight;

            float[,] noiseMap = new PerlinMap()
            {
                Size = Width,
                Octaves = Octaves,
                Scale = Scale,
                Offset = Offset,
                Persistance = Persistance,
                Lacunarity = Lacunarity
            }.Generate(out maxLocalNoiseHeight, out minLocalNoiseHeight);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var lerp = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);

                    if (falloffMap != null)
                    {
                        lerp -= falloffMap[x, y];
                    }

                    if (lerp >= 0)
                    {
                        noiseMap[x, y] = heightCurve.Evaluate(lerp);
                    }
                    else
                    {
                        noiseMap[x, y] = 0;
                    }
                }
            }

            return noiseMap;
        }

        public void Clear()
        {
            TerrainData terrainData = Terrain.activeTerrain.terrainData;
            terrainData.SetHeights(0, 0, new float[Width, Height]);
        }
    }
}