using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponType
{
    Sword,
    Gun,
    Candle,
    None
}

public class PlayerWeapons : MonoBehaviour
{
    public WeaponType currentWeapon = WeaponType.None;

    [SerializeField] private GameObject candle;
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject sword;


    void Update()
    {
        
    }
    [ContextMenu("Switch to Gun")]
    public void SwitchToGun()
    {
        SwitchWeapon(WeaponType.Gun);
    }

    public void SwitchWeapon(WeaponType weapon)
    {
        if (currentWeapon == WeaponType.Candle) HideWeapon(candle);
        else if (currentWeapon == WeaponType.Sword) HideWeapon(sword);
        else if (currentWeapon == WeaponType.Gun) HideWeapon(gun);
        currentWeapon = weapon;
        if (weapon == WeaponType.Candle) PullOutWeapon(candle);
        else if (weapon == WeaponType.Sword) PullOutWeapon(sword);
        else if (weapon == WeaponType.Gun) PullOutWeapon(gun);

    }

    public void HideWeapon(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }

    public void PullOutWeapon(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }
}