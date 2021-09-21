using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer {
    public float counter = 0;
    public float threshhold = 0;

    public Timer(float threshhold) {
        this.threshhold = threshhold;
    }

    public bool increase(float delta_time) {
        counter += delta_time;
        if (counter >= threshhold) {
            counter = 0;
            return true;
        }
        return false;
    }

    public void reset() {
        counter = 0;
    }
}
