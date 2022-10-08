using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

// フィールドにアタッチするクラス
public class DropPlace : MonoBehaviour, IDropHandler
{
    public int x,y;
    public UnityEvent<Vector2Int, CardController> OnChangeCard = new UnityEvent<Vector2Int, CardController>();
    [SerializeField] FieldController myFieldController;

    public void OnDrop(PointerEventData eventData) // ドロップされた時に行う処理
    {
        CardController card = eventData.pointerDrag.GetComponent<CardController>(); // 今回の書き換え部分
        if (card != null) // もしカードがあれば、
        {
            if (card.model.canUse == true)  // 使用可能なカードなら、
            {
                if (this.transform.position.y <= 384) //プレイヤーのフィールドの時
                {
                    card.movement.cardParent = this.transform; // カードの親要素を自分（アタッチされてるオブジェクト）にする 今回の書き換え部分
                    card.OnEndDrag.Invoke(this);
                    DropFieldCard(card);
                }
            }
        }
    }

    public void DropFieldCard(CardController card)
    {
        OnChangeCard.Invoke(new Vector2Int(x, y), card);
        card.DropField(myFieldController); // カードをフィールドに置いた時の処理を行う
    }
}