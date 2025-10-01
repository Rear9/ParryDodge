using System;
using System.Collections;
using UnityEngine;

public class PlayerColor : MonoBehaviour
{
    [Header("Colours")] 
    public Color neutralColor = new Color(1, 0, 0);
    public Color parryColor = new Color(1, .65f, 0);
    public Color dodgeColor = new Color(.3f, .6f, 1);

    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private float colourSpeed = .2f;
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    public IEnumerator ColorSprite(Color clr)
    {
        float time = 0;
        while (time < colourSpeed)
        {
            time += Time.deltaTime;
            sr.color = Color.Lerp(sr.color, clr, time / colourSpeed);
            yield return null;
        }
    }
    
}
