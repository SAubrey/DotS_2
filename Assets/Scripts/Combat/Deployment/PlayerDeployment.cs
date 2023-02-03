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
        Zones.Add(ZoneRange);
        IsPlayer = true;

        Init();
    }

    protected void Init()
    {
        ForwardZone = ZoneSword;
    }

    protected override void Update()
    {
        if (!CamSwitcher.I.BattleCam.isActiveAndEnabled)
            return;

        base.Update();
    }
    
    protected override void FixedUpdate()
    {
        if (!CamSwitcher.I.BattleCam.isActiveAndEnabled)
            return;

        base.FixedUpdate();
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
                if (u.ID == PlayerUnit.WARRIOR || u.ID == PlayerUnit.ARCHER)
                    PlaceUnit(u);
            }
        }
        MapUI.I.UpdateDeployment(b);
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
