using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;

public class Gun : MonoBehaviour
{
    [Header("General Data")]
    public Camera playerCamera;
    public GameObject player;
    public GameObject headJoint;

    [Header("Gun Damage Data")]
    public int damagePerBullet = 20;

    [Header("Gun Magazine Data")]
    public int magazineSize = 8;
    [HideInInspector]
    public int roundsRemaining;
    private bool isReloading;

    [Header("Gun Fire Rate Data")]
    public float fireRate = 0.25f;
    private float fireTimer;

    [Header("Gun ADS Data")]
    public Vector3 aimPosition;
    public float adsSpeed;
    private Vector3 originalPosition;

    [Header("Gun Pierce Data")]
    public int pierceHealth = 3;

    [Header("Effect Data")]
    public ParticleSystem[] onShootEffects;
    private GunDecals gDecs;
    public Color gDecsColor;

    [Header("Animation Data")]
    public Animator anim;

    [Header("Recoil Data")]
    public float recoilStrengthVertical;
    public float recoilStrengthHorizontal;
    private float RecoilUp;
    private float RecoilSide;
    private int NumberOfShotsFullAuto; //counts how many shots go off

    private float ResetLerpT = 0;
    private bool isReseting = false;
    private Quaternion RotXForReset = new Quaternion(1, 1, 1, 1);

    //For Gun Visuals
    [SerializeField]
    private GameObject BulletTrailPrefab;
    [SerializeField]
    private Transform BulletExit;
    public float TrailSpeed;


    //for grenade
    [SerializeField]
    private GameObject grenadePrefab;
    public float ThrowPower;


    void Start()
    {
        originalPosition = transform.localPosition;
        gDecs = GetComponent<GunDecals>();
        gDecsColor = gDecs.secondaryColor;
        roundsRemaining = magazineSize;
    }
    private void Update()
    {
        //Shooting Inputs
        if (Input.GetButton("Fire1"))
        {
            if (roundsRemaining > 0)
            {
                
                ApplyRecoil();
                Fire();
            }
            else
            {
                ResetRecoilSmooth();  //sets flag to reset recoil
                DoReload();
            }
        }
        else if (Input.GetButtonUp("Fire1"))
        {
            ResetRecoilSmooth(); //sets flag to reset recoil
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            GameObject nade = Instantiate(grenadePrefab, BulletExit.position, Quaternion.identity);
            Rigidbody rb = nade.GetComponent<Rigidbody>();
            rb.AddForceAtPosition(Camera.main.transform.forward * ThrowPower, Vector3.zero, ForceMode.Impulse);

        }


        if (isReseting) //This block handles the recoil reset, needs to be in update since it uses LERP
        {
            ResetLerpT += Time.deltaTime * 7;

            headJoint.transform.localRotation = Quaternion.Lerp(RotXForReset, Quaternion.Euler(-1,0,0), ResetLerpT);

            if (ResetLerpT >= 1)
            {
                isReseting = false;
                ResetLerpT = 0f;
            }

        }

        //Reloading Inputs
        if (Input.GetKeyDown(KeyCode.R)) DoReload();

        if (fireTimer < fireRate) fireTimer += Time.deltaTime;

        //Aiming Sinputs
        AimDownSights();
    }

    private void FixedUpdate()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        isReloading = info.IsName("Reload");
    }

    private void AimDownSights()
    {
        if (Input.GetButton("Fire2") && !isReloading)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, aimPosition, Time.deltaTime * adsSpeed);
            playerCamera.fieldOfView = 60;
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originalPosition, Time.deltaTime * adsSpeed);
            playerCamera.fieldOfView = 90f;
        }
    }

    private void Fire()
    {
        //Kickback if the gun is unable to shoot
        if (fireTimer < fireRate || roundsRemaining <= 0 || isReloading) return;
        else fireTimer = 0.0f;


        RecoilUp -= recoilStrengthVertical;
        RecoilSide = Random.Range(-recoilStrengthHorizontal, recoilStrengthHorizontal) / 5;

 

        //Casts a ray outwards for the player camera

        //Single Hit Code
        
        /*
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, Mathf.Infinity)) {

            //If an enemy is hit by the raycast
            if (hit.transform.gameObject.tag == "Enemy") {
                float damage = damagePerBullet * Random.Range(0.9f, 1.1f);
                hit.transform.gameObject.GetComponent<Enemy>().TakeDamage((int)damage, player, gDecsColor);
            }
        }
        */


        //Pierce Bullet Code
        
        RaycastHit[] hit = Physics.RaycastAll(playerCamera.transform.position, playerCamera.transform.forward, LayerMask.GetMask("Enemy"));

        GameObject bulletTrail = GameObject.Instantiate(BulletTrailPrefab, transform.position, Quaternion.identity);
        bulletTrail.GetComponent<Rigidbody>().AddForce(playerCamera.transform.forward * TrailSpeed);

        int tPierce = pierceHealth;
        foreach (RaycastHit r in hit)
        {

            if (r.transform.gameObject.tag == "EnemyHeavy" && tPierce > 0)
            {
                float damage = damagePerBullet * Random.Range(1.4f, 1.6f);
                r.transform.gameObject.GetComponentInParent<Enemy>().TakeDamage((int)damage, player, gDecsColor);
                tPierce--;
                /*
                ParticleSystem temp = Instantiate(gDecs.OnHitEffect, r.transform.position, Quaternion.identity);
                Destroy(temp, 4f);
                */
            }
            if (r.transform.gameObject.tag == "EnemyAverage" && tPierce > 0)
            {
                float damage = damagePerBullet * Random.Range(0.9f, 1.1f);
                r.transform.gameObject.GetComponentInParent<Enemy>().TakeDamage((int)damage, player, gDecsColor);
                tPierce--;
                /*
                ParticleSystem temp = Instantiate(gDecs.OnHitEffect, r.transform.position, Quaternion.identity);
                Destroy(temp, 4f);
                */
            }
            if (r.transform.gameObject.tag == "EnemyLow" && tPierce > 0)
            {
                float damage = damagePerBullet * Random.Range(0.4f, 0.6f);
                r.transform.gameObject.GetComponentInParent<Enemy>().TakeDamage((int)damage, player, gDecsColor);
                tPierce--;
                /*
                ParticleSystem temp = Instantiate(gDecs.OnHitEffect, r.transform.position, Quaternion.identity);
                Destroy(temp, 4f);
                */
            }

            //Provide a force tot he rigidbody of the enemies
            Collider[] colliders = Physics.OverlapSphere(r.transform.position, 1f);

            foreach (Collider closeObj in colliders) {
                Rigidbody rb = closeObj.GetComponent<Rigidbody>();

                if (rb != null) {
                    rb.AddExplosionForce(5000f, r.transform.position, 1f);
                }
            }

        }
        
        //Gun Logic Triggers
        roundsRemaining--;

        //Animation Triggers
        anim.CrossFadeInFixedTime("Fire", 0.1f);

        //Effect Triggers
        foreach (ParticleSystem p in onShootEffects)
        {
            p.Play();
        }

    }

    private void DoReload()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        if (isReloading) return;
        if (roundsRemaining == magazineSize) return;
        anim.CrossFadeInFixedTime("Reload", 0.01f);

        Reload();
    }

    private void Reload()
    {
        roundsRemaining = magazineSize;
        JSAM.AudioManager.PlaySound(Sounds.RELOAD);
    }



    private void ResetRecoilSmooth()
    {

        RecoilUp = 0;
        RecoilSide = 0;
        isReseting = true;

        RotXForReset = headJoint.transform.localRotation;


    }

    private void ApplyRecoil()
    {

        Vector3 RotationInEuler = headJoint.transform.localRotation.eulerAngles;
        RotationInEuler.x += RecoilUp;
        RotationInEuler.y += RecoilSide;

        if (RotationInEuler.x < 335 )
        {
            RotationInEuler.x = 335;

        }

        headJoint.transform.localRotation = Quaternion.Euler(RotationInEuler);
      

    }

}
