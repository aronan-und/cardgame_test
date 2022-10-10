using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class CardModel
{
    public int cardID;
    public string name;
    public int cost;
    public int power;
    public int hp;
    public string explanation;
    public Sprite icon;
    public bool magic;
    public int targetType;
    public bool syugo;
    public bool sokkou;
    public int drawCount;

    public bool PlayerCard = false;
    public bool canAttack = false;
    public bool canUse = false;
    public bool FieldCard = false;

    public CardModel(int _cardID, bool playerCard) // データを受け取り、その処理
    {
        CardEntity cardEntity = Resources.Load<CardEntity>("CardEntityList/Card" + _cardID);

        cardID = cardEntity.cardID;
        name = cardEntity.name;
        cost = cardEntity.cost;
        power = cardEntity.power;
        hp = cardEntity.hp;
        explanation = cardEntity.explanation;
        icon = cardEntity.icon;
        magic = cardEntity.magic;
        targetType = cardEntity.targetType;
        syugo = cardEntity.syugo;
        sokkou = cardEntity.sokkou;
        drawCount = cardEntity.drawCount;

        PlayerCard = playerCard;
    }
}