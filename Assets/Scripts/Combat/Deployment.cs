using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public abstract class Deployment : MonoBehaviour
{
    
    protected static Vector2 VEC_UP = new Vector2(0, 1f);
    protected static Vector2 VEC_DOWN = new Vector2(0, -1f);
    protected static Vector2 VEC_RIGHT = new Vector2(1f, 0);
    protected static Vector2 VEC_LEFT = new Vector2(-1f, 0);


    public bool isPlayer { get; protected set; } = false;
    public bool isEnemy { get; protected set; } = false;

    protected float MAX_VEL = 250;
    protected float velocity = 0f;
    private Vector3 rotation_left = new Vector3(0, 0, -1.1f);
    private Vector3 rotation_right = new Vector3(0, 0, 1.1f);
    protected float MAX_HP = 100f;
    public bool showingDamage = false;
    protected bool invulnerable;

    // Stamina
    public Slider staminaBar;
    public float MAX_STAMINA = 100f;
    public float stamina;
    protected float stamRegenAmount = .25f;
    protected float stamDashCost = 30f;

    public Slider healthbar;
    private Color healthbarFillColor = new Color(1, 1, 1, .8f);
    public Color healthbarBGColor;
    public event Action<Vector2> on_position_change;
    public event Action<float> on_velocity_change;
    public event Action<float> on_begin_rotation;
    public event Action<float> on_end_rotation;

    Vector2 position;
    public float unit_img_dir = Group.UP;

    protected List<Group[]> groups = new List<Group[]>();

    protected void set_position(Vector2 pos) {
        position = pos;
        on_position_change(position);
    }
    

    public void move(Vector2 movement) {
        movement *= MAX_VEL * Time.deltaTime;
        if (movement.magnitude == 0) {
            on_velocity_change(0);
            return;
        }
        on_velocity_change(MAX_VEL);
        Vector2 current_pos = gameObject.transform.position;
        Vector2 new_pos = current_pos += movement;
        gameObject.transform.position = new_pos;
        set_position(new_pos);
    }

    public Group get_highest_empty_group(Group[] groups) {
        foreach (Group g in groups) {
            if (!g.is_full) {
                return g;
            }
        }
        return null;
    }
    
    public void regen_stamina(float amount) {
        stamina += StaticOperations.GetAdjustedIncrease(stamina, amount, MAX_STAMINA);
        update_stamina();
    }

    
    public void update_stamina() {
        staminaBar.maxValue = MAX_STAMINA;
        staminaBar.value = stamina;

        float green = ((float)stamina / (float)MAX_STAMINA);
        staminaBar.fillRect.GetComponent<Image>().color = new Color(.1f, .8f, .1f, green);
    }

    public void rotate(int dir) {
        if (dir < 0) {
            gameObject.transform.Rotate(rotation_right);
        } else {
            gameObject.transform.Rotate(rotation_left);
        }
        face_slots_to_camera();
        determine_unit_img_direction(gameObject.transform.rotation.eulerAngles);
    }

    private void determine_unit_img_direction(Vector3 dir) {
        if ((dir.z > 90 && dir.z < 270) && unit_img_dir == Group.UP) {
            flip_slot_imgs(Group.DOWN);
            unit_img_dir = Group.DOWN;
        } else if ((dir.z <= 90 || dir.z >= 270) && unit_img_dir == Group.DOWN) {
            flip_slot_imgs(Group.UP);
            unit_img_dir = Group.UP;
        }
    }
    
    public int hp {
        get { 
            int sum = 0;
            foreach (Group[] zone in groups) {
                foreach (Group g in zone) {
                    sum += g.sum_unit_health();
                }
            }
            return sum;
         }
    }

    public virtual void update_healthbar() {
        healthbar.value = hp;

        float red = ((float)hp / (float)100);
        healthbar.fillRect.GetComponent<Image>().color = new Color(1f, .1f, .1f, red);
    }

    
    protected void flip_slot_imgs(int dir) {
        foreach (Group[] zone in groups) {
            foreach (Group g in zone) {
                g.rotate(dir);
            }
        }
    }

    protected void face_slots_to_camera() {
        foreach (Group[] zone in groups) {
            foreach (Group g in zone) {
                foreach (Slot s in g.slots) {
                    s.face_cam();
                }
            }
        }
    }

    protected void toggle_slot_dust_ps(int active) {
        foreach (Group[] zone in groups) {
            foreach (Group g in zone) {
                foreach (Slot s in g.slots) {
                    s.toggle_dust_ps(active);
                }
            }
        }
    }

    protected void trigger_begin_rotation_event() {
        on_begin_rotation(MAX_VEL);
    }

    protected void trigger_end_rotation_event() {
        on_end_rotation(MAX_VEL);
    }
}
