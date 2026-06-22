using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalPlayer : MonoBehaviour
{
    [SerializeField] Transform cardStack;
    [SerializeField] float distanceBetweenCards = -.3f;

    Client client;
    CardSprites cardSprites;

    float cardSize;
    float cardDistance;
    List<GameObject> cardsInHand;

    void Start()
    {
        client = FindFirstObjectByType<Client>();
        cardSprites = FindFirstObjectByType<CardSprites>();
        cardsInHand = new();
        cardSize = cardSprites.GetCardBackSprite().bounds.size.x;
        cardDistance = (cardSize - distanceBetweenCards);
    }

    public IEnumerator DrawCards(string[] cards)
    {
        foreach (string card in cards)
        {
            GameObject cardObject = Instantiate(cardSprites.CardPrefab, cardStack.position, Quaternion.identity);
            cardsInHand.Add(cardObject);
            //StartCoroutine(DoMoveCard(cardObject, card));
            DoMoveDrawnCard(cardObject, card);
            yield return new WaitForSeconds(cardSprites.DrawCardDelaySeconds);

        }
        yield return null;
    }

    void DoMoveDrawnCard(GameObject card, string cardString)
    {
        card.transform.DOScale(new Vector3(0 ,1 ,1), .2f).OnComplete(() => 
        {
            card.GetComponent<SpriteRenderer>().sprite = cardSprites.GetCardSpriteByString(cardString);
            card.GetComponent<SpriteRenderer>().sortingOrder = cardsInHand.Count;
            card.GetComponent<Card>().cardType = cardString;
            card.GetComponent<Card>().cardIndex = cardsInHand.Count - 1;
            card.transform.DOScale(new Vector3(1, 1, 1), .2f);
        });

        float cardX = ((cardsInHand.Count - 1) - (cardsInHand.Count - 1) / 2f) * cardDistance;
        card.transform.DOMove(new Vector3(cardX - (cardX / 2), -3, 0), .5f).SetEase(Ease.OutBack, 1.3f);
        MoveCenterCardDeck(false);
    }

    void MoveCenterCardDeck(bool includeLastCard)
    {
        int amount = includeLastCard ? cardsInHand.Count : cardsInHand.Count - 1;
        for (int i = 0; i < amount; i++)
        {
            float cardX = (i - (cardsInHand.Count - 1) / 2f) * cardDistance;
            cardsInHand[i].transform.DOMove(new Vector3(cardX - (cardX / 2), -3, 0), .5f).SetEase(Ease.OutBack, 1.1f);
        }
    }
}
