using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]

public class MouseTest: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image image { get { return GetComponent<Image>(); } }

    // オブジェクトの範囲内にマウスポインタが入った際に呼び出されます。
    // this method called by mouse-pointer enter the object.

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = Color.red;
    }

    // オブジェクトの範囲内からマウスポインタが出た際に呼び出されます。
    // 
    public void OnPointerExit(PointerEventData eventData)
    {
        image.color = Color.white;
    }
}