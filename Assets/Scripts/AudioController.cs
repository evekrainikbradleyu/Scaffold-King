/*****************************************************************************
// File Name : AudioController.cs
// Author : Eve "Goddamn" Krainik
// Creation Date : April 15, 2026
//
// Brief Description : Controls audio. Crazy, right?
*****************************************************************************/

using System.Collections;
using UnityEngine;

public class AudioController : MonoBehaviour
{

    #region variables

    // publics

    // privates

    // serialized privates

    [SerializeField] AudioSource backgroundMusic;
    [SerializeField] AudioSource dialogAudio;
    [SerializeField] AudioClip[] godClips;
    [SerializeField] AudioClip[] tonyClips;
    [SerializeField] AudioClip lavaClip;
    [SerializeField] ScaffoldingController scaffoldingController;

    #endregion

    #region start + update

    /// <summary>
    /// starts audios
    /// </summary>
    private void Start()
    {
        UpdateVolumes();

        if (scaffoldingController.level == 3)
        {
            StartCoroutine(PlayLavaClip());
        }
        else
        {
            StartCoroutine(DoDialogLoop());
        }
    }

    #endregion

    #region coroutines

    /// <summary>
    /// plays the lava audio clip at the beginning of level 3
    /// </summary>
    /// <returns>yields null</returns>
    private IEnumerator PlayLavaClip()
    {
        yield return new WaitForSeconds(5);
        dialogAudio.clip = lavaClip;
        dialogAudio.Play();
        StartCoroutine(DoDialogLoop());
        yield return null;
    }

    /// <summary>
    /// loops and plays random scaffold god + tony dialog every 2-3 minutes.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DoDialogLoop()
    {
        yield return new WaitForSeconds(Random.Range(120, 180));
        dialogAudio.clip = godClips[Random.Range(0, godClips.Length)];
        dialogAudio.Play();
        yield return new WaitForSeconds(dialogAudio.clip.length);
        dialogAudio.clip = tonyClips[Random.Range(0, tonyClips.Length)];
        dialogAudio.Play();
        yield return new WaitForSeconds(dialogAudio.clip.length);
        StartCoroutine(DoDialogLoop());
        yield return null;
    }

    #endregion

    #region public functions

    /// <summary>
    /// updates the volume of audio when the game settings change.
    /// </summary>
    public void UpdateVolumes()
    {
        backgroundMusic.volume = GameSettings.volume * 0.2f;
        dialogAudio.volume = GameSettings.volume;
    }

    #endregion

}
