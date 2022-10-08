using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardEntity", menuName = "Create CardEntity")]

public class CardEntity : ScriptableObject
{
    public int cardID;
    public new string name;
    public int cost;
    public int power;
    public int hp;
    public string explanation;
    public Sprite icon;
    public bool magic;
    public int targetType;
    public bool syugo;
    public bool sokkou;
    public bool draw1;
    public bool draw2;
    public bool draw3;
}
