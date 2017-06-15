using UnityEngine;
using System.Collections;

public class TVFlicker : MonoBehaviour {

    private float[] smoothing = new float[20];
    Light light;

    // Use this for initialization
    void Start () {
        light = GetComponent<Light>();
        for (int i = 0; i < smoothing.Length; i++)
        {
            smoothing[i] = .0f;
        }
    }
	
	// Update is called once per frame
	void Update () {
        float sum = .0f;

        // Shift values in the table so that the new one is at the
        // end and the older one is deleted.
        for (int i = 1; i < smoothing.Length; i++)
        {
            smoothing[i - 1] = smoothing[i];
            sum += smoothing[i - 1];
        }

        // Add the new value at the end of the array.
        smoothing[smoothing.Length - 1] = Random.value * 0.6f;
        sum += smoothing[smoothing.Length - 1];

        // Compute the average of the array and assign it to the
        // light intensity.
        light.intensity = sum / smoothing.Length;
    }
}
