using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TravelDeck : MonoBehaviour
{
    public static TravelDeck I { get; private set; }

    // <TYPE><TIER>_<NUM>
    public const int ATT1_1 = 1, ATT1_2 = 2, ATT1_3 = 3, ATT1_4 = 4, ATT1_5 = 5, ATT1_6 = 6;
    public const int CHANCE1_1 = 8, CHANCE1_2 = 9, CHANCE1_3 = 10;
    public const int BLESSING1_1 = 11;
    public const int CAVE1_1 = 12, CAVE1_2 = 13;
    public const int EVENT1_1 = 14, EVENT1_2 = 15, EVENT1_3 = 16, EVENT1_4 = 17, EVENT1_5 = 18, EVENT1_6 = 19;
    public const int EVENT2_1 = 101, EVENT2_2 = 102, EVENT2_3 = 103,
        EVENT2_4 = 104, EVENT2_5 = 105, EVENT2_6 = 106;
    public const int EVENT3_1 = 201;
    public const int RUINS1_1 = 20, RUINS1_2 = 21, RUINS1_3 = 22, RUINS1_4 = 23;
    public const int LOCATION1_1 = 24, LOCATION1_2 = 25, LOCATION1_3 = 26;
    public const int LOCATION2_1 = 107;

    //
    int[] tier1_cards = new int[] {
        ATT1_1, ATT1_2, ATT1_3, ATT1_3, ATT1_5, ATT1_6,
        CHANCE1_1, CHANCE1_2, CHANCE1_3,
        //BLESSING1_1,
        CAVE1_1, CAVE1_2,
        EVENT1_1, EVENT1_2, EVENT1_4, EVENT1_5,
        RUINS1_1, RUINS1_2, RUINS1_3, RUINS1_4,
        LOCATION1_2, LOCATION1_3};

    int[] tier2_cards = new int[] {
        ATT1_1, ATT1_2, ATT1_3, ATT1_3, ATT1_5, ATT1_6,
        CHANCE1_1, CHANCE1_2, CHANCE1_3,
        //BLESSING1_1,
        CAVE1_1, CAVE1_2,
        EVENT2_1, EVENT2_2, EVENT2_3, EVENT2_4, EVENT2_5, EVENT2_6,
        RUINS1_1, RUINS1_2, RUINS1_3, RUINS1_4,
        LOCATION2_1
    };
    int[] tier3_cards = new int[] {
        ATT1_1, ATT1_2, ATT1_3, ATT1_3, ATT1_5, ATT1_6,
        CHANCE1_1, CHANCE1_2, CHANCE1_3,
        //BLESSING1_1,
        CAVE1_1, CAVE1_2,
        EVENT2_1, EVENT2_2, EVENT2_3, EVENT2_4, EVENT2_5, EVENT2_6,
        RUINS1_1, RUINS1_2, RUINS1_3, RUINS1_4,
        LOCATION2_1
    };
    private int[][] Cards;

    // Inclusion dictionary limiting which card types are allowed
    // in which biomes. <MapCell.ID, List<TravelCard.type>>
    // Uses these mappings to aggregate relevant cards to draw from. 
    // For each list entry, join onto a new list the cards of that type.
    private Dictionary<int, List<int>> AllowedCards = new Dictionary<int, List<int>>() {
        {MapCell.IDPlains, new List<int>() },
        {MapCell.IDForest, new List<int>() },
        {MapCell.IDRuins, new List<int>() },
        {MapCell.IDCliff, new List<int>() },
        {MapCell.IDCave, new List<int>() },
        {MapCell.IDStar, new List<int>() },
        {MapCell.IDTitrum, new List<int>() },
        {MapCell.IDLushLand, new List<int>() },
        //{MapCell.MIRE_ID, new List<int>() },
        {MapCell.IDMountain, new List<int>() },
        {MapCell.IDSettlement, new List<int>() },
        {MapCell.IDRuneGate, new List<int>() },
    };

    void Awake()
    {
        if (I == null)
        {
            I = this;
        }
        else
        {
            Destroy(gameObject);
        }

        Cards = new int[][] { tier1_cards, tier2_cards, tier3_cards };
        // Which card types can be found in which biomes?
        AllowedCards[MapCell.IDPlains].AddRange(new int[] {
            TravelCard.COMBAT, TravelCard.BLESSING, TravelCard.CHANCE,
            TravelCard.EVENT, TravelCard.LOCATION});
        AllowedCards[MapCell.IDForest].AddRange(new int[] {
            TravelCard.COMBAT, TravelCard.BLESSING, TravelCard.CHANCE,
            TravelCard.EVENT, TravelCard.LOCATION});
        AllowedCards[MapCell.IDCliff].AddRange(new int[] {
            TravelCard.COMBAT, TravelCard.BLESSING, TravelCard.CHANCE,
            TravelCard.EVENT, TravelCard.LOCATION});
        AllowedCards[MapCell.IDMountain].AddRange(new int[] {
            TravelCard.COMBAT, TravelCard.BLESSING, TravelCard.CHANCE,
            TravelCard.EVENT, TravelCard.LOCATION});

        AllowedCards[MapCell.IDTitrum].AddRange(new int[] {
            TravelCard.COMBAT, TravelCard.CHANCE,
            TravelCard.EVENT, TravelCard.LOCATION});
        //allowed_cards[MapCell.MIRE_ID].AddRange(new int[] {
        //TravelCard.BLESSING, TravelCard.EVENT } );
        AllowedCards[MapCell.IDCave].Add(TravelCard.CAVE);
        AllowedCards[MapCell.IDRuins].Add(TravelCard.RUINS);
        // no cards for star, lush. Settlement = quest card?
    }

    void Start()
    {
    }

    public Button combat_cards_onlyB; // DEV ONLY
    public bool CombatCardsOnly { get; set; } = false;

    // Cards are drawn without replacement. Cards that are allowed
    // in the map cell biome are pulled from the deck then chosen from randomly.
    public TravelCard DrawCard(int tier, int biomeID)
    {
        // Negate or bypass random draw.
        if (biomeID == MapCell.IDLushLand || biomeID == MapCell.IDStar)
        {
            return null;
        }
        else if (biomeID == MapCell.IDRuneGate)
        {
            return MakeCard(LOCATION1_1);
        }
        else if (biomeID == MapCell.IDGuardianPass)
        {
            if (tier == 1)
            {
                return MakeCard(EVENT1_6);
            }
            else if (tier == 2)
            {
                return MakeCard(EVENT2_6);
            }
            else if (tier == 3)
            {
                return MakeCard(EVENT3_1);
            }
        }
        else if (CombatCardsOnly)
        {
            return MakeCard(ATT1_1); // debug only
        }

        List<int> drawableCards = AggregateDrawableCards(tier, biomeID);
        if (drawableCards.Count == 0)
            return null;
        int randIndex = Random.Range(0, drawableCards.Count - 1);
        return MakeCard(drawableCards[randIndex]);
    }


    // Returns cards that can be drawn at the current tile biome.
    private List<int> AggregateDrawableCards(int tier, int biomeID)
    {
        List<int> validCardIds = new List<int>();
        foreach (int cardID in Cards[tier - 1])
        {
            if (CheckIfCardInBiome(biomeID, cardID))
            {
                validCardIds.Add(cardID);
            }
        }
        return validCardIds;
    }

    private bool CheckIfCardInBiome(int biomeID, int cardID)
    {
        if (!AllowedCards.ContainsKey(biomeID))
            return false;
        return AllowedCards[biomeID].Contains(MakeCard(cardID).type);
    }

    public TravelCard MakeCard(int ID)
    {
        if (ID == ATT1_1) return new Att1_1();
        if (ID == ATT1_2) return new Att1_2();
        if (ID == ATT1_3) return new Att1_3();
        if (ID == ATT1_4) return new Att1_4();
        if (ID == ATT1_5) return new Att1_5();
        if (ID == ATT1_6) return new Att1_6();
        if (ID == CHANCE1_1) return new Chance1_1();
        if (ID == CHANCE1_2) return new Chance1_2();
        if (ID == CHANCE1_3) return new Chance1_3();
        if (ID == BLESSING1_1) return new Blessing1_1();
        if (ID == CAVE1_1) return new Cave1_1();
        if (ID == CAVE1_2) return new Cave1_2();
        if (ID == RUINS1_1) return new Ruins1_1();
        if (ID == RUINS1_2) return new Ruins1_2();
        if (ID == RUINS1_3) return new Ruins1_3();
        if (ID == RUINS1_4) return new Ruins1_4();
        if (ID == LOCATION1_1) return new Location1_1();
        if (ID == LOCATION1_2) return new Location1_2();
        if (ID == LOCATION1_3) return new Location1_3();
        if (ID == EVENT1_1) return new Event1_1();
        if (ID == EVENT1_2) return new Event1_2();
        if (ID == EVENT1_3) return new Event1_3();
        if (ID == EVENT1_4) return new Event1_4();
        if (ID == EVENT1_5) return new Event1_5();

        if (ID == LOCATION2_1) return new Location2_1();
        if (ID == EVENT2_1) return new Event2_1();
        if (ID == EVENT2_2) return new Event2_2();
        if (ID == EVENT2_3) return new Event2_3();
        if (ID == EVENT2_4) return new Event2_4();
        if (ID == EVENT2_5) return new Event2_5();
        if (ID == EVENT2_6) return new Event2_6();
        return null;
    }
}
