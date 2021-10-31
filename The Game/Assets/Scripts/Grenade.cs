using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float FuseLength;
    public float Radius;
    public int Damage;

    private float Timer;

    private bool hasExploded = false;

    private void Start()
    {
        Timer = FuseLength;
    }

    // Update is called once per frame
    void Update()
    {
        Timer -= Time.deltaTime;

        if (Timer <=0f && !hasExploded)
        {
            Explode();

            hasExploded = true;
        }
    }

    void Explode()
    {
        //instantiate visuals here

        Collider[] colliders = Physics.OverlapSphere(transform.position, Radius);

        foreach (Collider nearbyObject in colliders)
        {

            if (nearbyObject.gameObject.tag == "Enemy" || nearbyObject.tag == "EnemyLow" || nearbyObject.tag == "EnemyAverage" || nearbyObject.tag == "EnemyHeavy")
            nearbyObject.GetComponentInParent<Enemy>().TakeDamage(Damage, GameObject.FindGameObjectWithTag("Player"), Color.green);
        }

        Destroy(gameObject);


    }
}
