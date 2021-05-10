using UnityEngine;
using UnityEngine.UI;

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

    // Fisher–Yates shuffle algorithm
    private void ShuffleCards()
    {
        for (int i = 0; i < values.Length; ++i)
        {
            int randomCard = Random.Range(i, values.Length);

            // Swap sprites
            Sprite currCard = faces[i];     
            faces[i] = faces[randomCard];
            faces[randomCard] = currCard;

            // Swap card values
            int currValue = values[i];
            values[i] = values[randomCard];
            values[randomCard] = currValue;
        }
    }

    void StartGame()
    {
        for (int i = 0; i < 2; i++)
        {
            PushPlayer();
            PushDealer();

            CalculateProbabilities();
        }
    }

    private void CalculateProbabilities()
    {
        probMessage.text = ProbabilityDealerHigher() + " | " + 
            ProbabilityPlayerInBetween() + " | " +
            ProbabibilityPlayerOver();
    }

    // Teniendo la carta oculta, probabilidad de que el dealer tenga más puntuación que el jugador
    private float ProbabilityDealerHigher()
    {
        return 0.0f;
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
    
    void PushDealer()
    {
        dealer.GetComponent<CardHand>().Push(faces[cardIndex],values[cardIndex]);
        cardIndex++;        
    }

    void PushPlayer()
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
    
        if (playerPoints > 21) { EndGame(0); }
        else if (playerPoints == 21)
        {
            if (dealerPoints == 21) 
            {
                EndGame(01);
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

        /*TODO:
         * Repartimos cartas al dealer si tiene 16 puntos o menos
         * El dealer se planta al obtener 17 puntos o más
         * Mostramos el mensaje del que ha ganado
         */
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
            case '0':
                finalMessage.text = "You lose!";
                break;
            case '1':
                finalMessage.text = "Draw!";
                break;
            case '2':
                finalMessage.text = "You win!";
                break;
            case '3':
                finalMessage.text = "Blackjack!";
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