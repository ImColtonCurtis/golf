using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowConfetti : MonoBehaviour
{
    [SerializeField] GameObject myConf;
    public static GameObject confObj;

    [SerializeField] MeshCollider holeMesh;

    // Start is called before the first frame update
    void Start()
    {
        confObj = myConf;
        GameManager.holCol = holeMesh;
        GameManager.holeLocation = transform.position;
    }
}
