using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class Slot : MonoBehaviour {
    public Image img, unit_img;
    private Unit unit;
    private Camera cam;

    // --VISUAL-- 
    public TextMeshProUGUI name_T;
    public Color dead, injured;
    
    public static readonly Color selected_color = new Color(1, 1, 1, .8f);
    public static readonly Color unselected_color = new Color(1, 1, 1, .1f);
    private static readonly Color healthbar_fill_color = new Color(.8f, .1f, .1f, .45f);
    private static readonly Color healthbar_injured_fill_color = new Color(1f, .5f, 0f, .45f);
    private static readonly Color staminabar_fill_color = new Color(.1f, .8f, .1f, .45f);
    private static readonly Color defensebar_fill_color = new Color(.1f, .1f, .8f, .45f);
    private static readonly Color statbar_bg_color = new Color(.4f, .4f, .4f, .3f);
    private static readonly Color equipment_text_color = new Color(1f, .67f, .32f, 1f);
    public Slider healthbar, defensebar;
    public Canvas info_canv;
    public Image healthbar_bg, healthbar_fill;
    public Image defensebar_bg, defensebar_fill;
    public GameObject healthbar_obj, defensebar_obj;
    public UnityEngine.Experimental.Rendering.Universal.Light2D light2d;
    public Sprite range_icon, melee_icon, shield_icon, armor_icon;

    public TextMeshProUGUI attT, defT, hpT, stamT, defbarT;
    public Image attfgI, attbgI;
    public Image deffgI, defbgI;
    public Color attfgI_c, deffgI_c;

    [HideInInspector]
    public int col, row;
    public int num; // Hierarchy in group. 0, 1, 2
    public Group group;
    //public Button button;
    private bool _disabled = false;
    public AnimationPlayer animation_player;
    public GameObject animation_obj;

    ////// New
    public ParticleSystem dust_ps;
    public Transform slot_point_transform;
    public Deployment deployment;
    
    void Awake() {
        cam = GameObject.Find("BattleCamera").GetComponent<Camera>();
        light2d.enabled = false;

        face_cam();
    }

    void Start() {
        healthbar_fill.color = healthbar_fill_color;
        healthbar_bg.color = statbar_bg_color;
        defensebar_fill.color = defensebar_fill_color;
        defensebar_bg.color = statbar_bg_color;
        PlayerDeployment.I.on_velocity_change += toggle_dust_ps;
        dust_ps.enableEmission = false;
    }

    public float move_timer = 0;
    void Update() {
        move();
    }

    public void click() {
        if (disabled)
            return;
    }

    public float smooth_speed = 0.125f;

    public void move() {
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

    public bool fill(Unit u) {
        if (u == null)
            return false;
        set_unit(u);
        init_UI(u);
        unit_img.color = Color.white;
        unit_img.sprite = BatLoader.I.get_unit_img(u, 0);
        if (u.is_playerunit)
            toggle_light(true);
        return true;
    }

    // Full slots below the removed slot will be moved up if validated.
    public Unit empty(bool validate=true) {
        Unit removed_unit = unit;
        if (unit != null) {
            unit.set_slot(null);
            unit = null;
        }

        update_unit_img(group.direction);
        set_active_UI(false);
        set_nameT("");
        //show_selection(false);
        //set_button_color();
        unit_img.color = Color.clear;
        dust_ps.enableEmission = false;
        toggle_light(false);
        if (validate)
            group.validate_unit_order();
        return removed_unit;
    }

    private void set_unit(Unit u) {
        if (u == null) 
            return;
        if (u.is_playerunit) {
            unit = u as PlayerUnit;
        } else if (u.is_enemy) {
            unit = u as Enemy;
        }
        unit.set_slot(this);
    }

    public Unit get_unit() {
        return unit;
    }

    public bool disabled {
        get { return _disabled; }
        set { 
            _disabled = value;
            set_active_UI(_disabled);
            //button.interactable = !_disabled;
        }
    }

    public void init_UI(Unit u) {
        update_healthbar_color();
        show_equipment_boosts();
        set_nameT(unit.get_name());
        //update_unit_img(group.direction);
        set_active_UI(true);
        update_text_UI();
    }

    public void toggle_dust_ps(float velocity) {
        if (is_empty) {
            return;
        }
        dust_ps.enableEmission = velocity > 0;
    }

    private void set_attack_icon(Unit u) {
        // Change attack icon based on attack type.
        if (get_unit().is_range) {
            attfgI.sprite = range_icon;
            attbgI.sprite = range_icon;
        } else {
            attfgI.sprite = melee_icon;
            attbgI.sprite = melee_icon;
        }
        attfgI.color = Color.white;
        deffgI.color = Color.white;
    }

    private void set_defense_icon(Unit u) {
        if (u.is_playerunit) {
            deffgI.sprite = shield_icon;
            defbgI.sprite = shield_icon;
        } else {
            defbgI.sprite = armor_icon;
            deffgI.sprite = armor_icon;
        }
    }

    // Updated when a boost is removed or applied,
    // or an attribute is activated or deactivated.
    public void update_text_UI() {
        update_healthbar();
        //update_defensebar();
        //update_attack();
    }

    public void update_healthbar(Unit preview_unit=null) {
        healthbar.maxValue = get_unit().get_dynamic_max_health();
        
        int final_hp = 100;
        healthbar.value = final_hp;
        update_healthbar_color();
        
        hpT.text = build_health_string(final_hp, 0);
    }

    public string build_health_string(float hp, float preview_damage) {
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

    private void update_healthbar_color() {
        if (unit.is_playerunit) {
            if (healthbar.value < healthbar.maxValue / 2f)
                healthbar.fillRect.GetComponent<Image>().color = healthbar_injured_fill_color;
        } else {
            //float red = ((float)get_enemy().health / (float)get_enemy().max_health);
            //healthbar.fillRect.GetComponent<Image>().color = new Color(1, red, red, 1);
            healthbar.fillRect.GetComponent<Image>().color = healthbar_fill_color;
        }
    }

    public void update_defensebar(Unit preview_unit=null) {
        // The defense bar includes but does not show stat bonus.
        defensebar.maxValue = get_unit().get_defense();
        //defensebar.value = get_unit().get_defense() - prev_dmg;
        //defbarT.text = build_defense_string(defensebar.value, prev_dmg);
    }

    public string build_defense_string(float def, float preview_damage) {
        float def_boost = get_unit().get_stat_buff(Unit.DEFENSE)
            + get_unit().get_bonus_def();
        string str = def.ToString();
        if (preview_damage > 0)
            str += " (-" + preview_damage.ToString() + ")";
        if (def_boost > 0) 
            str += " (+" + def_boost.ToString() + ")";
        return str;
    }
/*
    public void update_attack() {
        attT.text = build_att_string();
        toggle_attack(get_unit().get_attack_dmg() > 0);
    }*/

    private void toggle_attack(bool showing) {
        attbgI.gameObject.SetActive(showing);
    }

    /*public string build_att_string() {
        float att_boost = get_unit().get_stat_buff(Unit.ATTACK)
            + get_unit().get_bonus_att_dmg();

        string str = (get_unit().get_raw_attack_dmg() +
            get_unit().get_bonus_from_equipment(Unit.ATTACK)).ToString();
        if (att_boost > 0 && get_unit().attack_set) 
            str += "+" + att_boost.ToString();
        return str;
    }*/
/*
    public void update_defense() {
        defT.text = build_def_string();
        toggle_defense(get_unit().get_defense() > 0);
        show_defending(get_unit());
    }*/
/*
    private void show_defending(Unit u) {
       u.get_slot().deffgI.color = verify_show_defending(u) ? deffgI_c : Color.white;
    }*/
/*
    // Determines all possible circumstances to color the defense icon blue.
    private bool verify_show_defending(Unit u) {
        if (u == null)
            return false;
        if (u.get_slot() == null)
            return false;
        if (u.get_slot().get_group().is_empty)
            return false;

        Unit highest_unit = get_group().get_highest_full_slot().get_unit();
        int num_grouped = highest_unit.count_grouped_units(); // accounts for grouping attr level.

        bool same_ID = u.get_ID() == highest_unit.get_ID();
        // If 2 other in group, both non-head units are grouped. 
        // If 1 other in group, unit is grouped if not head and same unit type.
        bool grouping = u.has_grouping && highest_unit.is_actively_grouping && same_ID &&
        ((num_grouped == 2 && (u != highest_unit)) || num_grouped == 3);

         // Always show an armored enemy as defending.
        bool defensive_enemy = has_enemy && u.get_defense() > 0;

        return u.defending || grouping || defensive_enemy;
    }*/

    private void toggle_defense(bool showing) {
        defbgI.gameObject.SetActive(showing);
    }
/*
    public string build_def_string() {
        int def_boost = get_unit().get_stat_buff(Unit.DEFENSE)
            + get_unit().get_bonus_def();

        string str = (get_unit().get_raw_defense() + 
            get_unit().get_bonus_from_equipment(Unit.DEFENSE)).ToString();
        if (def_boost > 0 && get_unit().defending) 
            str += "+" + def_boost.ToString();
        return str;
    }*/

    public void set_active_UI(bool state) {
        healthbar_obj.SetActive(state);
        //defensebar_obj.SetActive(state);
    }

    // Selection color is determined by opacity.
    public void show_selection(bool showing) {
        if (img != null) {
            Color color = img.color;
            //color.a = showing ? selected_color.a : unselected_color.a;
            if (has_punit) {
                color.a = showing ? selected_color.a + 0.2f : unselected_color.a + 0.7f;
            } else
                color.a = showing ? selected_color.a : unselected_color.a;
            img.color = color;
        }
    }

/*
    // Set the color of the square slot button's image.
    public void set_button_color() {
        if (img == null)
            return;
        
        if (!has_unit) {
            img.color = unselected_color;
            return;
        }

        // Show which discipline owns the unit.
        img.color = get_unit().is_playerunit ? 
            Statics.disc_colors[get_punit().owner_ID] : Color.white;
    }*/

    private void show_equipment_boosts() {
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

    public void show_dead() {
        if (img != null)
            img.color = dead;
    }

    public void show_injured() {
        if (img != null)
            img.color = injured;
    }

    public void play_animation(string anim) {
        animation_player.play(anim);
    }

    private void update_unit_img(int dir) {
        unit_img.color = Color.white;
        unit_img.sprite = BatLoader.I.get_unit_img(unit, dir);
        if (unit_img.sprite == null)
            unit_img.color = Color.clear;
        rotate_to_direction(dir); 
    }

    public void rotate_to_direction(int direction) {
        if (has_unit) {
            unit_img.sprite = BatLoader.I.get_unit_img(unit, direction);
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

    public void face_cam() {
        unit_img.transform.LookAt(cam.transform);
        info_canv.transform.LookAt(cam.transform); 
        info_canv.transform.forward *= -1; 
    }

    public void set_animation_depth(int direction) {
        if (direction == Group.UP || direction == Group.RIGHT) {
            animation_obj.transform.SetAsFirstSibling();
        } else {
            animation_obj.transform.SetAsLastSibling();
        }
    }

    private void toggle_light(bool state) {
        light2d.enabled = state;
    }
    // ---End GRAPHICAL--- 
    
    public bool is_type(int type) {
        return group.type == type;
    }

    public bool has_punit {
        get {
            if (unit == null) return false;
            if (unit.is_playerunit) return true;
            return false;
        }
    }

    public bool has_enemy {
        get {
            if (unit == null) return false;
            if (unit.is_enemy) return true;
            return false;
        }
    }

    public bool has_unit {
        get { return unit != null ? true : false; }
    }

    public bool is_empty {
        get { return unit == null ? true : false; }
    }

    public PlayerUnit get_punit() {
        return has_punit ? unit as PlayerUnit : null;
    }

    public Enemy get_enemy() {
        return has_enemy ? unit as Enemy : null;
    }

    private void set_nameT(string txt) {
        name_T.text = txt;
    }

    public Group get_group() {
        return group;
    }
}
