using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerDeployment : Deployment
{
    public static PlayerDeployment I { get; private set; }
    private GameObject LockedOnEnemy;

    [SerializeField] private LayerMask GroundMask;
    [SerializeField] protected Group[] ZoneSword = new Group[3];
    [SerializeField] protected Group[] ZonePolearm = new Group[3];
    [SerializeField] protected Group[] ZoneCenter = new Group[1];
    [SerializeField] protected Group[] ZoneRange = new Group[2];
    [SerializeField] protected Group[] ZoneMage = new Group[1];

    [SerializeField] private GameObject ZoneSwordParent;
    [SerializeField] private Vector3 ZoneSwordParentPos;
    [SerializeField] private GameObject ZonePolearmParent;
    [SerializeField] private Vector3 ZonePolearmParentPos;
    [SerializeField] private GameObject ZoneRangeParent;
    [SerializeField] private Vector3 ZoneRangeParentPos;
    [SerializeField] private GameObject ZoneMageParent;
    [SerializeField] private Vector3 ZoneMageParentPos;

    private Group[] ForwardZone;
    private Timer AttackDelayTimer = new Timer(.0005f);
    public Slider Manabar;
    public float ManaMax = 100f;
    private float _Mana = 100f;
    public float Mana
    {
        get { return _Mana; }
        set
        {
            _Mana = Mathf.Max(value, 0f);
            if (Manabar != null)
            {
                UpdateSlider(Manabar, ManaMax, Mana);
            }
        }
    }
    
    protected override void Awake()
    {
        base.Awake();
        if (I == null)
        {
            I = this;
        }
        else
        {
            Destroy(gameObject);
        }
        ZoneSwordParentPos = ZoneSwordParent.transform.localPosition;
        ZonePolearmParentPos = ZonePolearmParent.transform.localPosition;
        ZoneRangeParentPos = ZoneRangeParent.transform.localPosition;
        ZoneMageParentPos = ZoneMageParent.transform.localPosition;
        Destination = transform.position;
    }

    void Start()
    {
        Zones.Add(ZoneSword);
        Zones.Add(ZonePolearm);
        Zones.Add(ZoneCenter);
        Zones.Add(ZoneRange);
        Stamina = StamMax;
        IsPlayer = true;
        VelRun = VelRun + 50f;
        Init();
    }

    protected void Init()
    {
        UpdateSlider(staminabar, StamMax, Stamina);
        Stamina = StamMax;
        Mana = ManaMax;
        ForwardZone = ZoneSword;
        Destination = transform.position;
    }

    protected override void Update()
    {
        if (!CamSwitcher.I.battle_cam.isActiveAndEnabled)
            return;

        base.Update();
        PollControlScheme2();
    }
    
    protected override void FixedUpdate()
    {
        if (!CamSwitcher.I.battle_cam.isActiveAndEnabled)
            return;

        base.FixedUpdate();
        FixedUpdateControlScheme2();
    }

    private void FixedUpdateControlScheme1()
    {
        Vector3 movement = GetMovementInput();
        if (movement != Vector3.zero && CanMove)
        {
            MoveInDirection(movement, VelRun, PhysicsBody.MoveForce);
        }
        //RotateToMouse(GroundMask);
    }

    // Click to move/rotate, enemy lock on
    private void FixedUpdateControlScheme2()
    {
        if (CanMove)
        {
            MoveToDestination(Destination, VelRun, PhysicsBody.MoveForce);
            //RotateTowardsTarget(Destination);
        }
    }

    private void PollControlScheme1() 
    {
        if (Input.GetMouseButtonDown(0) && AttackDelayTimer.finished)
        { // Left click
            MeleeAttack(ForwardZone);
        }
        else if (Input.GetMouseButtonDown(1))
        { // Right click
            Block(true);
        }
        else if (Input.GetMouseButtonUp(1)) {
            Block(false);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            // show range_attack landing zone
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Vector3 p = Statics.GetMouseWorldPos(CamSwitcher.I.battle_cam, GroundMask);
            if (p != Vector3.zero)
                RangeAttack(ZoneRange, p);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            /*if (lockedOnEnemy == null) {
                lockedOnEnemy = FindNearestEnemyInSight();
            } else {
                lockedOnEnemy = null;
            }*/
        }
        Group[] g = CheckFormationInput();
        if (g != null)
        {
            ForwardZone = g;
            MoveFormations(g[0].Parent);
        }
    }

    private void PollControlScheme2() 
    {
        if (Input.GetMouseButtonDown(0))
        { // Left click
            Vector3 p = Statics.GetMouseWorldPos(CamSwitcher.I.battle_cam, GroundMask);
            if (p != Vector3.zero)
                Destination = p;
        } else if (Input.GetMouseButton(0))
        {
            Vector3 p = Statics.GetMouseWorldPos(CamSwitcher.I.battle_cam, GroundMask);
            if (p != Vector3.zero)
                Destination = p;
        }
        if (Input.GetKeyDown(KeyCode.A) && AttackDelayTimer.finished)
        {
            MeleeAttack(ForwardZone);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        { // Right click
            Block(true);
        }
        else if (Input.GetKeyUp(KeyCode.D)) {
            Block(false);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            // show range_attack landing zone
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Vector3 p = Statics.GetMouseWorldPos(CamSwitcher.I.battle_cam, GroundMask);
            if (p != Vector3.zero)
                RangeAttack(ZoneRange, p);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            if (LockedOnEnemy == null) {
                LockedOnEnemy = FindNearestEnemyInSight();
            } else {
                LockedOnEnemy = null;
            }
        }
        Group[] g = CheckFormationInput();
        if (g != null)
        {
            ForwardZone = g;
            MoveFormations(g[0].Parent);
        }
    }

    private GameObject FindNearestEnemyInSight()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float nearestDistance = Mathf.Infinity;
        float distance = 0;
        foreach (GameObject go in gos)
        {
            distance = Vector3.Distance(transform.position, go.transform.position);
            if (distance < nearestDistance)
            {
                nearest = go;
                nearestDistance = distance;
            }
        }
        return nearest;
    }

    protected void MoveFormations(GameObject forwardGameObject) 
    {
        if (forwardGameObject == ZoneSwordParent) 
        {      
            ZoneSword[0].Parent.transform.localPosition = ZoneSwordParentPos;
            ZonePolearm[0].Parent.transform.localPosition = ZonePolearmParentPos;
            ZoneRange[0].Parent.transform.localPosition = ZoneRangeParentPos;
            ZoneMage[0].Parent.transform.localPosition = ZoneMageParentPos;
        } else if (forwardGameObject == ZonePolearmParent)
        {
            ZonePolearm[0].Parent.transform.localPosition = ZoneSwordParentPos;
            ZoneSword[0].Parent.transform.localPosition = ZonePolearmParentPos;
            ZoneRange[0].Parent.transform.localPosition = ZoneRangeParentPos;
            ZoneMage[0].Parent.transform.localPosition = ZoneMageParentPos;
        } else if (forwardGameObject == ZoneRangeParent)
        {
            ZoneRange[0].Parent.transform.localPosition = ZoneSwordParentPos;
            ZoneSword[0].Parent.transform.localPosition = ZonePolearmParentPos;
            ZonePolearm[0].Parent.transform.localPosition = ZoneRangeParentPos;
            ZoneMage[0].Parent.transform.localPosition = ZoneMageParentPos;
        } else if (forwardGameObject == ZoneMageParent)
        {  
            ZoneMage[0].Parent.transform.localPosition = ZoneSwordParentPos;
            ZoneSword[0].Parent.transform.localPosition = ZonePolearmParentPos;
            ZonePolearm[0].Parent.transform.localPosition = ZoneRangeParentPos;
            ZoneRange[0].Parent.transform.localPosition = ZoneMageParentPos;
        }
    }

    private int CheckAbilityInput() 
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) 
        {
            return 1;
        } else if (Input.GetKeyDown(KeyCode.Alpha2)) 
        {
            return 2;
        } else if (Input.GetKeyDown(KeyCode.Alpha3)) 
        {
            return 3;
        } else if (Input.GetKeyDown(KeyCode.Alpha4)) 
        {
            return 4;
        } else if (Input.GetKeyDown(KeyCode.Alpha5)) 
        {
            return 5;
        }
        return 0;
    }

    private Group[] CheckFormationInput() {
        if (Input.GetKeyDown(KeyCode.Z)) 
        {
            return ZoneSword;
        } else if (Input.GetKeyDown(KeyCode.X)) 
        {
            return ZonePolearm;
        } else if (Input.GetKeyDown(KeyCode.C)) 
        {
            return ZoneRange;
        } else if (Input.GetKeyDown(KeyCode.V)) 
        {
            return ZoneCenter;
        }
        return null;
    }

    private Vector3 GetMovementInput()
    {
        Vector3 vec = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            vec += Vector3.forward;
        else if (Input.GetKey(KeyCode.S))
            vec += Vector3.back;
        if (Input.GetKey(KeyCode.A))
            vec += Vector3.left;
        else if (Input.GetKey(KeyCode.D))
            vec += Vector3.right;
        return vec;
    }

    public override void PlaceUnit(Unit unit)
    {
        Group[] zone = null;
        if (unit.IsMelee)
        {
            zone = unit.HasAttribute(Unit.PIERCING) ? ZonePolearm : ZoneSword;
        }
        else if (unit.IsRange)
        {
            zone = ZoneRange;
        }
        else if (unit.IsMage)
        {
            zone = ZoneMage;
        } else
        {
            zone = ZoneCenter;
        }

        Group g = GetHighestEmptyGroup(zone);
        if (g != null)
        {
            g.PlaceUnit(unit);
        }
    }

    public void PlaceUnits(Battalion b)
    {
        foreach (List<PlayerUnit> pu in b.Units.Values)
        {
            foreach (Unit u in pu)
            {
                PlaceUnit(u);
            }
        }
        MapUI.I.UpdateDeployment(b);
    }

    protected void Block(bool active)
    {
        if (Stamina < StamBlockCost)
        {
            Blocking = false;
            return;
        }
        Blocking = active;

        foreach (Group g in ForwardZone)
        {
            for (int i = 0; i < 3; i++)
            {
                if (g.Slots[i].HasUnit)
                {
                    Unit u = g.Slots[i].GetUnit();
                    u.Blocking = active;
                }
            }
        }
    }

    public override Group[] GetAttackingZone(bool melee)
    {
        return melee ? ForwardZone : ZoneRange;
    }

    /*private GameObject find_nearest_enemy_in_sight()
    {
        int mask = 1 << 9;
        // Scan for enemies with enemy layer mask.
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(
            new Vector2(transform.position.x, transform.position.y), light2d.pointLightOuterRadius, mask);

        Collider2D closestEnemy = Statics.DetermineClosestCollider(enemiesInRange,
            new Vector2(transform.position.x, transform.position.y));
        if (closestEnemy == null)
            return null;

        return closestEnemy.gameObject;
    }*/

    private void ValidateAllPunits()
    {
        foreach (Group g in ZoneSword)
        {
            g.ValidateUnitOrder();
        }
    }

    public float GetStamRegenAmount()
    {
        float vel = 0;//Mathf.Abs(body.velocity.x) + Mathf.Abs(body.velocity.y);
        // Double regeneration when not moving.
        return vel < VelRun / 10 ? StamRegenAmount * 2 : StamRegenAmount;
    }
}
