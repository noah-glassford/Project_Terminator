using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TxtDestroyTime : MonoBehaviour
{
    public float destroyTime;
    public Vector3 offset;
    public Vector3 RandomIntensity = new Vector3(0.5f, 0, 0);

    private void Start()
    {
        Destroy(gameObject, destroyTime);

        transform.localPosition += offset;
        transform.localPosition += new Vector3(Random.Range(RandomIntensity.x, -RandomIntensity.x),
            Random.Range(-RandomIntensity.y, -RandomIntensity.y),
            Random.Range(-RandomIntensity.z, -RandomIntensity.z));

        transform.Rotate(new Vector3(0, 180, 0));
    }
}
