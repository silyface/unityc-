using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playercontroller : MonoBehaviour
{
    public float moveSpeed = 5f; 
    public float turnSpeed = 10f;

    private void Update()
    {
        Vector2 inputVector = new Vector2(0,0);

        if(Input.GetKey(KeyCode.W))
        {inputVector.y=+1;}
        if(Input.GetKey(KeyCode.S))
        {inputVector.y=-1;}
        if(Input.GetKey(KeyCode.A))
        {inputVector.x=-1;}
        if(Input.GetKey(KeyCode.D))
        {inputVector.x=+1;}

        inputVector =inputVector.normalized;

        Vector3 moveDoir = new Vector3(inputVector.x,0f,inputVector.y);
        transform.position+= moveDoir*moveSpeed*Time.deltaTime;

        //物体旋转代码，可删
        if(Input.GetKey(KeyCode.LeftArrow))
             {transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);}
        if(Input.GetKey(KeyCode.RightArrow))
            {transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);}


        Debug.Log(Time.deltaTime);
    
    }
}