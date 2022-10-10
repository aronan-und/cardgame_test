using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldController : MonoBehaviour
{

    public CardController[,] cardControllerArr = new CardController[3,2];
    [SerializeField] GameObject LeaderWall;

    public bool EnemyGetRandomEmpty(out Vector2Int position)
    {
        position = Vector2Int.zero;
        int[] randomArr = new int[] {
            1,    //randomArr[0] LeftFront
            1,    //randomArr[1] CenterFront
            1,    //randomArr[2] RightFront
            1,    //randomArr[3] LeftBack
            1,    //randomArr[4] CenterBack
            1 };  //randomArr[5] RightBack
        int total = 0;
        int emptyCount = 0;
        int emptyCount2 = 0;

        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < 3; i++)
            {
                if (cardControllerArr[i, j] == null)
                {
                    total += randomArr[emptyCount];
                }
                emptyCount++;
            }
        }


        int randomPoint = Random.Range(0,total);

        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < 3; i++)
            {
                if (cardControllerArr[i, j] == null)
                {
                    if(randomPoint < randomArr[emptyCount2])
                    {
                        position = new Vector2Int(i, j);
                        return true;
                    }
                    else
                    {
                        randomPoint -= randomArr[emptyCount2];
                    }
                }
                emptyCount2++;
            }
        }
        return false;
    }

    public void CanAttackCard(out CardController targetCard)
    {
        //守護
        //ブロック
        //その他
        //リーダー

        targetCard = null;
        int syugoCount = 0;
        List<CardController> cardControllerList = GetList();
        if (cardControllerList != null && cardControllerList.Count >=2)
        {
            for (int i = 0; i < cardControllerList.Count; i++)
            {
                if (cardControllerList[i].model.syugo)
                {
                    syugoCount++;
                }
            }
            if (syugoCount > 0)//守護がいる時
            {
                for (int i = 0; i < cardControllerList.Count; i++)
                {
                    if (!cardControllerList[i].model.syugo)
                    {
                        cardControllerList.RemoveAt(i);
                    }
                }
                MostPowerCard(cardControllerList, out targetCard);
            }
            else
            {
                for(int i = 0; i < 3; i++)
                {
                    if (IsBlock(i))//ブロックチェック
                    {
                        for (int j = 0; j < cardControllerList.Count; j++)
                        {
                            if (cardControllerList[j].gameObject.name == cardControllerArr[i, 1].gameObject.name) //シリアル番号
                            {
                                cardControllerList.RemoveAt(j);
                            }
                        }
                    }
                }
                MostPowerCard(cardControllerList, out targetCard);
            }
        }
        else if (cardControllerList != null && cardControllerList.Count == 1) //相手は１体
        {
            targetCard = cardControllerList[0];
        }
    }

    public void MostPowerCard(List<CardController> cardList , out CardController targetCard)
    {
        targetCard = null;
        int mostPower = 0;
        for (int i = 0; i < cardList.Count; i++)
        {
            if (mostPower < cardList[i].model.power)
            {
                mostPower = cardList[i].model.power;
            }
        }
        for (int i = 0; i < cardList.Count; i++)
        {
            if (mostPower == cardList[i].model.power)
            {
                targetCard = cardList[i];
            }
        }
    }

    public bool IsBlock(int _x)
    {
        if(cardControllerArr[_x,0] != null && cardControllerArr[_x,1] != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsWall()
    {
        //int wallChecker = 0;
        for (int i = 0; i < 3; i++)
        {
            //Debug.Log(i);
            if (cardControllerArr[i,0] == null && cardControllerArr[i, 1] == null)
            {
                return false;
            }
            //wallChecker++;
        }
        return true;
    }

    public void removeCard(CardController cardController)
    {
        for(int i =0; i < cardControllerArr.Length; i++)
        {
            int x = i % 3;
            int y = i / 3;
            if(cardControllerArr[x,y] == cardController)
            {
                cardControllerArr[x, y] = null;
            }
        }
    }

    public void RefreshWall()
    {
        LeaderWall.SetActive(IsWall());
    }

    public void RefreshBlock()
    {
        for(int i = 0; i < 3; i++)
        {
            if(cardControllerArr[i, 0] == null & cardControllerArr[i, 1] != null)
            {
                cardControllerArr[i, 1].view.SetBlockIcon(IsBlock(i));
            }
        }
    }

    public void SetCard(int _x,int _y,CardController _card)
    {
        cardControllerArr[_x, _y] = _card;
        if(_card != null)
        {
            if (cardControllerArr[_x, _y].model.magic)
            {
                cardControllerArr[_x, _y].DestroyCard(cardControllerArr[_x, _y]);
            }
            else
            {
                if (IsWall())
                {
                    //Debug.Log("Wall発動");
                    LeaderWall.SetActive(true);
                }
                if (IsBlock(_x))
                {
                    //Debug.Log("Block発動");
                    cardControllerArr[_x, 1].view.SetBlockIcon(true);
                }
                if (cardControllerArr[_x, 0] != null && cardControllerArr[_x, 0].model.syugo)
                {
                    //Debug.Log("守護発動");
                    cardControllerArr[_x, 0].view.SetSyugoIcon(true);
                }
                if (cardControllerArr[_x, _y].model.sokkou)
                {
                    //Debug.Log("速攻発動");
                    cardControllerArr[_x, _y].model.canAttack = true;
                    //PanelはCardController.DropField()にて
                }
            }
        }
    }
    public void SetCard(Vector2Int _pos,CardController _card)
    {
        SetCard(_pos.x, _pos.y, _card);
    }

    public List<CardController> GetList()
    {
        List<CardController> ret = new();
        for (int i = 0; i < cardControllerArr.Length; i++)
        {
            int x = i % 3;
            int y = i / 3;
            if (cardControllerArr[x, y] != null)
            {
                ret.Add(cardControllerArr[x, y]);
            }
        }
        return ret;
    }
}
