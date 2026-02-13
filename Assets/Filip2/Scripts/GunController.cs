using Photon.Pun;
using StarterAssets; // D˘leûitÈ: P¯id·no pro p¯Ìstup k StarterAssetsInputs
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

    // ODSTRANÃNO: InputActionReference uû nepot¯ebujeme, pouûÌv·me StarterAssetsInputs
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
    [SerializeField] private float fireRate = 4f; // kolik st¯el za sekundu

    private float nextFireTime = 0f;

    private Vector3 originalLocalPos;
    private Quaternion originalLocalRot;

    private Vector3 targetPos;
    private Quaternion targetRot;

    [SerializeField] private Light muzzleLight;
    [SerializeField] private float flashDuration = 0.05f;

    // NOV…: Odkaz na vstupy hr·Ëe
    private StarterAssetsInputs _input;

    private void Start()
    {
        shootSource = GetComponent<AudioSource>();

        originalLocalPos = gunVisual.localPosition;
        originalLocalRot = gunVisual.localRotation;

        targetPos = originalLocalPos;
        targetRot = originalLocalRot;

        // Pokud to nenÌ moje zbraÚ, vypnu muzzle light na zaË·tku pro jistotu
        if (muzzleLight != null) muzzleLight.enabled = false;

        // NOV…: Najdeme komponentu StarterAssetsInputs na rodiËi (PlayerCapsule)
        _input = GetComponentInParent<StarterAssetsInputs>();

        if (_input == null && photonView.IsMine)
        {
            Debug.LogError("GunController: Nenalezen StarterAssetsInputs! Ujisti se, ûe zbraÚ je dÌtÏtem objektu s tÌmto skriptem.");
        }
    }

    // ODSTRANÃNO: OnEnable, OnDisable a OnFire (callback) uû nejsou pot¯eba.
    // Input ¯eöÌ StarterAssetsInputs automaticky.

    private void Update()
    {
        // 1. Recoil animace (lok·lnÌ)
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

        // 2. Logika st¯elby
        // PodmÌnka: Je to m˘j hr·Ë? && M·m input komponentu? && DrûÌm tlaËÌtko st¯elby?
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
        // Kontrola kadence st¯elby
        if (Time.time < nextFireTime)
            return;

        nextFireTime = Time.time + (1f / fireRate);

        // A. Zavol·me vizu·lnÌ efekty (Zvuk + Flash + Recoil) pro VäECHNY hr·Ëe
        photonView.RPC("RPC_ShootEffects", RpcTarget.All);

        // B. Raycast a PoökozenÌ ¯eöÌme jen MY (Lok·lnÏ)
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            // Debug.Log("Trefil jsem: " + hit.collider.name);

            // ZkusÌme najÌt PhotonView na trefenÈm objektu
            PhotonView targetView = hit.collider.GetComponent<PhotonView>();

            // Pokud m· objekt PhotonView (je to hr·Ë), poöleme mu RPC
            if (targetView != null)
            {
                // Vol·me metodu "TakeDamage" na trefenÈm hr·Ëi
                targetView.RPC("TakeDamage", RpcTarget.All, damage);
            }
            else
            {
                // Pokud to nenÌ sÌùov˝ objekt (nap¯. terË v singlu), pouûijeme star˝ interface
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damage);
                }
            }
        }
    }

    [PunRPC]
    public void RPC_ShootEffects()
    {
        // Spustit Muzzle Flash
        StartCoroutine(MuzzleFlash());

        // P¯ehr·t zvuk
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