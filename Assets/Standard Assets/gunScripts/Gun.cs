using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JSAM;
using UnityEngine.VFX;

public class Gun : MonoBehaviour
{
    [Header("General Data")]
    public Camera playerCamera;
    public GameObject player;
    public GameObject headJoint;

    [Header("Gun Shooting Data")]
    public bool isAutomatic = true;

    public bool isBurst = false;
    public float burstRate = 0.05f;
    private bool canBurst = true;

    public bool isShotgun = false;

    [Header("Gun Damage Data")]
    public int damagePerBullet = 20;

    public float heavyDamageMulti = 2.5f;
    public float midDamageMulti = 1.0f;
    public float lowDamageMulti = 0.75f;

    [Header("Gun Magazine Data")]
    public int magazineSize = 8;
    [HideInInspector]
    public int roundsRemaining;
    private bool isReloading;

    [Header("Gun Fire Rate Data")]
    public float fireRate = 0.25f;
    private float fireTimer;

    [Header("Gun Pierce Data")]
    public int pierceHealth = 3;

    [Header("Effect Data")]
    public GameObject MuzzleFlash;
    public GameObject sparksParticle;
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



    void Start()
    {
        gDecs = GetComponent<GunDecals>();
        gDecsColor = gDecs.secondaryColor;
        roundsRemaining = magazineSize;
    }
    private void Update()
    {
        //Automatic Inputs
        if (Input.GetButton("Fire1") && isAutomatic)
        {
            if (roundsRemaining > 0 && isBurst)
            {
                StartCoroutine(BurstFire());
            }
            else if (roundsRemaining > 0 && !isBurst) {
                Fire();
            }
            else
            {
                DoReload();
            }
        }
        //Semi Auto Inputs
        else if (Input.GetButtonDown("Fire1")) {
            if (roundsRemaining > 0 && isBurst)
            {
                StartCoroutine(BurstFire());
            }
            else if (roundsRemaining > 0 && !isBurst)
            {
                Fire();
            }
            else
            {
                DoReload();
            }
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
        //AimDownSights();
    }

    private void FixedUpdate()
    {
        AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);

        isReloading = info.IsName("Reload");
    }

    private void AimDownSights()
    {
        if (Input.GetButton("Fire2") && !isReloading && playerCamera.fieldOfView > 60)
        {
            playerCamera.fieldOfView -= 6f;
        }
        if (!Input.GetButton("Fire2") && playerCamera.fieldOfView < 90 || isReloading && playerCamera.fieldOfView < 90)
        {
            playerCamera.fieldOfView += 6f;
        }
    }

    IEnumerator BurstFire() {
        if (canBurst && fireTimer >= fireRate)
        {
            canBurst = false;
            for (int i = 0; i < 3; i++)
            {
                Fire();
                yield return new WaitForSeconds(burstRate); // wait till the next round
            }
            fireTimer = 0.0f;
            canBurst = true;
        }
    }

    public void Fire()
    {
        //Kickback if the gun is unable to shoot
        if (fireTimer < fireRate || roundsRemaining <= 0 || isReloading) return;
        else
        {
            //spawn muzzle flash
            GameObject flashObj = Instantiate(MuzzleFlash, BulletExit.position, Quaternion.identity);
            flashObj.transform.parent = BulletExit;
            flashObj.transform.localRotation = Quaternion.Euler(0, -90, 0);
            Destroy(flashObj, 0.15f);

            if(!isBurst) fireTimer = 0.0f;
        }

        //RecoilUp -= recoilStrengthVertical;
        //RecoilSide = Random.Range(-recoilStrengthHorizontal, recoilStrengthHorizontal) / 5;


        //Pierce Bullet Code
        
        RaycastHit[] hit = Physics.RaycastAll(playerCamera.transform.position, playerCamera.transform.forward, LayerMask.GetMask("Enemy"));

        GameObject bulletTrail = GameObject.Instantiate(BulletTrailPrefab, transform.position, Quaternion.identity);
        bulletTrail.GetComponent<Rigidbody>().AddForce(playerCamera.transform.forward * TrailSpeed);

        int tPierce = pierceHealth;
        foreach (RaycastHit r in hit)
        {
            if (r.transform.gameObject.tag == "EnemyHeavy" && tPierce > 0)
            {
                GameObject newParticle = Instantiate(sparksParticle, r.point, r.transform.rotation);
                GameObject.Destroy(newParticle, 0.3f);

                float damage = damagePerBullet * Random.Range(heavyDamageMulti - 0.1f, heavyDamageMulti + 0.1f);
                r.transform.gameObject.GetComponentInParent<Enemy>().TakeDamage((int)damage, player, gDecsColor);
                tPierce--;
            }
            else if (r.transform.gameObject.tag == "EnemyAverage" || r.transform.gameObject.tag == "Enemy" && tPierce > 0)
            {
                GameObject newParticle = Instantiate(sparksParticle, r.point, r.transform.rotation);
                GameObject.Destroy(newParticle, 0.3f);

                float damage = damagePerBullet * Random.Range(midDamageMulti - 0.1f, midDamageMulti + 0.1f);
                r.transform.gameObject.GetComponentInParent<Enemy>().TakeDamage((int)damage, player, gDecsColor);
                tPierce--;
            }
            else if (r.transform.gameObject.tag == "EnemyLow" && tPierce > 0)
            {
                GameObject newParticle = Instantiate(sparksParticle, r.point, r.transform.rotation);
                GameObject.Destroy(newParticle, 1f);

                float damage = damagePerBullet * Random.Range(lowDamageMulti - 0.1f, lowDamageMulti + 0.1f);
                r.transform.gameObject.GetComponentInParent<Enemy>().TakeDamage((int)damage, player, gDecsColor);
                tPierce--;
            }

            //Provide a force to the rigidbody of the enemies
            Collider[] colliders = Physics.OverlapSphere(r.transform.position, 1f);

            foreach (Collider closeObj in colliders) {
                Rigidbody rb = closeObj.GetComponent<Rigidbody>();

                if (rb != null) {
                    rb.AddExplosionForce(100f, r.transform.position, 1f);
                }
            }

        }
        

        //Gun Logic Triggers
        roundsRemaining--;

        //Animation Triggers
        anim.CrossFadeInFixedTime("Fire", 0.1f);

     


    }

    public void DoReload()
    {
        fireTimer = 0.0f;

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


    /*
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
    */

}
