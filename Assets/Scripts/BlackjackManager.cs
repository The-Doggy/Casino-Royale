using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackManager : MonoBehaviour
{
    [SerializeField] private List<Card> cards; // This list keeps track of all the cards currently active in the scene, scripts should not need to ever change this
    [SerializeField] private Vector3 playerCardPos; // The position where the first card a player is dealt should be placed
    [SerializeField] private Vector3 dealerCardPos; // The position where the first card the dealer is dealt should be placed
    [SerializeField] private Vector3 cardOffset; // A position offset to add to each subsequent card that is dealt to the dealer/player
    [SerializeField] private TMP_Text dealerValueText; // Text for showing the value of the dealer's hand
    [SerializeField] private TMP_Text playerValueText; // Text for showing the value of the player's hand
    [SerializeField] private Button startGameButton; // Button for starting the game
    [SerializeField] private Button hitButton; // Button for hitting
    [SerializeField] private Button standButton; // Button for standing
    [SerializeField] private TMP_Text gameOverText; // Text for showing whether the player or dealer has won
    [SerializeField] private Transform deckPos; // Position in worldspace where the deck of cards is

    private List<Card> deck = new List<Card>(); // List of cards that are currently in play in the deck
    private List<Card> dealerHand = new List<Card>(); // List of cards that are in the dealers hand
    private List<Card> playerHand = new List<Card>(); // List of cards that are in the players hand
    private bool isStanding; // Is the player standing?
    private bool hasBust; // Has the player gone bust?
    private bool dealerShowCards; // Should the dealer be showing their cards?
    private DealerState dealerState; // Keeps track of what the dealer should be doing at any given time
    private GameState gameState; // Keeps track of what state the game is at

    enum DealerState
    {
        None,
        WaitingForPlayer,
        Thinking,
        Standing,
        Bust
    }

    enum GameState
    {
        None,
        Started,
        DealerWin,
        PlayerWin,
        Push,
        Blackjack
    }

    private void Start()
    {
        // Set game UI components to inactive until the game has started
        dealerValueText.gameObject.SetActive(false);
        playerValueText.gameObject.SetActive(false);
        hitButton.gameObject.SetActive(false);
        standButton.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Ensure that the game has started before running any other logic
        if(gameState == GameState.Started)
        {
            // Get values of each hand
            int displayValue = GetHandValueDisplay(dealerHand);
            int dealerValue = GetHandValue(dealerHand);
            int playerValue = GetHandValue(playerHand);

            // Display hand values
            dealerValueText.text = $"Dealer Hand: {displayValue}";
            playerValueText.text = $"Player Hand: {playerValue}";

            // Player has a blackjack
            if(playerValue == 21 && playerHand.Count == 2 && isStanding)
            {
                gameState = GameState.Blackjack;
                GameOver();
            }

            // Player has bust
            if(playerValue > 21)
            {
                hasBust = true;
                DisableButtons();
            }

            // Player has bust and dealer hasn't bust
            if(hasBust && dealerState != DealerState.Bust)
            {
                Debug.Log("player has bust");
                gameState = GameState.DealerWin;
                GameOver();
            }

            // Player hasn't bust and dealer has bust
            if(hasBust && dealerState == DealerState.Bust)
            {
                Debug.Log("both bust, dealer wins");
                gameState = GameState.DealerWin;
                GameOver();
            }

            // Player standing and dealer has bust
            if(isStanding && dealerState == DealerState.Bust)
            {
                gameState = GameState.PlayerWin;
                GameOver();
            }

            // Is it the dealers turn or is the player standing?
            if(dealerState == DealerState.Thinking)
            {
                // Delay dealer think to add some perspective that the dealer is actually taking time to think, edit 13/06/22: I don't think this does what it should do lol
                StartCoroutine(DealerThink(dealerValue));
            }

            // Player is standing and hasn't bust and dealer is standing
            if ((isStanding && !hasBust) && dealerState == DealerState.Standing)
            {
                if(playerValue > dealerValue)
                {
                    Debug.Log("player wins!");
                    gameState = GameState.PlayerWin;
                    GameOver();
                }
                else if(dealerValue > playerValue)
                {
                    Debug.Log("dealer wins!");
                    gameState = GameState.DealerWin;
                    GameOver();
                }
                else if(playerValue == dealerValue)
                {
                    Debug.Log("push");
                    gameState = GameState.Push;
                    GameOver();
                }
            }
        }       
    }

    // Deals with the decision making process of the dealer
    IEnumerator DealerThink(int dealerValue)
    {
        Debug.Log("DealerThink called");
        // Check the current value of the dealers hand to decide whether to hit or stand
        if (dealerValue >= 17 && dealerValue <= 21)
        {
            dealerState = DealerState.Standing;
            Debug.Log("Dealer is standing");
            EnableButtons();
        }
        // Has the dealer bust?
        else if (dealerValue > 21)
        {
            dealerState = DealerState.Bust;
            Debug.Log("Dealer has bust");
            EnableButtons();
        }
        // Dealer should hit
        else
        {
            DealCard(dealerHand);
            // Check new value of dealer hand
            int newValue = GetHandValue(dealerHand);
            if (newValue > 21)
            {
                dealerState = DealerState.Thinking; // This just triggers the thinking loop to run again, kinda ugly but it works for now
            }
            else if (!isStanding)
            {
                dealerState = DealerState.WaitingForPlayer;
            }

            Debug.Log("Dealer has hit");
            EnableButtons();
        }

        yield return new WaitForSeconds(2f);
    }

    public void StartGame()
    {
        // If the player has no chips they cannot start a game
        if (PlayerManager.Instance.Chips <= 0)
        {
            return;
        }

        // If this somehow gets called while a game is already in progress return
        if (gameState == GameState.Started)
        {
            return;
        }

        // If there are cards in either hand move them to the deck face down
        foreach(Card card in playerHand)
        {
            card.transform.position = deckPos.position;
            card.transform.rotation = deckPos.rotation;
        }
        foreach(Card card in dealerHand)
        {
            card.transform.position = deckPos.position;
            card.transform.rotation = deckPos.rotation;
        }

        // Make sure that all our lists are empty from possible previous games
        deck.Clear();
        dealerHand.Clear();
        playerHand.Clear();

        // Make sure the deck always has the same cards that are in our cards list
        deck.AddRange(cards);

        // Check that all the cards in the deck are loaded and not invalid
        bool canStart = true;
        foreach(Card card in deck)
        {
            // Don't start the game if any cards aren't loaded or are invalid
            if (!card.loaded)
            {
                Debug.LogWarning($"Tried to start blackjack game before card {card.name} has been loaded.");
                canStart = false;
                continue;
            }

            if(card.suit == Card.CardSuit.Invalid || card.value == Card.CardValue.Invalid)
            {
                Debug.LogWarning($"Tried to start blackjack game with invalid card {card.name}.");
                canStart = false;
                continue;
            }
        }

        if(!canStart)
        {
            Debug.LogError($"Blackjack game could not be started due to an error.");
            return;
        }

        // Set the game as started
        gameState = GameState.Started;

        // Reset player vars
        isStanding = false;
        hasBust = false;
        EnableButtons();

        // Deal cards to dealer and player
        DealCard(dealerHand, 2);
        DealCard(playerHand, 2);

        // Set dealer state
        dealerState = DealerState.WaitingForPlayer;

        // Set game related components to their proper state
        startGameButton.gameObject.SetActive(false);
        dealerValueText.gameObject.SetActive(true);
        playerValueText.gameObject.SetActive(true);
        hitButton.gameObject.SetActive(true);
        standButton.gameObject.SetActive(true);
        gameOverText.gameObject.SetActive(false);
    }

    void DealCard(List<Card> hand, int cardsToDeal = 1)
    {
        // Deal the amount of cards specified by cardsToDeal
        for (int i = 0; i < cardsToDeal; i++)
        {
            // Get a random card from the deck
            int cardIndex = Random.Range(0, deck.Count - 1);
            GameObject cardObj = deck[cardIndex].gameObject;

            // Remove the card from the deck and add it to the hand
            hand.Add(deck[cardIndex]);
            deck.RemoveAt(cardIndex);
            
            // Set the gameobject transform to the position it should be
            Vector3 pos = hand.Equals(playerHand) ? playerCardPos : dealerCardPos;
            cardObj.transform.position = pos + (cardOffset * hand.Count);

            // Make sure that the dealer's cards aren't showing if they aren't supposed to be
            if(hand.Equals(dealerHand) && !dealerShowCards && hand.IndexOf(cardObj.GetComponent<Card>()) != 0)
            {
                cardObj.transform.rotation = Quaternion.Euler(Vector3.zero);
            }
            else
            {
                cardObj.transform.rotation = Quaternion.Euler(new Vector3(180f, 0f, 0f));
            }
            
        }
    }

    // Since the dealer has their cards hidden from the player we need a separate method for getting the display
    // value of the dealer's hand
    int GetHandValueDisplay(List<Card> hand)
    {
        int value = 0;
        bool hasAce = false;
        int aceCount = 0;
        foreach(Card card in hand)
        {
            // Don't count dealers cards past the first one if they shouldn't be shown
            if(hand.Equals(dealerHand) && !dealerShowCards && hand.IndexOf(card) != 0)
            {
                continue;
            }

            // Is the card an ace?
            if(card.value == Card.CardValue.Ace)
            {
                hasAce = true;
                aceCount++;
            }
            else
            {
                value += (int)card.value;
            }
        }

        // Hand has an ace in it
        if(hasAce)
        {
            // Would the ace cause a bust?
            if(value + 11 > 21)
            {
                // Yes bust
                value += aceCount;
            }
            else
            {
                // No bust
                value += 11;
            }
        }

        return value;
    }

    // Returns the current integer value of a given hand
    int GetHandValue(List<Card> hand)
    {
        int value = 0;
        bool hasAce = false;
        int aceCount = 0;
        foreach (Card card in hand)
        {
            // Is the card an ace?
            if (card.value == Card.CardValue.Ace)
            {
                hasAce = true;
                aceCount++;
            }
            else
            {
                value += (int)card.value;
            }
        }

        // Hand has an ace in it
        if (hasAce)
        {
            // Would the ace cause a bust?
            if (value + 11 > 21)
            {
                // Yes bust
                value += aceCount;
            }
            else
            {
                // No bust
                value += 11;
            }
        }

        return value;
    }

    // Deals a card to the player
    public void PlayerHit()
    {
        DealCard(playerHand);
        dealerState = dealerState == DealerState.WaitingForPlayer ? DealerState.Thinking : dealerState; // If the dealerState is currently WaitingForPlayer set it to Thinking
        //DisableButtons();
        Debug.Log("Player hit");
    }

    // Makes the player stand
    public void PlayerStand()
    {
        isStanding = true;
        dealerState = dealerState == DealerState.WaitingForPlayer ? DealerState.Thinking : dealerState; // If the dealerState is currently WaitingForPlayer set it to Thinking
        DisableButtons();
        Debug.Log("Player stand");
    }

    void EnableButtons()
    {
        hitButton.interactable = true;
        standButton.interactable = true;
    }

    void DisableButtons()
    {
        hitButton.interactable = false;
        standButton.interactable = false;
    }

    void GameOver()
    {
        // Show the dealers cards to the player
        foreach(Card card in dealerHand)
        {
            card.gameObject.transform.rotation = Quaternion.Euler(new Vector3(180f, 0f, 0f));
        }
        
        // Display text for whether the dealer won, player won, player blackjack or push
        gameOverText.text = gameState == GameState.DealerWin ? "Dealer Wins!" : gameState == GameState.PlayerWin ? "Player Wins!" : gameState == GameState.Blackjack ? "Blackjack!" : "Push!";

        // Change the start button text to show Play Again instead
        startGameButton.GetComponentInChildren<TMP_Text>().text = "Play Again";

        // Enable the text and button
        gameOverText.gameObject.SetActive(true);
        startGameButton.gameObject.SetActive(true);
        DisableButtons(); // This just disables the hit and stand buttons
        gameState = GameState.None; // Reset the game state

        // Show the actual value of the dealer's hand
        int dealerValue = GetHandValue(dealerHand);
        dealerValueText.text = $"Dealer Hand: {dealerValue}";
    }
}