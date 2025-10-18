using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public int health;
    public float speed;
    public float displaceDistance;
    public bool dodger;
    public float minDist;
    private bool hasDodged = false;
    private PlayerScript player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
    }

    void Update()
    {
        if (Vector3.Distance(player.transform.position, transform.position) >= minDist)
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
    }

    public void TakeDamage(int damage)
    {
        Debug.Log("Inimigo recebeu dano");
        if (dodger && !hasDodged)
        {
            Dodge();
        }
        else
        {
            health -= damage;
            if (health <= 0)
            {
                Destroy(gameObject);
            }
            else
            {
                ApplyKnockback();
            }
        }
    }

    public void ApplyKnockback()
    {
        if (transform.position.x > player.transform.position.x)
        {
            transform.position += Vector3.right * displaceDistance;
        }
        else
        {
            transform.position += Vector3.left * displaceDistance;
        }
    }

    public void Dodge() {;

        if (transform.position.x > player.transform.position.x)
        {
            transform.position += Vector3.left * displaceDistance;
        }
        else
        {
            transform.position += Vector3.right * displaceDistance;
        }

        //transform.position == player.transform.;
    }
}
