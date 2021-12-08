using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    public bool Finished { get; protected set; } = false;
    private float Time = 0;
    private float Threshold = 0;
    private bool Randomize = false;
    private float RandMinThresh, RandMaxThresh;
    // Timers that yield after meeting their threshhold will have to be reset to continue counting.
    private bool YieldAfter = false; 

    public Timer(float threshhold, bool yieldAfter=false, float randMin = 0, float randMax = 0)
    {
        this.Threshold = threshhold;
        this.YieldAfter = yieldAfter;
        if (randMax > 0)
        {
            Randomize = true;
            this.RandMinThresh = randMin;
            this.RandMaxThresh = randMax;
            RandomizeThresh(randMin, randMax);
        }
    }

    public bool Increase(float deltaTime)
    {
        if (Finished)
            return false;
        Time += deltaTime;
        if (Time >= Threshold)
        {
            Time = 0;
            if (Randomize)
            {
                RandomizeThresh(RandMinThresh, RandMaxThresh);
            }
            if (YieldAfter)
                Finished = true;
            return true;
        }
        return false;
    }

    public void Reset()
    {
        Time = 0;
        Finished = false;
    }

    private void RandomizeThresh(float min, float max)
    {
        Threshold = Random.Range(min, max);
    }
}
