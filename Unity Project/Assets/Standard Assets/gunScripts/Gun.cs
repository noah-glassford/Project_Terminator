using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public int roundsRemaining;
    private bool isReloading;

    [Header("Gun Fire Rate Data")]
    public float fireRate = 0.25f;
    private float fireTimer;

    [Header("Gun ADS Data")]
    public Vector3 aimPosition;
    public float adsSpeed;
    private Vector3 originalPosition;

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

    void Start()
    {
        originalPosition = transform.localPosition;
        gDecs = GetComponent<GunDecals>();
        gDecsColor = gDecs.secondaryColor;
    }
    private void Update()
    {


        //Shooting Inputs
        if (Input.GetButton("Fire1"))
        {
            if (roundsRemaining > 0)
                Fire();
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

        if (isReseting) //This block handles the recoil reset, needs to be in update since it uses LERP
        {
            ResetLerpT += Time.deltaTime * 7;

            headJoint.transform.localRotation = Quaternion.Lerp(RotXForReset, Quaternion.identity, ResetLerpT);

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
        RecoilSide = Random.Range(-recoilStrengthHorizontal, recoilStrengthHorizontal);


        ApplyRecoil();

        //Casts a ray outwards for the player camera
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, Mathf.Infinity))
        {

            //If an enemy is hit by the raycast
            if (hit.transform.gameObject.tag == "Enemy")
            {
                float damage = damagePerBullet * Random.Range(0.9f, 1.1f);
                hit.transform.gameObject.GetComponent<Enemy>().TakeDamage((int)damage, player, gDecsColor);
            }

            Debug.Log(hit.transform.name);
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

    private void ApplyRecoil()
    {

        Vector3 RotationInEuler = headJoint.transform.localRotation.eulerAngles;
        RotationInEuler.x += RecoilUp;
        RotationInEuler.y += RecoilSide;

        if (RotationInEuler.x < 90 || RotationInEuler.x > -90)
        {
            headJoint.transform.localRotation = Quaternion.Euler(RotationInEuler);

        }

    }

    private void ResetRecoilSmooth()
    {

        RecoilUp = 0;
        RecoilSide = 0;
        isReseting = true;

        RotXForReset = headJoint.transform.localRotation;


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
    }
}
