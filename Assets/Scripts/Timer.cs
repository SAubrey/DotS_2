using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    public bool Finished { get; protected set; } = false;
    private float Counter = 0;
    private float Threshhold = 0;
    private bool Randomize = false;
    private float RandMin, RandMax;
    // Timers that yield after meeting their threshhold will have to be reset to continue counting.
    private bool YieldAfter = false; 

    public Timer(float threshhold, bool yieldAfter=false, float randMin = 0, float randMax = 0)
    {
        this.Threshhold = threshhold;
        this.YieldAfter = yieldAfter;
        if (randMax > 0)
        {
            Randomize = true;
            this.RandMin = randMin;
            this.RandMax = randMax;
            RandomizeThresh(randMin, randMax);
        }
    }

    public bool Increase(float deltaTime)
    {
        if (Finished)
            return false;
        Counter += deltaTime;
        if (Counter >= Threshhold)
        {
            Counter = 0;
            if (Randomize)
            {
                RandomizeThresh(RandMin, RandMax);
            }
            if (YieldAfter)
                Finished = true;
            return true;
        }
        return false;
    }

    public void Reset()
    {
        Counter = 0;
        Finished = false;
    }

    private void RandomizeThresh(float min, float max)
    {
        Threshhold = Random.Range(min, max);
    }
}
