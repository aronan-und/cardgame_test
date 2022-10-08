using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//状態一覧
/// <summary>
///マナを増やす OK
///カードを山札から引く OK
///場に出すカードを決める OK
///カードを場に出してマナを減らす OK
///場のカードが攻撃する OK
///ターンエンド OK
/// </summary>

public class EnemyTurnState : StateMachineBase<EnemyTurnState>
{
    [SerializeField] GameObject enemyDeckImage;
    [SerializeField] Transform enemyHand;
    [SerializeField] GameManager gameManager;
    private int enemyManaPoint;
    private int enemyDefaultManaPoint;
    [SerializeField] private Text enemyManaPointText;
    [SerializeField] private Text enemyDefaultManaPointText;
    private Action onFinishedTurn;
    CardController nextCard;
    [SerializeField] FieldController fieldController;
    [SerializeField] FieldController p_FieldController;
    [SerializeField] Transform enemyCenterFront, enemyRightFront, enemyLeftFront, enemyCenterBack, enemyRightBack, enemyLeftBack;
    //CardController attackCard;
    [SerializeField] Transform playerLeaderTransform;
    [SerializeField] Define define;
    private Transform targetPlace;
    private Vector2Int emptyPoint = Vector2Int.zero;

    void Start()
    {
        ChangeState(new EnemyTurnState.Wait(this));
    }
    public void TurnStart(int defaultMana, Action _onFinished)
    {
        enemyDefaultManaPoint = defaultMana;
        enemyManaPoint = enemyDefaultManaPoint;
        EnemyShowManaPoint();
        ChangeState(new EnemyTurnState.DrawCard(this));
        CardRefresh();
        onFinishedTurn = _onFinished;
    }
    public void TurnEnd()
    {
        ChangeState(new EnemyTurnState.Wait(this));
    }
    public void EnemyShowManaPoint() // 敵マナポイントを表示するメソッド
    {
        enemyManaPointText.text = enemyManaPoint.ToString();
        enemyDefaultManaPointText.text = enemyDefaultManaPoint.ToString();
    }
    public void CardRefresh()
    {
        CardController[,] e_CardArr = new CardController[3, 2]
                {
                    {enemyLeftFront.GetComponentInChildren<CardController>(),
                        enemyLeftBack.GetComponentInChildren<CardController>()},
                    {enemyCenterFront.GetComponentInChildren<CardController>(),
                        enemyCenterBack.GetComponentInChildren<CardController>()},
                    {enemyRightFront.GetComponentInChildren<CardController>(),
                        enemyRightBack.GetComponentInChildren<CardController>()}
                };

        for (int i = 0; i < e_CardArr.Length; i++)
        {
            int x = i % 3;
            int y = i / 3;
            if (e_CardArr[x, y] != null)
            {
                gameManager.SetAttackableFieldCard(e_CardArr[x, y], true);
            }
        }
    }


    private class Wait : StateBase<EnemyTurnState>
    {
        public Wait(EnemyTurnState _machine) : base(_machine)
        {
        }
    }

    private class DrawCard : StateBase<EnemyTurnState>
    {
        private bool drawCardEnd = false;
        public DrawCard(EnemyTurnState _machine) : base(_machine)
        {
        }
        public override void OnEnterState()
        {
            // デッキがないなら引かない
            if (machine.gameManager.enemyDeckCount == 0)
            {
                machine.enemyDeckImage.SetActive(false);
                drawCardEnd = true;
            }
            else
            {
                CardController[] enemyHandCardList = machine.enemyHand.GetComponentsInChildren<CardController>();

                if (enemyHandCardList.Length < 10)
                {
                    // デッキの一番上のカードを抜き取り、手札に加える
                    int enemyCardID = machine.gameManager.enemyDeck[0];
                    machine.gameManager.enemyDeck.RemoveAt(0);
                    machine.gameManager.CreateCard(enemyCardID, machine.enemyHand, () =>
                    {
                        //machine.gameManager.enemyDeckCount = machine.enemyDeck.Count;
                        if (machine.gameManager.enemyDeck.Count == 0)
                        {
                            machine.enemyDeckImage.SetActive(false);
                        }
                        machine.gameManager.ShowDeckText();
                        drawCardEnd = true;
                    });
                }
                else
                {
                    machine.gameManager.enemyDeck.RemoveAt(0);
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
                machine.ChangeState(new EnemyTurnState.SummonIdel(machine));
            }
        }
    }

    private class SummonIdel : StateBase<EnemyTurnState>
    {
        private float timer = 0f;
        private bool isSummonCard = false;
        public SummonIdel(EnemyTurnState _machine) : base(_machine)
        {
        }
        public override void OnEnterState()
        {
            CardController[] enemyHandCardArr = machine.enemyHand.GetComponentsInChildren<CardController>();
            machine.nextCard = null;
            int manaChecker = -1;
            for (int i = 0; i < enemyHandCardArr.Length; i++)
            {
                if (enemyHandCardArr[i].model.cost > manaChecker &&
                    machine.enemyManaPoint >= enemyHandCardArr[i].model.cost)
                {
                    manaChecker = enemyHandCardArr[i].model.cost;
                }
            }
            for (int i = 0; i < enemyHandCardArr.Length; i++)
            {
                if (enemyHandCardArr[i].model.cost == manaChecker)
                {
                    machine.nextCard = enemyHandCardArr[i];　//所持マナ以下のコスト最大のカード
                    break;
                }
            }
            if (machine.nextCard != null && machine.fieldController.EnemyGetRandomEmpty(out machine.emptyPoint) ||
                machine.nextCard != null && machine.nextCard.model.magic)
            {
                if (machine.nextCard.model.magic)
                {
                    machine.emptyPoint = new Vector2Int(0, 0);//仮
                }
                else
                {
                    Transform[,] enemyCardPlaceArr = new Transform[3, 2]
                    {
                        {machine.enemyLeftFront,machine.enemyLeftBack},
                        {machine.enemyCenterFront,machine.enemyCenterBack},
                        {machine.enemyRightFront,machine.enemyRightBack}
                    };
                    machine.targetPlace = enemyCardPlaceArr[machine.emptyPoint.x, machine.emptyPoint.y];
                }
                isSummonCard = true;
            }

        }
        public override void OnUpdate()
        {
            timer += Time.deltaTime;
            if (0.5f < timer)
            {
                if (isSummonCard)
                {
                    machine.ChangeState(new EnemyTurnState.SummonCard(machine));
                }
                else
                {
                    machine.ChangeState(new EnemyTurnState.AttackIdel(machine));
                }
            }
        }
    }

    private class SummonCard : StateBase<EnemyTurnState>
    {
        private float timer = 0f;
        public SummonCard(EnemyTurnState _machine) : base(_machine)
        {
        }
        public override void OnEnterState()
        {
            int nextEnemyCardID = machine.nextCard.model.cardID;
            if (machine.nextCard.model.magic)
            {
                CardController[] enemyHandCardList = machine.enemyHand.GetComponentsInChildren<CardController>();
                int removeCardIndex = -1;
                for (int i = 0; i < enemyHandCardList.Length; i++)
                {
                    if (enemyHandCardList[i].model.cardID == nextEnemyCardID)
                    {
                        removeCardIndex = i;
                        break;
                    }
                }
                if (removeCardIndex >= 0)
                {
                    Destroy(enemyHandCardList[removeCardIndex].gameObject);
                }
                else
                {
                    Debug.Log(enemyHandCardList.Length);
                }
                machine.gameManager.MagicCard(machine.nextCard);
            }
            else
            {
                machine.gameManager.SummonCardEnemy(nextEnemyCardID, machine.targetPlace);
                CardController createdCard = machine.targetPlace.GetComponentInChildren<CardController>();
                machine.fieldController.SetCard(machine.emptyPoint, createdCard);
                createdCard.model.FieldCard = true;
                createdCard.DropField(machine.fieldController);//エネミー側はマナ計算をここで実施
            }
            machine.enemyManaPoint -= machine.nextCard.model.cost;//1
            machine.EnemyShowManaPoint();//2
        }
        public override void OnUpdate()
        {
            timer += Time.deltaTime;
            if (0.5f < timer)
            {
                machine.ChangeState(new EnemyTurnState.SummonIdel(machine));
            }
        }
    }

    private class AttackIdel : StateBase<EnemyTurnState>
    {
        private bool isAttackPhase = false;
        private float timer = 0f;
        private CardController attackCard;
        public AttackIdel(EnemyTurnState _machine) : base(_machine)
        {
        }
        public override void OnEnterState()
        {
            CardController[,] e_CardArr = new CardController[3, 2]
                {
                    {machine.enemyLeftFront.GetComponentInChildren<CardController>(),
                        machine.enemyLeftBack.GetComponentInChildren<CardController>()},
                    {machine.enemyCenterFront.GetComponentInChildren<CardController>(),
                        machine.enemyCenterBack.GetComponentInChildren<CardController>()},
                    {machine.enemyRightFront.GetComponentInChildren<CardController>(),
                        machine.enemyRightBack.GetComponentInChildren<CardController>()}
                };

            for (int i = 0; i < e_CardArr.Length; i++)
            {
                int x = i % 3;
                int y = i / 3;
                if (e_CardArr[x, y] != null && e_CardArr[x,y].model.canAttack)
                {
                    attackCard = e_CardArr[x, y];
                    isAttackPhase = true;
                }
            }
        }
        public override void OnUpdate()
        {
            timer += Time.deltaTime;
            if(0.5f < timer)
            {
                if (isAttackPhase)
                {
                    machine.ChangeState(new EnemyTurnState.AttackPhase(machine,attackCard));
                }
                else
                {
                    machine.ChangeState(new EnemyTurnState.TurnFinish(machine));
                }
            }
        }
    }

    private class AttackPhase : StateBase<EnemyTurnState>
    {
        private float timer = 0f;
        CardController attackCard = null;
        CardController defenceCard = null;
        Vector3 c_Position;
        Transform c_Parent;
        public AttackPhase(EnemyTurnState _machine,CardController attackCard) : base(_machine)
        {
            this.attackCard = attackCard;
        }
        public override void OnEnterState()
        {
            c_Position = attackCard.transform.position;
            c_Parent = attackCard.transform.parent;

            machine.p_FieldController.CanAttackCard(out defenceCard);

            if (defenceCard != null)
            {
                /*
                machine.gameManager.StartCoroutine(attackCard.movement.AttackMotion(defenceCard.transform,()=>
                {
                    //machine.gameManager.CardBattle(machine.attackCard, defenceCard);//移動を始める前にバトルが終わる（エネミー側が死ぬ時、バグる）
                    if(attackCard.model.hp > 0)
                    {
                        attackCard.On_E_CardMove2.AddListener(E_CardMove);
                    }
                    else
                    {
                        machine.ChangeState(new EnemyTurnState.AttackIdel(machine));
                    }
                }));
                */
                /*
                Vector3 cardPosition = machine.attackCard.transform.position;
                Transform cardTransform = machine.attackCard.transform.parent;
                machine.attackCard.transform.SetParent(cardTransform.parent);
                machine.gameManager.StartCoroutine(machine.attackCard.movement.AttackMotion2(defenceCard.transform.position));
                machine.gameManager.CardBattle(machine.attackCard, defenceCard);
                if(machine.attackCard != null)
                {
                    machine.gameManager.StartCoroutine(machine.attackCard.movement.AttackMotion2(cardPosition));
                    machine.attackCard.transform.SetParent(cardTransform);
                }
                */
                //machine.attackCard.On_E_CardMove.AddListener(E_CardMove);
                machine.ChangeState(new EnemyTurnState.CardBattle(machine, attackCard, defenceCard, c_Position, c_Parent));
            }
            else
            {
                /*
                machine.gameManager.StartCoroutine(attackCard.movement.AttackMotion(machine.playerLeaderTransform,()=> {
                    //machine.gameManager.AttackToLeader(machine.attackCard, false);
                    attackCard.On_E_CardMove2.AddListener(E_CardMoveLeader);
                }));
                */
                //machine.attackCard.OnBattle.AddListener(E_CardMoveEnd);
                machine.ChangeState(new EnemyTurnState.CardBattleLeader(machine, attackCard, c_Position, c_Parent));
            }
        }
        
        public override void OnUpdate()
        {
            timer += Time.deltaTime;
            if (3f < timer)
            {
                machine.ChangeState(new EnemyTurnState.AttackIdel(machine));
            }
        }
        
        /*
        public override void OnExitState()
        {
            machine.attackCard.On_E_CardMove.RemoveListener(E_CardMove);
        }
        */
    }

    private class CardBattle : StateBase<EnemyTurnState>
    {
        CardController atkCard;
        CardController defCard;
        Vector3 c_Position;
        Transform c_Parent;
        public CardBattle(EnemyTurnState _machine,CardController attackCard, CardController defenceCard, Vector3 currentPosition, Transform cardParent) : base(_machine)
        {
            atkCard = attackCard;
            defCard = defenceCard;
            c_Position = currentPosition;
            c_Parent = cardParent;
        }
        public override void OnEnterState()
        {
            machine.gameManager.StartCoroutine(atkCard.movement.AttackMotion(defCard.transform, () =>
            {
                machine.gameManager.CardBattle(atkCard, defCard);//移動を始める前にバトルが終わる（エネミー側が死ぬ時、バグる）
                if (atkCard.model.hp > 0)
                {
                    atkCard.On_E_CardMove2.AddListener(()=> {
                        machine.ChangeState(new EnemyTurnState.AttackIdel(machine));
                    });
                }
                else
                {
                    machine.ChangeState(new EnemyTurnState.AttackIdel(machine));
                }
            }));

            //else
            //{
            //machine.ChangeState(new EnemyTurnState.AttackIdel(machine));
            //}
        }
    }

    private class CardBattleLeader : StateBase<EnemyTurnState>
    {
        CardController atkCard;
        Vector3 c_Position;
        Transform c_Parent;
        public CardBattleLeader(EnemyTurnState _machine, CardController attackCard, Vector3 currentPosition, Transform cardParent) : base(_machine)
        {
            atkCard = attackCard;
            c_Position = currentPosition;
            c_Parent = cardParent;
        }
        public override void OnEnterState()
        {
            atkCard.On_E_CardMove2.AddListener(() => {
                machine.ChangeState(new EnemyTurnState.AttackIdel(machine));
            });
            machine.gameManager.StartCoroutine(atkCard.movement.AttackMotion(machine.playerLeaderTransform, () => {
                //machine.gameManager.AttackToLeader(machine.attackCard, false);
                machine.gameManager.AttackToLeader(atkCard, false);
            }));
            //machine.gameManager.StartCoroutine(machine.attackCard.movement.AttackMotion2(c_Position, c_Parent));
            //machine.ChangeState(new EnemyTurnState.AttackIdel(machine));
        }
    }

    private class TurnFinish : StateBase<EnemyTurnState>
    {
        public TurnFinish(EnemyTurnState _machine) : base(_machine)
        {
        }
        public override void OnEnterState()
        {
            machine.onFinishedTurn.Invoke();
        }
    }


}