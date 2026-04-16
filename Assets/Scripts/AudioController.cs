/*****************************************************************************
// File Name : AudioController.cs
// Author : Eve "Goddamn" Krainik
// Creation Date : April 15, 2026
//
// Brief Description : Controls audio (crazy)
*****************************************************************************/

using UnityEngine;

public class AudioController : MonoBehaviour
{

    #region variables

    // publics

    // privates

    // serialized privates

    [SerializeField] AudioSource backgroundMusic;

    #endregion

    #region start + update

    /// <summary>
    /// starts audios
    /// </summary>
    private void Start()
    {
        //backgroundMusic.Play();
    }

    #endregion

}
