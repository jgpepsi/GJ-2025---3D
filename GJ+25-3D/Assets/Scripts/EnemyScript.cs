using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class EnemyScript : MonoBehaviour
{
    public int rarity;
    public int health;
    public float speed;
    public float displaceDistance;
    public DodgeType dodgeType;
    public float minDist;
    public float displaceDuration;
    public float autoDodgeDist;
    public Collider col;
    public SpawnManager spawnManager;
    private bool hasDodged = false;
    private bool isDisplacing = false;
    private PlayerScript player;
    private Rigidbody rb;

    public enum DodgeType
    {
        noDodge,
        dodge,
        autododge
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        rb = GetComponent<Rigidbody>(); 
    }

    void Update()
    {
        if (Vector3.Distance(player.transform.position, transform.position) >= minDist && !isDisplacing)
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

        if (Vector3.Distance(player.transform.position, transform.position) <= autoDodgeDist && dodgeType == DodgeType.autododge && !hasDodged && !isDisplacing)
        {
            StartCoroutine(Dodge());
        }

        
    }

    public void TakeDamage(int damage)
    {
        if (dodgeType == DodgeType.dodge && !hasDodged)
        {
            StartCoroutine(Dodge());
        }
        else
        {
            health -= damage;
            if (health <= 0)
            {
                spawnManager.AddWaveProgress(rarity);
                Destroy(gameObject);
            }
            else
            {
                StartCoroutine(ApplyKnockback());
                
            }
        }
    }
    
    

    private IEnumerator ApplyKnockback()
    {
        isDisplacing = true;

        Vector3 start = transform.position;
        Vector3 end;

        if (transform.position.x > player.transform.position.x)
        {
            end = player.transform.position + Vector3.right * displaceDistance;
        }
        else
        {
            end = player.transform.position + Vector3.left * displaceDistance;
        }

        float elapsed = 0f;
        while (elapsed < displaceDuration)
        {
            float t = elapsed / displaceDuration;
            Vector3 next = Vector3.Lerp(start, end, t);
            transform.position = next;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;

        isDisplacing = false;
        StartCoroutine(Paralyze(0.5f));
    }

    private IEnumerator Dodge() 
    {
        isDisplacing = true;
        col.enabled = false;

        Vector3 start = transform.position;
        Vector3 end;

        if (transform.position.x > player.transform.position.x)
        {
            end = player.transform.position + Vector3.left * displaceDistance;
        }
        else
        {
            end = player.transform.position + Vector3.right * displaceDistance;
        }

        float elapsed = 0f;
        while (elapsed < displaceDuration)
        {
            float t = elapsed / displaceDuration;
            Vector3 next = Vector3.Lerp(start, end, t);
            transform.position = next;
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end;

        isDisplacing = false;
        col.enabled = true;

        hasDodged = true;
        //transform.position == player.transform.;
    }

    public IEnumerator Paralyze(float duration)
    {
        float originalSpeed = speed;
        speed = 0;
        yield return new WaitForSeconds(duration);
        speed = originalSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DEATH"))
        {
            Destroy(gameObject);
        }
    }
}
