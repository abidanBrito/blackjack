﻿#undef  ARRAY_SHUFFLE

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

internal static class Constants
{
    public const int Blackjack = 21;
}

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

    public int[] values = new int[52];
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

    // Swap algorithms by un/definining `ARRAY_SHUFFLE` at the top.
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
            // Random index
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

    // 2 * O(n) = O(n) -> Linear time complexity
    private void ArrayShuffle() 
    {
        // Randomized indices array -> [0, 52]
        System.Random rnd = new System.Random();
        int[] index = Enumerable.Range(0, values.Length).ToArray().OrderBy(x => rnd.Next()).ToArray();
        
        // Temporary arrays for shuffling
        int[] tmpValues = new int[52];
        Sprite[] tmpFaces = new Sprite[52];

        // Copy the elements by means of the randomized indices
        for (int i = 0; i < 52; ++i)
        {
            tmpValues[index[i]] = values[i];
            tmpFaces[index[i]] = faces[i];
        }

        // Update the resulting arrays
        for (int i = 0; i < 52; ++i)
        {
            values[i] = tmpValues[i];
            faces[i] = tmpFaces[i];
        }
    }

    private void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();
        }
        
        int playerPoints = values[0] + values[2];
        int dealerPoints = values[1] + values[3];

        if (checkBlackJack(player))
        {
            if (checkBlackJack(dealer)) { EndGame(1); }     // Draw
            else { EndGame(2); }                            // Player wins
        }
        else if (checkBlackJack(dealer)) { EndGame(0); }

        CalculateProbabilities();
    }

    private bool checkBlackJack(GameObject whoever)
    {
        return whoever.GetComponent<CardHand>().points == 21;
    }

    private void CalculateProbabilities()
    {
        probMessage.text = ProbabilityDealerHigher() + " % | " + 
            ProbabilityPlayerInBetween() + " | " +
            ProbabibilityPlayerOver();
    }

    // Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador
    private float ProbabilityDealerHigher()
    {
        int playerPoints = values[0] + values[2];
        int dealerPoints = values[3];
        float favorableCases = 0;

        if ((dealerPoints + values[1]) > playerPoints || 
            (values[1] == 11 && (dealerPoints + 1) > playerPoints)) 
        { 
            favorableCases++; 
        }

        // Se realiza un recorrido por todos los valores de las cartas que están por salir y se
        // cuentas las cartas favorables para que el valor del dealer sea mayor que el valor del jugador
        for (int i = cardIndex; i < values.Length - 1; i++)
        {
            if ((dealerPoints + values[i]) > playerPoints) favorableCases++;

            // Si el valor del dealer es superior a 10, los ases contarán 1 en vez de 11 y se
            // añadirán a cartas favorables si lo son.
            if (dealerPoints > 10 && (dealerPoints + values[i]) > playerPoints) favorableCases++;
        }

        return Mathf.Floor(favorableCases / (52 - cardIndex)  * 100);
    }

    //  Probabilidad de que el jugador obtenga entre un 17 y un 21 si pide una carta
    private float ProbabilityPlayerInBetween()
    {
        return 0.0f;
    }

    // Probabilidad de que el jugador obtenga más de 21 si pide una carta
    private float ProbabibilityPlayerOver()
    {
        return 0.0f;
    }
    
    private void PushDealer()
    {
        dealer.GetComponent<CardHand>().Push(faces[cardIndex],values[cardIndex]);
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
        FlipDealerCard();
        
        // Deal a card to the player
        PushPlayer();

        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points;
    
        if (playerPoints > Constants.Blackjack) { EndGame(0); }
        else if (playerPoints == Constants.Blackjack)
        {
            if (dealerPoints == Constants.Blackjack) 
            {
                EndGame(1);
                return;
            }

            EndGame(2);
        }
 
        // Update probabilities
        CalculateProbabilities();
    }

    public void Stand()
    {
        FlipDealerCard();

        int playerPoints = player.GetComponent<CardHand>().points;
        int dealerPoints = dealer.GetComponent<CardHand>().points;

        while (dealerPoints < 17)
        {
            PushDealer();
            dealerPoints = dealer.GetComponent<CardHand>().points;
        }

        if (dealerPoints > 21 || playerPoints > dealerPoints) { EndGame(2); }
        else if (playerPoints < dealerPoints) { EndGame(0); }
        else { EndGame(1); }
    }
    
    public void FlipDealerCard()
    {
        if (player.GetComponent<CardHand>().cards.Count == 2) 
        {
            dealer.GetComponent<CardHand>().cards[0].GetComponent<CardModel>().ToggleFace(true);
        }
    }

    private void EndGame(int exitCode)
    {
        hitButton.interactable = false;
        stickButton.interactable = false;

        switch (exitCode)
        {
            case 0:
                finalMessage.text = "You lose!";
                break;
            case 1:
                finalMessage.text = "Draw!";
                break;
            case 2:
                finalMessage.text = "You win!";
                break;
            default:
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