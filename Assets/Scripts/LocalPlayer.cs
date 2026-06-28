using DG.Tweening;
using NaughtyAttributes;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class LocalPlayer : MonoBehaviour
{
    [SerializeField] Transform cardStack;
    [SerializeField] float distanceBetweenCards = -.3f;
    [SerializeField] Transform currentCardPos;
    [Header("Other Players")]
    [SerializeField] OtherPlayerCard player2;
    [SerializeField] OtherPlayerCard player3;
    [SerializeField] OtherPlayerCard player4;
    [Header("Turn Indicator")]
    [SerializeField] Transform turnIndicator;
    [SerializeField] Transform turnIndicatorPos;
    [Header("Joker")]
    [SerializeField] TextMeshPro suitText; //used for showing the suit chosen for the joker
    [SerializeField] GameObject suitChoice;


    Client client;
    CardSprites cardSprites;

    float cardSize;
    float cardDistance;
    bool finishedDealingCards = false;
    List<Card> cardsInHand;
    Card currentCard; //the card currently on the table
    int playedCardCounter = 0;
    bool isThisPlayerTurn = false;

    Card jokerToBePlayed;
    bool isChoosingSuit = false;
    string chosenCarduit = "";
    bool isNewJoker = false;

    void Start()
    {
        client = FindFirstObjectByType<Client>();
        cardSprites = FindFirstObjectByType<CardSprites>();
        cardsInHand = new();
        cardSize = cardSprites.GetCardBackSprite().bounds.size.x;
    }

    /// <summary>
    /// Creates/draws cards to the hand of this player.
    /// </summary>
    /// <param name="cards">Array of cards.</param>
    public IEnumerator DrawCards(string[] cards)
    {
        foreach (string card in cards)
        {
            Card cardObject = Instantiate(cardSprites.CardPrefab, cardStack.position, Quaternion.identity).GetComponent<Card>();
            cardObject.transform.position = new Vector3(cardObject.transform.position.x, cardObject.transform.position.y, cardsInHand.Count * -.1f);
            cardObject.name = "card " + cardsInHand.Count;
            cardsInHand.Add(cardObject);
            DoMoveDrawnCard(cardObject, card);
            yield return new WaitForSeconds(cardSprites.DrawCardDelaySeconds);

        }
        finishedDealingCards = true;
        yield return null;
    }

    /// <summary>
    /// Creates/sets the first card in play.
    /// </summary>
    public IEnumerator SetFirstCard(string card)
    {
        yield return new WaitUntil(() => finishedDealingCards);
        yield return new WaitForSeconds(cardSprites.DrawCardDelaySeconds * 1.6f);
        Card cardObject = Instantiate(cardSprites.CardPrefab, cardStack.position, Quaternion.identity).GetComponent<Card>();
        cardObject.name = "firstCard";
        DoMovePlayedCard(cardObject, card);
        yield return new WaitForSeconds(.5f);
        if (isThisPlayerTurn)
            SetCardsUsable(true);
    }

    /// <summary>
    /// Moves a drawn card to this players hand.
    /// </summary>
    void DoMoveDrawnCard(Card card, string cardString)
    {
        card.IsPlayerCard = true;
        card.CardType = cardString;
        card.CardIndex = cardsInHand.Count - 1;
        card.CanBeUsed = false;
        card.transform.DOScale(new Vector3(0, 1, 1), .2f).OnComplete(() =>
        {
            card.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = cardSprites.GetCardSpriteByString(cardString);
            card.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = cardsInHand.Count;
            card.transform.DOScale(new Vector3(1, 1, 1), .2f);
        });

        float cardX = ((cardsInHand.Count - 1) - (cardsInHand.Count - 1) / 2f) * cardDistance;
        card.transform.DOMove(new Vector3(cardX - (cardX / 2), -3, card.transform.position.z), .5f).SetEase(Ease.OutBack, 1.3f);
        MoveCenterCardDeck(false);
    }

    /// <summary>
    /// Moves a card to the center play area while flipping.
    /// </summary>
    void DoMovePlayedCard(Card card, string cardString)
    {
        card.IsPlayerCard = true;
        card.CardType = cardString;
        card.CanBeUsed = false;
        card.transform.DOScale(new Vector3(0, 1, 1), .2f).OnComplete(() =>
        {
            card.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = cardSprites.GetCardSpriteByString(cardString);
            card.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = (playedCardCounter + 10) * 10;
            playedCardCounter++;
            card.transform.DOScale(new Vector3(1, 1, 1), .2f);
        });

        card.transform.DOMove(currentCardPos.position, .5f).SetEase(Ease.OutBack, 1.3f).OnComplete(() =>
        {
            if (currentCard != null)
            {
                Destroy(currentCard.gameObject);
            }
            currentCard = card;
        });
    }

    /// <summary>
    /// Moves the cards in hand to be centered around the middle of the screen.
    /// </summary>
    /// <param name="includeLastCard">Whether the last card in hand will be moved as well.</param>
    void MoveCenterCardDeck(bool includeLastCard)
    {
        cardDistance = (cardSize + distanceBetweenCards);
        int amount = includeLastCard ? cardsInHand.Count : cardsInHand.Count - 1;
        SetCardsUsable(false);
        for (int i = 0; i < amount; i++)
        {
            float cardX = (i - (cardsInHand.Count - 1) / 2f) * cardDistance;
            cardsInHand[i].transform.DOMove(new Vector3(cardX - (cardX / 2), -3, cardsInHand[i].transform.position.z), .5f).SetEase(Ease.OutBack, 1.1f);
            cardsInHand[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = i;
        }
        if (finishedDealingCards && isThisPlayerTurn)
            StartCoroutine(SetCardsUsable(true, .5f)); //this cannot be done in oncomplete, because it will try to reference i after the delay, meaning it will reference the wrong thing
    }

    /// <summary>
    /// Sets all cards in hand to be usable/unusable.
    /// </summary>
    void SetCardsUsable(bool usable)
    {
        foreach (Card card in cardsInHand)
        {
            card.CanBeUsed = usable;
        }
    }

    /// <summary>
    /// Sets all cards in hand to be usable/unusable after a delay of delaySeconds
    /// </summary>
    IEnumerator SetCardsUsable(bool usable, float delaySeconds, bool checkThisPlayerTurn = true)
    {
        yield return new WaitForSeconds(delaySeconds);
        if (!isThisPlayerTurn) yield break;
        foreach (Card card in cardsInHand)
        {
            card.CanBeUsed = usable;
        }
    }

    /// <summary>
    /// Sort the current hand of this player.
    /// </summary>
    public void SortDeck()
    {
        List<(int, Card)> intCards = new();
        foreach (Card card in cardsInHand)
        {
            switch (card.CardType[0])
            {
                case 'C':
                    intCards.Add((0 + int.Parse(card.CardType.Remove(0, 1)), card));
                    break;
                case 'S':
                    intCards.Add((100 + int.Parse(card.CardType.Remove(0, 1)), card));
                    break;
                case 'H':
                    intCards.Add((200 + int.Parse(card.CardType.Remove(0, 1)), card));
                    break;
                case 'D':
                    intCards.Add((300 + int.Parse(card.CardType.Remove(0, 1)), card));
                    break;
                case 'J':
                    intCards.Add((400, card));
                    break;
            }
        }

        intCards = intCards.OrderBy(x => x.Item1).ToList();

        cardsInHand.Clear();
        for (int i = 0; i < intCards.Count; i++)
        {
            cardsInHand.Add(intCards[i].Item2);
            intCards[i].Item2.transform.position = new Vector3(intCards[i].Item2.transform.position.x, intCards[i].Item2.transform.position.y, i * -.1f);
            intCards[i].Item2.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = i;
        }

        MoveCenterCardDeck(true);
    }

    /// <summary>
    /// When this player plays a card.
    /// </summary>
    public void PlayCard(Card card)
    {
        if (CheckCardCanBePlayed(card.CardType))
        {
            chosenCarduit = "";
            suitText.text = "";
            Debug.Log("Played card: " + card.CardType + "  of index: " + card.CardIndex);
            card.PlayCard();
            card.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = (playedCardCounter + 10) * 10;
            playedCardCounter++;
            GameObject oldCard = currentCard.gameObject;
            card.transform.DOMove(currentCardPos.position, .5f).SetEase(Ease.OutBack, 1.3f).OnComplete(() => { Destroy(oldCard); });
            currentCard = card;
            cardsInHand.Remove(card);

            foreach (Card restCard in cardsInHand)
            {
                if (restCard.CardIndex > card.CardIndex) restCard.CardIndex--;
            }

            MoveCenterCardDeck(true);
            if (card.CardType == "J")
            {
                jokerToBePlayed = card;
                StartJokerChoice();
                return;
            }
            client.PlayCard(card.CardIndex);
        }
        else
        {
            card.DownCard();
        }
    }

    void StartJokerChoice()
    {
        SetCardsUsable(false);
        suitChoice.SetActive(true);
        isChoosingSuit = true;
    }

    public void ChooseSuite(string suit)
    {
        suitChoice.SetActive(false);
        client.PlayCard(jokerToBePlayed.CardIndex); //put some sort of different var here for jack
        client.ChooseJokerSuit(suit);
        jokerToBePlayed = null;
        isThisPlayerTurn = false;
        isChoosingSuit = false;
    }

    /// <summary>
    /// When another player played a card.
    /// </summary>
    /// <param name="player">Player index starting at 1</param>
    public void PlayerPlayedCard(int player, string card)
    {
        print("Is turn: " + isThisPlayerTurn + "   this playerID: " + client.playerID + "   playerPlayingCard: " + player);
        if (isThisPlayerTurn || client.playerID == player - 1) return;
        chosenCarduit = "";
        suitText.text = "";
        Vector3 position = cardStack.position;
        int thisPlayerId = client.playerID + 1;
        int other = thisPlayerId - player > 0 ? thisPlayerId - player : 4 + (thisPlayerId - player);
        switch (other)
        {
            case 1:
                position = player4.transform.position;
                break;
            case 2:
                position = player3.transform.position;
                break;
            case 3:
                position = player2.transform.position;
                break;
        }

        Card cardObject = Instantiate(cardSprites.CardPrefab, position, Quaternion.identity).GetComponent<Card>();
        cardObject.name = "playedCard " + card;
        DoMovePlayedCard(cardObject, card);
        ChangePlayerCardCount(player, -1);
    }

    public void PlayerChoseSuit(string suit)
    {
        chosenCarduit = suit;
        isNewJoker = true;
        switch (suit)
        {
            case "C":
                suitText.text = "Clubs";
                break;
            case "S":
                suitText.text = "Spades";
                break;
            case "H":
                suitText.text = "Hearts";
                break;
            case "D":
                suitText.text = "Diamonds";
                break;
        }
    }

    /// <summary>
    /// Sends the request to draw a card to the server.
    /// </summary>
    public void PlayerDrawNewCard()
    {
        if (!isThisPlayerTurn || isChoosingSuit) return;
        client.DrawCard();
        isThisPlayerTurn = false;
        SetCardsUsable(false);
    }

    /// <summary>
    /// Draws cards to the hand of another player.
    /// </summary>
    /// <param name="player">Player index starting at 1.</param>
    public IEnumerator PlayerDrawCards(int player, int count)
    {
        isNewJoker = false;
        for (int i = 0; i < count; i++)
        {
            GameObject card = Instantiate(cardSprites.CardPrefab, cardStack.position, Quaternion.identity);
            card.transform.DOMove(GetOtherPlayerHandPosition(player), .5f).SetEase(Ease.OutBack, 1.3f).OnComplete(() =>
            {
                ChangePlayerCardCount(player, 1);
                Destroy(card);
            });
            yield return new WaitForSeconds(cardSprites.DrawCardDelaySeconds);
        }
    }

    /// <summary>
    /// Gets the hand position of the player index.
    /// </summary>
    /// <param name="player">Player index starting at 1.</param>
    Vector3 GetOtherPlayerHandPosition(int player)
    {
        Vector3 position = cardStack.position;
        int thisPlayerId = client.playerID + 1;

        int other = thisPlayerId - player > 0 ? thisPlayerId - player : 4 + (thisPlayerId - player);
        switch (other)
        {
            case 1:
                position = player4.transform.position;
                break;
            case 2:
                position = player3.transform.position;
                break;
            case 3:
                position = player2.transform.position;
                break;
        }
        return position;
    }

    /// <summary>
    /// Processes the turn change.
    /// </summary>
    /// <param name="playerTurn">Player ID whose Turn it is. Starting at 1.</param>
    public void OnChangeTurn(int playerTurn)
    {
        if (playerTurn - 1 == client.playerID)
        {
            isThisPlayerTurn = true;
            SetCardsUsable(true);
            turnIndicator.DOScale(0, .2f).OnComplete(() =>
            {
                turnIndicator.position = turnIndicatorPos.position;
                turnIndicator.DOScale(1, .2f);
            });
        }
        else
        {
            isThisPlayerTurn = false;
            SetCardsUsable(false);
            Vector3 arrowPos = GetOtherPlayerHandPosition(playerTurn) + new Vector3(0, -1.3f, 0);
            turnIndicator.DOScale(new Vector3(0, 1, 1), .2f).OnComplete(() => 
            { 
                turnIndicator.position = arrowPos;
                turnIndicator.DOScale(1, .2f);
            });
            
        }
    }

    /// <summary>
    /// Checks whether a card can be played.
    /// </summary>
    bool CheckCardCanBePlayed(string card)
    {
        if (currentCard == null) return true;
        if (isNewJoker && card != "J") return false;
        if (chosenCarduit != "")
        {
            if (chosenCarduit == card[0].ToString()) return true;
            else if (card == "J") return true;
            else return false;
        }

        if (card == "J") return true;
        if (currentCard.CardType[0] == card[0]) return true;
        if (currentCard.CardType.Remove(0, 1) == card.Remove(0, 1)) return true;
        return false;
    }

    /// <summary>
    /// Sets the name of the other players.
    /// </summary>
    public void SetOtherPlayers()
    {
        player2.SetPlayerName("Player " + (client.playerID + 2 > 4 ? Mathf.Abs(4 - (client.playerID + 2)) : client.playerID + 2));
        player3.SetPlayerName("Player " + (client.playerID + 3 > 4 ? Mathf.Abs(4 - (client.playerID + 3)) : client.playerID + 3));
        player4.SetPlayerName("Player " + (client.playerID + 4 > 4 ? Mathf.Abs(4 - (client.playerID + 4)) : client.playerID + 4));
    }

    /// <summary>
    /// Changes the card count on another player by player id.
    /// </summary>
    /// <param name="player">Player id starting at 0.</param>
    void ChangePlayerCardCount(int player, int change)
    {
        int thisPlayerId = client.playerID + 1;
        int other = thisPlayerId - player > 0 ? thisPlayerId - player : 4 + (thisPlayerId - player);
        switch (other)
        {
            case 1:
                player4.ChangeCardCount(change);
                break;
            case 2:
                player3.ChangeCardCount(change);
                break;
            case 3:
                player2.ChangeCardCount(change);
                break;
        }
    }
}
