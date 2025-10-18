using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    public int pierceCount;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyScript>().TakeDamage(1);
            pierceCount--;
            if(pierceCount < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
