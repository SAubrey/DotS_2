using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeploymentUI : MonoBehaviour
{
    [SerializeField] private Group[] ZoneSword = new Group[1];
    [SerializeField] private Group[] ZonePolearm = new Group[1];
    [SerializeField] private Group[] ZoneCenter = new Group[1];
    [SerializeField] private Group[] ZoneRange = new Group[1];
    [SerializeField] private Group[] ZoneMage = new Group[1];
    protected List<Group[]> Groups = new List<Group[]>();

    void Start()
    {
        Groups.Add(ZoneSword);
        Groups.Add(ZonePolearm);
        Groups.Add(ZoneCenter);
        Groups.Add(ZoneRange);
        Groups.Add(ZoneMage);
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
    }

    public void PlaceUnit(Unit unit)
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

    public Group GetHighestEmptyGroup(Group[] groups)
    {
        foreach (Group g in groups)
        {
            if (!g.IsFull)
            {
                return g;
            }
        }
        return null;
    }
}
