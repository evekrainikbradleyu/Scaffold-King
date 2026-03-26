/*****************************************************************************
// File Name : CurrentScaffoldMover.cs
// Author : Eve "4:25am" Krainik
// Creation Date : March 26, 2026
//
// Brief Description : Causes the current scaffold display to shift up when the
// mouse hovers over it.
*****************************************************************************/

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CurrentScaffoldMover : MonoBehaviour, IPointerEnterHandler, 
    IPointerExitHandler, IPointerClickHandler
{
    #region variables

    // publics

    // *crickets*

    // privates

    private bool hovering;

    // serialized privates

    [SerializeField] private Vector2 startAndEndPosYValues;
    [SerializeField] private float moveSpeed;
    [SerializeField] private ScaffoldingController scaffoldingController;

    #endregion

    #region pointer enter + leave functions

    /// <summary>
    /// detects when the mouse is hovering over the UI
    /// </summary>
    /// <param name="eventData">event data</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    /// <summary>
    /// detects when the mouse stops hovering over the UI
    /// </summary>
    /// <param name="eventData">event data</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    #endregion

    #region on pointer click function

    /// <summary>
    /// toggles place mode
    /// </summary>
    /// <param name="eventData">event data</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // only toggles if there's scaffolding remaining.
        scaffoldingController.placing = scaffoldingController.
            scaffoldingRemaining > 0 ? !scaffoldingController.placing : false;
    }

    #endregion

    #region start + update

    /// <summary>
    /// sets variables
    /// </summary>
    private void Start()
    {
        hovering = false;
    }

    /// <summary>
    /// performs UI movement when mouse is hovering
    /// </summary>
    private void Update()
    {
        DoHoveringMovement();
        DoReturnMovement();
    }

    #endregion

    #region miscellaneous functions

    /// <summary>
    /// moves the UI up if the mouse is hovering over it
    /// </summary>
    private void DoHoveringMovement()
    {
        // keeps current scaffold icon tucked away if there's no scaffolding
        // remaining
        if (scaffoldingController.scaffoldingRemaining <= 0 ) { return; }

        if (hovering && GetComponent<RectTransform>().anchoredPosition.y <
            startAndEndPosYValues.y)
        {
            // moves the UI up
            GetComponent<RectTransform>().anchoredPosition = new Vector2
                (
                    GetComponent<RectTransform>().anchoredPosition.x,
                    GetComponent<RectTransform>().anchoredPosition.y + moveSpeed * Time
                        .deltaTime
                );
            // moves it to the furthest point if it goes too far up
            if (GetComponent<RectTransform>().anchoredPosition.y >
                startAndEndPosYValues.y)
            {
                GetComponent<RectTransform>().anchoredPosition = new Vector2
                (
                    GetComponent<RectTransform>().anchoredPosition.x,
                    startAndEndPosYValues.y
                );
            }
        }
    }

    /// <summary>
    /// moves the UI back down when the mouse stops hovering over it
    /// </summary>
    private void DoReturnMovement()
    {
        if (!hovering && GetComponent<RectTransform>().anchoredPosition.y >
            startAndEndPosYValues.x)
        {
            // moves the UI down
            GetComponent<RectTransform>().anchoredPosition = new Vector2
                (
                    GetComponent<RectTransform>().anchoredPosition.x,
                    GetComponent<RectTransform>().anchoredPosition.y - moveSpeed * Time
                        .deltaTime
                );
            // moves it to the furthest point down if it goes too far down
            if (GetComponent<RectTransform>().anchoredPosition.y <
                startAndEndPosYValues.x)
            {
                GetComponent<RectTransform>().anchoredPosition = new Vector2
                (
                    GetComponent<RectTransform>().anchoredPosition.x,
                    startAndEndPosYValues.x
                );
            }
        }
    }

    #endregion

}
