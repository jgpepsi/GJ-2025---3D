using System.Collections;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int health;
    public float speed;
    private PlayerScript player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.x > player.transform.position.x)
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
        }
        else if(transform.position.x < player.transform.position.x)
        {
            transform.position += Vector3.right * speed * Time.deltaTime;
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Inimigo recebeu dano");
        health -= damage;
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator Paralyze(float duration)
    {
        float originalSpeed = speed;
        speed = 0;
        yield return new WaitForSeconds(duration);
        speed = originalSpeed;
    }
}
