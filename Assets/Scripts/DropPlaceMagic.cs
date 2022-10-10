using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlaceMagic : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        CardController card = eventData.pointerDrag.GetComponent<CardController>();
        CardController magicCard = card;
        DropPlace dropPlace = null;
        if (magicCard != null && magicCard.model.magic)
        {
            //Debug.Log(card.model.name);
            card.movement.cardParent = this.transform;
            card.OnEndDrag.Invoke(dropPlace);
            GameManager.instance.MagicCard(magicCard);
            Destroy(magicCard);
        }
    }
}
