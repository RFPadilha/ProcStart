using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Update is called once per frame
    [SerializeField] float moveStrength = 5;
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += new Vector3(0, 0, 1f * Time.deltaTime * moveStrength);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position += new Vector3(-1f * Time.deltaTime * moveStrength, 0,0);

        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position += new Vector3(0, 0, -1f * Time.deltaTime * moveStrength);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += new Vector3(1f * Time.deltaTime * moveStrength, 0, 0);
        }
    }
}
