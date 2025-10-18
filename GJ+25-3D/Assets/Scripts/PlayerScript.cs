using System.Collections;
using System.Security.Cryptography;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerScript : MonoBehaviour
{
    public int health;
    public float attackCooldown;
    public float attackTimer;
    private SpriteRenderer spr;
    public Transform leftAtkPoint, rightAtkPoint;
    public LayerMask enemyLayer;
    public float atkRange;
    public float speed;
    private Rigidbody rb;
    public bool isDashing;
    public float dashDuration;
    public GameObject projectilePrefab, piercingProjectilePrefab;
    public float shotSpeed;
    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();
        isDashing = false;
        attackTimer = 0f;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        

        attackTimer += Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.LeftArrow) && attackTimer >= attackCooldown)
        {
            Attack(leftAtkPoint);
            FlipX(horizontal);
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow) && attackTimer >= attackCooldown)
        {
            Attack(rightAtkPoint);
            FlipX(horizontal);
        }
        
    }

    public void FlipX(float horizontal)
    {
        switch (horizontal)
        {
            case > 0:
                spr.flipX = false;
                break;
            case < 0:
                spr.flipX = true;
                break;
            default:
                break;
        }
    }

    public void Attack(Transform atkPoint)
    {
        Collider[] hits = Physics.OverlapSphere(atkPoint.position, atkRange, enemyLayer);
        

        if (hits.Length > 0)
        {
            Transform closestHit = hits[0].transform;
            Collider actualTarget = hits[0]; 
            foreach (Collider hit in hits)
            {
                if (Vector3.Distance(hit.transform.position, transform.position) < Vector3.Distance(closestHit.position, transform.position))
                {
                    actualTarget = hit;
                }
            }
            StartCoroutine(DashAndHit(atkPoint, actualTarget));
        }
        else
        {
            StartCoroutine(DashAndMiss(atkPoint));
        }

    }

    public void IncreaseRange(float number)
    {
        atkRange += number;
        leftAtkPoint.transform.position -= new Vector3(number, 0f, 0f);
        rightAtkPoint.transform.position += new Vector3(number, 0f, 0f);
    }

    public void DecreaseRange(float number)
    {
        atkRange -= number;
        leftAtkPoint.transform.position += new Vector3(number, 0f, 0f);
        rightAtkPoint.transform.position -= new Vector3(number, 0f, 0f);
    }

    public void DoubleAttack(Transform atkPoint)
    {
        Collider[] hits = Physics.OverlapSphere(atkPoint.position, atkRange, enemyLayer);
        if (hits.Length > 0)
        {
            StartCoroutine(DoubleDashAndHit(atkPoint, hits));
        }
        else
        {
            StartCoroutine(DoubleDashAndMiss(atkPoint));
        }
        
    }

    public IEnumerator NoDebuffAttack(Transform atkPoint)
    {
        isDashing = true;

        Vector3 start = transform.position;
        Vector3 dir = (atkPoint.position - transform.position).normalized;
        Vector3 end = transform.position + dir * atkRange;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            Vector3 next = Vector3.Lerp(start, end, t);
            rb.MovePosition(next);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(end);
        rb.linearVelocity = Vector3.zero;

        isDashing = false;
    }

    private IEnumerator DashAndHit(Transform atkPoint, Collider target)
    {
        isDashing = true;

        Vector3 start = transform.position;
        Vector3 end = target.transform.position;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            Vector3 next = Vector3.Lerp(start, end, t);
            rb.MovePosition(next);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(end);
        var enemy = target.GetComponent<EnemyScript>();
        if (enemy != null) enemy.TakeDamage(1);

        rb.linearVelocity = Vector3.zero;
        isDashing = false;
    }

    private IEnumerator DashAndMiss(Transform atkPoint)
    {
        isDashing = true;

        Vector3 start = transform.position;
        Vector3 dir = (atkPoint.position - transform.position).normalized;
        Vector3 end = transform.position + dir * atkRange;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            Vector3 next = Vector3.Lerp(start, end, t);
            rb.MovePosition(next);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(end);
        rb.linearVelocity = Vector3.zero;

        attackTimer = 0f;

        isDashing = false;
    }

    private IEnumerator DoubleDashAndHit(Transform atkPoint, Collider[] hits)
    {
        isDashing = true;

        int counter = 0;
        Transform closestHit = hits[0].transform;
        foreach (Collider hit in hits)
        {
            var enemy = hit.GetComponent<EnemyScript>();
            if (enemy != null) enemy.TakeDamage(1);

            if (Vector3.Distance(hit.transform.position, transform.position) < Vector3.Distance(closestHit.position, transform.position))
            {
                closestHit = hit.transform;
            }

            counter++;
            if (counter >= 2) break;
        }

        Vector3 start = transform.position;
        Vector3 end = closestHit.position;
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            Vector3 next = Vector3.Lerp(start, end, t);
            rb.MovePosition(next);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(end);
        rb.linearVelocity = Vector3.zero;
        isDashing = false;
    }

    public IEnumerator DoubleDashAndMiss(Transform atkPoint)
    {
        StartCoroutine(DashAndMiss(atkPoint));
        yield return new WaitForSeconds(dashDuration + 0.1f);
        StartCoroutine(DashAndMiss(atkPoint));
    }

    public void ShootProjectile(bool isPiercing)
    {
        GameObject bullet = isPiercing ? Instantiate(piercingProjectilePrefab, transform.position, Quaternion.identity) : Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        ProjectileScript pb = bullet.GetComponent<ProjectileScript>();
        Vector3 direction = spr.flipX ? Vector3.left : Vector3.right;

        if (rb != null)
        {
            rb.linearVelocity = direction * shotSpeed;
        }
    }
    


    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(leftAtkPoint.position, atkRange);
        Gizmos.DrawWireSphere(rightAtkPoint.position, atkRange);
    }

    





}
