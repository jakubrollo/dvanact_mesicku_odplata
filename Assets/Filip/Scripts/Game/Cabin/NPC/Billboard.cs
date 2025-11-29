using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private Transform player; // odkaz na hr·Ëe nebo kameru

    private void Update()
    {
        if (player == null) return;

        Vector3 direction = player.position - transform.position;

        direction.y = 0;

        transform.rotation = Quaternion.LookRotation(direction);
    }
}
