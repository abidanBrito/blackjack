#undef ARRAY_SHUFFLE

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

internal static class Constants
{
    public const int DeckCards = 52;
    public const int Blackjack = 21;
    public const int DealerStand = 17;
}

internal enum WinCode { DealerWins, PlayerWins, Draw }

public class Deck : MonoBehaviour
{
    public Sprite[] faces;
    public GameObject dealer;
    public GameObject player;
    public Button hitButton;
    public Button stickButton;
    public Button playAgainButton;
    public Text finalMessage;
    public Text probMessage;

    public int[] values = new int[Constants.DeckCards];
    int cardIndex = 0;  
       
    private void Awake()
    {    
        InitCardValues();
    }

    private void Start()
    {
        ShuffleCards();
        StartGame();   
    }

    // O(n) -> Linear time complexity
    private void InitCardValues()
    {
        int count = 0;
        for (int i = 0; i < values.Length; ++i) 
        {
            // This only affects the J, Q, K cards
            if (count > 9)
            {
                values[i] = 10; 
                values[++i] = 10;
                values[++i] = 10;
                count = 0;
            }
            else
            {
                values[i] = count + 1; 
                count++;
            }
        }
    }

    // Swap algorithms by un/definining `ARRAY_SHUFFLE` at the top
    private void ShuffleCards()
    {
        #if (ARRAY_SHUFFLE)
            ArrayShuffle();
        #else
            FisherYatesShuffle();
        #endif
    }

    // O(n) -> Linear time complexity
    private void FisherYatesShuffle()
    {
        for (int i = 0; i < values.Length; ++i)
        {
            int rndIndex = Random.Range(i, values.Length);

            // Swap sprites
            Sprite currCard = faces[i];
            faces[i] = faces[rndIndex];
            faces[rndIndex] = currCard;

            // Swap card values
            int currValue = values[i];
            values[i] = values[rndIndex];
            values[rndIndex] = currValue;
        }
    }

    // O(n * log n) -> Linearithmic time complexity
    private void ArrayShuffle()
    {
        // Randomized indices array -> [0, values.Length - 1]
        System.Random rnd = new System.Random();
        int[] index = Enumerable.Range(0, values.Length).ToArray();
        index.OrderBy(_ => rnd.Next()).ToArray();
        
        // Temporary arrays for shuffling
        int[] tmpValues = new int[Constants.DeckCards];
        Sprite[] tmpFaces = new Sprite[Constants.DeckCards];

        // Copy the elements by means of the randomized indices
        for (int i = 0; i < Constants.DeckCards; ++i)
        {
            tmpValues[index[i]] = values[i];
            tmpFaces[index[i]] = faces[i];
        }

        // Update the resulting arrays
        for (int i = 0; i < Constants.DeckCards; ++i)
        {
            values[i] = tmpValues[i];
            faces[i] = tmpFaces[i];
        }
    }

    private void StartGame()
    {
        for (int i = 0; i < 2; ++i)
        {
            PushPlayer();
            PushDealer();
        }
        
        if (CheckBlackJack(player))
        {
            if (CheckBlackJack(dealer)) { EndHand(WinCode.Draw); }         // Draw
            else { EndHand(WinCode.PlayerWins); }                          // Player wins
        }
        else if (CheckBlackJack(dealer)) { EndHand(WinCode.DealerWins); }  // Dealer wins
    }

    private bool CheckBlackJack(GameObject whoever) =>
        whoever.GetComponent<CardHand>()?.points == 21;

    private int GetPlayerPoints() => player.GetComponent<CardHand>().points;

    private int GetDealerPoints() => dealer.GetComponent<CardHand>().points;

    private void CalculateProbabilities()
    {
        probMessage.text = ProbabilityDealerHigher() + " % | " + 
            ProbabilityPlayerInBetween() + " % | " +
            ProbabibilityPlayerOver() + " %";
    }

    // Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador
    private float ProbabilityDealerHigher()
    {
        int playerPoints = GetPlayerPoints();
        int dealerPoints = values[3];
        float favorableCases = 0;

        if ((dealerPoints + values[1]) > playerPoints ||
            (values[1] == 11 && (dealerPoints + 1) > playerPoints))
        {
            favorableCases++;
        }

        for (int i = cardIndex; i < values.Length - 1; ++i)
        {
            if ((dealerPoints + values[i]) > playerPoints) favorableCases++;
            if (dealerPoints > 10 && (dealerPoints + values[i]) > playerPoints) favorableCases++;
        }

        return Mathf.Floor(favorableCases / (Constants.DeckCards - cardIndex)  * 100);
    }

    //  Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
    private float ProbabilityPlayerInBetween()
    {
        return 0.0f;
    }

    // Probabilidad de que el jugador obtenga más de 21 si pide una carta
    private float ProbabibilityPlayerOver()
    {

    }

    private void PushDealer()
    {
        dealer.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;
    }

    private void PushPlayer()
    {
        player.GetComponent<CardHand>().Push(faces[cardIndex], values[cardIndex]);
        cardIndex++;

        CalculateProbabilities();
    }

    public void Hit()
    {
        PushPlayer();
        FlipDealerCard();

        int playerPoints = GetPlayerPoints();

        // Check for Blackjack and the win
        if (playerPoints > Constants.Blackjack) { EndHand(WinCode.DealerWins); }
        else if (playerPoints == Constants.Blackjack) { EndHand(WinCode.PlayerWins); }
    }

    public void Stand()
    {
        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points;

        while (dealerPoints < Constants.DealerStand)
        {
            PushDealer();
            dealerPoints = dealer.GetComponent<CardHand>().points;
        }

        if (dealerPoints > Constants.Blackjack || dealerPoints < playerPoints) 
        { 
            EndHand(WinCode.PlayerWins); 
        }
        playerPoints < dealerPoints ? EndHand(WinCode.DealerWins) : EndHand(WinCode.Draw);
    }

    public void FlipDealerCard() => dealer.GetComponent<CardHand>().cards[0].
                                    GetComponent<CardModel>().ToggleFace(true);

    private void EndHand(WinCode code)
    {
        hitButton.interactable = false;
        stickButton.interactable = false;
        FlipDealerCard();

        switch (code)
        {
            case WinCode.DealerWins:
                finalMessage.text = "You lose!";
                break;
            case WinCode.PlayerWins:
                finalMessage.text = "You win!";
                break;
            case WinCode.Draw:
                finalMessage.text = "Draw!";
                break;
            default:
                Debug.Assert(false);    // Report invalid input
                break;
        }
    }

    public void PlayAgain()
    {
        hitButton.interactable = true;
        stickButton.interactable = true;
        finalMessage.text = "";
        player.GetComponent<CardHand>().Clear();
        dealer.GetComponent<CardHand>().Clear();          
        cardIndex = 0;
        ShuffleCards();
        StartGame();
    }
}