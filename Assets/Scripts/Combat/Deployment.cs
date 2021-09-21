using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public abstract class Deployment : MonoBehaviour {
    protected static Vector2 VEC_UP = new Vector2(0, 1f);
    protected static Vector2 VEC_DOWN = new Vector2(0, -1f);
    protected static Vector2 VEC_RIGHT = new Vector2(1f, 0);
    protected static Vector2 VEC_LEFT = new Vector2(-1f, 0);

    public bool isPlayer { get; protected set; } = false;
    public bool isEnemy { get; protected set; } = false;

    protected float MAX_VEL = 250;
    private Vector3 rotation_left = new Vector3(0, 0, -1.1f);
    private Vector3 rotation_right = new Vector3(0, 0, 1.1f);
    protected bool invulnerable;

    // Stamina
    public Slider staminabar;
    public float MAX_STAMINA = 100f;
    private float _stamina = 100f;
    public float stamina {
        get { return _stamina; }
        set { _stamina = Mathf.Max(value, 0f);
            if (staminabar != null) {
                update_staminabar(); 
            }
        }
    }
    private Timer stam_regen_timer = new Timer(.025f);
    protected float stam_regen_amount = .05f;
    protected float stam_dash_cost = 30f;
    protected float stam_attack_cost = 20f;
    protected float stam_range_cost = 20f;
    public bool stunned = false;


    protected float MAX_HP = 100f;
    public Slider healthbar;
    private Color healthbarFillColor = new Color(1, 1, 1, .8f);
    public Color healthbarBGColor;
    public event Action<Vector2> on_position_change;
    public event Action<float> on_velocity_change;
    public event Action<float> on_begin_rotation;
    public event Action<float> on_end_rotation;

    Vector2 position;
    public float unit_img_dir = Group.UP;

    protected List<Group[]> zones = new List<Group[]>();
    public LayerMask target_layer_mask;

    public abstract void place_unit(Unit unit);

    protected virtual void animate_slot_attack(bool melee) { }

    protected virtual void Update() {
        update_timers(Time.deltaTime);
        if (stam_regen_timer.increase(Time.deltaTime) && stamina <= MAX_STAMINA - stam_regen_amount) {
            regen_stamina(stam_regen_amount);
        }
    }
    
    public void update_timers(float dt) {
        foreach (Group[] gs in zones) {
            foreach (Group g in gs) {
                foreach (Slot s in g.slots) {
                    if (s.has_unit)
                        s.get_unit().update_timers(dt);
                } 
            }
        }    
    }

    protected void set_position(Vector2 pos) {
        position = pos;
        if (on_position_change != null)
            on_position_change(position);
    }
    
    public void move(Vector2 movement) {
        movement.Normalize();
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

    public void melee_attack(Group[] zone) {
        stamina -= stam_attack_cost;
        foreach (Group g in zone) {
            for (int i = 0; i < 3; i++) {
                if (g.slots[i].has_unit) {
                    g.slots[i].get_unit().melee_attack(target_layer_mask);
                }
            }
        }
    }

    
    public void range_attack(Group[] zone, Vector3 target_pos) {
        stamina -= stam_range_cost;
        foreach (Group g in zone) {
            for (int i = 0; i < 3; i++) {
                if (g.slots[i].has_unit) {
                    Unit u = g.slots[i].get_unit();
                    u.range_attack(target_layer_mask, target_pos);
                }
            }
        }
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
        //stamina += StaticOperations.GetAdjustedIncrease(stamina, amount, MAX_STAMINA);
        stamina += amount;
    }

    public void update_staminabar() {
        staminabar.maxValue = MAX_STAMINA;
        staminabar.value = stamina;

        float green = ((float)stamina / (float)MAX_STAMINA);
        staminabar.fillRect.GetComponent<Image>().color = new Color(.1f, .65f, .1f, green);
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
            foreach (Group[] zone in zones) {
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
        foreach (Group[] zone in zones) {
            foreach (Group g in zone) {
                g.rotate(dir);
            }
        }
    }

    protected void face_slots_to_camera() {
        foreach (Group[] zone in zones) {
            foreach (Group g in zone) {
                foreach (Slot s in g.slots) {
                    s.face_cam();
                }
            }
        }
    }

    protected void toggle_slot_dust_ps(int active) {
        foreach (Group[] zone in zones) {
            foreach (Group g in zone) {
                foreach (Slot s in g.slots) {
                    s.toggle_dust_ps(active);
                }
            }
        }
    }

    /*protected void play_animation_on_forward_slots(Group[] gs, string anim) {
        foreach (Group g in gs) {
            for (int i = 0; i < 3; i++) {
                if (g.slots[i].has_unit) {
                    g.slots[i].play_animation(anim);
                }
            }
        }
    }*/

    protected void trigger_begin_rotation_event() {
        on_begin_rotation(MAX_VEL);
    }

    protected void trigger_end_rotation_event() {
        on_end_rotation(MAX_VEL);
    }

    public virtual void delete() {
        GameObject.Destroy(gameObject);
    }
}
