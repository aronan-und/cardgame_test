using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;
using UnityEngine.Events;

public class CardMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    
    public Transform cardParent;
    bool canDrag = true; // カードを動かせるかどうかのフラグ
    bool canDragField = true;
    public UnityEvent BeginDragAction = new UnityEvent();
    public UnityEvent FieldCardDrag = new UnityEvent();
    public UnityEvent CardMoveCancel = new UnityEvent();
    public UnityEvent FieldCardMoveCacel = new UnityEvent();
    public Transform hand;
    public UnityEvent E_CardMove = new UnityEvent();
    public UnityEvent E_CardMove2 = new UnityEvent();

    public void OnBeginDrag(PointerEventData eventData) // ドラッグを始めるときに行う処理
    {
        CardController card = GetComponent<CardController>();
        canDrag = true;
        canDragField = true;

        if (card.model.FieldCard == false) // 手札のカードなら
        {
            if (card.model.canUse == false) // マナコストより少ないカードは動かせない
            {
                canDrag = false;
            }
        }
        else
        {
            if (card.model.canAttack == false) // 攻撃不可能なカードは動かせない
            {
                canDragField = false;
            }
        }

        if (canDrag == false || canDragField == false || card.model.PlayerCard == false)
        {
            return;
        }

        if (card.model.magic)
        {
            GameManager.instance.MagicDropArea.SetActive(true);
        }

        if(card.model.FieldCard == true)
        {
            if (FieldCardDrag != null)
            {
                FieldCardDrag.Invoke();
            }
        }
        else
        {
            if (BeginDragAction != null)
            {
                BeginDragAction.Invoke();
            }
        }

        cardParent = transform.parent;
        Transform canvasTransform = GameObject.Find("Canvas").transform;
        transform.SetParent(canvasTransform, false);
        GetComponent<CanvasGroup>().blocksRaycasts = false; // blocksRaycastsをオフにする
    }

    public void OnDrag(PointerEventData eventData) // ドラッグした時に起こす処理
    {
        CardController card = GetComponent<CardController>();
        if (canDrag == false || canDragField == false || card.model.PlayerCard == false)
        {
            return;
        }

        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData) // カードを離したときに行う処理
    {
        CardController card = GetComponent<CardController>();

        if (card.model.magic)
        {
            GameManager.instance.MagicDropArea.SetActive(false);
        }

        if (canDrag == false || canDragField == false || card.model.PlayerCard == false)
        {
            return;
        }

        if (CardMoveCancel != null)
        {
            if(hand == cardParent)
            {
                CardMoveCancel.Invoke();
            }
        }
        if (card.model.canAttack == true)
        {
            if (FieldCardMoveCacel != null)
            {
                FieldCardMoveCacel.Invoke();
            }
        }

        transform.SetParent(cardParent, false);
        GetComponent<CanvasGroup>().blocksRaycasts = true; // blocksRaycastsをオンにする

        ///

        transform.localPosition = new Vector3(0, 0, 0);

        ///
    }

    public IEnumerator AttackMotion(Transform target,Action onAttacking)
    {
        Vector3 currentPosition = transform.position;
        cardParent = transform.parent;

        transform.SetParent(cardParent.parent.parent); // cardの親を一時的にCanvasにする

        transform.DOMove(target.position, 0.25f);
        yield return new WaitForSeconds(0.25f);
        if (E_CardMove != null)
        {
            E_CardMove.Invoke();
        }
        onAttacking.Invoke();

        transform.DOMove(currentPosition, 0.25f);
        yield return new WaitForSeconds(0.25f);
        transform.SetParent(cardParent); // cardの親を元に戻す
        if (E_CardMove2 != null)
        {
            E_CardMove2.Invoke();
        }
        
    }

    public IEnumerator AttackMotion2(Vector3 currentPosition,Transform cardParent)
    {
        transform.DOMove(currentPosition, 0.25f);
        yield return new WaitForSeconds(0.25f);
        transform.SetParent(cardParent); // cardの親を元に戻す
        if (E_CardMove2 != null)
        {
            E_CardMove2.Invoke();
        }
    }
}