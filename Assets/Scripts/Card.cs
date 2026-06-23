using UnityEngine;

public class Card : MonoBehaviour
{
    public bool IsPlayerCard = false;
    public bool CanBeUsed = false;
    public string CardType;
    public int CardIndex;

    private void OnMouseEnter()
    {
        Debug.Log("I'm in.   " + name);
    }

    private void OnMouseExit()
    {
        Debug.Log("1.3 seconds     " + name);
    }
}
