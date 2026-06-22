using System.Collections;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    CardSprites cardSprites;

    private void Start()
    {
        cardSprites = FindFirstObjectByType<CardSprites>();
    }
}
