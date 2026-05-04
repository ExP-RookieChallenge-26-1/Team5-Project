using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
   [SerializeField] private CanvasGroup StartScreen;
   [SerializeField] private CanvasGroup ModeSelectScreen;
   [SerializeField] private CanvasGroup StageSelect_story;
   [SerializeField] private CanvasGroup StageSelect_casual;
   [SerializeField] private CanvasGroup Settings;

    private void ShowElement(CanvasGroup element)
    {
        element.alpha = 1f;
        element.interactable = true;
        element.blocksRaycasts = true;
    }
    private void HideElement(CanvasGroup element)
    {
        element.alpha = 0f;
        element.interactable = false;
        element.blocksRaycasts = false;
    }

    private void HideEverything()
    {
        HideElement(Settings);
        HideElement(StartScreen);
        HideElement(ModeSelectScreen);
        HideElement(StageSelect_casual);
        HideElement(StageSelect_story);
    }

    private void showbydepth(int a)
    {
        if (a == 0)
        {
            HideEverything();
            ShowElement(StartScreen);
        }
        if (a == 1)//모드설렉션
        {
            HideEverything();
            ShowElement(ModeSelectScreen);
        }
        if (a == 2) //스토리모드
        {
            HideEverything();
            ShowElement(StageSelect_story);
        }
        if (a == 3) //캐쥬얼모드
        {
            HideEverything();
            ShowElement(StageSelect_casual);
        }
    }

    public void ReturntoStartMenu()
    {
        showbydepth(0);
    }

    public void EnterModeSelect()
    {
        showbydepth(1);
    }

    public void EnterStoryModeSelect()
    {
        showbydepth(2);
    }

    public void EnterCasualModeSelect()
    {
        showbydepth(3);
    }

    public void GameExit()
    {
        Application.Quit();
        Debug.Log("겜종료");
    }

    public void SettingsShow()
    {
        ShowElement(Settings);
    }

    public void SettingsHide()
    {
        HideElement(Settings);
    }
    public void ContinueGame()
    {

    }
}
