using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 1;
    public float lifeTime = 20f;  // auto-destroy if uncollected

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            /*PlayerInventory inv = other.GetComponent<PlayerInventory>();
            if (inv != null)
            {
                inv.AddCoins(value);
            }*/

            // Optional: play pickup sound/FX before destroy
            Destroy(gameObject);
        }
    }
}