using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chip : MonoBehaviour
{
    // The actual value of a chip, we could just use the chip type
    // for the value but this is easier to manage
    public int Value { private set; get; }

    // Serialized so we can set in inspector
    [SerializeField] private ChipType type;

    // Defines what type a chip is
    private enum ChipType
    {
        Red = 1,
        Blue = 5,
        Green = 10,
        Black = 25,
        Purple = 50,
        Yellow = 100
    }

    // Sets the value of a chip
    public void SetValue(int value)
    {
        // Ensure that the value is within the bounds of the chip types before
        // setting it
        if(value >= (int)ChipType.Red && value <= (int)ChipType.Yellow)
        {
            this.Value = value;
        }
    }

    // Helper method for getting the int value of ChipType enum
    public int GetValueFromChipType()
    {
        return (int)type;
    }

    // We override the ToString() method because we want to get the name
    // of the ChipType enum when calling ToString()
    public override string ToString()
    {
        return type.ToString();
    }

    // We need to use Awake() here because it's the earliest that we're
    // able to set the value of a chip and be able to access it in other
    // scripts
    void Awake()
    {
        Debug.Log($"Value before SetValue: {Value}");
        SetValue((int)type);
        Debug.Log($"Value after SetValue: {Value}");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
