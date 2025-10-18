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
    public bool extraRangeActive;
    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();
        isDashing = false;
        extraRangeActive = false;
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
        else if (Input.GetKeyDown(KeyCode.UpArrow) && attackTimer >= attackCooldown)
        {
            Howl(10f, 2f);
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
            StartCoroutine(DashAndMiss(atkPoint, true));
        }

    }


    public void DoubleAttack(Transform atkPoint)
    {
        StartCoroutine(DoubleDashSequence(atkPoint));
    }

    private IEnumerator DoubleDashSequence(Transform atkPoint)
    {
        for (int i = 0; i < 2; i++)
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
                        closestHit = hit.transform;
                        actualTarget = hit;
                    }
                }
                yield return StartCoroutine(DashAndHit(atkPoint, actualTarget));
            }
            else
            {
                yield return StartCoroutine(DashAndMiss(atkPoint, false));
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    public void NoDebuffAttack(Transform atkPoint)
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
            StartCoroutine(DashAndMiss(atkPoint, false));
        }
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

    private IEnumerator DashAndMiss(Transform atkPoint, bool canDebuff)
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

        if (canDebuff)
        {
            attackTimer = 0f;

        }

        isDashing = false;
    }

    private IEnumerator DoubleDashAndHit(Transform atkPoint, Collider[] hits)
    {
        isDashing = true;

        Transform closestHit = hits[0].transform;
        foreach (Collider hit in hits)
        {

            if (Vector3.Distance(hit.transform.position, transform.position) < Vector3.Distance(closestHit.position, transform.position))
            {
                closestHit = hit.transform;
            }
            
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
        yield return new WaitForSeconds(0.1f);
        NoDebuffAttack(atkPoint);
    }

    public IEnumerator DoubleDashAndMiss(Transform atkPoint)
    {
        StartCoroutine(DashAndMiss(atkPoint, true));
        yield return new WaitForSeconds(dashDuration + 0.1f);
        StartCoroutine(DashAndMiss(atkPoint, true));
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

    public void LongAttack(Transform atkPoint, float extraRange)
    {
        StartCoroutine(LongAttackDash(atkPoint, extraRange));
    }

    private IEnumerator LongAttackDash(Transform atkPoint, float extraRange)
    {
        float originalRange = atkRange;
        atkRange += extraRange;

        Collider[] hits = Physics.OverlapSphere(atkPoint.position, atkRange, enemyLayer);

        if (hits.Length > 0)
        {
            Transform closestHit = hits[0].transform;
            Collider actualTarget = hits[0];
            foreach (Collider hit in hits)
            {
                if (Vector3.Distance(hit.transform.position, transform.position) < Vector3.Distance(closestHit.position, transform.position))
                {
                    closestHit = hit.transform;
                    actualTarget = hit;
                }
            }
            yield return StartCoroutine(DashAndHit(atkPoint, actualTarget));
        }
        else
        {
            yield return StartCoroutine(DashAndMiss(atkPoint, true));
        }

        atkRange = originalRange;
    }

    public void Howl(float range, float duration)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyLayer);

        foreach (Collider hit in hits)
        {
            bool isRight = !spr.flipX;
            bool enemyIsRight = hit.transform.position.x > transform.position.x;

            if ((isRight && enemyIsRight) || (!isRight && !enemyIsRight))
            {
                EnemyScript enemy = hit.GetComponent<EnemyScript>();
                if (enemy != null)
                {
                    enemy.StartCoroutine(enemy.Paralyze(duration));
                }
            }
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
