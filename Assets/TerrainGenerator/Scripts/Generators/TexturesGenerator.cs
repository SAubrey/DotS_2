using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.MapGenerator.Generators
{
    public class TexturesGenerator : MonoBehaviour, IGenerator
    {
        public List<_Texture> textures = new List<_Texture>();

        public void Generate()
        {
            if (textures == null)
            {
                throw new NullReferenceException("Textures list not setted");
            }

            TerrainData terrainData = Terrain.activeTerrain.terrainData;

            SplatPrototype[] splatPrototypes = new SplatPrototype[textures.Count];

            for (int i = 0; i < textures.Count; i++)
            {
                splatPrototypes[i] = new SplatPrototype() { texture = (Texture2D)textures[i].Texture, tileSize = textures[i].Tilesize };
            }

            terrainData.splatPrototypes = splatPrototypes;

            if (terrainData.alphamapResolution != terrainData.size.x)
            {
                Debug.LogError("terrainData.alphamapResolution must fit terrain size");
            }

            float[,,] splatmaps = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

            float terrainMaxHeight = terrainData.size.y;

            float x = 0.0f;
            while (x < terrainData.alphamapHeight)
            {
                float y = 0.0f;
                while (y < terrainData.alphamapWidth)
                {
                    float height = terrainData.GetHeight((int)x, (int)y);
                    float heightScaled = height / terrainMaxHeight;

                    float xS = x / terrainData.heightmapResolution;
                    float yS = y / terrainData.heightmapResolution;

                    float steepness = terrainData.GetSteepness(xS, yS);
                    float angleScaled = steepness / 90.0f;

                    for (int i = 0; i < terrainData.alphamapLayers; i++)
                    {
                        switch (textures[i].Type)
                        {
                            case (0):
                                if (i != 0)
                                {
                                    splatmaps[(int)y, (int)x, i] = textures[i].HeightCurve.Evaluate(heightScaled);
                                    for (int hi = 0; hi < i; hi++)
                                    {
                                        splatmaps[(int)y, (int)x, hi] *= (splatmaps[(int)y, (int)x, i] - 1) / -1;
                                    }
                                }
                                else
                                {
                                    splatmaps[(int)y, (int)x, i] = textures[i].HeightCurve.Evaluate(heightScaled);
                                }
                                break;
                            case (1):
                                splatmaps[(int)y, (int)x, i] = textures[i].AngleCurve.Evaluate(angleScaled);
                                for (int ai = 0; ai < i; ai++)
                                {
                                    splatmaps[(int)y, (int)x, ai] *= (splatmaps[(int)y, (int)x, i] - 1) / -1;
                                }
                                break;
                            default:
                                break;
                        }

                        if (splatmaps[(int)y, (int)x, i] > 1.0f) { splatmaps[(int)y, (int)x, i] = 1.0f; }
                    }
                    y++;
                }
                x++;
            }

            terrainData.SetAlphamaps(0, 0, splatmaps);
        }

        public void Clear()
        {
            textures = new List<_Texture>();
            Generate();
        }
    }

    public class _Texture
    {
        public Texture2D Texture { get; set; }
        public Color Color { get; set; }
        public Vector2 Tilesize = new Vector2(1, 1);
        public int Type { get; set; }
        public AnimationCurve HeightCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);
        public AnimationCurve AngleCurve = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);
    }
}
