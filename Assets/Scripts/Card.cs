using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    // Keeps track of a cards suit
    public CardSuit suit { get; private set; } = CardSuit.Invalid;

    // Keeps track of a cards value
    public CardValue value { get; private set; } = CardValue.Invalid;

    // Keeps track of whether or not the card is loaded
    public bool loaded { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {
        // Big ugly elif chain for checking the names of card gameobjects and assigning
        // them with their respective suit and value. There are better ways of doing this
        // but this was one of the easiest and fastest
        if(name.Contains("Heart"))
        {
            suit = CardSuit.Heart;
        }
        else if(name.Contains("Diamond"))
        {
            suit = CardSuit.Diamond;
        }
        else if(name.Contains("Spade"))
        {
            suit = CardSuit.Spade;
        }
        else if(name.Contains("Clover"))
        {
            suit = CardSuit.Clover;
        }
        else
        {
            suit = CardSuit.Invalid;
            Debug.LogWarning($"GameObject {name} is not recognized as a valid card.");
        }

        if(name.Contains("Ace"))
        {
            value = CardValue.Ace;
        }
        else if(name.Contains("02"))
        {
            value = CardValue.Two;
        }
        else if(name.Contains("03"))
        {
            value = CardValue.Three;
        }
        else if(name.Contains("04"))
        {
            value = CardValue.Four;
        }
        else if(name.Contains("05"))
        {
            value = CardValue.Five;
        }
        else if(name.Contains("06"))
        {
            value = CardValue.Six;
        }
        else if(name.Contains("07"))
        {
            value = CardValue.Seven;
        }
        else if(name.Contains("08"))
        {
            value = CardValue.Eight;
        }
        else if(name.Contains("09"))
        {
            value = CardValue.Nine;
        }
        else if(name.Contains("10"))
        {
            value = CardValue.Ten;
        }
        else if(name.Contains("Jack"))
        {
            value = CardValue.Jack;
        }
        else if(name.Contains("Queen"))
        {
            value = CardValue.Queen;
        }
        else if(name.Contains("King"))
        {
            value = CardValue.King;
        }
        else
        {
            value = CardValue.Invalid;
            Debug.LogWarning($"GameObject {name} is not recognized as a valid card.");
        }

        // Card has it's values set at this point even if they're invalid values so we
        // can set it as loaded
        loaded = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Enum for card suits
    public enum CardSuit
    {
        Invalid,
        Heart,
        Diamond,
        Spade,
        Clover
    }

    // Enum for card values
    public enum CardValue
    {
        Invalid = -1,
        Ace = 0, // Aces are a special case and need to be handled differently from other values
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 10,
        Queen = 10,
        King = 10
    }
}
