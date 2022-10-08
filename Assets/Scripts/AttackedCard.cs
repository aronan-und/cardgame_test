using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

// 攻撃される側のコード
public class AttackedCard : MonoBehaviour, IDropHandler
{
    public UnityEvent<CardController> OnAttacked = new UnityEvent<CardController>();
    public void OnDrop(PointerEventData eventData)
    {
        /// 攻撃
        // attackerを選択　マウスポインターに重なったカードをアタッカーにする
        CardController attackCard = eventData.pointerDrag.GetComponent<CardController>();

        // defenderを選択　
        CardController defenceCard = GetComponent<CardController>();

        // フィールドのカードとバトルする
        if (defenceCard.model.FieldCard)
        {
            if(GameManager.instance.CheckCardBattle(attackCard, defenceCard))
            {
                Debug.Log("CheckCardBattle true");
                OnAttacked.Invoke(defenceCard);
            }
        }
    }
}
