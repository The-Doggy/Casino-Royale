using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    // Singleton for PlayerManager
    public static PlayerManager Instance { get; private set; }

    // The amount of chips the player has
    [SerializeField] public int Chips { get; private set; }

    // Key for PlayerPrefs to store chip amounts in
    private string chipsKey = "PlayerChips";

    private void Awake()
    {
        // Destroys any other instances that might exist
        // before they can initalize their behaviours
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Get the player's chips from PlayerPrefs if it exists otherwise set the chips to 0
        Chips = PlayerPrefs.GetInt(chipsKey, 0);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Sets the player's chips to the amount specified
    public void SetChips(int chips)
    {
        Chips = chips;
        PlayerPrefs.SetInt(chipsKey, Chips);
    }

    // Adds the specified amount of chips to the player, use negative values to remove chips
    public void AddChips(int chips)
    {
        Chips += chips;
        PlayerPrefs.SetInt(chipsKey, Chips);
    }
}
