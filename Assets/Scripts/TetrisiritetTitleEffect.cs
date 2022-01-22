using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TetrisiritetTitleEffect : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Pingpong(1,0.75f,1);
        Text text = transform.GetChild(0).GetComponent<Text>();
        text.DOText(">>HIGO::Just put some shit here.", 3f).SetDelay(2f); ;
    }

    void Pingpong(float fromValue, float toValue, float duration)
    {
        Color temColor = gameObject.GetComponent<Image>().color;
        temColor.a = fromValue;
        Tweener tweener = DOTween.ToAlpha(() => temColor, x => temColor = x, toValue, duration);
        tweener.onUpdate = () => { gameObject.GetComponent<Image>().color = temColor; };
        tweener.onComplete = () =>
        {
            Pingpong(toValue, fromValue, duration);
        };
    }
}
