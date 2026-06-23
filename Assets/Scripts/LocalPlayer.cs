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

    Client client;
    CardSprites cardSprites;

    float cardSize;
    float cardDistance;
    List<Card> cardsInHand;

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
            cardsInHand.Add(cardObject);
            //StartCoroutine(DoMoveCard(cardObject, card));
            DoMoveDrawnCard(cardObject, card);
            yield return new WaitForSeconds(cardSprites.DrawCardDelaySeconds);

        }
        yield return null;
    }

    void DoMoveDrawnCard(Card card, string cardString)
    {
        card.IsPlayerCard = true;
        card.CardType = cardString;
        card.CardIndex = cardsInHand.Count - 1;
        card.CanBeUsed = false;
        card.transform.DOScale(new Vector3(0, 1, 1), .2f).OnComplete(() =>
        {
            card.GetComponent<SpriteRenderer>().sprite = cardSprites.GetCardSpriteByString(cardString);
            card.GetComponent<SpriteRenderer>().sortingOrder = cardsInHand.Count;
            card.transform.DOScale(new Vector3(1, 1, 1), .2f);
        });

        float cardX = ((cardsInHand.Count - 1) - (cardsInHand.Count - 1) / 2f) * cardDistance;
        card.transform.DOMove(new Vector3(cardX - (cardX / 2), -3, 0), .5f).SetEase(Ease.OutBack, 1.3f).OnComplete(() => { card.CanBeUsed = true; });
        MoveCenterCardDeck(false);
    }

    void MoveCenterCardDeck(bool includeLastCard)
    {
        cardDistance = (cardSize + distanceBetweenCards);
        int amount = includeLastCard ? cardsInHand.Count : cardsInHand.Count - 1;
        for (int i = 0; i < amount; i++)
        {
            cardsInHand[i].CanBeUsed = false;
            float cardX = (i - (cardsInHand.Count - 1) / 2f) * cardDistance;
            cardsInHand[i].transform.DOMove(new Vector3(cardX - (cardX / 2), -3, 0), .5f).SetEase(Ease.OutBack, 1.1f).OnComplete(() => { cardsInHand[i].CanBeUsed = true; });
            cardsInHand[i].GetComponent<SpriteRenderer>().sortingOrder = i;
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
        foreach ((int, Card) card in intCards)
        {
            cardsInHand.Add(card.Item2);
        }

        MoveCenterCardDeck(true);
    }
}
