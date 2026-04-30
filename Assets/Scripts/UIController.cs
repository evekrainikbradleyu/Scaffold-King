/*****************************************************************************
// File Name : UIController.cs
// Author : Eve "I once drank a gallon of milk then ran a mile" Krainik
// Creation Date : March 26, 2026
//
// Brief Description : Controls the game's UI.
*****************************************************************************/

using System;
using System.Collections;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region variables

    // publics

    // *crickets*

    // privates

    private MapController mapController;
    private Image blackscreen;
    private int currentTutorialPanel;
    private bool conveyorsTaughtYet;
    private bool wallsTaughtYet;

    // serialized privates

    [SerializeField] private PlayerController playerController;
    [SerializeField] private MapShellController mapShellController;
    [SerializeField] private ScaffoldingController scaffoldingController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private TMP_Text playerHeightDisplay;
    [SerializeField] private TMP_Text scaffoldingRemainingDisplay;
    [SerializeField] private TMP_Text keyDisplayText;
    [SerializeField] private RawImage nextScaffold;
    [SerializeField] private RawImage currentScaffold;
    [SerializeField] private GameObject blackscreenObj;
    [SerializeField] private Texture2D[] scaffoldIcons;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject keyDisplay;
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject[] tutorialPanels;
    [SerializeField] private GameObject[] mechanicPanels;
    [SerializeField] private bool isTutorial;


    #endregion

    #region start + update

    /// <summary>
    /// assigns vaariables and sets up starting gui
    /// </summary>
    private void Start()
    {
        mapController = mapShellController.shellController;

        heightSlider.minValue = 1;
        heightSlider.maxValue = mapController.MapHeight;
        heightSlider.enabled = false;
        winPanel.SetActive(false);
        menu.SetActive(false);
        currentTutorialPanel = 0;
        conveyorsTaughtYet = scaffoldingController.level != 1;
        wallsTaughtYet = conveyorsTaughtYet;

        blackscreen = blackscreenObj.GetComponent<Image>();

        StartCoroutine(FadeBlackScreen(3, 0));

        if (isTutorial)
        {
            tutorialPanels[0].SetActive(true);
            tutorialPanels[3].transform.Find("next").gameObject.SetActive(false
                );
            tutorialPanels[4].transform.Find("next").gameObject.SetActive(false
                );
            tutorialPanels[5].transform.Find("next").gameObject.SetActive(false
                );
            tutorialPanels[6].transform.Find("next").gameObject.SetActive(false
                );
        }

        if (scaffoldingController.level == 2)
        {
            StartCoroutine(CheckForElevators());
        }

    }

    /// <summary>
    /// updates the height, scaffolding remaining, and scaffold displays every
    /// frame
    /// </summary>
    private void Update()
    {
        UpdateHeightDisplay();
        UpdateScaffoldsRemainingDisplay();
        UpdateScaffoldDisplays();
        UpdateKeyDisplay();
        CheckForLoss();
        DoTutorialPanels();


    }

    #endregion

    #region coroutines

    /// <summary>
    /// coroutine to fade the blackscreen
    /// </summary>
    /// <param name="fadeTime">time the fade should take</param>
    /// <param name="endAlpha">alpha value for screen to end on</param>
    /// <returns>yields null</returns>
    private IEnumerator FadeBlackScreen(float fadeTime, float endAlpha)
    {
        blackscreen.gameObject.SetActive(true);

        float elapsedTime = 0;
        float startAlpha = blackscreen.color.a;

        while (elapsedTime < fadeTime)
        {
            blackscreen.color = new Color
            (
                blackscreen.color.r,
                blackscreen.color.g,
                blackscreen.color.b,
                Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeTime)
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        blackscreen.color = new Color
        (
            blackscreen.color.r,
            blackscreen.color.g,
            blackscreen.color.b,
            endAlpha
        );

        blackscreen.gameObject.SetActive(endAlpha != 0);

        yield return null;
    }

    /// <summary>
    /// does the blackscreen fade at the end and times the win panel to open 
    /// slightly after
    /// </summary>
    /// <returns>yields null</returns>
    private IEnumerator DoWinFade()
    {
        yield return StartCoroutine(FadeBlackScreen(3, 1));

        yield return new WaitForSeconds(0.5f);

        winPanel.SetActive(true);
        
        yield return null;
    }

    private IEnumerator CheckForElevators()
    {
        yield return new WaitUntil(() => scaffoldingController.
            PlayerHasEncounteredElevator());
        mechanicPanels[2].SetActive(true);
        yield return null;
    }
    #endregion

    #region private functions

    /// <summary>
    /// updates height display
    /// </summary>
    private void UpdateHeightDisplay()
    {
        // enables then disables to update the display without allowing player
        // to click slider
        heightSlider.enabled = true;
        heightSlider.value = playerController.playerYLayer + 1;
        heightSlider.enabled = false;
        playerHeightDisplay.text = heightSlider.value.ToString();
    }
    
    /// <summary>
    /// manages panels during the tutorial
    /// </summary>
    private void DoTutorialPanels()
    {
        if (isTutorial)
        {
            if (cameraController.PlayerHasTurnedCamera() && !tutorialPanels[3].
                transform.Find("next").gameObject.activeSelf)
            {
                tutorialPanels[3].transform.Find("next").gameObject.SetActive(
                    true);
            }
            if (playerController.PlayerHasMoved() && !tutorialPanels[4].
                transform.Find("next").gameObject.activeSelf)
            {
                tutorialPanels[4].transform.Find("next").gameObject.SetActive(
                    true);
            }
            if (playerController.PlayerHasPlacedScaffolding() && !
                tutorialPanels[5].transform.Find("next").gameObject.activeSelf)
            {
                tutorialPanels[5].transform.Find("next").gameObject.SetActive(
                    true);
            }
            if (playerController.PlayerHasUsedLadder() && !tutorialPanels[6].
                transform.Find("next").gameObject.activeSelf)
            {
                tutorialPanels[6].transform.Find("next").gameObject.SetActive(
                    true);
            }
        }
    }

    /// <summary>
    /// updates the scaffolds remaining display
    /// </summary>
    private void UpdateScaffoldsRemainingDisplay()
    {
        scaffoldingRemainingDisplay.text = scaffoldingController.
            scaffoldingRemaining.ToString();
    }

    /// <summary>
    /// updates the next and current scaffold display
    /// </summary>
    private void UpdateScaffoldDisplays()
    {
        nextScaffold.texture = scaffoldIcons[scaffoldingController.
            nextScaffolding];

        currentScaffold.texture = scaffoldIcons[scaffoldingController.
            currentScaffolding];

        // opens conveyor tutorial panel if it's level 1 and the first time the
        // player's seen a conveyor
        if (scaffoldingController.currentScaffolding == 3 && !conveyorsTaughtYet)
        {
            mechanicPanels[0].SetActive(true);
            conveyorsTaughtYet = true;
        }

        // same thing for walls  
        if (Array.Exists<int>(new int[] { 5, 7 }, i => i == 
            scaffoldingController.currentScaffolding) && !wallsTaughtYet)
        {
            CloseMechanicPanels();
            mechanicPanels[1].SetActive(true);
            wallsTaughtYet = true;
        }

        // changes shade of current scaffold gui when in placing mode
        currentScaffold.color = scaffoldingController.placing ? new Color(0.78f
            , 0.78f, 0.78f) : Color.white;

    }

    /// <summary>
    /// updates the key display
    /// </summary>
    private void UpdateKeyDisplay()
    {
        if (playerController.keyCount == 0)
        {
            keyDisplay.SetActive(false);
        }
        else
        {
            keyDisplay.SetActive(true);
            keyDisplayText.gameObject.SetActive(false);

            if (playerController.keyCount > 1)
            {
                keyDisplayText.gameObject.SetActive(true);
                keyDisplayText.text = "x" + playerController.keyCount;
            }
        }
    }

    #endregion

    #region public functions

    /// <summary>
    /// makes the win panel visible; called from PlayerController when the 
    /// player wins.
    /// </summary>
    public void EnableWinPanel()
    {
        StartCoroutine(DoWinFade());
    }

    /// <summary>
    /// opens the lose panel if the player's out of scaffolding.
    /// </summary>
    public void CheckForLoss()
    {
        //       |
        //   |   |  ||
        // -------------       <---    it's there!
        //   ||  |  |_
        //       |
        if (scaffoldingController.scaffoldingRemaining <= 0 && !losePanel.
            activeSelf)
        {
            losePanel.SetActive(true);
        }
    }

    /// <summary>
    /// exits the level
    /// </summary>
    public void ExitLevel()
    {
        // currently just quits the game. will be updated in beta to go back to
        // menu. next level option will also be added.

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// restarts the current scene
    /// </summary>
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// opens menu
    /// </summary>
    public void OpenMenu()
    {
        menu.SetActive(!menu.activeSelf);
    }

    public void NextTutorialPanel()
    {
        tutorialPanels[currentTutorialPanel].SetActive(false);
        currentTutorialPanel++;
        if (currentTutorialPanel < tutorialPanels.Length)
        {
            tutorialPanels[currentTutorialPanel].SetActive(true);
        }
    }

    public void CloseMechanicPanels()
    {
        foreach (GameObject panel in mechanicPanels)
        {
            if (panel.activeSelf)
            {
                panel.SetActive(false);
            }
        }
    }

    #endregion

}
