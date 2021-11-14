using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{
    public float counter = 0;
    public float threshhold = 0;
    public bool finished { get; protected set; } = false;
    private bool randomize = false;
    private float randMin, randMax;
    private bool yieldAfter = false; 


    public Timer(float threshhold, bool yieldAfter=false, float randMin = 0, float randMax = 0)
    {
        this.threshhold = threshhold;
        this.yieldAfter = yieldAfter;
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
        // Do not continue counting after time met. Assures time starts from zero.
        if (finished && yieldAfter)
        {
            counter += deltaTime;
        }    
        if (counter >= threshhold)
        {
            counter = 0;
            finished = true;
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
