using System.Text.RegularExpressions;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
   [SerializeField] private CanvasGroup StartScreen;
   [SerializeField] private CanvasGroup StageSelect_story;
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
        HideElement(StageSelect_story);
    }

    private void showbydepth(int a)
    {
        if (a == 0)
        {
            HideEverything();
            ShowElement(StartScreen);
        }
        if (a == 2) //스토리모드
        {
            HideEverything();
            ShowElement(StageSelect_story);
        }
    }

    public void ReturntoStartMenu()
    {
        showbydepth(0);
    }

    public void EnterStoryModeSelect()
    {
        showbydepth(2);
    }

    public void StoryModeStart()
    {
        SceneManager.LoadScene("Sanghyun_Scene");
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
