using UnityEngine;
using System.Collections;

public class CircleAnimTemp : MonoBehaviour {

    [Range(0.1f, 10f)]
    public float radius = 1.0f;
    [Range(0.1f, 10f)]
    public float speed = 1.0f;
    public bool lookInWalkingDir = false;
    private Vector3 startPos;

    void Awake()
    {
        startPos = transform.position;
    }

	// Update is called once per frame
	void Update () {
        float x = Mathf.Sin(Time.time / Mathf.PI * speed);
        float y = Mathf.Cos(Time.time / Mathf.PI * speed);

        var newPos = startPos + radius * new Vector3(x, 0, y); 
        if (lookInWalkingDir)
        {
            transform.forward = (newPos - transform.position).normalized;
        }
        transform.position = newPos;
	}
}
