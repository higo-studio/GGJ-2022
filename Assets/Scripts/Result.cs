using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Result : MonoBehaviour
{
    float fromValue = 0;
    float toValue = 0.9f;
    float fromValueRes = 0;
    float toValueRes = 1f;

    public Transform P1Result;
    public Transform P2Result;
    // Start is called before the first frame update
    void Start()
    {
        //这里改变玩家结果的图片



        Color temColor = GetComponent<Image>().color;
        temColor.a = fromValue;
        Tweener tweener = DOTween.ToAlpha(() => temColor, x => temColor = x, toValue, 1.5f);
        Color temColor1 = Color.white;
        temColor1.a = fromValueRes;
        Tweener tweener1 = DOTween.ToAlpha(() => temColor1, x => temColor1 = x, toValueRes, 1f);
        tweener.onUpdate = () => {
            GetComponent<Image>().color = temColor;
            P1Result.GetComponent<Image>().color = temColor1;
            P2Result.GetComponent<Image>().color = temColor1;
        };

        P1Result.transform.localScale *= 2;
        P2Result.transform.localScale *= 2;
        P1Result.DOScale(1, 0.5f);
        P2Result.DOScale(1, 0.5f);
    }

}
