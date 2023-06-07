using Assets.Scripts.MapGenerator.Abstract;
using UnityEngine;

namespace Assets.Scripts.MapGenerator.Maps
{
    public class PerlinMap : IMap
    {
        public int Size { get; set; }

        public int Octaves { get; set; }

        public float Scale { get; set; }
        public float Offset { get; set; }
        public float Persistance { get; set; }
        public float Lacunarity { get; set; }

        public float[,] Generate()
        {
            float maxLocalNoiseHeight;
            float minLocalNoiseHeight;
            return GenerateNoise(out maxLocalNoiseHeight, out minLocalNoiseHeight);
        }

        public float[,] Generate(out float maxLocalNoiseHeight, out float minLocalNoiseHeight)
        {
            return GenerateNoise(out maxLocalNoiseHeight, out minLocalNoiseHeight);
        }

        float[,] GenerateNoise(out float maxLocalNoiseHeight, out float minLocalNoiseHeight)
        {
            float[,] noiseMap = new float[Size, Size];

            Vector2[] octaveOffsets = new Vector2[Octaves];

            float maxPossibleHeight = 0;
            float amplitude = 1;
            float frequency = 1;

            for (int i = 0; i < Octaves; i++)
            {
                octaveOffsets[i] = new Vector2(Offset, Offset);

                maxPossibleHeight += amplitude;
                amplitude *= Persistance;
            }

            if (Scale <= 0)
            {
                Scale = 0.0001f;
            }

            maxLocalNoiseHeight = float.MinValue;
            minLocalNoiseHeight = float.MaxValue;

            float halfSize = Size / 2f;

            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < Octaves; i++)
                    {
                        float sampleX = (x - halfSize + octaveOffsets[i].x) / Scale * frequency;
                        float sampleY = (y - halfSize + octaveOffsets[i].y) / Scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= Persistance;
                        frequency *= Lacunarity;
                    }

                    if (noiseHeight > maxLocalNoiseHeight)
                    {
                        maxLocalNoiseHeight = noiseHeight;
                    }
                    else if (noiseHeight < minLocalNoiseHeight)
                    {
                        minLocalNoiseHeight = noiseHeight;
                    }
                    noiseMap[x, y] = noiseHeight;
                }
            }

            return noiseMap;
        }
    }
}
