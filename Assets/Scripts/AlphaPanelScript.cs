/*****************************************************************************
// File Name : AlphaPanelScript.cs
// Author : Eve "Kowabungo" Krainik
// Creation Date : March 26, 2026
//
// Brief Description : Controls the dismiss button for the alpha tutorial 
// panel.
*****************************************************************************/

using UnityEngine;

public class AlphaPanelScript : MonoBehaviour
{
    [SerializeField] GameObject panel;

    /// <summary>
    /// destroys panel upon clicking the button.
    /// </summary>
    public void Dismiss()
    {
        Destroy(panel);
    }
}
