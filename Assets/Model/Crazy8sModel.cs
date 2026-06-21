using System;
using System.Collections.Generic;
using UnityEngine;

public class Crazy8sModel
{
    public event Action<int, string> OnSetCardsInHand;

    public const int NumPlayers = 4;
    public const int StartCardCount = 7;

    //C clubs S spades H hearts D diamonds     1 ace     1-10 normal    11-13 jack queen king     J joker
    List<string> cardDeck;
    Dictionary<int, string[]> playerCards = new();

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
        cardDeck = new List<string>( new string[54] ); //52 normal cards + 2 jokers

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

    void SetPlayerStartingCards()
    {
        for (int i = 0; i < NumPlayers; i++)
        {
            string playerCards = "";
            for (int j = 0; j < StartCardCount; j++)
            {
                if (cardDeck.Count == 0) //would only be true if there are more than Players * StartCardCount than cards in the deck
                {
                    FillDeck();
                    ShuffleDeck();
                }

                playerCards += cardDeck[0] + " ";
                cardDeck.RemoveAt(0);
            }
            OnSetCardsInHand(i, playerCards);
        }
    }
}
