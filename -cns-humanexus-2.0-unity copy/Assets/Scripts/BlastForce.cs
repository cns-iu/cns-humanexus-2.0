using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;


public class BlastForce : MonoBehaviour
{
    public int blastForce = 0;
    public int blastRadius = 10;
    public bool implode = true;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void FixedUpdate()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, blastRadius);
        Debug.Log("hitColliders: " + hitColliders.Count());

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.attachedRigidbody != GetComponent<Rigidbody>())
            {
                Vector3 offset = transform.position - hitCollider.transform.position;
                Debug.Log("hit: " + hitCollider.name);
                hitCollider.attachedRigidbody.AddForce(offset / offset.magnitude * -blastForce);
                //hitCollider.attachedRigidbody.AddForce(0, 1, 0);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            implode = false;
            blastForce = -50;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            implode = true;
            blastForce = 50;
        }
    }
}
