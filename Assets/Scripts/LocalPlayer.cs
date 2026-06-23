using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using NaughtyAttributes;

public class LocalPlayer : MonoBehaviour
{
    [SerializeField] Transform cardStack;
    [SerializeField] float distanceBetweenCards = -.3f;
    [SerializeField] Transform currentCardPos;

    Client client;
    CardSprites cardSprites;

    float cardSize;
    float cardDistance;
    bool finishedDealingCards = false;
    List<Card> cardsInHand;
    Card currentCard; //the card currently on the table
    int playedCardCounter = 0;
    bool isThisPlayerTurn = false;

    void Start()
    {
        client = FindFirstObjectByType<Client>();
        cardSprites = FindFirstObjectByType<CardSprites>();
        cardsInHand = new();
        cardSize = cardSprites.GetCardBackSprite().bounds.size.x;
    }

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

    public IEnumerator SetFirstCard(string card)
    {
        yield return new WaitUntil(() => finishedDealingCards);
        yield return new WaitForSeconds(cardSprites.DrawCardDelaySeconds * 1.6f);
        Card cardObject = Instantiate(cardSprites.CardPrefab, cardStack.position, Quaternion.identity).GetComponent<Card>();
        cardObject.name = "firstCard";
        DoMovePlayedCard(cardObject, card);
        yield return new WaitForSeconds(.5f);
        SetCardsUsable(true);
    }

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
            if (currentCard != null){
                Destroy(currentCard.gameObject);}
            currentCard = card;
        });
    }

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
        if (finishedDealingCards)
            StartCoroutine(SetCardsUsable(true, .5f)); //this cannot be done in oncomplete, because it will try to reference i after the delay, meaning it will reference the wrong thing
    }

    void SetCardsUsable(bool usable)
    {
        foreach (Card card in cardsInHand)
        {
            card.CanBeUsed = usable;
        }
    }

    IEnumerator SetCardsUsable(bool usable, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        foreach (Card card in cardsInHand)
        {
            card.CanBeUsed = usable;
        }
    }

    [Button]
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

    public void PlayCard(Card card)
    {
        if (CheckCardCanBePlayed(card.CardType))
        {
            SetCardsUsable(false);
            Debug.Log("Played card: " + card.CardType + "  of index: " + card.CardIndex);
            card.PlayCard();
            card.transform.GetChild(0).GetComponent<SpriteRenderer>().sortingOrder = (playedCardCounter + 10) * 10;
            playedCardCounter++;
            GameObject oldCard = currentCard.gameObject;
            card.transform.DOMove(currentCardPos.position, .5f).SetEase(Ease.OutBack, 1.3f).OnComplete(() => { Destroy(oldCard); });
            currentCard = card;
            client.PlayCard(card.CardIndex);
        }
        else
        {
            card.DownCard();
        }
    }

    public void PlayerPlayedCard(int player, string card)
    {
        if (isThisPlayerTurn) return;
        Card cardObject = Instantiate(cardSprites.CardPrefab, cardStack.position, Quaternion.identity).GetComponent<Card>();
        cardObject.name = "playedCard " + card;
        DoMovePlayedCard(cardObject, card);
    }

    public void OnChangeTurn(int playerTurn, int thisPlayerID)
    {
        if (playerTurn - 1 == thisPlayerID)
        {
            isThisPlayerTurn = true;
        }
        else
        {
            isThisPlayerTurn = false;
        }
        Debug.Log("Is this player turn? " + isThisPlayerTurn);
    }

    bool CheckCardCanBePlayed(string card)
    {
        if (currentCard == null) return true;
        if (card == "J") return true;
        if (currentCard.CardType[0] == card[0]) return true;
        if (currentCard.CardType.Remove(0, 1) == card.Remove(0, 1)) return true;
        return false;
    }
}
