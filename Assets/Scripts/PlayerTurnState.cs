using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTurnState : StateMachineBase<PlayerTurnState>
{
    private int p_ManaPoint;
    private int p_DefaultManaPoint;
    private Action onFinishedTurn;
    [SerializeField] private Text p_ManaPointText;
    [SerializeField] private Text p_DefaultManaPointText;
    [SerializeField] Transform playerCenterFront, playerRightFront, playerLeftFront, playerCenterBack, playerRightBack, playerLeftBack;
    [SerializeField] Transform enemyCenterFront, enemyRightFront, enemyLeftFront, enemyCenterBack, enemyRightBack, enemyLeftBack;
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject p_DeckImage;
    [SerializeField] Transform p_Hand;
    [SerializeField] Button TurnEndButton;


    void Start()
    {
        ChangeState(new PlayerTurnState.Wait(this));
    }
    public void TurnStart(int defaultMana, Action _onFinished)
    {
        p_DefaultManaPoint = defaultMana;
        p_ManaPoint = p_DefaultManaPoint;
        ShowManaPoint();
        ChangeState(new PlayerTurnState.DrawCard(this));
        CardRefresh(true);
        onFinishedTurn = _onFinished;
    }
    public void TurnEnd()
    {
        ChangeState(new PlayerTurnState.Wait(this));
    }
    public void ShowManaPoint() // マナポイントを表示するメソッド
    {
        p_ManaPointText.text = p_ManaPoint.ToString();
        p_DefaultManaPointText.text = p_DefaultManaPoint.ToString();
    }
    public void AddManaPoint(int cost)
    {
        p_ManaPoint += cost;
        ShowManaPoint();
    }
    public void CardRefresh(bool isPanel) //場のカードにパネルを付けるor消す
    {
        CardController[,] e_CardArr = new CardController[3, 2]
                {
                    {playerLeftFront.GetComponentInChildren<CardController>(),
                        playerLeftBack.GetComponentInChildren<CardController>()},
                    {playerCenterFront.GetComponentInChildren<CardController>(),
                        playerCenterBack.GetComponentInChildren<CardController>()},
                    {playerRightFront.GetComponentInChildren<CardController>(),
                        playerRightBack.GetComponentInChildren<CardController>()}
                };

        for (int i = 0; i < e_CardArr.Length; i++)
        {
            int x = i % 3;
            int y = i / 3;
            if (e_CardArr[x, y] != null)
            {
                gameManager.SetAttackableFieldCard(e_CardArr[x, y], isPanel);
            }
        }
    }

    private class Wait : StateBase<PlayerTurnState>
    {
        public Wait(PlayerTurnState _machine) : base(_machine)
        {
        }
    }

    private class DrawCard : StateBase<PlayerTurnState>
    {
        private bool drawCardEnd = false;
        public DrawCard(PlayerTurnState _machine) : base(_machine)
        {
        }
        public override void OnEnterState()
        {
            // デッキがないなら引かない
            if (machine.gameManager.playerDeckCount == 0)
            {
                machine.p_DeckImage.SetActive(false);
                drawCardEnd = true;
            }
            else
            {
                CardController[] p_HandCardList = machine.p_Hand.GetComponentsInChildren<CardController>();

                if (p_HandCardList.Length < 10)
                {
                    // デッキの一番上のカードを抜き取り、手札に加える
                    int p_CardID = machine.gameManager.playerDeck[0];
                    machine.gameManager.playerDeck.RemoveAt(0);
                    machine.gameManager.CreateCard(p_CardID, machine.p_Hand, () =>
                    {
                        if (machine.gameManager.playerDeck.Count == 0)
                        {
                            machine.p_DeckImage.SetActive(false);
                        }
                        machine.gameManager.ShowDeckText();
                        drawCardEnd = true;
                    });
                }
                else
                {
                    machine.gameManager.playerDeck.RemoveAt(0);
                    Debug.Log("手札が１０枚を超えるとドローと同時に破棄");
                    machine.gameManager.ShowDeckText();
                    drawCardEnd = true;
                }
            }
        }
        public override void OnUpdate()
        {
            if (drawCardEnd)
            {
                machine.ChangeState(new PlayerTurnState.PlayerIdle(machine));
            }
        }
    }

    private class PlayerIdle : StateBase<PlayerTurnState>
    {
        CardController[] playerHandCardList;
        CardController[,] e_CardArr;
        CardController moveCard;
        public PlayerIdle(PlayerTurnState _machine) : base(_machine)
        {
        }
        public override void OnEnterState()
        {
            machine.TurnEndButton.interactable = true;
            machine.TurnEndButton.onClick.AddListener(onTurnEnd);
            playerHandCardList = machine.p_Hand.GetComponentsInChildren<CardController>();
            foreach (CardController card in playerHandCardList)
            {
                card.HandCanDrag(machine.p_ManaPoint);
                card.OnBeginDrag.AddListener(OnBeginDragHand);
            }
            e_CardArr = new CardController[3, 2]
            {
                {machine.playerLeftFront.GetComponentInChildren<CardController>(),
                    machine.playerLeftBack.GetComponentInChildren<CardController>()},
                {machine.playerCenterFront.GetComponentInChildren<CardController>(),
                    machine.playerCenterBack.GetComponentInChildren<CardController>()},
                {machine.playerRightFront.GetComponentInChildren<CardController>(),
                    machine.playerRightBack.GetComponentInChildren<CardController>()}
                        };
            for (int i = 0; i < e_CardArr.Length; i++)
            {
                int x = i % 3;
                int y = i / 3;
                if (e_CardArr[x, y] != null)
                {
                    e_CardArr[x, y].OnFieldDrag.AddListener(OnBeginDragField);
                }
            }
        }

        private void OnBeginDragField(CardController arg0)
        {
            machine.ChangeState(new PlayerTurnState.FieldCardMove(machine, arg0));
            moveCard = arg0;
        }

        private void OnBeginDragHand(CardController arg0)
        {
            machine.ChangeState(new PlayerTurnState.DeckCardMove(machine,arg0));
            moveCard = arg0;
        }

        private void onTurnEnd()
        {
            machine.ChangeState(new PlayerTurnState.TurnFinish(machine));
        }
        public override void OnExitState()
        {
            machine.TurnEndButton.onClick.RemoveListener(onTurnEnd); //ステート処理の時は不要
            machine.TurnEndButton.interactable = false;
            foreach (CardController card in playerHandCardList)
            {
                if(card != moveCard)
                {
                    card.HandCanDrag(-1);
                    card.OnBeginDrag.RemoveListener(OnBeginDragHand);
                }
            }
            CardController[,] e_CardArr = new CardController[3, 2]
            {
                {machine.playerLeftFront.GetComponentInChildren<CardController>(),
                    machine.playerLeftBack.GetComponentInChildren<CardController>()},
                {machine.playerCenterFront.GetComponentInChildren<CardController>(),
                    machine.playerCenterBack.GetComponentInChildren<CardController>()},
                {machine.playerRightFront.GetComponentInChildren<CardController>(),
                    machine.playerRightBack.GetComponentInChildren<CardController>()}
            };
            for (int i = 0; i < e_CardArr.Length; i++)
            {
                int x = i % 3;
                int y = i / 3;
                if (e_CardArr[x, y] != null)
                {
                    e_CardArr[x, y].OnFieldDrag.AddListener(OnBeginDragField);
                }
            }
        }
    }

    private class DeckCardMove : StateBase<PlayerTurnState>
    {
        CardController activeCard;
        //DropPlace cardDrop;
        public DeckCardMove(PlayerTurnState _machine,CardController _card) : base(_machine)
        {
            activeCard = _card;
        }
        public override void OnEnterState()
        {
            //cardDrop = activeCard.GetComponent<DropPlace>();
            activeCard.OnMoveCancel.AddListener(OnCardMoveCancel);
            activeCard.OnEndDrag.AddListener(OnEndDragHand);
        }
        private void OnCardMoveCancel(CardController arg0)
        {
            machine.ChangeState(new PlayerTurnState.PlayerIdle(machine));
        }

        private void OnEndDragHand(DropPlace arg0)
        {
            machine.ChangeState(new PlayerTurnState.SummonCard(machine,activeCard));
        }

        public override void OnExitState()
        {
            activeCard.OnEndDrag.RemoveListener(OnEndDragHand);
            activeCard.OnMoveCancel.RemoveListener(OnCardMoveCancel);
        }
    }

    private class SummonCard : StateBase<PlayerTurnState>
    {
        CardController activeCard;
        public SummonCard(PlayerTurnState _machine, CardController _card) : base(_machine)
        {
            activeCard = _card;
        }
        public override void OnEnterState()
        {
            machine.AddManaPoint(-activeCard.model.cost);
            machine.ChangeState(new PlayerTurnState.PlayerIdle(machine));
        }
    }

    private class FieldCardMove : StateBase<PlayerTurnState>
    {
        CardController activeCard;
        public FieldCardMove(PlayerTurnState _machine, CardController _card) : base(_machine)
        {
            activeCard = _card;
        }
        public override void OnEnterState()
        {
            activeCard.OnFieldMoveCancel.AddListener(OnAttackMoveCancel);
            List<CardController> cardControllerList = machine.gameManager.EnemyFieldController.GetList();
            //Debug.Log("エネミーのカードは　"+cardControllerList.Count+"　枚");
            foreach (CardController card in cardControllerList)
            {
                AttackedCard attackedCard = card.gameObject.AddComponent(typeof(AttackedCard)) as AttackedCard;
                attackedCard.OnAttacked.AddListener(ToBattle);
            }
        }

        private void ToBattle(CardController arg0)
        {
            activeCard.OnFieldMoveCancel.RemoveListener(OnAttackMoveCancel);
            machine.ChangeState(new PlayerTurnState.AttackCard(machine, activeCard,arg0));
        }
        private void OnAttackMoveCancel(CardController arg0)
        {
            machine.ChangeState(new PlayerTurnState.PlayerIdle(machine));
        }
        public override void OnExitState()
        {
            activeCard.OnFieldMoveCancel.RemoveListener(OnAttackMoveCancel);
            List<CardController> cardControllerList = machine.gameManager.EnemyFieldController.GetList();
            foreach (CardController card in cardControllerList)
            {
                Destroy(card.gameObject.GetComponent(typeof(AttackedCard)));
            }
        }
    }

    private class AttackCard : StateBase<PlayerTurnState>
    {
        private float timer = 0f;
        CardController attackCard;
        CardController defenceCard;
        public AttackCard(PlayerTurnState _machine,CardController atkCard,CardController defCard) : base(_machine)
        {
            attackCard = atkCard;
            defenceCard = defCard;
        }
        public override void OnEnterState()
        {
            machine.gameManager.CardBattle(attackCard, defenceCard);
        }
        public override void OnUpdate()
        {
            timer += Time.deltaTime;
            if (0.5f < timer)
            {
                machine.ChangeState(new PlayerTurnState.PlayerIdle(machine));
            }
        }
    }

    private class TurnFinish : StateBase<PlayerTurnState>
    {
        public TurnFinish(PlayerTurnState _machine) : base(_machine)
        {
        }
        public override void OnEnterState()
        {
            machine.CardRefresh(false);
            machine.onFinishedTurn.Invoke();
        }
    }
}
