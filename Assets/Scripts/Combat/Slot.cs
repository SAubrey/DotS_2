using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;


public class Slot : PhysicsBody
{
    private static readonly Color ColorHealthbar = new Color(.8f, .1f, .1f, .45f);
    private static readonly Color ColorStatbarBg = new Color(.4f, .4f, .4f, .3f);
    private static readonly Color ColorEquipmentText = new Color(1f, .67f, .32f, 1f);
    
    public SpriteRenderer SpriteUnit;
    protected Camera Cam;
    public GameObject Frame;
    public Slider Healthbar;
    public Image HealthbarBg, HealthbarFill;
    public GameObject HealthbarObj;
    public Sprite IconRange, IconMelee, IconShield;

    public TextMeshProUGUI TName;
    public TextMeshProUGUI THp;

    protected Unit Unit;
    public Group Group;
    public AnimationPlayer AnimationPlayer;
    public GameObject AnimatorObj;
    public ParticleSystem PSDust;
    public Transform SlotPointTransform;
    public Transform SlotLookPointTransform;
    public Deployment Deployment;
    public GameObject MeleeAttZone;
    public GameObject PrefabArrow;
    public event Action<Vector2> OnVelocityChange;
    public float MovementForce = 400f;
    public float MaxVelocity = 400f;
    [SerializeField] public Animator Animator;
    [SerializeField] private GameObject CharacterModel;

    protected override void Awake()
    {
        base.Awake();
        Cam = GameObject.Find("BattleCamera").GetComponent<Camera>();
        FaceCam();
        gameObject.SetActive(false);
    }

    protected virtual void Start()
    {
        HealthbarFill.color = ColorHealthbar;
        HealthbarBg.color = ColorStatbarBg;
        OnVelocityChange += ToggleDustPs;
        PSDust.enableEmission = false;
    }

    protected virtual void FixedUpdate() {
        OnVelocityChange(Rigidbody.velocity);

        Animator.SetFloat("Velocity", Rigidbody.velocity.magnitude);
        Vector3 destPos = new Vector3(SlotPointTransform.position.x, Rigidbody.position.y, SlotPointTransform.position.z);
        MoveToDestination(destPos, MaxVelocity, PhysicsBody.MoveForce);
        //RotateRigidbodyToTarget(Rigidbody, SlotPointTransform.position);
        //RotateTransformToTarget(transform, SlotLookPointTransform.position);
        FaceCam();
    }

    public virtual bool Fill(Unit u)
    {
        if (u == null)
            return false;
        gameObject.SetActive(true);
        SetUnit(u);
        InitUI(u);
        //SpriteUnit.color = Color.white;
        SpriteUnit.sprite = BatLoader.I.GetUnitImg(u, 0);
        CharacterModel.SetActive(true);
        return true;
    }

    // Full slots below the removed slot will be moved up if validated.
    public Unit Empty(bool validate = true)
    {
        Unit removedUnit = Unit;
        if (Unit != null)
        {
            Unit.SetSlot(null);
            Unit = null;
        }
        gameObject.SetActive(false);

        UpdateUnitImg(0);
        SetActiveUI(false);
        SetNameT("");
        SpriteUnit.color = Color.clear;
        CharacterModel.SetActive(false);
        PSDust.enableEmission = false;
        if (validate)
            Group.ValidateUnitOrder();
        return removedUnit;
    }

    protected virtual void SetUnit(Unit u)
    {
        if (u == null)
            return;
        if (u.IsPlayer)
        {
            Unit = u as PlayerUnit;
        }
        else if (!u.IsPlayer)
        {
            Unit = u as Enemy;
        }
        Unit.SetSlot(this);
    }

    public Unit GetUnit()
    {
        return Unit;
    }

    public void RangeAttack(LayerMask mask, Vector2 targetPos)
    {
        GameObject a = Instantiate(PrefabArrow, gameObject.transform);
        a.gameObject.transform.position = gameObject.transform.position;
        Arrow a_script = a.GetComponent<Arrow>();
        Vector2 launchPos = new Vector2(gameObject.transform.position.x,
                                        gameObject.transform.position.y);
        a_script.init(mask, launchPos, targetPos);
        Animator.SetTrigger("Attack");
    }

    public void InitUI(Unit u)
    {
        ShowEquipmentBoosts();
        SetNameT(Unit.GetName());
        SetActiveUI(true);
        UpdateTextUI();
    }
    
    public void RotateToDirection(int direction)
    {
        if (HasUnit)
        {
            SpriteUnit.sprite = BatLoader.I.GetUnitImg(Unit, direction);
        }
        FaceCam();
    }

    public void ToggleDustPs(Vector2 v)
    {
        if (IsEmpty)
        {
            return;
        }
        PSDust.enableEmission = v.magnitude > 0;
    }

    // Updated when a boost is removed or applied,
    // or an attribute is activated or deactivated.
    public void UpdateTextUI()
    {
        UpdateHealthbar();
    }

    public void UpdateHealthbar()
    {
        Healthbar.maxValue = GetUnit().GetDynamicMaxHealth();
        Healthbar.value = GetUnit().Health;
        THp.text = BuildHealthString(GetUnit().Health, 0);
        Animator.SetFloat("Health", GetUnit().Health);
    }

    public string BuildHealthString(float hp, float previewDamage)
    {
        //float hp = GetUnit().health; // This will already include the boost but not the bonus.
        float hp_boost = GetUnit().get_stat_buff(Unit.HEALTH)
            + GetUnit().GetBonusHealth();

        string str = (hp + GetUnit().get_bonus_from_equipment(Unit.HEALTH)).ToString();
        if (previewDamage > 0)
            str += " (-" + previewDamage.ToString() + ")";
        if (hp_boost > 0)
            str += " (+" + hp_boost.ToString() + ")";
        return str;
    }


    public void SetActiveUI(bool state)
    {
        HealthbarObj.SetActive(state);
    }

    public void PlayAnimationEffect(string anim)
    {
        AnimationPlayer.Play(anim);
    }

    private void UpdateUnitImg(int dir)
    {
        SpriteUnit.color = Color.white;
        SpriteUnit.sprite = BatLoader.I.GetUnitImg(Unit, dir);
        if (SpriteUnit.sprite == null)
            SpriteUnit.color = Color.clear;
        RotateToDirection(dir);
    }


    private void ShowEquipmentBoosts()
    {
        if (Unit.IsEnemy)
            return;

        EquipmentInventory ei = TurnPhaser.I.GetDisc(GetPunit().OwnerID).equipment_inventory;
        THp.color = ei.get_stat_boost_amount(Unit.GetID(), Unit.HEALTH) > 0 ?
            ColorEquipmentText : Color.white;
    }

    public virtual void FaceCam()
    {
        Frame.transform.LookAt(Cam.transform);
        Frame.transform.forward *= -1;
    }
    // ---End GRAPHICAL--- 

    public bool HasPunit
    {
        get
        {
            if (Unit == null) return false;
            if (Unit.IsPlayer) return true;
            return false;
        }
    }

    public bool HasEnemy
    {
        get
        {
            if (Unit == null) return false;
            if (Unit.IsEnemy) return true;
            return false;
        }
    }

    public bool HasUnit
    {
        get { return Unit != null ? true : false; }
    }

    public bool IsEmpty
    {
        get { return Unit == null ? true : false; }
    }

    public PlayerUnit GetPunit()
    {
        return HasPunit ? Unit as PlayerUnit : null;
    }

    public Enemy GetEnemy()
    {
        return HasEnemy ? Unit as Enemy : null;
    }

    public Group GetGroup()
    {
        return Group;
    }

    private void SetNameT(string txt)
    {
        TName.text = txt;
    }
}
