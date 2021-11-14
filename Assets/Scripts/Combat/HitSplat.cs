using UnityEngine;

// Object exists for duration of the visual hitsplat.
public class HitSplat : RisingInfo
{
    public static Color Blue = new Color(.1f, .1f, 1, 1);
    public static Color Red = new Color(1, .1f, .1f, 1);

    public void Init(int dmg, int state, Slot endSlot)
    {
        translation_distance = 1f;
        set_text(dmg.ToString());
        SetColor(state);
        transform.position = endSlot.transform.position;
    }

    private void SetColor(int state)
    {
        if (state == Unit.ALIVE)
            T.color = Blue;
        else if (state == Unit.DEAD)
            T.color = Red;
    }
}
