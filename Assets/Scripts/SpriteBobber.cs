using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBobber : MonoBehaviour
{
    public const float FreqIdle = 1f;
    public const float FreqWalk = 5f;
    public const float FreqRun = 20f;
    public float BobPower = 0.1f;
    public float RandomOffset = 1f;
    public Slot Slot;
    private Deployment Deployment;
    private Vector2 DefaultPos;
    private float Velocity;

    void Start()
    {
        DefaultPos = transform.localPosition;
        RandomOffset = UnityEngine.Random.Range(0, 100);
        Deployment = Slot.Deployment;
        Slot.OnVelocityChange += SetVelocity;
    }

    void Update()
    {
        BobStep();
    }

    private void SetVelocity(Vector2 v)
    {
        Velocity = v.magnitude;
    }

    private float GetBobFrequency(float v) {
        float vm = v / Deployment.MaxSpeed;
        if (vm > .6f) 
        {
            return FreqRun;
        }
        if (vm > .2f) 
        {
            return FreqWalk;
        }
        return FreqIdle;
    }

    private void BobStep()
    {
        float bp = BobPower;
        if (Velocity > 2f)
        {
            bp += 0.5f;
        }

        // High velocity creates shorter arc lengths, or faster bob frequency.
        // Time determines position along wave, animating the sprite.
        // Bob power determines amplitude (height).
        // Note: rapid change in velocity will cause jittering 
        float sin_y = Mathf.Sin(GetBobFrequency(Velocity) * ((Time.time) + RandomOffset)) * bp;
        //float sin_y = Mathf.Sin(velocity * ((Time.time) + random_offset)) * bp;
        //sin_y = Mathf.Lerp(transform.localPosition.y, sin_y, smoothe);
        transform.localPosition = new Vector3(DefaultPos.x,
            DefaultPos.y + sin_y, 0);
    }
}
