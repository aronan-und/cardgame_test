using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CardController : MonoBehaviour
{
    private CardView m_view; // カードの見た目の処理
    public CardView view
    {
        get
        {
            return m_view;
        }
    }
    
    public CardModel model; // カードのデータを処理
    public CardMovement movement;  // 移動(movement)に関することを操作
    public FieldController myField;
    public UnityEvent<CardController> OnBeginDrag = new UnityEvent<CardController>();
    public UnityEvent<CardController> OnMoveCancel = new UnityEvent<CardController>();
    public UnityEvent<DropPlace> OnEndDrag = new UnityEvent<DropPlace>();

    public UnityEvent<CardController> OnFieldDrag = new UnityEvent<CardController>();
    public UnityEvent<CardController> OnFieldMoveCancel = new UnityEvent<CardController>();
    public UnityEvent OnBattle = new UnityEvent();

    public UnityEvent On_E_CardMove = new UnityEvent();
    public UnityEvent On_E_CardMove2 = new UnityEvent();

    private void Awake()
    {
        m_view = GetComponent<CardView>();
        movement = GetComponent<CardMovement>();
        movement.BeginDragAction.AddListener(BeginDrag);
        movement.FieldCardDrag.AddListener(FieldDrag);
        movement.CardMoveCancel.AddListener(MoveCancel);
        movement.FieldCardMoveCacel.AddListener(FieldMoveCancel);
        movement.E_CardMove.AddListener(E_CardMove);
        movement.E_CardMove2.AddListener(E_CardMove2);
    }

    private void E_CardMove()
    {
        On_E_CardMove.Invoke();
    }

    private void E_CardMove2()
    {
        On_E_CardMove2.Invoke();
    }

    private void FieldMoveCancel()
    {
        OnFieldMoveCancel.Invoke(this);
    }

    private void FieldDrag()
    {
        OnFieldDrag.Invoke(this);
    }

    private void MoveCancel()
    {
        OnMoveCancel.Invoke(this);
    }

    private void BeginDrag()
    {
        OnBeginDrag.Invoke(this);
    }

    public void Init(int cardID, bool playerCard, Transform hand) // カードを生成した時に呼ばれる関数
    {
        model = new CardModel(cardID, playerCard); // カードデータを生成 // カードデータを生成
        if(movement == null)
        {
            movement = GetComponent<CardMovement>();
        }
        movement.hand = hand;
        RefreshView();
    }
    private void OnDestroy()
    {
        if (myField != null)
        {
            myField.RefreshWall();
            //myField.RefreshBlock(myField.cardControllerArr.GetLength(0));//()の中にOnDestroyされるカードのxを入れる0,1,2
            myField.RefreshBlock();
        }
    }

    public void DestroyCard(CardController card)
    {
        Destroy(card.gameObject);
        //Debug.Log(card.gameObject.name);
        if(myField != null)
        {
            myField.removeCard(card);
        }
    }
    public void DropField(FieldController fieldController)
    {
        myField = fieldController;
        //GameManager.instance.ReduceManaPoint(model.cost); //ゲームマネージャーのマナ変数
        model.FieldCard = true; // フィールドのカードのフラグを立てる
        model.canUse = false;
        if (model.sokkou)
        {
            m_view.SetCanUsePanel(true);
        }
        else
        {
            m_view.SetCanUsePanel(model.canUse); // 出した時にCanUsePanelを消す
        }
    }

    public void RefreshView()
    {
        m_view.Show(model); // 表示
    }

    public void HandCanDrag(int _Mana)
    {
        if (model.cost <= _Mana)
        {
            model.canUse = true;
            view.SetCanUsePanel(model.canUse);
        }
        else
        {
            //Debug.Log("HandCanDrag canUse=false");
            model.canUse = false;
            view.SetCanUsePanel(model.canUse);
        }
    }
}