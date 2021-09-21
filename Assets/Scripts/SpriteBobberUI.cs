using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBobberUI : SpriteBobber {

    void Start() {
        random_offset = UnityEngine.Random.Range(0, 100);   
        bob_power = .001f; 
    }
}
