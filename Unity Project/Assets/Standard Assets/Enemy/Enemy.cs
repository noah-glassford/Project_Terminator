using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("General Enemy Values")]
    public float minSeek = 5f;
    public float maxSeek = 10f;

    private float timeTillNavUpdate = 0f;

    [Header("Backend Inputs")]
    public GameObject floatingText;

    [Header("Debug Information")]
    public int Health;
    public GameObject[] players;
    public GameManager gm;
    private NavMeshAgent nav;

    private void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        nav = GetComponent<NavMeshAgent>();
        gm = GameObject.FindGameObjectWithTag("GM").GetComponent<GameManager>();
    }

    private void Update()
    {
        if(timeTillNavUpdate <= 0)
            UpdateNavMesh(); //Will auto optimize the nav mesh agent and hunt the closest player
        timeTillNavUpdate -= Time.deltaTime;
    }

    private void UpdateNavMesh() {
        float dis = Mathf.Infinity;
        foreach (GameObject p in players) 
        {
            float t = Vector3.Distance(transform.position, p.transform.position);
            if (t < dis) {
                dis = t;
                nav.SetDestination(p.transform.position);
            }
        }
        if (dis > maxSeek) timeTillNavUpdate = 2.5f;
        else if (dis > minSeek) timeTillNavUpdate = 1f;
        else timeTillNavUpdate = 0.25f;
    }

    public void TakeDamage(int damage, GameObject p, Color c) {
        GameObject tempTxt = Instantiate(floatingText, transform.position, Quaternion.identity);
        tempTxt.GetComponent<TextMesh>().text = damage.ToString();
        tempTxt.GetComponent<TextMesh>().color = c;
        tempTxt.transform.LookAt(p.transform.position);
        Health -= damage;
        Debug.Log(damage);
        if (Health <= 0) Die();
    }

    private void Die() {
        gm.enemyDeath();
        Destroy(gameObject);
    }
}
