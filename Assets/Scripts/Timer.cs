using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    public float counter = 0;
    public float threshhold = 0;
    private bool randomize = false;
    private float randMin, randMax;

    public Timer(float threshhold, float randMin = 0, float randMax = 0)
    {
        this.threshhold = threshhold;
        if (randMax > 0)
        {
            randomize = true;
            this.randMin = randMin;
            this.randMax = randMax;
            RandomizeThresh(randMin, randMax);
        }
    }

    public bool Increase(float deltaTime)
    {
        counter += deltaTime;
        if (counter >= threshhold)
        {
            counter = 0;
            if (randomize)
            {
                RandomizeThresh(randMin, randMax);
            }
            return true;
        }
        return false;
    }

    public void Reset()
    {
        counter = 0;
    }

    private void RandomizeThresh(float min, float max)
    {
        threshhold = Random.Range(min, max);
    }
}
