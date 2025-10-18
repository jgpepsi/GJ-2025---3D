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
}
