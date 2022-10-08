using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttckRule : MonoBehaviour
{
    [SerializeField]
    Transform playerHand, playerCenterFront, playerRightFront, playerLeftFront,
        playerCenterBack, playerRightBack, playerLeftBack, enemyHand, enemyCenterFront,
        enemyRightFront, enemyLeftFront, enemyCenterBack, enemyRightBack, enemyLeftBack;
    CardController attackCard;
    CardController defenceCard;

    // Start is called before the first frame update
    public void AttckRule()
    {
        ///エネミーが攻撃したい対象が攻撃できない時のルール
        //守護がいる時、守護以外は攻撃不可
        //ウォール時、リーダー攻撃不可
        //ブロック時、後列攻撃不可

        //①前列の守護付きのカード（２体以上の時は攻撃が高いカードを狙う）
        //②自身の攻撃より低い体力のカードで最も攻撃が高いカード（ブロック対象ならその前にいるカードを狙う）
        //③最も攻撃が高いカード

        CardController playerCenterFrontCard = playerCenterFront.GetComponentInChildren<CardController>();
        CardController playerRightFrontCard = playerRightFront.GetComponentInChildren<CardController>();
        CardController playerLeftFrontCard = playerLeftFront.GetComponentInChildren<CardController>();
        CardController playerCenterBackCard = playerCenterBack.GetComponentInChildren<CardController>();
        CardController playerRightBackCard = playerRightBack.GetComponentInChildren<CardController>();
        CardController playerLeftBackCard = playerLeftBack.GetComponentInChildren<CardController>();

        //①
        int powerChecker = -1;
        if (playerCenterFrontCard != null && playerCenterFrontCard.model.syugo)
        {
            powerChecker = playerCenterFrontCard.model.power;
        }
        if (playerRightFrontCard != null && playerRightFrontCard.model.syugo)
        {
            powerChecker = playerRightFrontCard.model.power;
        }
        if (playerLeftFrontCard != null && playerLeftFrontCard.model.syugo)
        {
            powerChecker = playerLeftFrontCard.model.power;
        }
        if(powerChecker > -1)
        {
            if (playerCenterFrontCard != null && playerCenterFrontCard.model.power == powerChecker)
            {
                defenceCard = playerCenterFrontCard;
                //playerCenterFrontCardに攻撃
            }
            if (playerRightFrontCard != null && playerRightFrontCard.model.power == powerChecker)
            {
                defenceCard = playerRightFrontCard;
                //playerRightFrontCardに攻撃
            }
            if (playerLeftFrontCard != null && playerLeftFrontCard.model.power == powerChecker)
            {
                defenceCard = playerLeftFrontCard;
                //playerLeftFrontCardに攻撃
            }
        }
        //②
        else
        {
            powerChecker = -1;
            if (playerCenterFrontCard != null && attackCard.model.power - playerCenterFrontCard.model.hp >= 0)
            {
                powerChecker = playerCenterFrontCard.model.power;
            }
            if (playerRightFrontCard != null && attackCard.model.power - playerRightFrontCard.model.hp >= 0)
            {
                powerChecker = playerRightFrontCard.model.power;
            }
            if (playerLeftFrontCard != null && attackCard.model.power - playerLeftFrontCard.model.hp >= 0)
            {
                powerChecker = playerLeftFrontCard.model.power;
            }
            if (playerCenterBackCard != null && attackCard.model.power - playerCenterBackCard.model.hp >= 0)
            {
                powerChecker = playerCenterBackCard.model.power;
            }
            if (playerRightBackCard != null && attackCard.model.power - playerRightBackCard.model.hp >= 0)
            {
                powerChecker = playerRightBackCard.model.power;
            }
            if (playerLeftBackCard != null && attackCard.model.power - playerLeftBackCard.model.hp >= 0)
            {
                powerChecker = playerLeftBackCard.model.power;
            }
            if(powerChecker > -1)
            {
                if (playerCenterFrontCard != null && playerCenterFrontCard.model.power == powerChecker)
                {
                    defenceCard = playerCenterFrontCard;
                    //playerCenterFrontCardに攻撃
                }
                if (playerRightFrontCard != null && playerRightFrontCard.model.power == powerChecker)
                {
                    defenceCard = playerRightFrontCard;
                    //playerRightFrontCardに攻撃
                }
                if (playerLeftFrontCard != null && playerLeftFrontCard.model.power == powerChecker)
                {
                    defenceCard = playerLeftFrontCard;
                    //playerLeftFrontCardに攻撃
                }
                if (playerCenterBackCard != null && playerCenterBackCard.model.power == powerChecker)
                {
                    if (playerCenterFrontCard != null)
                    {
                        defenceCard = playerCenterBackCard;
                        //playerCenterBackCardに攻撃
                    }
                    else
                    {
                        defenceCard = playerCenterFrontCard;
                        //playerCenterFrontCardに攻撃
                    }
                }
                if (playerRightBackCard != null && playerRightBackCard.model.power == powerChecker)
                {
                    if(playerRightFrontCard != null)
                    {
                        defenceCard = playerRightBackCard;
                        //playerRightBackCardに攻撃
                    }
                    else
                    {
                        defenceCard = playerRightFrontCard;
                        //playerRightFrontCardに攻撃
                    }
                }
                if (playerLeftBackCard != null && playerLeftBackCard.model.power == powerChecker)
                {
                    if(playerLeftFrontCard != null)
                    {
                        defenceCard = playerLeftBackCard;
                        //playerLeftBackCardに攻撃
                    }
                    else
                    {
                        defenceCard = playerLeftFrontCard;
                        //playerLeftFrontCardに攻撃
                    }
                }
            }
            //③
            else
            {
                powerChecker = -1;
                if (playerCenterFrontCard != null)
                {
                    powerChecker = playerCenterFrontCard.model.power;
                }
                if (playerRightFrontCard != null)
                {
                    powerChecker = playerRightFrontCard.model.power;
                }
                if (playerLeftFrontCard != null)
                {
                    powerChecker = playerLeftFrontCard.model.power;
                }
                if (playerCenterBackCard != null)
                {
                    powerChecker = playerCenterBackCard.model.power;
                }
                if (playerRightBackCard != null)
                {
                    powerChecker = playerRightBackCard.model.power;
                }
                if (playerLeftBackCard != null)
                {
                    powerChecker = playerLeftBackCard.model.power;
                }
                if (playerCenterFrontCard != null && playerCenterFrontCard.model.power == powerChecker)
                {
                    defenceCard = playerCenterFrontCard;
                    //playerCenterFrontCardに攻撃
                }
                if (playerRightFrontCard != null && playerRightFrontCard.model.power == powerChecker)
                {
                    defenceCard = playerRightFrontCard;
                    //playerRightFrontCardに攻撃
                }
                if (playerLeftFrontCard != null && playerLeftFrontCard.model.power == powerChecker)
                {
                    defenceCard = playerLeftFrontCard;
                    //playerLeftFrontCardに攻撃
                }
                if (playerCenterBackCard != null && playerCenterBackCard.model.power == powerChecker)
                {
                    if (playerCenterFrontCard != null)
                    {
                        defenceCard = playerCenterBackCard;
                        //playerCenterBackCardに攻撃
                    }
                    else
                    {
                        defenceCard = playerCenterFrontCard;
                        //playerCenterFrontCardに攻撃
                    }
                }
                if (playerRightBackCard != null && playerRightBackCard.model.power == powerChecker)
                {
                    if (playerRightFrontCard != null)
                    {
                        defenceCard = playerRightBackCard;
                        //playerRightBackCardに攻撃
                    }
                    else
                    {
                        defenceCard = playerRightFrontCard;
                        //playerRightFrontCardに攻撃
                    }
                }
                if (playerLeftBackCard != null && playerLeftBackCard.model.power == powerChecker)
                {
                    if (playerLeftFrontCard != null)
                    {
                        defenceCard = playerLeftBackCard;
                        //playerLeftBackCardに攻撃
                    }
                    else
                    {
                        defenceCard = playerLeftFrontCard;
                        //playerLeftFrontCardに攻撃
                    }
                }
            }
        }
    }
}
