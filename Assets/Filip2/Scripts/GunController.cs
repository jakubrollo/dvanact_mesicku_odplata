using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviour
{
    [SerializeField] private int damage = 25;
    [SerializeField] private float range = 100f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask hitMask;

    [SerializeField] private InputActionReference shootAction;

    [SerializeField] private AudioClip shootClip;
    private AudioSource shootSource;

    [Header("Recoil")]
    [SerializeField] private Transform gunVisual;   
    [SerializeField] private float recoilBackAmount = 0.1f;
    [SerializeField] private float recoilUpAmount = 5f;
    [SerializeField] private float recoilSpeed = 10f;
    [SerializeField] private float returnSpeed = 5f;

    [Header("Fire Settings")]
    [SerializeField] private float fireRate = 4f; // kolik støel za sekundu

    private float nextFireTime = 0f;

    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;

    private Vector3 targetPos;
    private Quaternion targetRot;

   // private bool canShoot = true;

    [SerializeField] private Light muzzleLight;
    [SerializeField] private float flashDuration = 0.05f;



    private void Start()
    {
        shootSource = GetComponent<AudioSource>();

        originalLocalPos = gunVisual.localPosition;
        originalLocalRot = gunVisual.localRotation;

        targetPos = originalLocalPos;
        targetRot = originalLocalRot;
    }

    private void OnEnable()
    {
        shootAction.action.Enable();
        shootAction.action.performed += OnFire;
    }

    private void OnDisable()
    {
        shootAction.action.performed -= OnFire;
        shootAction.action.Disable();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Fire();
        }
    }

    public void Fire()
    {
        if (Time.time < nextFireTime)
            return;

        nextFireTime = Time.time + (1f / fireRate);


        StartCoroutine(MuzzleFlash());


        if (shootClip != null)
            shootSource.PlayOneShot(shootClip);

        ApplyRecoil();

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
    }

    private void ApplyRecoil()
    {
        gunVisual.localPosition = originalLocalPos;
        gunVisual.localRotation = originalLocalRot;

        targetPos -= new Vector3(0, 0, recoilBackAmount);
        targetRot *= Quaternion.Euler(-recoilUpAmount, 0, 0);
    }

    private void Update()
    {
        gunVisual.localPosition = Vector3.Lerp(
            gunVisual.localPosition,
            targetPos,
            Time.deltaTime * recoilSpeed);

        gunVisual.localRotation = Quaternion.Lerp(
            gunVisual.localRotation,
            targetRot,
            Time.deltaTime * recoilSpeed);

        targetPos = Vector3.Lerp(targetPos, originalLocalPos, Time.deltaTime * returnSpeed);
        targetRot = Quaternion.Lerp(targetRot, originalLocalRot, Time.deltaTime * returnSpeed);
    }

    private IEnumerator MuzzleFlash()
    {
        muzzleLight.enabled = true;
        yield return new WaitForSeconds(flashDuration);
        muzzleLight.enabled = false;
    }

}
