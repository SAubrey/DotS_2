using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBobberUI : SpriteBobber
{

    void Start()
    {
        RandomOffset = UnityEngine.Random.Range(0, 100);
        BobPower = .01f;
    }
}
