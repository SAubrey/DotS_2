using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBobbing : MonoBehaviour {
    //public Image image;
    public float bob_power = 0.01f;
    public float random_offset = 1f;
    private float velocity = 0;
    private Deployment deployment;
    public Slot slot;

    void Start() {
        random_offset = UnityEngine.Random.Range(0, 100);
        deployment = slot.deployment;
        deployment.on_velocity_change += set_velocity;
        deployment.on_begin_rotation += set_velocity;
        deployment.on_end_rotation += set_velocity;
    }

    void Update() {
        bob_step();
    }

    private void set_velocity(float v) {
        velocity = v * 0.1f;
    }

    // Bobbing period should scale with velocity
    private void bob_step() {
        float bp = bob_power;
        if (velocity > 0) {
            bp += 0.6f;
        }
        transform.position = new Vector3(transform.position.x, 
            transform.position.y + (Mathf.Sin(Mathf.Max(velocity, 3f) * (Time.time + random_offset)) * bp),
            0);
    }
}
