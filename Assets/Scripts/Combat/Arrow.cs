using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    public LineRenderer lr;
    public LayerMask target_mask;
    private float gravity = 800f;
    private float velocity = 500f;
    private float vertical_velocity;
    private float duration;
    private Vector2 direction;
    private Vector2 shadow_pos;
    private bool in_flight = false;
    public SpriteRenderer shadow;
    public SpriteRenderer arrow;

    Timer t = new Timer(0.02f);
    public int p = 0;
    private float duration_counter = 0;

    public void init(LayerMask mask, Vector2 launch_pos, Vector2 target_pos)
    {
        target_mask = mask;

        direction = (target_pos - launch_pos).normalized;
        float distance = Vector2.Distance(launch_pos, target_pos);
        duration = distance / velocity;
        //Debug.Log("distance: " + distance + "DURATION " + duration);
        vertical_velocity = duration * (gravity / 2f) - (1f / duration);
        //Debug.Log("vertical v" + vertical_velocity);
        shadow_pos = launch_pos;
        in_flight = true;
        arrow.transform.up = direction;
        GetComponent<SpriteRenderer>().transform.LookAt(CamSwitcher.I.battle_cam.transform);

        t.Increase(0.09f);
    }

    void Update()
    {
        if (!in_flight)
            return;
        calc_shadow();

        if (t.Increase(Time.deltaTime))
        {
            update_arc();
        }
        duration_counter += Time.deltaTime;
        if (duration_counter > duration)
        {
            // Check hit
            die();
        }
    }

    private void update_arc()
    {
        lr.positionCount = p + 1;
        lr.SetPosition(p, gameObject.transform.position);
        p++;
    }

    float y;
    public void calc_shadow()
    {
        // https://stackoverflow.com/questions/38692488/how-to-calculate-initial-velocity-of-projectile-motion-by-a-given-distance-and-a
        // Use direction and distance to send linear 'shadow' to target. Projectile takes shadow's x and adjusts y by physics.
        //shadow_pos = Vector2.MoveTowards(shadow_pos, target_pos, velocity * Time.deltaTime);
        Vector2 movement = direction * velocity * Time.deltaTime;
        shadow_pos = shadow_pos + movement;
        shadow.transform.position = shadow_pos;

        y += vertical_velocity * Time.deltaTime;
        gameObject.transform.position = new Vector3(shadow_pos.x, shadow_pos.y + y, 0);
        vertical_velocity -= gravity * Time.deltaTime;
    }

    private void die()
    {
        GameObject.Destroy(gameObject);
    }
}
