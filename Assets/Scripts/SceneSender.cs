/*****************************************************************************
// File Name : SceneSender.cs
// Author : Eve "Sender of Scenes" Krainik
// Creation Date : March 26, 2026
//
// Brief Description : Attached to buttons. Sends to the chosen scene upon a 
// click.
*****************************************************************************/

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSender : MonoBehaviour
{
    [SerializeField] int nextScene;

    /// <summary>
    /// sends player to the chosen scene
    /// </summary>
    public void SendToScene()
    {
        if (nextScene == -1)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        else
        {
            SceneManager.LoadScene(nextScene);
        }
    }
}
