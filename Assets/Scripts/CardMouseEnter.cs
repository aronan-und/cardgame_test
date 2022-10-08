using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class CardMouseEnter : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private void Show(CardModel cardModel,bool visible)
    {
        GameObject nameText;
        GameObject powerText;
        GameObject hpText;
        GameObject costText;
        GameObject explanationText;
        GameObject iconImage;
        GameObject panel;
        GameObject costPanel;

        if (cardModel.PlayerCard)
        {
            nameText = GameObject.Find("CMEPlayerName");
            powerText = GameObject.Find("CMEPlayerPower");
            hpText = GameObject.Find("CMEPlayerHp");
            costText = GameObject.Find("CMEPlayerCost");
            explanationText = GameObject.Find("CMEPlayerExplanation");
            iconImage = GameObject.Find("CMEPlayerIcon");
            panel = GameObject.Find("CMEPlayerPanel");
            costPanel = GameObject.Find("CMEPlayerCostPanel");
        }
        else
        {
            nameText = GameObject.Find("CMEEnemyName");
            powerText = GameObject.Find("CMEEnemyPower");
            hpText = GameObject.Find("CMEEnemyHp");
            costText = GameObject.Find("CMEEnemyCost");
            explanationText = GameObject.Find("CMEEnemyExplanation");
            iconImage = GameObject.Find("CMEEnemyIcon");
            panel = GameObject.Find("CMEEnemyPanel");
            costPanel = GameObject.Find("CMEEnemyCostPanel");
        }
        if (visible)
        {
            nameText.GetComponent<Text>().text = cardModel.name;
            powerText.GetComponent<Text>().text = "攻" + cardModel.power.ToString();
            hpText.GetComponent<Text>().text = "耐" + cardModel.hp.ToString();
            costText.GetComponent<Text>().text = cardModel.cost.ToString();
            explanationText.GetComponent<Text>().text = cardModel.explanation.ToString();
            iconImage.GetComponent<Image>().sprite = cardModel.icon;
            iconImage.GetComponent<Image>().enabled = true;
            panel.GetComponent<Image>().enabled = true;
            costPanel.GetComponent<Image>().enabled = true;
        }
        else
        {
            nameText.GetComponent<Text>().text = "";
            powerText.GetComponent<Text>().text = "";
            hpText.GetComponent<Text>().text = "";
            costText.GetComponent<Text>().text = "";
            explanationText.GetComponent<Text>().text = "";
            iconImage.GetComponent<Image>().enabled = false;
            panel.GetComponent<Image>().enabled = false;
            costPanel.GetComponent<Image>().enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Show(GetComponent<CardController>().model,true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Show(GetComponent<CardController>().model, false);
    }
}
