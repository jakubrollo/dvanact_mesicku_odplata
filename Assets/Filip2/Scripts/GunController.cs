using Photon.Pun;
using StarterAssets; // Dùležité: Pøidáno pro pøístup k StarterAssetsInputs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GunController : MonoBehaviourPun
{
    [SerializeField] private int damage = 25;
    [SerializeField] private float range = 100f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask hitMask;

    // ODSTRANÌNO: InputActionReference už nepotøebujeme, používáme StarterAssetsInputs
    // [SerializeField] private InputActionReference shootAction;

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

    [SerializeField] private Light muzzleLight;
    [SerializeField] private float flashDuration = 0.05f;

    // NOVÉ: Odkaz na vstupy hráèe
    private StarterAssetsInputs _input;

    private void Start()
    {
        shootSource = GetComponent<AudioSource>();

        originalLocalPos = gunVisual.localPosition;
        originalLocalRot = gunVisual.localRotation;

        targetPos = originalLocalPos;
        targetRot = originalLocalRot;

        // Pokud to není moje zbraò, vypnu muzzle light na zaèátku pro jistotu
        if (muzzleLight != null) muzzleLight.enabled = false;

        // NOVÉ: Najdeme komponentu StarterAssetsInputs na rodièi (PlayerCapsule)
        _input = GetComponentInParent<StarterAssetsInputs>();

        if (_input == null && photonView.IsMine)
        {
            Debug.LogError("GunController: Nenalezen StarterAssetsInputs! Ujisti se, že zbraò je dítìtem objektu s tímto skriptem.");
        }
    }

    // ODSTRANÌNO: OnEnable, OnDisable a OnFire (callback) už nejsou potøeba.
    // Input øeší StarterAssetsInputs automaticky.

    private void Update()
    {
        // 1. Recoil animace (lokální)
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

        // 2. Logika støelby
        // Podmínka: Je to mùj hráè? && Mám input komponentu? && Držím tlaèítko støelby?
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            Debug.Log($"[Gun Debug] Fire: {_input?.fire}, IsMine: {photonView.IsMine}, InputFound: {_input != null}");
        }

        if (photonView.IsMine && _input != null && _input.fire)
        {
            TryShoot();
            _input.fire = false;
        }
    }
    public void TryShoot()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + (1f / fireRate);

        // A. Efekty
        photonView.RPC("RPC_ShootEffects", RpcTarget.All);

        // B. Raycast
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            // Debug.Log("Trefil jsem: " + hit.collider.name);

            // ZMÌNA: Hledáme IDamageable na objektu NEBO NA JEHO RODIÈÍCH
            // To vyøeší problém, když trefíš collider ruky, ale životy jsou na hlavním objektu.
            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();

            if (target != null)
            {
                // Zavoláme metodu rozhraní. 
                // EnemyHealthManager (nebo PlayerHealth) uvnitø této metody sám zavolá své RPC.
                // Tím oddìlíme logiku zbranì od logiky sítì cíle.
                target.TakeDamage(damage);
            }
        }
    }

    [PunRPC]
    public void RPC_ShootEffects()
    {
        // Spustit Muzzle Flash
        StartCoroutine(MuzzleFlash());

        // Pøehrát zvuk
        if (shootClip != null && shootSource != null)
            shootSource.PlayOneShot(shootClip);

        // Aplikovat recoil
        ApplyRecoil();
    }

    private void ApplyRecoil()
    {
        targetPos -= new Vector3(0, 0, recoilBackAmount);
        targetRot *= Quaternion.Euler(-recoilUpAmount, 0, 0);
    }

    private IEnumerator MuzzleFlash()
    {
        if (muzzleLight != null)
        {
            muzzleLight.enabled = true;
            yield return new WaitForSeconds(flashDuration);
            muzzleLight.enabled = false;
        }
    }
}