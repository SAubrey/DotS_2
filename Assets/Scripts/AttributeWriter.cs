using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class AttributeWriter
{

    public static string GROUPING_DESC =
    "Active. Unit's attack or defense increases by 1 for each unit in the same group up to the attribute level.";
    public static string HEAL_DESC = "Active. Select an injured unit from the selection bar for placement in a green reserve group.";

    private static Dictionary<Unit.Attributes, string> Descriptions
        = new Dictionary<Unit.Attributes, string>() {
            {Unit.Attributes.Flanking, "Flanking\nUnit spawns to the side of your battalion."},
            {Unit.Attributes.Flying, "Flying\nUnit cannot be hit by melee units that do not have Reach."},
            {Unit.Attributes.Stalk, "Stalk\nUnit spawns behind your battalion."},
            {Unit.Attributes.Piercing, "Piercing\nPassive. Attacks ignore defense."},
            {Unit.Attributes.ArcingStrike, "Arcing Strike\nUnit’s attack sweeps over multiple groups."},
            {Unit.Attributes.Aggressive, "Aggressive\nUnit has an additional action per combat phase."},
            {Unit.Attributes.Stun, "Stun\nRemoves one action from the target while doing half damage."},
            {Unit.Attributes.WeaknessPolearm, "Polearm Weakness\n"},
            {Unit.Attributes.Charge, "Charge\nUnit can attack immediately after battle begins."},
            //{Unit.INSPIRE, "Inspire\nActive. "},
            {Unit.Attributes.Harvest, "Harvest\nThis unit can mine resources from certain biomes."},
            {Unit.Attributes.CounterCharge, "Counter Charge\nUnit inflicts double damage against charging units."},
           //{Unit.BOLSTER, "Bolster\nActive. "},
            //{Unit.TRUE_SIGHT, "True Sight\nUnit will reveal or unlock travel card features."},
            {Unit.Attributes.Heal, "Heal 1\n" + HEAL_DESC},
        };

    public static void WriteAttributeText(TextMeshProUGUI text, Unit u)
    {
        text.text = GetDescription(u.Attribute1) + "\n" +
                    GetDescription(u.Attribute2) + "\n" +
                    GetDescription(u.Attribute3);
    }

    public static string GetDescription(Unit.Attributes attribute)
    {
        string s;
        Descriptions.TryGetValue(attribute, out s);
        return s != null ? s : "";
    }
}
