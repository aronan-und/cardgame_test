using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    [SerializeField] UIManager uIManager;
    [SerializeField] CardController cardPrefab;
    [SerializeField] Transform playerHand, playerCenterFront, playerRightFront, playerLeftFront,
        playerCenterBack, playerRightBack, playerLeftBack, enemyHand, enemyCenterFront,
        enemyRightFront, enemyLeftFront, enemyCenterBack, enemyRightBack, enemyLeftBack;
    [SerializeField] Text playerLeaderHPText;
    [SerializeField] Text enemyLeaderHPText;
    [SerializeField] Text playerManaPointText;
    [SerializeField] Text playerDefaultManaPointText;
    [SerializeField] Text enemyManaPointText;
    [SerializeField] Text enemyDefaultManaPointText;
    [SerializeField] Transform playerLeaderTransform;
    [SerializeField] Transform enemyLeaderTransform;
    [SerializeField] Text playerDeckText, enemyDeckText;
    [SerializeField] GameObject playerDeckImage, enemyDeckImage;
    [SerializeField] FieldController enemyFieldController;
    [SerializeField] GameObject magicDropArea;
    public GameObject MagicDropArea { get { return magicDropArea; } }
    [SerializeField] PlayerTurnState playerTurnState;
    [SerializeField] EnemyTurnState enemyTurnState;

    public int playerManaPoint; // 使用すると減るマナポイント
    public int playerDefaultManaPoint; // 毎ターン増えていくベースのマナポイント
    public int enemyManaPoint; // 使用すると減るマナポイント
    public int enemyDefaultManaPoint; // 毎ターン増えていくベースのマナポイント
    public int playerDeckCount;
    public int enemyDeckCount;

    public bool isPlayerTurn = true; //
    public bool turnChangeTrigger = true;
    public List<int> playerDeck = new List<int>() { 4, 7, 6, 1, 5, 3, 1, 2, 3, 4, 5, 6, 1, 2, 3, 4 };  //
    public List<int> enemyDeck = new List<int>() { 6, 4, 7, 1, 2, 5, 2, 5, 1, 1, 5, 5, 1, 5, 6, 1 };  //
    public FieldController EnemyFieldController => enemyFieldController;

    public static GameManager instance;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        StartGame();
    }

    void StartGame() // 初期値の設定 
    {
        enemyLeaderHP = 20;
        playerLeaderHP = 20;
        ShowLeaderHP();

        playerDeckCount = playerDeck.Count;
        enemyDeckCount = enemyDeck.Count;
        ShowDeckText();

        /// マナの初期値設定 ///
        playerManaPoint = 0;
        playerDefaultManaPoint = 0;
        enemyManaPoint = 0;
        enemyDefaultManaPoint = 0;
        ShowManaPoint();
        EnemyShowManaPoint();

        // 初期手札を配る
        SetStartHand();

        // ターンの決定
        StartCoroutine(TurnCalc());
    }

    public void ShowDeckText()
    {
        playerDeckText.text = "残り" + playerDeckCount.ToString() + "枚";
        enemyDeckText.text = "残り" + enemyDeckCount.ToString() + "枚";
    }

    void ShowManaPoint() // マナポイントを表示するメソッド
    {
        playerManaPointText.text = playerManaPoint.ToString();
        playerDefaultManaPointText.text = playerDefaultManaPoint.ToString();
    }
    public void EnemyShowManaPoint() // 敵マナポイントを表示するメソッド
    {
        enemyManaPointText.text = enemyManaPoint.ToString();
        enemyDefaultManaPointText.text = enemyDefaultManaPoint.ToString();
    }

    public void ReduceManaPoint(int cost) // コストの分、マナポイントを減らす
    {
        if (isPlayerTurn)
        {
            Debug.Log("ReduceManaPoint");
            playerManaPoint -= cost;
            ShowManaPoint();
            SetCanUsePanelHand(playerManaPoint);
        }
    }

    void SetCanUsePanelHand(int _iMana) // 手札のカードを取得して、使用可能なカードにCanUseパネルを付ける
    {
        CardController[] playerHandCardList = playerHand.GetComponentsInChildren<CardController>();
        foreach (CardController card in playerHandCardList)
        {
            if (card.model.cost <= _iMana)
            {
                card.model.canUse = true;
                card.view.SetCanUsePanel(card.model.canUse);
            }
            else
            {
                //Debug.Log("SetCanUsePanelHand canUse=false");
                card.model.canUse = false;
                card.view.SetCanUsePanel(card.model.canUse);
            }
        }
    }

    private static int PlayerCardSerial;
    private static int EnemyCardSerial;

    public void CreateCard(int cardID, Transform place, Action onFinished)
    {
        CardController card = null;
        // Playerの手札に生成されたカードはPlayerのカードとする
        if (place == playerHand)
        {
            GameObject canvas = GameObject.Find("Canvas");
            GameObject playerDeck = GameObject.Find("PlayerDeck");
            card = Instantiate(cardPrefab,canvas.transform);
            card.transform.position = playerDeck.transform.position;
            card.Init(cardID, true, place);
            card.gameObject.name = $"Player_{PlayerCardSerial:D3}";
            PlayerCardSerial++;
        }
        else
        {
            //card = Instantiate(cardPrefab, place);
            //card.Init(cardID, false);
            GameObject canvas = GameObject.Find("EnemyDeck");
            card = Instantiate(cardPrefab, canvas.transform);
            card.Init(cardID, false, place);
            card.gameObject.name = $"Enemy_{EnemyCardSerial:D3}";
            EnemyCardSerial++;
        }
        card.GetComponent<RectTransform>().DOMove(place.position, Define.cardMoveTime).OnComplete(() => {
            card.transform.SetParent(place);
            onFinished.Invoke();
        });
    }

    public void SummonCardEnemy(int cardID, Transform place)
    {
        //CreateCard(cardID, place);
        CardController card = Instantiate(cardPrefab, place);
        card.Init(cardID, false, place);

        CardController[] enemyHandCardList = enemyHand.GetComponentsInChildren<CardController>();
        int removeCardIndex = -1;
        for (int i = 0; i < enemyHandCardList.Length; i++)
        {
            if (enemyHandCardList[i].model.cardID == cardID)
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
        MagicCard(enemyHandCardList[removeCardIndex]);
    }
    public void MagicCard(CardController card)
    {
        //ドロー用
        if (card.model.draw1)
        {
            EnemyDrawCard(enemyHand);
        }
        if (card.model.draw2)
        {
            for (int i = 0; i < 2; i++)
            {
                StartCoroutine(DelayMethod(0.5f * i, () =>
                {
                    EnemyDrawCard(enemyHand);
                }));
            }
        }
        if (card.model.draw3)
        {
            for (int i = 0; i < 3; i++)
            {
                StartCoroutine(DelayMethod(0.5f * i, () =>
                {
                    EnemyDrawCard(enemyHand);
                }));
            }
        }
    }

    public void PlayerDrawCard(Transform hand) // カードを引く
    {
        // デッキがないなら引かない
        if (playerDeck.Count == 0)
        {
            playerDeckImage.SetActive(false);
            playerLeaderHP -= 1;
        }
        else
        {
            CardController[] playerHandCardList = playerHand.GetComponentsInChildren<CardController>();

            if (playerHandCardList.Length < 10)
            {
                // デッキの一番上のカードを抜き取り、手札に加える
                int playerCardID = playerDeck[0];
                playerDeck.RemoveAt(0);

                CreateCard(playerCardID, hand,() =>
                {
                    playerDeckCount = playerDeck.Count;

                    if (playerDeck.Count == 0)
                    {
                        playerDeckImage.SetActive(false);
                    }
                });
            }
            else
            {
                playerDeck.RemoveAt(0);
                Debug.Log("手札が１０枚を超えるとドローと同時に破棄");
            }
            ShowDeckText();
            SetCanUsePanelHand(playerManaPoint);
        }
    }

    ///

    void EnemyDrawCard(Transform hand) // カードを引く
    {
        // デッキがないなら引かない
        if (enemyDeck.Count == 0)
        {
            enemyDeckImage.SetActive(false);
        }
        else
        {
            CardController[] enemyHandCardList = enemyHand.GetComponentsInChildren<CardController>();

            if (enemyHandCardList.Length < 10)
            {
                // デッキの一番上のカードを抜き取り、手札に加える
                int enemyCardID = enemyDeck[0];
                enemyDeck.RemoveAt(0);
                CreateCard(enemyCardID, hand,() =>
                {
                    enemyDeckCount = enemyDeck.Count;
                    if (enemyDeck.Count == 0)
                    {
                        enemyDeckImage.SetActive(false);
                    }
                });
            }
            else
            {
                enemyDeck.RemoveAt(0);
                Debug.Log("手札が１０枚を超えるとドローと同時に破棄");
            }
            ShowDeckText();
        }
    }
    ///

    private IEnumerator DelayMethod(float waitTime, Action action)
    {
        yield return new WaitForSeconds(waitTime);
        action();
    }

    void SetStartHand() // 手札を3枚配る
    {
        for (int i = 0; i < 3; i++)
        {
            StartCoroutine(DelayMethod(0.5f * i, () =>
            {
                PlayerDrawCard(playerHand);
            }));
        }
        for (int i = 0; i < 3; i++)
        {
            StartCoroutine(DelayMethod(0.5f * i, () =>
            {
                EnemyDrawCard(enemyHand);
            }));
        }
    }

    IEnumerator TurnCalc() // ターンを管理する
    {
        yield return StartCoroutine(uIManager.ShowChangeTurnPanel());
        //StartCoroutine(uIManager.ShowChangeTurnPanel());
        if (isPlayerTurn)
        {
            PlayerTurn();
        }
        else
        {
            //EnemyTurn(); // コメントアウトする
            StartCoroutine(EnemyTurn()); // StartCoroutineで呼び出す
        }
    }

    public void ChangeTurn() // ターンエンドボタンにつける処理
    {
        isPlayerTurn = !isPlayerTurn; // ターンを逆にする
        StartCoroutine(TurnCalc()); // ターンを相手に回す
    }
    public void ChangeTurnBottun()
    {
        if (turnChangeTrigger)
        {
            turnChangeTrigger = false;
            PlayerTurnEnd();
            isPlayerTurn = !isPlayerTurn; // ターンを逆にする
            StartCoroutine(TurnCalc()); // ターンを相手に回す
        }
    }

    void PlayerTurn()
    {
        if (playerDefaultManaPoint < 10)
        {
            playerDefaultManaPoint++;
        }
        else
        {
            playerDefaultManaPoint = 10;
        }
        playerTurnState.TurnStart(playerDefaultManaPoint, () => {
            playerTurnState.TurnEnd();
            ChangeTurn();
        });

        /*
        //Debug.Log("Playerのターン");
        turnChangeTrigger = true;

        CardController playerCenterFrontCard = playerCenterFront.GetComponentInChildren<CardController>();
        CardController playerRightFrontCard = playerRightFront.GetComponentInChildren<CardController>();
        CardController playerLeftFrontCard = playerLeftFront.GetComponentInChildren<CardController>();
        CardController playerCenterBackCard = playerCenterBack.GetComponentInChildren<CardController>();
        CardController playerRightBackCard = playerRightBack.GetComponentInChildren<CardController>();
        CardController playerLeftBackCard = playerLeftBack.GetComponentInChildren<CardController>();
        SetAttackableFieldCard(playerCenterFrontCard, true);
        SetAttackableFieldCard(playerRightFrontCard, true);
        SetAttackableFieldCard(playerLeftFrontCard, true);
        SetAttackableFieldCard(playerCenterBackCard, true);
        SetAttackableFieldCard(playerRightBackCard, true);
        SetAttackableFieldCard(playerLeftBackCard, true);
        
        /// マナを増やす
        if (playerDefaultManaPoint < 10)
        {
            playerDefaultManaPoint++;
        }
        else
        {
            playerDefaultManaPoint = 10;
        }
        playerManaPoint = playerDefaultManaPoint;
        ShowManaPoint();

        PlayerDrawCard(playerHand); // 手札を一枚加える
        */
    }

    private void PlayerTurnEnd()
    {
        SetCanUsePanelHand(-1); //マナを内部で−１に

        //フィールドのカードのcanUsePanelとcanAttackをfalse
        CardController playerCenterFrontCard = playerCenterFront.GetComponentInChildren<CardController>();
        CardController playerRightFrontCard = playerRightFront.GetComponentInChildren<CardController>();
        CardController playerLeftFrontCard = playerLeftFront.GetComponentInChildren<CardController>();
        CardController playerCenterBackCard = playerCenterBack.GetComponentInChildren<CardController>();
        CardController playerRightBackCard = playerRightBack.GetComponentInChildren<CardController>();
        CardController playerLeftBackCard = playerLeftBack.GetComponentInChildren<CardController>();
        if (playerCenterFrontCard != null)
        {
            playerCenterFrontCard.view.SetCanUsePanel(playerCenterFrontCard.model.canUse);
            playerCenterFrontCard.model.canAttack = false;
        }
        if (playerRightFrontCard != null)
        {
            playerRightFrontCard.view.SetCanUsePanel(playerRightFrontCard.model.canUse);
            playerRightFrontCard.model.canAttack = false;
        }
        if (playerLeftFrontCard != null)
        {
            playerLeftFrontCard.view.SetCanUsePanel(playerLeftFrontCard.model.canUse);
            playerLeftFrontCard.model.canAttack = false;
        }
        if (playerCenterBackCard != null)
        {
            playerCenterBackCard.view.SetCanUsePanel(playerCenterBackCard.model.canUse);
            playerCenterBackCard.model.canAttack = false;
        }
        if (playerRightBackCard != null)
        {
            playerRightBackCard.view.SetCanUsePanel(playerRightBackCard.model.canUse);
            playerRightBackCard.model.canAttack = false;
        }
        if (playerLeftBackCard != null)
        {
            playerLeftBackCard.view.SetCanUsePanel(playerLeftBackCard.model.canUse);
            playerLeftBackCard.model.canAttack = false;
        }
    }

    IEnumerator EnemyTurn()
    {
        //Debug.Log("Enemyのターン");
        
        /// マナを増やす
        if (enemyDefaultManaPoint < 10)
        {
            enemyDefaultManaPoint++;
        }
        else
        {
            enemyDefaultManaPoint = 10;
        }
        yield return 0;
        enemyTurnState.TurnStart(enemyDefaultManaPoint,()=> {
            enemyTurnState.TurnEnd();
            ChangeTurn();
        });
        /*
        enemyManaPoint = enemyDefaultManaPoint;
        EnemyShowManaPoint();

        yield return new WaitForSeconds(1f);

        EnemyDrawCard(enemyHand);
        

        CardController enemyCenterFrontCard = enemyCenterFront.GetComponentInChildren<CardController>();
        CardController enemyRightFrontCard = enemyRightFront.GetComponentInChildren<CardController>();
        CardController enemyLeftFrontCard = enemyLeftFront.GetComponentInChildren<CardController>();
        CardController enemyCenterBackCard = enemyCenterBack.GetComponentInChildren<CardController>();
        CardController enemyRightBackCard = enemyRightBack.GetComponentInChildren<CardController>();
        CardController enemyLeftBackCard = enemyLeftBack.GetComponentInChildren<CardController>();
        SetAttackableFieldCard(enemyCenterFrontCard, true);
        SetAttackableFieldCard(enemyRightFrontCard, true);
        SetAttackableFieldCard(enemyLeftFrontCard, true);
        SetAttackableFieldCard(enemyCenterBackCard, true);
        SetAttackableFieldCard(enemyRightBackCard, true);
        SetAttackableFieldCard(enemyLeftBackCard, true);

        CardController[] enemyHandCardArr = enemyHand.GetComponentsInChildren<CardController>();
        int cardCount = enemyHandCardArr.Length;

        ////手札の最低コストカード>マナ　になるまで繰り返す
        for (int j = 0; j < cardCount; j++)
        {
            yield return new WaitForSeconds(1f);
            ///カードを一枚召喚
            //残マナ以下　かつ　手札の最大マナカードを指定
            enemyHandCardArr = enemyHand.GetComponentsInChildren<CardController>();
            int manaChecker = -1;
            CardController nextCard = null;
            for (int i = 0; i < enemyHandCardArr.Length; i++)
            {
                if (enemyHandCardArr[i].model.cost > manaChecker &&
                    enemyManaPoint >= enemyHandCardArr[i].model.cost)
                {
                    manaChecker = enemyHandCardArr[i].model.cost;
                }
            }
            for (int i = 0; i < enemyHandCardArr.Length; i++)
            {
                if (enemyHandCardArr[i].model.cost == manaChecker)
                {
                    nextCard = enemyHandCardArr[i];
                    break;
                }
            }

            //手札から場に召喚、マナ減らし
            if (nextCard != null)
            {
                int nextEnemyCardID = nextCard.model.cardID;
                int nextEnemyCardMana = nextCard.model.cost;
      
                if(!enemyFieldController.EnemyGetRandomEmpty(out Vector2Int emptyPoint))
                {
                    if (nextCard.model.magic)//マジックカードの時
                    {
                        emptyPoint = new Vector2Int(0,0);//仮
                    }
                    else
                    {
                        break;
                    }
                }

                Transform[,] enemyCardPlaceArr = new Transform[3, 2]
                {
                    {enemyLeftFront,enemyLeftBack},
                    {enemyCenterFront,enemyCenterBack},
                    {enemyRightFront,enemyRightBack}
                };
                Debug.Log(emptyPoint);
                Transform targetPlace = enemyCardPlaceArr[emptyPoint.x, emptyPoint.y];
                CreateCardEnemy(nextEnemyCardID, targetPlace);
                enemyManaPoint -= nextEnemyCardMana;
                CardController createdCard = targetPlace.GetComponentInChildren<CardController>();
                enemyFieldController.SetCard(emptyPoint, createdCard);
                createdCard.model.FieldCard = true;

                DropPlace.instance.DropFieldCard(nextCard);  //DropPlace経由でFieldControllerのSetCard()へ

                EnemyShowManaPoint();
            }
            else
            {
                break;
            }
        }

        yield return new WaitForSeconds(1f);

        ///攻撃AI
        

        CardController enemyCenterFrontCardSecond = enemyCenterFront.GetComponentInChildren<CardController>();
        CardController enemyRightFrontCardSecond = enemyRightFront.GetComponentInChildren<CardController>();
        CardController enemyLeftFrontCardSecond = enemyLeftFront.GetComponentInChildren<CardController>();
        CardController enemyCenterBackCardSecond = enemyCenterBack.GetComponentInChildren<CardController>();
        CardController enemyRightBackCardSecond = enemyRightBack.GetComponentInChildren<CardController>();
        CardController enemyLeftBackCardSecond = enemyLeftBack.GetComponentInChildren<CardController>();
        CardController playerCenterFrontCard = playerCenterFront.GetComponentInChildren<CardController>();
        CardController playerRightFrontCard = playerRightFront.GetComponentInChildren<CardController>();
        CardController playerLeftFrontCard = playerLeftFront.GetComponentInChildren<CardController>();
        CardController playerCenterBackCard = playerCenterBack.GetComponentInChildren<CardController>();
        CardController playerRightBackCard = playerRightBack.GetComponentInChildren<CardController>();
        CardController playerLeftBackCard = playerLeftBack.GetComponentInChildren<CardController>();

        if (enemyCenterFrontCardSecond != null && enemyCenterFrontCardSecond.model.canAttack)
        {
            CardController attackCard = enemyCenterFrontCardSecond;
            CardController defenceCard = playerCenterFrontCard;
            if (playerCenterFrontCard != null)
            {
                yield return StartCoroutine(attackCard.movement.AttackMotion(defenceCard.transform));
                CardBattle(attackCard, defenceCard);
            }
            else
            {
                yield return StartCoroutine(attackCard.movement.AttackMotion(playerLeaderTransform));
                AttackToLeader(attackCard, false);
            }
            yield return new WaitForSeconds(1f);
            enemyCenterFrontCard = enemyCenterFront.GetComponentInChildren<CardController>();
        }
        if (enemyRightFrontCardSecond != null && enemyRightFrontCardSecond.model.canAttack)
        {
            CardController attackCard = enemyRightFrontCardSecond;
            CardController defenceCard = playerRightFrontCard;
            if (playerRightFrontCard != null)
            {
                yield return StartCoroutine(attackCard.movement.AttackMotion(defenceCard.transform));
                CardBattle(attackCard, defenceCard);
            }
            else
            {
                yield return StartCoroutine(attackCard.movement.AttackMotion(playerLeaderTransform));
                AttackToLeader(attackCard, false);
            }
            yield return new WaitForSeconds(1f);
            enemyRightFrontCard = enemyRightFront.GetComponentInChildren<CardController>();
        }
        if (enemyLeftFrontCardSecond != null && enemyLeftFrontCardSecond.model.canAttack)
        {
            CardController attackCard = enemyLeftFrontCardSecond;
            CardController defenceCard = playerLeftFrontCard;
            if (playerLeftFrontCard != null)
            {
                yield return StartCoroutine(attackCard.movement.AttackMotion(defenceCard.transform));
                CardBattle(attackCard, defenceCard);
            }
            else
            {
                yield return StartCoroutine(attackCard.movement.AttackMotion(playerLeaderTransform));
                AttackToLeader(attackCard, false);
            }
            yield return new WaitForSeconds(1f);
            enemyLeftFrontCard = enemyLeftFront.GetComponentInChildren<CardController>();
        }
        if (enemyCenterBackCardSecond != null && enemyCenterBackCardSecond.model.canAttack)
        {
            CardController attackCard = enemyCenterBackCardSecond;
            CardController defenceCard = playerCenterBackCard;
            if (playerCenterBackCard != null)
            {
                yield return StartCoroutine(attackCard.movement.AttackMotion(defenceCard.transform));
                CardBattle(attackCard, defenceCard);
            }
            else
            {
                yield return StartCoroutine(attackCard.movement.AttackMotion(playerLeaderTransform));
                AttackToLeader(attackCard, false);
            }
            yield return new WaitForSeconds(1f);
            enemyCenterBackCard = enemyCenterBack.GetComponentInChildren<CardController>();
        }
        if (enemyRightBackCardSecond != null && enemyRightBackCardSecond.model.canAttack)
        {
            CardController attackCard = enemyRightBackCardSecond;
            CardController defenceCard = playerRightBackCard;
            if (playerRightBackCard != null)
            {
                yield return StartCoroutine(attackCard.movement.AttackMotion(defenceCard.transform));
                CardBattle(attackCard, defenceCard);
            }
            else
            {
                yield return StartCoroutine(attackCard.movement.AttackMotion(playerLeaderTransform));
                AttackToLeader(attackCard, false);
            }
            yield return new WaitForSeconds(1f);
            enemyRightBackCard = enemyRightBack.GetComponentInChildren<CardController>();
        }
        if (enemyLeftBackCardSecond != null && enemyLeftBackCardSecond.model.canAttack)
        {
            CardController attackCard = enemyLeftBackCardSecond;
            CardController defenceCard = playerLeftBackCard;
            if (playerLeftBackCard != null)
            {
                yield return StartCoroutine(attackCard.movement.AttackMotion(defenceCard.transform));
                CardBattle(attackCard, defenceCard);
            }
            else
            {
                yield return StartCoroutine(attackCard.movement.AttackMotion(playerLeaderTransform));
                AttackToLeader(attackCard, false);
            }
            yield return new WaitForSeconds(1f);
            enemyLeftBackCard = enemyLeftBack.GetComponentInChildren<CardController>();
        }
        ChangeTurn(); // ターンエンドする
                
        */
    }

    public bool CheckCardBattle(CardController attackCard, CardController defenceCard)
    {
        // 攻撃カードと攻撃されるカードが同じプレイヤーのカードならバトルしない
        if (attackCard.model.PlayerCard == defenceCard.model.PlayerCard)
        {
            return false;
        }
        // 攻撃カードがアタック可能でなければ攻撃しないで処理終了する
        if (attackCard.model.canAttack == false)
        {
            return false;
        }
        return true;
    }

    public void CardBattle(CardController attackCard, CardController defenceCard)
    {
        if (!CheckCardBattle(attackCard,defenceCard))
        {
            return;
        }
        defenceCard.model.hp -= attackCard.model.power;
        attackCard.model.hp -= defenceCard.model.power;
        attackCard.RefreshView();
        defenceCard.RefreshView();
        

        if (defenceCard.model.hp < 1)
        {
            defenceCard.DestroyCard(defenceCard);
        }

        if (attackCard.model.hp < 1)
        {
            attackCard.DestroyCard(attackCard);
        }


        attackCard.model.canAttack = false;
        attackCard.view.SetCanAttackPanel(false);
    }
    public void SetAttackableFieldCard(CardController card, bool canAttack)
    {
        if(card != null)
        {
            card.model.canAttack = canAttack;
            card.view.SetCanAttackPanel(canAttack);
        }
    }

    public int playerLeaderHP;
    public int enemyLeaderHP;

    public void AttackToLeader(CardController attackCard, bool isPlayerCard)
    {
        if (attackCard.model.canAttack == false)
        {
            return;
        }

        if (attackCard.model.PlayerCard == true) // attackCardがプレイヤーのカードなら
        {
            enemyLeaderHP -= attackCard.model.power; // 敵のリーダーのHPを減らす
        }
        else // attackCardが敵のカードなら
        {
            playerLeaderHP -= attackCard.model.power; // プレイヤーのリーダーのHPを減らす
        }

        attackCard.model.canAttack = false;
        attackCard.view.SetCanAttackPanel(false);
        //Debug.Log("敵のHPは、" + enemyLeaderHP);
        ShowLeaderHP();
    }
 
    public void ShowLeaderHP()
    {
        if (playerLeaderHP <= 0)
        {
            playerLeaderHP = 0;
        }
        if (enemyLeaderHP <= 0)
        {
            enemyLeaderHP = 0;
        }

        playerLeaderHPText.text = playerLeaderHP.ToString();
        enemyLeaderHPText.text = enemyLeaderHP.ToString();
    }
}