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

    public void EnterModeSelect()
    {
        HideElement(StartScreen);
        ShowElement(ModeSelectScreen);
    }

    public void EnterStoryModeSelect()
    {
        HideElement(ModeSelectScreen);
        ShowElement(StageSelect_story);
    }

    public void EnterCasualModeSelect()
    {
        HideElement(ModeSelectScreen);
        ShowElement(StageSelect_casual); 
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
