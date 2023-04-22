using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightTracker : MonoBehaviour
{
    float smoothTime = 0.4f;
    Vector3 velocity = Vector3.zero;

    Transform myTransform;

    private void Awake()
    {
        myTransform = transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 targetPosition = new Vector3(0, PlayerController.hieghtOffset, 0);

        // Smoothly move the camera towards that target position
        if ((!GameManager.levelPassed && !GameManager.levelFailed && !miniGameManager.inMiniGame) || (miniGameManager.inMiniGame && !miniGameManager.levelFailed2))
            myTransform.localPosition = Vector3.SmoothDamp(myTransform.localPosition, targetPosition, ref velocity, smoothTime);
    }
}
