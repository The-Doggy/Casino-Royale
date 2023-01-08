using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlotsManager : MonoBehaviour
{
    [SerializeField] private Transform lever; // Transform component of the lever used to start a pull
    [SerializeField] private Animation anim; // Animation component responsible for controlling slot spin animations
    [SerializeField] private float rotateSpeed; // The speed that the lever will rotate
    [SerializeField] private float spinTime; // The amount of time that the slots should be spinning
    [SerializeField] private float resetTime; // The amount of time that we show end of game text before resetting
    [SerializeField] private TMP_Text gameOverText; // Text for showing whether the player win/lost

    private GameState gameState = GameState.None; // Stores the current state of the game
    private PlayerManager player; // PlayerManager component for adding/removing chips from the player
    private int score; // Keeps track of the score of the player

    // Enum for game state
    enum GameState
    {
        None,
        PrePullDown,
        PrePullUp,
        PullStarted,
        PullEnded
    }

    // Start is called before the first frame update
    void Start()
    {
        // Find the PlayerManager component and assign it to our player object
        player = GameObject.Find("PlayerManager").GetComponent<PlayerManager>();
        if (player == null)
        {
            Debug.LogError("UHOH STINKY NO PLAYER");
        }

        gameOverText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Check what state the game is in
        switch (gameState)
        {
            // No game is currently running
            case (GameState.None):
                {
                    if (Input.touches.Length != 0)
                    {
                        // Create a raycast to check if the player is touching the collider on the lever
                        Ray ray = Camera.main.ScreenPointToRay(Input.touches[0].position);
                        RaycastHit hit;
                        if (Input.touches[0].phase == UnityEngine.TouchPhase.Began && Physics.Raycast(ray, out hit) && hit.collider.name == "Lever")
                        {
                            gameState = GameState.PrePullDown;
                        }
                    }
                    break;
                }
            // Game is in lever being pulled state
            case (GameState.PrePullDown):
                {
                    // Rotate the lever so that it looks like it's being pulled down
                    lever.Rotate(Vector3.right * (rotateSpeed * Time.deltaTime));
                    Debug.Log(lever.eulerAngles.x);

                    // Once the euler angles of the lever are greater than 350.0 we want to change the
                    // game state so that the lever is rotated back up and the game starts
                    if (lever.eulerAngles.x >= 350f)
                    {
                        gameState = GameState.PrePullUp;
                    }
                    break;
                }
            // Lever has been pulled
            case (GameState.PrePullUp):
                {
                    // Rotate the lever back to it's default position
                    lever.Rotate(Vector3.left * (rotateSpeed * Time.deltaTime));

                    // Once the euler angles of the lever are <= 280.0 we can change the game state
                    // and start the game properly
                    if(lever.eulerAngles.x <= 280f)
                    {
                        gameState = GameState.PullStarted;
                        StartPull();
                    }
                    break;
                }
        }

    }

    private void StartPull()
    {
        // Don't do anything if there shouldn't be a pull running or if the player doesn't have enough chips to play
        if (gameState != GameState.PullStarted || player.Chips < 10)
        {
            ResetGame();
            return;
        }

        // Take 10 chips to start the game, AddChips does this by passing in a negative amount
        player.AddChips(-10);

        // Play the slot spinning loop animation for spinTime seconds
        anim.Play("Slots_Loop", PlayMode.StopAll);
        Invoke(nameof(PickRandomResult), spinTime);
    }

    private void PickRandomResult()
    {
        // Don't do anything if there shouldn't be a pull running
        if (gameState != GameState.PullStarted)
        {
            ResetGame();
            return;
        }

        // Picks a random number between 0 and 100 for which outcome,
        // is chosen, each outcome has an assigned animation and score
        // value. Outcomes are weighted towards lower payouts
        switch(Random.Range(0, 100))
        {
            case 69: // :D
                {
                    anim.clip = anim.GetClip("Slots_Three_7");
                    score = 1500;
                    break;
                }
            case < 5:
                {
                    anim.clip = anim.GetClip("Slots_Three_Bar");
                    score = 30;
                    break;
                }
            case < 15:
                {
                    anim.clip = anim.GetClip("Slots_Three_Bell");
                    score = 20;
                    break;
                }
            case < 25:
                {
                    anim.clip = anim.GetClip("Slots_Three_Cherry");
                    score = 20;
                    break;
                }
            case < 40:
                {
                    anim.clip = anim.GetClip("Slots_Two_Bar_7");
                    score = 5;
                    break;
                }
            case < 58:
                {
                    anim.clip = anim.GetClip("Slots_Two_Bar_Bell");
                    score = 3;
                    break;
                }
            case < 80:
                {
                    anim.clip = anim.GetClip("Slots_Two_Bar_Cherry");
                    score = 2;
                    break;
                }
            default:
                {
                    anim.clip = anim.GetClip("Slots_Lose");
                    score = 0;
                    break;
                }
        }

        // Play the animation and set the game state to ended
        anim.Play();
        gameState = GameState.PullEnded;
        // Invoke the slots ended method when the animation clip ends
        Invoke(nameof(SlotsPullEnded), anim.clip.length);
    }

    private void SlotsPullEnded()
    {
        // Don't do anything if there's no pull that ended
        if (gameState != GameState.PullEnded)
        {
            ResetGame();
            return;
        }

        if (score != 0)
        {
            // Give player chips from winnings
            player.AddChips(score);

            // Set win text and amount
            gameOverText.text = $"Winner! You Won {score} Chips!";
        }
        else
        {
            gameOverText.text = $"Loser! Better Luck Next Time!";
        }

        // Show text for a couple seconds then reset the game for next spin
        gameOverText.gameObject.SetActive(true);
        Invoke(nameof(ResetGame), resetTime);
    }

    // Resets game variables and objects to default states
    private void ResetGame()
    {
        gameState = GameState.None;
        gameOverText.gameObject.SetActive(false);
        score = 0;
    }
}
