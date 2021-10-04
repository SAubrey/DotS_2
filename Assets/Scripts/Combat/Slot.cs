using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class Slot : MonoBehaviour
{
    public SpriteRenderer sprite_unit;
    protected Unit unit;
    protected Camera cam;

    // --VISUAL-- 
    public TextMeshProUGUI name_T;

    public static readonly Color selected_color = new Color(1, 1, 1, .8f);
    public static readonly Color unselected_color = new Color(1, 1, 1, .1f);
    private static readonly Color healthbar_fill_color = new Color(.8f, .1f, .1f, .45f);
    private static readonly Color healthbar_injured_fill_color = new Color(1f, .5f, 0f, .45f);
    private static readonly Color staminabar_fill_color = new Color(.1f, .8f, .1f, .45f);
    private static readonly Color defensebar_fill_color = new Color(.1f, .1f, .8f, .45f);
    private static readonly Color statbar_bg_color = new Color(.4f, .4f, .4f, .3f);
    private static readonly Color equipment_text_color = new Color(1f, .67f, .32f, 1f);
    public Slider healthbar;
    public Canvas info_canv;
    public Image healthbar_bg, healthbar_fill;
    public GameObject healthbar_obj;
    public UnityEngine.Experimental.Rendering.Universal.Light2D light2d;
    public Sprite range_icon, melee_icon, shield_icon, armor_icon;

    public TextMeshProUGUI attT, defT, hpT;
    //public Image attfgI, attbgI;
    //public Image deffgI, defbgI;
    //public Color attfgI_c, deffgI_c;

    [HideInInspector]
    public int col, row;
    public Group group;
    //public Button button;
    public AnimationPlayer animation_player;
    public GameObject animation_obj;

    ////// New
    public ParticleSystem dust_ps;
    public Transform slot_point_transform;
    public Deployment deployment;
    public GameObject melee_att_zone;
    public GameObject arrow_prefab;
    private bool rotating = false;

    void Awake()
    {
        cam = GameObject.Find("BattleCamera").GetComponent<Camera>();
        light2d.enabled = false;

        face_cam();
    }

    void Start()
    {
        healthbar_fill.color = healthbar_fill_color;
        healthbar_bg.color = statbar_bg_color;
        deployment.on_velocity_change += toggle_dust_ps;
        deployment.on_begin_rotation += begin_rotation;
        deployment.on_end_rotation += end_rotation;
        dust_ps.enableEmission = false;
    }

    public float move_timer = 0;
    void Update()
    {
        move();
        if (rotating)
        {
            gameObject.transform.rotation = deployment.gameObject.transform.rotation;
        }
    }

    public float smooth_speed = 0.125f;

    public void move()
    {
        // Slots hold units and smooth lerp towards the static slot points. Slots points are fixed in a grid in group. Thus, slots are not under the hierarchy of groups anymore.
        // Rotation, movement, and formation change will incur smooth movement. This also allows slots to move forward towards a destination.
        //float t = move_timer / duration;
        //t = t * t * (3f - 2f * t);
        if (slot_point_transform == null)
            return;
        //transform.position = Vector3(startPosition, endPosition, t);
        Vector3 v = Vector3.zero;
        Vector3 desired_pos = slot_point_transform.position;
        transform.position = Vector2.Lerp(transform.position, desired_pos, smooth_speed);
        //transform.position = Vector3.SmoothDamp(transform.position, desired_pos, ref v, .3f, 300f);
        //transform.position = StaticOperations.GetSmoothedNextPosition(transform.position, desired_pos, smooth_speed);
    }

    private void begin_rotation(float v)
    {
        rotating = true;
    }

    private void end_rotation(float v)
    {
        rotating = false;
    }

    public virtual bool fill(Unit u)
    {
        if (u == null)
            return false;
        set_unit(u);
        init_UI(u);
        //unit_img.color = Color.white;
        sprite_unit.color = Color.white;
        sprite_unit.sprite = BatLoader.I.get_unit_img(u, 0);
        if (u.is_playerunit)
            toggle_light(true);
        return true;
    }

    // Full slots below the removed slot will be moved up if validated.
    public Unit empty(bool validate = true)
    {
        Unit removed_unit = unit;
        if (unit != null)
        {
            unit.set_slot(null);
            unit = null;
        }

        update_unit_img(0);
        set_active_UI(false);
        set_nameT("");
        sprite_unit.color = Color.clear;
        dust_ps.enableEmission = false;
        toggle_light(false);
        if (validate)
            group.validate_unit_order();
        return removed_unit;
    }

    protected virtual void set_unit(Unit u)
    {
        if (u == null)
            return;
        if (u.is_playerunit)
        {
            unit = u as PlayerUnit;
        }
        else if (u.is_enemy)
        {
            unit = u as Enemy;
        }
        unit.set_slot(this);
    }

    public Unit get_unit()
    {
        return unit;
    }

    public void range_attack(LayerMask mask, Vector2 target_pos)
    {
        GameObject a = Instantiate(arrow_prefab, gameObject.transform);
        a.gameObject.transform.position = gameObject.transform.position;
        Arrow a_script = a.GetComponent<Arrow>();
        Vector2 launch_pos = new Vector2(gameObject.transform.position.x,
                                        gameObject.transform.position.y);
        a_script.init(mask, launch_pos, target_pos);
    }

    public void init_UI(Unit u)
    {
        update_healthbar_color();
        show_equipment_boosts();
        set_nameT(unit.get_name());
        //update_unit_img(group.direction);
        set_active_UI(true);
        update_text_UI();
    }

    public void toggle_dust_ps(float velocity)
    {
        if (is_empty)
        {
            return;
        }
        dust_ps.enableEmission = velocity > 0;
    }

    // Updated when a boost is removed or applied,
    // or an attribute is activated or deactivated.
    public void update_text_UI()
    {
        update_healthbar();
    }

    public void update_healthbar()
    {
        healthbar.maxValue = get_unit().get_dynamic_max_health();
        healthbar.value = get_unit().health;
        update_healthbar_color();
        hpT.text = build_health_string(get_unit().health, 0);
    }

    public string build_health_string(float hp, float preview_damage)
    {
        //float hp = get_unit().health; // This will already include the boost but not the bonus.
        float hp_boost = get_unit().get_stat_buff(Unit.HEALTH)
            + get_unit().get_bonus_health();

        string str = (hp + get_unit().get_bonus_from_equipment(Unit.HEALTH)).ToString();
        if (preview_damage > 0)
            str += " (-" + preview_damage.ToString() + ")";
        if (hp_boost > 0)
            str += " (+" + hp_boost.ToString() + ")";
        return str;
    }

    private void update_healthbar_color()
    {
        //healthbar.fillRect.GetComponent<Image>().color = new Color(1, red, red, 1);
        healthbar.fillRect.GetComponent<Image>().color = healthbar_fill_color;
    }

    public void set_active_UI(bool state)
    {
        healthbar_obj.SetActive(state);
    }

    private void show_equipment_boosts()
    {
        if (unit.is_enemy)
            return;

        EquipmentInventory ei = Controller.I.get_disc(get_punit().owner_ID).equipment_inventory;
        hpT.color = ei.get_stat_boost_amount(unit.get_ID(), Unit.HEALTH) > 0 ?
            equipment_text_color : Color.white;
        defT.color = ei.get_stat_boost_amount(unit.get_ID(), Unit.DEFENSE) > 0 ?
            equipment_text_color : Color.white;
        attT.color = ei.get_stat_boost_amount(unit.get_ID(), Unit.ATTACK) > 0 ?
            equipment_text_color : Color.white;
    }

    public void play_animation(string anim)
    {
        animation_player.play(anim);
    }

    private void update_unit_img(int dir)
    {
        sprite_unit.color = Color.white;
        sprite_unit.sprite = BatLoader.I.get_unit_img(unit, dir);
        if (sprite_unit.sprite == null)
            sprite_unit.color = Color.clear;
        rotate_to_direction(dir);
    }

    public void rotate_to_direction(int direction)
    {
        if (has_unit)
        {
            sprite_unit.sprite = BatLoader.I.get_unit_img(unit, direction);
        }
        set_animation_depth(direction);
        //unit_img.transform.forward *= -1;
        // Used if only using forward/back images to mirror them for right/left.
        /*
       if (direction == 0 || direction == 180) {
           unit_img.transform.rotation = Quaternion.AngleAxis(0, Vector3.up);
       } else 
           unit_img.transform.rotation = Quaternion.AngleAxis(180, Vector3.up);
       */
        face_cam();
    }

    public void face_cam()
    {
        sprite_unit.transform.LookAt(cam.transform);
        info_canv.transform.LookAt(cam.transform);
        info_canv.transform.forward *= -1;
    }

    public void set_animation_depth(int direction)
    {
        if (direction == Group.UP || direction == Group.RIGHT)
        {
            animation_obj.transform.SetAsFirstSibling();
        }
        else
        {
            animation_obj.transform.SetAsLastSibling();
        }
    }

    private void toggle_light(bool state)
    {
        light2d.enabled = state;
    }
    // ---End GRAPHICAL--- 

    public bool has_punit
    {
        get
        {
            if (unit == null) return false;
            if (unit.is_playerunit) return true;
            return false;
        }
    }

    public bool has_enemy
    {
        get
        {
            if (unit == null) return false;
            if (unit.is_enemy) return true;
            return false;
        }
    }

    public bool has_unit
    {
        get { return unit != null ? true : false; }
    }

    public bool is_empty
    {
        get { return unit == null ? true : false; }
    }

    public PlayerUnit get_punit()
    {
        return has_punit ? unit as PlayerUnit : null;
    }

    public Enemy get_enemy()
    {
        return has_enemy ? unit as Enemy : null;
    }

    private void set_nameT(string txt)
    {
        name_T.text = txt;
    }

    public Group get_group()
    {
        return group;
    }
}
