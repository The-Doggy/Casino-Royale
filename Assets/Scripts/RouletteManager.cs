using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RouletteManager : MonoBehaviour
{
    [SerializeField] private TMP_Text selectedChipText; // Text for showing the currently selected chip
    [SerializeField] private TMP_Text gameOverText; // Text for showing whether the player won or lost
    [SerializeField] private Button spinButton; // Button for starting the roulette spin
    [SerializeField] private Animation spinAnim; // Animation component for the roulette spinner
    [SerializeField] private Animation ballAnim; // Animation component for the ball
    [SerializeField] private float spinTime; // Amount of time that the roulette spin should last

    private Dictionary<Collider, List<GameObject>> colliderValues = new Dictionary<Collider, List<GameObject>>(); // Contains all of the colliders for the roulette spots and lists for each chip that is in each spot
    private GameObject selectedChip; // The currently selected chip to use for betting
    private bool gameStarted; // Whether the game is started
    private PlayerManager player; // PlayerManager component for adding/removing chips from player

    // Start is called before the first frame update
    void Start()
    {
        gameOverText.gameObject.SetActive(false);

        // Add all the colliders that have the RouletteSpot tag to colliderValues and create a new list for each one
        foreach(Collider col in FindObjectsOfType<Collider>())
        {
            if (col.CompareTag("RouletteSpot"))
            {
                colliderValues[col] = new List<GameObject>();
            }
        }

        // Find the PlayerManager component in the scene and assign it to our player object
        player = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        if (player == null)
        {
            Debug.LogError("UHOH STINKY NO PLAYER");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touches.Length != 0 && !gameStarted)
        {
            // Create a raycast to check if the player is touching a chip stack or one of the roulette spots
            Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
            RaycastHit hit;

            // Make sure that the touch has begun so that we aren't doing stuff when the player moves their finger or leaves it on the screen
            if (Input.touches[0].phase == TouchPhase.Began && Physics.Raycast(ray, out hit))
            {
                Debug.Log($"Touching: {hit.collider.name}");
                // Player touching chip stack
                if (hit.collider.gameObject.GetComponent<ChipStack>() != null)
                {
                    // Check whether the selected chip is null and whether the player is touching the ChipStack of the current selectedChip
                    if (selectedChip == null || (selectedChip != null && selectedChip != hit.collider.gameObject.GetComponent<ChipStack>().SpawnChip))
                    {
                        // Assign the selected chip to the SpawnChip GameObject of the ChipStack
                        selectedChip = hit.collider.gameObject.GetComponent<ChipStack>().SpawnChip;
                        selectedChipText.text = $"Selected Chip: {selectedChip.GetComponent<Chip>()}"; // We overrode the ToString method for the Chip class so that it returns the enum value for the chip as a string
                    }
                    else
                    {
                        // Player is touching the chip they already have selected so we deselect it by setting selectedChip to null
                        selectedChip = null;
                        selectedChipText.text = $"Selected Chip: None";
                    }
                }
                // Player touching roulette spot
                else if(hit.collider.CompareTag("RouletteSpot"))
                {
                    // Player has a chip selected and has enough chips to bet
                    if(selectedChip != null && player.Chips >= selectedChip.GetComponent<Chip>().GetValueFromChipType())
                    {
                        // Instantiate a new chip gameobject from our selectedChip prefab and add it to the list associated with the hit collider
                        GameObject chip = Instantiate(selectedChip, hit.point, Quaternion.Euler(90f, 0f, 0f), hit.collider.transform);
                        colliderValues[hit.collider].Add(chip);

                        // Take player's chips for the bet
                        Debug.Log($"Value in RouletteManager: {selectedChip.GetComponent<Chip>().GetValueFromChipType()}");
                        player.AddChips(-selectedChip.GetComponent<Chip>().GetValueFromChipType());
                    }
                    // Player doesn't have a chip selected
                    else if(selectedChip == null)
                    {
                        // Remove any chips that are currently assigned to the collider
                        foreach(GameObject chip in colliderValues[hit.collider])
                        {
                            if (chip != null)
                            {
                                // Give the player back the chips they bet
                                player.AddChips(chip.GetComponent<Chip>().Value);
                                Destroy(chip);
                            }
                        }
                        // Clear the list of all the GameObjects it contains
                        colliderValues[hit.collider].Clear();
                    }
                }
            }
        }
    }

    public void StartSpin()
    {
        // Don't start a spin if one is already running
        if(gameStarted)
        {
            return;
        }

        gameStarted = true;
        spinButton.gameObject.SetActive(false);

        // Play spinning animations and pick outcome after spinTime has elapsed
        spinAnim.Play("Roulette_Loop", PlayMode.StopAll);
        ballAnim.Play("Roulette_Ball_Loop", PlayMode.StopAll);
        Invoke(nameof(PickRandomOutcome), spinTime);
    }

    private void PickRandomOutcome()
    {
        // Don't do anything if a game isn't running
        if(!gameStarted)
        {
            return;
        }

        // Get a random animation clip for the outcome, 36 is the amount of animations we have
        int rand = Random.Range(0, 36);
        ballAnim.clip = ballAnim.GetClip($"Roulette_Ball_{rand}");
        ballAnim.Play();

        // Invoke after the animation finishes minus a 0.1 delay to incorporate the blending
        Invoke(nameof(BlendRouletteSpinAnimation), ballAnim.clip.length - 0.1f);
    }

    private void BlendRouletteSpinAnimation()
    {
        // Blends the spinning and stopping animations into a *somewhat* smooth animation
        spinAnim.Blend("Roulette_Stop", 10f, 0.1f);
        Invoke(nameof(GetRouletteOutcome), 0.5f);
    }

    private void GetRouletteOutcome()
    {
        // Make sure all animations have stopped
        spinAnim.Stop();
        ballAnim.Stop();

        // Get the name of the anim clip to use as the outcome
        string outcome = ballAnim.clip.name;
        Debug.Log($"Outcome: {outcome}");

        int chipsWon = 0;
        // Loop over each collider in colliderValues
        foreach(Collider col in colliderValues.Keys)
        {
            // Check if the collider name contains our outcome string
            if(outcome.Contains(col.name))
            {
                // Loop over each chip GameObject in the list of the collider
                foreach(GameObject chip in colliderValues[col])
                {
                    // Add the value of the chip to the chipsWon var
                    chipsWon += chip.GetComponent<Chip>().Value;
                }
            }
        }

        // Give the player the chips they won
        player.AddChips(chipsWon);

        // Display the respective text and change it's color based on whether the player won or lost
        gameOverText.text = chipsWon > 0 ? $"You Won {chipsWon} Chips!" : "No Winner :(";
        gameOverText.color = chipsWon > 0 ? Color.green : Color.red;
        gameOverText.gameObject.SetActive(true);

        // Show the game over text for a couple seconds then reset the game
        Invoke(nameof(ResetGame), 2.0f);
    }

    private void ResetGame()
    {
        // Set gameobjects to default values
        gameOverText.gameObject.SetActive(false);
        spinButton.gameObject.SetActive(true);
        gameStarted = false;

        // Loops over each collider and their lists to destroy any gameobjects that are still in them
        foreach(Collider col in colliderValues.Keys)
        {
            foreach(GameObject chip in colliderValues[col])
            {
                Destroy(chip);
            }
        }
        
        // Clear each list of any leftover objects
        foreach(List<GameObject> list in colliderValues.Values)
        {
            list.Clear();
        }
    }
}
