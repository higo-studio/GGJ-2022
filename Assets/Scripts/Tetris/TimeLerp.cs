using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeLerp : MonoBehaviour
{
    public float CurrentTime;
    public float TargetTime;
    private Text text;
    private float Velocity;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        CurrentTime = Mathf.SmoothDamp(CurrentTime, TargetTime,ref Velocity, Time.deltaTime, 50f);
        text.text = Mathf.FloorToInt(CurrentTime).ToString();
    }
}
