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

        textToSpeak = "aaaa";
        textBox.GetComponent<TMPro.TMP_Text>().text = textToSpeak;
        currentTextLength = textToSpeak.Length;
        TextHandler.runTextPrint = true;
        yield return new WaitForSeconds(0.05f);
        yield return new WaitForSeconds(1);
        yield return new WaitUntil(() => textLength == currentTextLength);


        textBox.SetActive(true);

    }
}
