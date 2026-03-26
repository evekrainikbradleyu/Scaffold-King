/*****************************************************************************
// File Name : ScaffoldingController.cs
// Author : Eve "I once drank a gallon of milk then ran a mile" Krainik
// Creation Date : March 26, 2026
//
// Brief Description : Controls the game's UI.
*****************************************************************************/

using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    #region variables

    // publics

    // *crickets*

    // privates

    private MapController mapController;

    // serialized privates

    [SerializeField] private PlayerController playerController;
    [SerializeField] private MapShellController mapShellController;
    [SerializeField] private ScaffoldingController scaffoldingController;
    [SerializeField] private Slider heightSlider;
    [SerializeField] private TMP_Text playerHeightDisplay;
    [SerializeField] private TMP_Text scaffoldingRemainingDisplay;
    [SerializeField] private RawImage nextScaffold;
    [SerializeField] private RawImage currentScaffold;
    [SerializeField] private Texture2D[] scaffoldIcons;

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
    }

    #endregion

    #region miscellaneous functions

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

        // changes shade of current scaffold gui when in placing mode
        currentScaffold.color = scaffoldingController.placing ? new Color(0.78f
            , 0.78f, 0.78f) : Color.white;
    }

    #endregion

}
