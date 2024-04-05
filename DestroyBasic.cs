using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBasic : MonoBehaviour
{
    public GameObject other1;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            Destroy(gameObject,1f);
            Destroy(other1,2f);
        }
    }
}
