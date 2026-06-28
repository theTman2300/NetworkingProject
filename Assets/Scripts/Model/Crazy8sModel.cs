using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Crazy8sModel
{
    public event Action<int, string> OnSetCardsInHand;
    public event Action<string> OnSetFirstCard;
    public event Action<int> OnChangePlayerTurn;
    public event Action<int, string> OnPlayerPlayedCard;
    public event Action<int, string> OnPlayerDrawCards;
    public event Action<string> OnPlayerChooseSuit;

    public const int NumPlayers = 4;
    public const int StartCardCount = 7;

    //C clubs S spades H hearts D diamonds     1 ace     1-10 normal    11-13 jack queen king     J joker
    List<string> cardDeck;
    Dictionary<int, string> playerCards = new();
    int currentPlayer = 1;

    string currentCard = "";
    bool rotationIsClockwise = true;

    //suit choice
    bool expectingSuitChoice = false;
    string suitChoice = "";

    //card drawing
    int jokerCounter = 0;
    int card2Counter = 0; //counter for the 2 cards

    public void Initialize()
    {
        FillDeck();
        ShuffleDeck();
    }

    public void Reset()
    {
        Initialize();
    }

    public void StartGame()
    {
        SetPlayerStartingCards();
    }

    /// <summary>
    /// Resets the decek to be a standard deck of 52 cards + 2 jokers in the order of C clubs S spades H hearts D diamonds.
    /// </summary>
    void FillDeck()
    {
        cardDeck = new List<string>(new string[54]); //52 normal cards + 2 jokers
        //cardDeck = new List<string>(new string[60]); //52 normal cards + 2 jokers

        //C clubs S spades H hearts D diamonds     1 ace     1-10 normal    11-13 jack queen king     J joker
        for (int i = 0; i < 13; i++)
        {
            cardDeck[i] = "C" + (i + 1);
        }
        for (int i = 0; i < 13; i++)
        {
            cardDeck[i + 13] = "S" + (i + 1);
        }
        for (int i = 0; i < 13; i++)
        {
            cardDeck[i + 13 * 2] = "H" + (i + 1);
        }
        for (int i = 0; i < 13; i++)
        {
            cardDeck[i + 13 * 3] = "D" + (i + 1);
        }
        cardDeck[52] = "J";
        cardDeck[53] = "J";


        //these are extra
        //cardDeck[54] = "J";
        //cardDeck[55] = "J";
        //cardDeck[56] = "J";
        //cardDeck[57] = "J";
        //cardDeck[58] = "J";
        //cardDeck[59] = "J";
    }

    /// <summary>
    /// Shuffles the current deck.
    /// </summary>
    void ShuffleDeck()
    {
        //Swaps 2 cards in the deck
        //The amount of swaps is not very important so I just chose a high number
        for (int i = 0; i < 42069; i++)
        {
            int rand1 = UnityEngine.Random.Range(0, cardDeck.Count);
            int rand2 = UnityEngine.Random.Range(0, cardDeck.Count);
            string card1 = cardDeck[rand1];
            string card2 = cardDeck[rand2];
            cardDeck[rand1] = card2;
            cardDeck[rand2] = card1;
        }
    }

    /// <summary>
    /// Prints all the card in the current deck in order of top to bottom.
    /// </summary>
    void PrintDeck()
    {
        string cardTestString = "";
        foreach (string card in cardDeck)
        {
            cardTestString += card + "  ";
        }
        Debug.Log(cardTestString);
    }

    /// <summary>
    /// Tells all players the cards they start out with.
    /// </summary>
    void SetPlayerStartingCards()
    {
        //sets player hands
        for (int i = 0; i < NumPlayers; i++)
        {
            string playerCardsString = "";
            for (int j = 0; j < StartCardCount; j++)
            {
                if (cardDeck.Count == 0) //would only be true if there are more than Players * StartCardCount than cards in the deck
                {
                    FillDeck();
                    ShuffleDeck();
                }

                playerCardsString += cardDeck[0] + " ";
                cardDeck.RemoveAt(0);
            }
            playerCardsString.Trim();
            playerCards[i] = playerCardsString;
            OnSetCardsInHand(i, playerCardsString);
        }

        //sets the first card
        bool firstCardValid = false;
        while (!firstCardValid)
        {
            if (cardDeck[0] == "J") ShuffleDeck();
            else firstCardValid = true;
        }
        if (cardDeck.Count == 0)
        {
            FillDeck();
            ShuffleDeck();
        }
        OnSetFirstCard.Invoke(cardDeck[0]);
        currentCard = cardDeck[0];
        cardDeck.RemoveAt(0);

        OnChangePlayerTurn.Invoke(1);
        currentPlayer = 1;
    }

    /// <summary>
    /// Handles a player playing a card.
    /// </summary>
    /// <param name="playerID">Player ID starting at 0.</param>
    /// <param name="cardIndex">Index of the card in hand, starting at 0.</param>
    public void PlayerPlayedCard(int player, int cardIndex)
    {
        string[] currentPlayerCards = CardStringToArray(playerCards[player - 1]);
        string card = currentPlayerCards[cardIndex];
        if (player != currentPlayer || !CheckCardCanBePlayed(card))
        {
            Debug.Log("someone cheated/error occured");
            return;
        }
        currentCard = card;

        string newPlayerCards = "";
        for (int i = 0; i < currentPlayerCards.Length; i++)
        {
            if (i == cardIndex) continue;
            newPlayerCards += currentPlayerCards[i] + " ";
        }
        newPlayerCards.Trim();
        playerCards[player - 1] = newPlayerCards;
        OnPlayerPlayedCard.Invoke(player, card);

        if (card == "J") //joker (next player draw cards and this player change suit)
        {
            expectingSuitChoice = true;
            jokerCounter++;
            return;
        }

        if (card.Remove(0, 1) == "11") //jack (suit change)
        {
            expectingSuitChoice = true;
            return;
        }

        if (card.Remove(0, 1) == "2") //2 card (next player grab 2 cards)
        {
            card2Counter++;
        }

        if (card.Remove(0, 1) == "8") //8 card (next player skip turn)
        {
            SkipNextPlayerTurn();
            return;
        }

        if (card.Remove(0, 1) == "7") //7 card (you get another turn)
        {
            OnChangePlayerTurn.Invoke(currentPlayer);
            return;
        }

        if (card.Remove(0, 1) == "1") //Ace card (switch direction of play)
        {
            rotationIsClockwise = !rotationIsClockwise;
        }

        NextPlayerTurn();
    }

    public void ChooseSuit(int player, string suit)
    {
        if (player != currentPlayer || !expectingSuitChoice)
        {
            Debug.Log("someone cheated/error occured");
            return;
        }
        expectingSuitChoice = false;

        OnPlayerChooseSuit.Invoke(suit);
        suitChoice = suit;

        NextPlayerTurn();
    }

    /// <summary>
    /// Handles a player drawing a card.
    /// </summary>
    /// <param name="playerID">Player ID starting at 0.</param>
    public void PlayerDrawCard(int playerID)
    {
        //check whether player drawing a card is the current player
        if (playerID != currentPlayer)
        {
            Debug.Log("someone cheated/error occured");
            return;
        }

        int cardAmount = 1;
        if (jokerCounter > 0)
        {
            cardAmount = jokerCounter * 5;
            jokerCounter = 0;
        }
        if (card2Counter > 0)
        {
            cardAmount = card2Counter * 2;
            card2Counter = 0;
        }

        string cardsString = "";
        for (int i = 0; i < cardAmount; i++)
        {
            if (cardDeck.Count == 0)
            {
                FillDeck();
                ShuffleDeck();
            }
            cardsString += cardDeck[0] + " ";
            List<string> currentPlayerCards = CardStringToArray(playerCards[playerID - 1]).ToList();
            currentPlayerCards.Add(cardDeck[0]);
            playerCards[playerID - 1] = CardArrayToString(currentPlayerCards.ToArray());
            cardDeck.RemoveAt(0);
        }
        cardsString.Trim();
        OnPlayerDrawCards.Invoke(playerID, cardsString);
        NextPlayerTurn();
    }

    /// <summary>
    /// Sets the turn to the next player.
    /// </summary>
    void NextPlayerTurn()
    {
        if (rotationIsClockwise)
            currentPlayer = currentPlayer + 1 > NumPlayers ? 1 : currentPlayer + 1;
        else
            currentPlayer = currentPlayer - 1 < 1 ? NumPlayers : currentPlayer - 1;

        OnChangePlayerTurn.Invoke(currentPlayer);
    }

    void SkipNextPlayerTurn()
    {
        if (rotationIsClockwise)
            currentPlayer = currentPlayer + 2 > NumPlayers ? currentPlayer + 2 - NumPlayers : currentPlayer + 2;
        else
            currentPlayer = currentPlayer - 2 < 1 ? currentPlayer - 2 + NumPlayers : currentPlayer - 2;

        OnChangePlayerTurn.Invoke(currentPlayer);
    }

    bool CheckCardCanBePlayed(string card)
    {
        if (currentCard == null) return true;
        if (jokerCounter != 0 && card != "J") return false;
        if (card2Counter != 0 && card.Remove(0, 1) != "2") return false;

        if (suitChoice != "")
        {
            string suit = suitChoice;
            suitChoice = "";
            if (suit == card[0].ToString()) return true;
            else if (card == "J") return true;
            else return false;
        }

        if (card == "J") return true;
        if (currentCard[0] == card[0]) return true;
        if (currentCard.Remove(0, 1) == card.Remove(0, 1)) return true;
        return false;
    }

    /// <summary>
    /// Converts a string of cards into an array of cards.
    /// </summary>
    /// <param name="cardString">String of cards, seperated by a space ' '.</param>
    string[] CardStringToArray(string cardString)
    {
        return cardString.Trim().Split(" "); //trim not necessary but added just to be safe
    }

    /// <summary>
    /// Converts an array of cards into a string of cards, seperated by a space ' '.
    /// </summary>
    string CardArrayToString(string[] cards)
    {
        string cardString = "";
        foreach (string card in cards)
        {
            cardString += card + " ";
        }
        cardString.Trim();
        return cardString;
    }
}
