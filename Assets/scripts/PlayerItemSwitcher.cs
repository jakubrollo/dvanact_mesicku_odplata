using UnityEngine;
using Photon.Pun;
using StarterAssets;

public class PlayerItemSwitcher : MonoBehaviourPun
{
    [Header("Items")]
    [Tooltip("Pøetáhni sem celý objekt Zbranì (i s GunControllerem)")]
    [SerializeField] private GameObject gunObject;

    [Tooltip("Pøetáhni sem celý objekt Svíèky")]
    [SerializeField] private GameObject candleObject;

    // Stav: true = mám zbraò, false = mám svíèku
    private bool isGunActive = true;

    private StarterAssetsInputs _input;

    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();

        // Na zaèátku nastavíme správný stav (napø. zbraò aktivní)
        // Voláme to lokálnì, synchronizace probìhne pøi zmìnì nebo pøipojení
        if (gunObject != null) gunObject.SetActive(true);
        if (candleObject != null) candleObject.SetActive(false);
    }

    void Update()
    {
        // Pøepínat mùže jen majitel
        if (!photonView.IsMine || _input == null) return;

        // Pokud hráè zmáèkl Q (switchItem je true)
        if (_input.switchItem)
        {
            Debug.Log("[Input] SwitchItem triggered! Current state: " + (isGunActive ? "Gun" : "Candle"));
            // Okamžitì resetujeme input, aby to nepøepínalo tam a zpátky 60x za vteøinu
            _input.switchItem = false;

            // Prohodíme stav
            isGunActive = !isGunActive;

            // Pošleme informaci všem (vèetnì sebe) pøes RPC
            photonView.RPC("RPC_SwitchItem", RpcTarget.AllBuffered, isGunActive);
        }
    }

    [PunRPC]
    public void RPC_SwitchItem(bool gunState)
    {
        // 1. Nastavíme Zbraò
        if (gunObject != null)
        {
            gunObject.SetActive(gunState);
        }

        // 2. Nastavíme Svíèku (vždy opak zbranì)
        if (candleObject != null)
        {
            candleObject.SetActive(!gunState);
        }
    }
}