using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VNManager : MonoBehaviour
{
    public GameObject char1;
    public GameObject char2;
    public GameObject textBox;

    [SerializeField] string textToSpeak;
    [SerializeField] int currentTextLength;
    [SerializeField] int textLength;
    [SerializeField] GameObject mainTextObject;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(EventStarter());
    }

    // Update is called once per frame
    void Update()
    {
        textLength = TextHandler.charCount;
    }

    IEnumerator EventStarter()
    {
        yield return new WaitForSeconds(1);
        mainTextObject.SetActive(true);

        textToSpeak = "안녕하세요 ㅁㄷㄴ래ㅑㅂ멪ㄱ햡ㅈ몯ㄱㅎ댐ㄴ야혼ㄷㅇㄱㄹ해넝랴ㅗ헌애ㅗㅅ레ㅗㅈㅇㅁㄴㄱㄹ솢ㄷ메ㅐㄱㄴㄹ호ㅑㅈㄷ넝";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;
        currentTextLength = textToSpeak.Length;
        TextHandler.runTextPrint = true;
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLength == currentTextLength);

        textBox.SetActive(true);
    }
}
