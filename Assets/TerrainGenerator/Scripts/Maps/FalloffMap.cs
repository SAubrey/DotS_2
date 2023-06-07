using Assets.Scripts.MapGenerator.Abstract;
using UnityEngine;

namespace Assets.Scripts.MapGenerator.Maps
{
    public class FalloffMap : IMap
    {
        public float FalloffDirection;
        public float FalloffRange;
        public int Size;

        public float[,] Generate()
        {
            float[,] map = new float[Size, Size];

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    float x = i / (float)Size * 2 - 1;
                    float y = j / (float)Size * 2 - 1;

                    float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                    map[i, j] = Evaluate(value);
                }
            }

            return map;
        }

        float Evaluate(float value)
        {
            return Mathf.Pow(value, FalloffDirection) / (Mathf.Pow(value, FalloffDirection) + Mathf.Pow(FalloffRange - FalloffRange * value, FalloffDirection));
        }
    }
}
