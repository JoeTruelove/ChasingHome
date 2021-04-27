using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterFront : MonoBehaviour
{
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag == "Ground")
        {


            player.GetComponent<NewFPPlayerMove>().SetFront();

        }

        
    }
}
