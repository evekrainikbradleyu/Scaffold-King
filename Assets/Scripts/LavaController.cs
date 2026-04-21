using System.Collections;
using UnityEngine;

public class LavaController : MonoBehaviour
{
    #region variables

    // publics

    // privates

    // serialized privates 

    [SerializeField] float lavaRiseTime;

    #endregion

    #region start + update



    #endregion

    #region coroutines

    private IEnumerator MoveLava(float riseTime, float height)
    {
        float timeElapsed = 0;
        float originalHeight = transform.position.y + transform.localScale.y / 
            2;

        while (timeElapsed < lavaRiseTime)
        {
            transform.localScale = new Vector3
            (
                transform.localScale.x,
                Mathf.Lerp(originalHeight, height, timeElapsed / riseTime),
                transform.localScale.z
            );

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = new Vector3
        (
            transform.localScale.x,
            height,
            transform.localScale.z
        );

        yield return null;
    }

    #endregion

    #region public functions

    public void UpdateLava(float height)
    {
        if (height < 0 ) { return; }

        StartCoroutine(MoveLava(lavaRiseTime, height));
    }

    #endregion

}
