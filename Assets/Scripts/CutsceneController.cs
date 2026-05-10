/*****************************************************************************
// File Name : CutsceneController.cs
// Author : Eve "Storyteller" Krainik
// Creation Date : April 9, 2026
//
// Brief Description : Controls the cutscenes between levels.
*****************************************************************************/

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneController : MonoBehaviour
{
    #region variables

    // publics

    public int cutsceneID;
    public int nextScene;
    public GameObject blackScreen;
    public AudioClip[] cutsceneAudios;
    public Texture2D[] scaffoldGodSprites;
    public GameObject camera;
    public GameObject scaffoldGod;
    public AudioSource audioPlayer;
    public GameObject skipText;

    // privates

    private IEnumerator[] sceneEvents;
    private bool talking;
    private bool talkingDisgustedly;
    private float totalTimeElapsed;
    private InputAction interact;

    #endregion

    #region start + update

    /// <summary>
    /// sets variables, input actions, volume, then starts the cutscene's
    /// coroutines
    /// </summary>
    private void Start()
    {
        talking = false;
        totalTimeElapsed = 0;
        interact = InputSystem.actions.FindAction("Interact");
        interact.performed += InteractPerformed;

        audioPlayer.volume = Mathf.Clamp(GameSettings.volume * 2, 0, 1);

        AssignSceneEvents();
        SetUpScene();
        StartCoroutine(ExecuteSceneEvents());
    }


    /// <summary>
    /// changes between mouth open and closed animations when the scaffold god
    /// is talking
    /// </summary>
    private void Update()
    {
        totalTimeElapsed += Time.deltaTime;

        if (talking)
        {
            scaffoldGod.GetComponent<RawImage>().texture = scaffoldGodSprites[
                totalTimeElapsed % 0.4f > 0.2f ? talkingDisgustedly ? 4 : 0 : 1
                ];
        }
    }

    #endregion

    #region coroutines

    /// <summary>
    /// executes all the cutscene animations in order
    /// </summary>
    /// <returns>yields null</returns>
    private IEnumerator ExecuteSceneEvents()
    {
        foreach (IEnumerator sceneEvent in sceneEvents)
        {
            yield return StartCoroutine(sceneEvent);
        }

        yield return null;
    }

    /// <summary>
    /// fades an object in or out
    /// </summary>
    /// <param name="thing">object to fade</param>
    /// <param name="fadeIn">true if the object is fading in rather than out
    /// </param>
    /// <param name="fadeTime">how long the fade takes</param>
    /// <param name="isBlack">true if the object is all black (used for 
    /// blackscreen)</param>
    /// <returns>yields null</returns>
    private IEnumerator FadeInThing(GameObject thing, bool fadeIn, float fadeTime, bool isBlack)
    {
        float timeElapsed = 0;
        float startAlpha = fadeIn ? 0 : 1;
        float targetAlpha = fadeIn ? 1 : 0;
        while (timeElapsed < fadeTime)
        {
            Color nextColor = new Color(isBlack ? 0 : 1, isBlack ? 0 : 1, 
                isBlack ? 0 : 1, Mathf.Lerp(startAlpha, targetAlpha, 
                timeElapsed / fadeTime));
            thing.GetComponent<RawImage>().color = nextColor;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        thing.GetComponent<RawImage>().color = fadeIn ? isBlack ? Color.black :
            Color.white : new Color(isBlack ? 0 : 1, isBlack ? 0 : 1, isBlack ?
            0 : 1, 0);
        yield return null;
    }

    /// <summary>
    /// tilts the camera up in the first cutscene
    /// </summary>
    /// <param name="tiltTime">how long to take</param>
    /// <returns>yields null</returns>
    private IEnumerator TiltUpCamera(float tiltTime)
    {
        float timeElapsed = 0;

        while (timeElapsed < tiltTime)
        {
            Vector3 nextAngle = new Vector3(Mathf.Lerp(40, -20, timeElapsed / 
                tiltTime), 45, 0);
            camera.transform.eulerAngles = nextAngle;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        camera.transform.eulerAngles = new Vector3(-20, 45, 0);
        yield return null;
    }

    /// <summary>
    /// just waits for time, but in coroutine form for the array
    /// </summary>
    /// <param name="seconds">how long to wait</param>
    /// <returns>yields null</returns>
    private IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        yield return null;
    }

    /// <summary>
    /// makes the scaffold god's face change
    /// </summary>
    /// <param name="face">ID of the face to change to</param>
    /// <param name="time">how long to make the face before continuing</param>
    /// <returns>yields a waitforseconds instance</returns>
    private IEnumerator MakeFace(int face, float time)
    {
        scaffoldGod.GetComponent<RawImage>().texture = scaffoldGodSprites[face]
            ;
        yield return new WaitForSeconds(time);
    }

    /// <summary>
    /// makes the scaffold god talk
    /// </summary>
    /// <param name="time">how long to talk</param>
    /// <param name="disgusted">true if he's disgusted while talking</param>
    /// <returns>yields null</returns>
    private IEnumerator Talk(float time, bool disgusted = false)
    {
        talkingDisgustedly = disgusted;
        talking = true;
        yield return new WaitForSeconds(time);
        talkingDisgustedly = false;
        talking = false;
        yield return null;
    }

    /// <summary>
    /// starts the cutscene audio, but in coroutine form for the array
    /// </summary>
    /// <returns>yields null</returns>
    private IEnumerator StartAudio()
    {
        audioPlayer.Play();
        yield return null;
    }

    /// <summary>
    /// sends the player to the next scene, but in coroutine form for the array
    /// </summary>
    /// <returns>yields null</returns>
    private IEnumerator SendToNextScene()
    {
        SceneManager.LoadScene(nextScene);
        yield return null;
    }

    /// <summary>
    /// removes the skip notification after 5 seconds
    /// </summary>
    /// <returns>yields null</returns>
    private IEnumerator RemoveSkipButton()
    {
        yield return new WaitForSeconds(5);
        Destroy(skipText);
        yield return null;
    }

    #endregion

    #region private functions

    /// <summary>
    /// sets up the scene depending on which cutscene it is
    /// </summary>
    private void SetUpScene()
    {
        switch (cutsceneID) // this could be an if statement, didn't know if i
        {                   // would need another cutscene setup at time of 
            case 0:         // writing

                // starts with camera tilted down and absent scaffold god in
                // first cutscene
                camera.transform.eulerAngles = new Vector3(40, 45, 0);
                scaffoldGod.GetComponent<RawImage>().texture = 
                    scaffoldGodSprites[2];
                scaffoldGod.GetComponent<RawImage>().color = new Color(1, 1, 1,
                    0);
                blackScreen.GetComponent<RawImage>().color = new Color(0, 0, 0, 
                    1);
                audioPlayer.clip = cutsceneAudios[0];
                audioPlayer.Play();
                break;
            default:
                camera.transform.eulerAngles = new Vector3(-20, 45, 0);
                scaffoldGod.GetComponent<RawImage>().texture =
                    scaffoldGodSprites[2];
                scaffoldGod.GetComponent<RawImage>().color = new Color(1, 1, 1,
                    1);
                blackScreen.GetComponent<RawImage>().color = new Color(0, 0, 0, 
                    1);
                audioPlayer.clip = cutsceneAudios[cutsceneID];
                break;
        }
        StartCoroutine(RemoveSkipButton());
    }

    /// <summary>
    /// assigns the scene events array depending on what cutscene it is
    /// </summary>
    private void AssignSceneEvents()
    {
        switch(cutsceneID)
        {
            case 0:
                sceneEvents = new IEnumerator[] 
                {
                    FadeInThing(blackScreen, false, 4, true),
                    TiltUpCamera(1),
                    Wait(2),
                    FadeInThing(scaffoldGod, true, 2, false),
                    Talk(7.6f),
                    MakeFace(2, 3),
                    Talk(3.9f),
                    MakeFace(3, 1.9f),
                    Talk(6),
                    MakeFace(2, 3.2f),
                    Talk(4),
                    MakeFace(2, 5),
                    Talk(13),
                    MakeFace(4, 2),
                    Talk(6.7f),
                    MakeFace(2, 2),
                    Talk(4),
                    MakeFace(2, 0.1f),
                    FadeInThing(blackScreen, true, 1, true),
                    SendToNextScene()
                };
                break;
            case 1:
                sceneEvents = new IEnumerator[]
                {
                    FadeInThing(blackScreen, false, 2, true),
                    StartAudio(),
                    Talk(4.2f),
                    MakeFace(2, 5.5f),
                    Talk(2.9f),
                    MakeFace(3, 1.8f),
                    Talk(7.9f),
                    MakeFace(2, 2),
                    Talk(7.5f),
                    MakeFace(3, 3.2f),
                    Talk(2.3f),
                    MakeFace(2, 2),
                    FadeInThing(blackScreen, true, 1, true),
                    SendToNextScene()
                };
                break;
            case 2:
                sceneEvents = new IEnumerator[]
                {
                    FadeInThing(blackScreen, false, 2, true),
                    StartAudio(),
                    Talk(3, disgusted: true),
                    MakeFace(4, 3.3f),
                    Talk(6f, disgusted: true),
                    MakeFace(4, 1.8f),
                    Talk(1.8f),
                    MakeFace(2, 1.6f),
                    MakeFace(4, 0.4f),
                    Talk(1.5f),
                    MakeFace(2, 0.1f),
                    FadeInThing(blackScreen, true, 1, true),
                    SendToNextScene()
                };
                break;
            case 3:
                sceneEvents = new IEnumerator[]
                {
                    FadeInThing(blackScreen, false, 2, true),
                    StartAudio(),
                    Talk(3.5f, disgusted: true),
                    Talk(2.8f),
                    MakeFace(2, 3),
                    Talk(1.5f),
                    MakeFace(2, 1),
                    Talk(2f),
                    MakeFace(2, 0.5f),
                    Talk(1),
                    MakeFace(2, 4f),
                    Talk(4.5f),
                    MakeFace(3, 1),
                    Talk(4.3f),
                    MakeFace(2, 2),
                    FadeInThing(blackScreen, true, 1, true),
                    SendToNextScene()
                };
                break;
        }
    }

    #endregion

    #region Input Actions

    /// <summary>
    /// skips cutscene on space bar press
    /// </summary>
    /// <param name="obj">context</param>
    private void InteractPerformed(InputAction.CallbackContext obj)
    {
        StartCoroutine(SendToNextScene());
    }

    #endregion
}
