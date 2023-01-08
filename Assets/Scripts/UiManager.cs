using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    // This instance variable ensures that only one of these
    // scripts is running at any time as we need to keep it
    // throughout the entire lifetime of the game
    public static UiManager Instance { get; private set; }

    // The text object for the chips display in the top right
    // of the screen
    public TMP_Text chipsText;

    private void Awake()
    {
        // This ensures that if another instance of this
        // script exists it will be destroyed before it can
        // do anything
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
        
    }

    // Update is called once per frame
    void Update()
    {
        // Keeps the text constantly updated with the amount
        // of chips the player has
        chipsText.text = $"Chips: {PlayerManager.Instance.Chips}";
    }
}
