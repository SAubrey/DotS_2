using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class PlayerDeployment : Deployment
{
    public static PlayerDeployment I { get; private set; }

    [SerializeField] private LayerMask GroundMask;
    [SerializeField] protected Group[] ZoneSword = new Group[2];
    [SerializeField] protected Group[] ZonePolearm = new Group[2];
    [SerializeField] protected Group[] ZoneCenter = new Group[1];
    [SerializeField] protected Group[] ZoneRange = new Group[1];
    [SerializeField] protected Group[] ZoneMage = new Group[1];

    [SerializeField] private GameObject ZoneSwordParent;
    [SerializeField] private Vector3 ZoneSwordParentPos;
    [SerializeField] private GameObject ZonePolearmParent;
    [SerializeField] private Vector3 ZonePolearmParentPos;
    [SerializeField] private GameObject ZoneRangeParent;
    [SerializeField] private Vector3 ZoneRangeParentPos;
    [SerializeField] private GameObject ZoneMageParent;
    [SerializeField] private Vector3 ZoneMageParentPos;

    public Group[] ForwardZone;
    private Timer AttackDelayTimer = new Timer(.05f, true);
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
    [SerializeField] private GameObject LightObject;
    public event Action<Slot> OnLockOn;
    
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
        
        Agent.updateRotation = false;
    }

    void Start()
    {
        Zones.Add(ZoneSword);
        Zones.Add(ZonePolearm);
        Zones.Add(ZoneCenter);
        Zones.Add(ZoneRange);
        Stamina = StamMax;
        IsPlayer = true;

        Init();
    }

    protected void Init()
    {
        UpdateSlider(staminabar, StamMax, Stamina);
        Stamina = StamMax;
        Mana = ManaMax;
        ForwardZone = ZoneSword;
        Agent.destination = transform.position;
    }

    protected override void Update()
    {
        if (!CamSwitcher.I.BattleCam.isActiveAndEnabled)
            return;

        base.Update();
        PollControlScheme();
    }
    
    protected override void FixedUpdate()
    {
        if (!CamSwitcher.I.BattleCam.isActiveAndEnabled)
            return;

        base.FixedUpdate();
    }

    private void PollControlScheme() 
    {
        if (Controller.I.Move.triggered)
        {
            /*
            Vector3 p = Statics.GetMouseWorldPos(CamSwitcher.I.BattleCamCaster, GroundMask);
            if (p != Vector3.zero)
            {
                SetAgentDestination(p);
            }
            */
        } else if (Controller.I.LeftClickHeld.phase == InputActionPhase.Performed)
        {
            Vector3 p = Statics.GetMouseWorldPos(CamSwitcher.I.BattleCamCaster, GroundMask);
            if (p != Vector3.zero)
            {
                SetAgentDestination(p);
            }
        }
        AttackDelayTimer.Increase(Time.deltaTime);
        if (Controller.I.Attack.triggered && AttackDelayTimer.Finished)
        {
            Debug.Log("Melee attack");
            MeleeAttack(ForwardZone);
            AttackDelayTimer.Reset();
        }
        else if (Controller.I.Block.phase == InputActionPhase.Performed)
        {
            Block(true);
        }
        else if (Controller.I.Block.phase == InputActionPhase.Canceled) 
        {
            Block(false);
        }
        else if (Controller.I.FireArrow.triggered)
        {
            // show range_attack landing zone
        }
        if (Controller.I.FireArrow.triggered)
        {
            Vector3 p = Statics.GetMouseWorldPos(CamSwitcher.I.BattleCamCaster, GroundMask);
            if (p != Vector3.zero)
                RangeAttack(ZoneRange, p);
        }
        else if (Controller.I.LockOn.triggered)
        {
            ToggleLockOn();
        }
        Group[] g = CheckFormationInput();
        if (g != null)
        {
            ForwardZone = g;
            MoveFormations(g[0].Parent);
        }
    }

    private void ToggleLockOn()
    {
        LockedOnTarget = LockedOnTarget == null ? FindNearestEnemyInSight() : null;
        OnLockOn(LockedOnTarget);
    }

    private Slot FindNearestEnemyInSight()
    {
        Slot[] slots = GameObject.FindObjectsOfType<Slot>();
        Slot nearest = null;
        float nearestDistance = Mathf.Infinity;
        float distance = 0;
        foreach (Slot s in slots)
        {
            if (!s.HasEnemy)
                continue;
            distance = Vector3.Distance(transform.position, s.transform.position);
            if (distance < nearestDistance)
            {
                nearest = s;
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

    private Group[] CheckFormationInput() {
        if (Controller.I.MoveForwardSwordsmen.triggered) 
        {
            return ZoneSword;
        } else if (Controller.I.MoveForwardPolearm.triggered) 
        {
            return ZonePolearm;
        } else if (Controller.I.MoveForwardRanger.triggered) 
        {
            return ZoneRange;
        } else if (Controller.I.MoveForwardMage.triggered) 
        {
            return ZoneMage;
        }
        return null;
    }

    public override void PlaceUnit(Unit unit)
    {
        Group[] zone = null;
        if (unit.IsMelee)
        {
            zone = unit.HasAttribute(Unit.Attributes.Piercing) ? ZonePolearm : ZoneSword;
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
        foreach (List<PlayerUnit> unitType in b.Units.Values)
        {
            foreach (Unit u in unitType)
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
                    g.Slots[i].Unit.Blocking = active;
                }
            }
        }
    }

    public override Group[] GetAttackingZone(bool melee)
    {
        return melee ? ForwardZone : ZoneRange;
    }

    private void ValidateAllPunits()
    {
        foreach (Group g in ZoneSword)
        {
            g.ValidateUnitOrder();
        }
    }

    private void CenterCamOnTransform(Transform t) 
    {
        t.gameObject.SetActive(true);
        CamSwitcher.I.FollowTransform = t;
        LightObject.transform.SetParent(t);
    }

    public Slot GetNearestUnit(Vector3 pos)
    {
        // Given some location, return the closest slot's unit for enemy targeting.
        Slot nearestSlot = null;
        float nearestSlotDistance = Mathf.Infinity;
        foreach (Group[] zone in Zones)
        {
            foreach (Group g in zone)
            {
                foreach (Slot slot in g.GetFullSlots())
                {
                    if (Vector3.Distance(slot.transform.position, pos) < nearestSlotDistance)
                    {
                        nearestSlot = slot;
                    }
                }
            }
        }
        return nearestSlot;
    }
}
