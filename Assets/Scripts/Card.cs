using DG.Tweening;
using UnityEngine;

public class Card : MonoBehaviour
{
    public bool IsPlayerCard = false;
    public bool CanBeUsed = false;
    public string CardType;
    public int CardIndex;
    [Space]
    [SerializeField] float hoverHeight = .8f;

    bool isHovering = false;
    Transform spriteObject;

    private void Start()
    {
        spriteObject = transform.GetChild(0).transform;
    }

    private void OnMouseEnter()
    {
        if (!CanBeUsed || isHovering) return;
        spriteObject.DOMoveY(transform.position.y + hoverHeight, .2f).SetEase(Ease.OutBack);
        isHovering = true;
    }

    private void OnMouseExit()
    {
        if (!CanBeUsed || !isHovering) return;
        spriteObject.DOMoveY(transform.position.y, .2f).SetEase(Ease.OutBack);
        isHovering = false;
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (!isHovering) return;


        isHovering = false;
        CanBeUsed = false;
        FindFirstObjectByType<LocalPlayer>().PlayCard(this);
    }

    public void PlayCard()
    {
        //set card to the spriteObject position
        transform.position = spriteObject.position;
        spriteObject.position = transform.position;
        IsPlayerCard = false;
    }

    /// <summary>
    /// Returns card to the down/unhovered position
    /// </summary>
    public void DownCard()
    {
        spriteObject.DOMoveY(transform.position.y, .2f).SetEase(Ease.OutBack);
    }
}
