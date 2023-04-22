using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTracker : MonoBehaviour
{
    float smoothTime = 0.65f;
    Vector3 velocity = Vector3.zero;

    Transform myTransform;

    private void Awake()
    {
        myTransform = transform;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 target = new Vector3(0, PlayerController.rotationOffset, 0);

        // turn left logic // 0 -> 270
        if (Mathf.RoundToInt(target.y) == 270 && Mathf.RoundToInt(myTransform.localEulerAngles.y) == 0)
            target = new Vector3(target.x, target.y-360, target.z);

        // turn right // 270 -> 0
        if (Mathf.RoundToInt(target.y) == 0 && Mathf.RoundToInt(myTransform.localEulerAngles.y) >= 270)
            target = new Vector3(target.x, target.y + 360, target.z);

        // rotate
        if (!GameManager.levelFailed)
            myTransform.localEulerAngles = Vector3.SmoothDamp(myTransform.localEulerAngles, target, ref velocity, smoothTime);
    }
}