using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTracker : MonoBehaviour
{
    [SerializeField] Transform targetObj, rotationTarget;
    [SerializeField] Vector3 offset;
    Transform myTransform;

    [SerializeField] bool changeY, lockY;

    // Start is called before the first frame update
    void Start()
    {
        myTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        myTransform.position = targetObj.position + offset; //3.5171,  2.0977
        if (changeY)
            myTransform.localPosition = new Vector3(myTransform.localPosition.x, myTransform.localPosition.y, myTransform.localPosition.z);
        else
        {
            myTransform.localEulerAngles = new Vector3(0, rotationTarget.transform.localEulerAngles.y, 0);
            if (lockY)
                myTransform.localPosition = new Vector3(myTransform.localPosition.x, -7.9f, myTransform.localPosition.z);
            else
                myTransform.localPosition = new Vector3(myTransform.localPosition.x, -6.4f, myTransform.localPosition.z);
        }
    }
}
