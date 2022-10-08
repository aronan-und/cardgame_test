using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    [SerializeField] Text nameText, powerText, hpText, costText, explanationText;
    [SerializeField] Image iconImage;
    [SerializeField] GameObject canAttackPanel, canUsePanel;
    [SerializeField] GameObject frontBlock;
    [SerializeField] GameObject frontSyugo;

    public void Show(CardModel cardModel) // cardModelのデータ取得と反映
    {
        nameText.text = cardModel.name;
        if(cardModel.power > 0 && cardModel.hp > 0)
        {
            powerText.text = "攻" + cardModel.power.ToString();
            hpText.text = "耐" + cardModel.hp.ToString();
        }
        else
        {
            powerText.text = "";
            hpText.text = "";
        }
        costText.text = cardModel.cost.ToString();
        explanationText.text = cardModel.explanation.ToString();
        iconImage.sprite = cardModel.icon;
    }

    public void SetCanAttackPanel(bool flag)
    {
        canAttackPanel.SetActive(flag);
    }

    public void SetCanUsePanel(bool flag) // フラグに合わせてCanUsePanelを付けるor消す
    {
        canUsePanel.SetActive(flag);
    }

    public void SetBlockIcon(bool flag)
    {
        frontBlock.SetActive(flag);
    }

    public void SetSyugoIcon(bool flag)
    {
        frontSyugo.SetActive(flag);
    }
}