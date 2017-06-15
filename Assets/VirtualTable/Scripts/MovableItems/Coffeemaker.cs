using UnityEngine;
using System.Collections;

public class Coffeemaker : MonoBehaviour
{
    public Vector3 centerOfMass;
    public Rigidbody rigidBody;

    // Use this for initialization
    void Start () {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass = centerOfMass;
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
