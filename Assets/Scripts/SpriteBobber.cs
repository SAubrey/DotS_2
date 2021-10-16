using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBobber : MonoBehaviour
{
    public const float FREQ_IDLE = 1f;
    public const float FREQ_WALK = 5f;
    public const float FREQ_RUN = 20f;
    public float bob_power = 0.1f;
    public float random_offset = 1f;
    private float velocity;
    private Deployment deployment;
    public Slot slot;
    private Vector2 default_pos;

    void Start()
    {
        default_pos = transform.localPosition;
        random_offset = UnityEngine.Random.Range(0, 100);
        deployment = slot.deployment;
        slot.on_velocity_change += set_velocity;
    }

    void Update()
    {
        bob_step();
    }

    private void set_velocity(Vector2 v)
    {
        velocity = v.magnitude;
    }

    private float get_bob_frequency(float v) {
        float vm = v / deployment.MAX_VELOCITY;
        if (vm > .6f) 
        {
            return FREQ_RUN;
        }
        if (vm > .2f) 
        {
            return FREQ_WALK;
        }
        return FREQ_IDLE;
    }

    private void bob_step()
    {
        float bp = bob_power;
        if (velocity > 2f)
        {
            bp += 0.5f;
        }

        // High velocity creates shorter arc lengths, or faster bob frequency.
        // Time determines position along wave, animating the sprite.
        // Bob power determines amplitude (height).
        // Note: rapid change in velocity will cause jittering 
        float sin_y = Mathf.Sin(get_bob_frequency(velocity) * ((Time.time) + random_offset)) * bp;
        //float sin_y = Mathf.Sin(velocity * ((Time.time) + random_offset)) * bp;
        //sin_y = Mathf.Lerp(transform.localPosition.y, sin_y, smoothe);
        transform.localPosition = new Vector3(default_pos.x,
            default_pos.y + sin_y, 0);
    }
}
