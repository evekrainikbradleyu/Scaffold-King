/*****************************************************************************
// File Name : CutsceneController.cs
// Author : Eve "Storyteller" Krainik
// Creation Date : April 9, 2026
//
// Brief Description : Controls the cutscenes between levels
*****************************************************************************/

using System;
using System.Collections;
using UnityEditor.ShaderGraph.Serialization;
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

    private void Start()
    {
        talking = false;
        interact = InputSystem.actions.FindAction("Interact");
        interact.performed += InteractPerformed;

        AssignSceneEvents();
        SetUpScene();
        StartCoroutine(ExecuteSceneEvents());
    }



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

    private IEnumerator ExecuteSceneEvents()
    {
        foreach (IEnumerator sceneEvent in sceneEvents)
        {
            yield return StartCoroutine(sceneEvent);
        }

        yield return null;
    }

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

    private IEnumerator Wait(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        yield return null;
    }

    private IEnumerator MakeFace(int face, float time)
    {
        scaffoldGod.GetComponent<RawImage>().texture = scaffoldGodSprites[face]
            ;
        yield return new WaitForSeconds(time);
    }

    private IEnumerator Talk(float time, bool disgusted = false)
    {
        talkingDisgustedly = disgusted;
        talking = true;
        yield return new WaitForSeconds(time);
        talkingDisgustedly = false;
        talking = false;
        yield return null;
    }

    private IEnumerator StartAudio()
    {
        audioPlayer.Play();
        yield return null;
    }

    private IEnumerator SendToNextScene()
    {
        SceneManager.LoadScene(nextScene);
        yield return null;
    }

    private IEnumerator RemoveSkipButton()
    {
        yield return new WaitForSeconds(5);
        Destroy(skipText);
        yield return null;
    }

    #endregion

    #region private functions

    private void SetUpScene()
    {
        switch (cutsceneID)
        {
            case 0:
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
                    FadeInThing(blackScreen, false, 2, true)

                };
                break;
        }
    }

    #endregion

    #region Input Actions

    private void InteractPerformed(InputAction.CallbackContext obj)
    {
        StartCoroutine(SendToNextScene());
    }

    #endregion
}
