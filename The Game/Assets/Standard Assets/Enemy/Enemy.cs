using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using JSAM;

public class Enemy : MonoBehaviour
{
    private bool canDie = true;

    [Header("General Enemy data")]
    public float attackRange = 5f;
    private bool canAttack = true;

    public float minSeek = 5f;
    public float maxSeek = 10f;

    private float timeTillNavUpdate = 0f;

    [Header("Backend Data")]
    public GameObject floatingText;
    public Animator enemyAnims;

    [Header("Points Distribution Data")]
    public int pointsPerHit;
    public int pointsPerKill;

    [Header("Other Cool Stuff")]
    public GameObject enemyHead;

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
        setRidigState(true);
        setColidState(true);
    }

    private void Update()
    {
        if(timeTillNavUpdate <= 0)
            UpdateNavMesh(); //Will auto optimize the nav mesh agent and hunt the closest player
        timeTillNavUpdate -= Time.deltaTime;
    }

    private void UpdateNavMesh() {
        PlayerGameData playerGameDataTemp = null;
        float dis = Mathf.Infinity;
        foreach (GameObject p in players) 
        {
            float t = Vector3.Distance(transform.position, p.transform.position);
            if (t < dis) {
                dis = t;
                nav.SetDestination(p.transform.position);
                enemyHead.transform.LookAt(p.transform.position);
                playerGameDataTemp = p.gameObject.GetComponent<PlayerGameData>();
            }
        }
        if (dis < attackRange && canAttack) StartCoroutine(AttackPlayer(playerGameDataTemp));
        else if (dis > maxSeek) timeTillNavUpdate = 2.5f;
        else if (dis > minSeek) timeTillNavUpdate = 1f;
        else timeTillNavUpdate = 0.25f;
    }

    public void TakeDamage(int damage, GameObject p, Color c) {
        if (canDie)
        {
            GameObject tempTxt = Instantiate(floatingText, transform.position, Quaternion.identity);
            tempTxt.GetComponent<TextMesh>().text = damage.ToString();
            tempTxt.GetComponent<TextMesh>().color = c;
            tempTxt.transform.LookAt(p.transform.position);
            Health -= damage;
            Debug.Log(damage);
            p.GetComponent<PlayerGameData>().currentPoints += pointsPerHit;
            JSAM.AudioManager.PlaySound(Sounds.HITMARKER);
            if (Health <= 0) Die(p);
        }
    }

    private void Die(GameObject p) {
        canDie = false;
        p.GetComponent<PlayerGameData>().currentPoints += pointsPerKill;
        gm.enemyDeath();

        nav.enabled = false;
        setRidigState(false);

        JSAM.AudioManager.PlaySound(Sounds.ENEMYDIE);

        enemyAnims.enabled = false;

        Destroy(gameObject, 5f);

        this.enabled = false;
    }

    void setRidigState(bool state) {
        Rigidbody[] rigibodies = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rig in rigibodies) {
            rig.isKinematic = state;
        }
    }
    void setColidState(bool state)
    {
        Collider[] col = GetComponentsInChildren<Collider>();

        foreach (Collider c in col)
        {
            c.enabled = state;
        }
    }

    private void OnCollisionEnter(Collision collision) //does damage to player
    {

        PlayerGameData pgd;
        if (collision.gameObject.TryGetComponent<PlayerGameData>(out pgd))
        {
            Debug.Log(pgd.health);
            pgd.TakeDamage(2);
        }
    }
    IEnumerator AttackPlayer(PlayerGameData _pgd) {
        canAttack = false;
        _pgd.TakeDamage(Random.Range(1,3));
        float holdSpeed = nav.speed;
        nav.speed = 0;
        yield return new WaitForSeconds(1.5f);
        nav.speed = holdSpeed;
        canAttack = true;
    }
    
}
