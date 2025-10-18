using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerScript : MonoBehaviour
{
    public static PlayerScript Instance { get; private set; }

    [Header("Vida / Status")]
    public int health = 3;

    [Header("Movimento / Dash")]
    public float speed = 5f;
    public bool isDashing;
    public float dashDuration = 0.15f;

    [Header("Ataque Base")]
    public float attackCooldown = 0.3f;
    public float attackTimer;

    [Header("Pontos / Alcance / Alvos")]
    public Transform leftAtkPoint;
    public Transform rightAtkPoint;
    public LayerMask enemyLayer = ~0;
    public float atkRange = 1.5f;

    [Header("Projéteis")]
    public GameObject projectilePrefab;
    public GameObject piercingProjectilePrefab;
    public float shotSpeed = 12f;

    private SpriteRenderer spr;
    private Rigidbody rb;

    // =======================
    //         COMBO
    // =======================
    public enum AttackType
    {
        Melee,
        DoubleMelee,
        Shoot,
        ShootPiercing,
        NoDebuffDash
    }

    [System.Serializable]
    public class AttackStep
    {
        public AttackType type = AttackType.Melee;
        [Tooltip("Multiplica o cooldown base do Player (1 = igual ao attackCooldown).")]
        public float cooldownMultiplier = 1f;
    }

    [Header("Combo")]
    public List<AttackStep> comboSteps = new List<AttackStep>()
    {
        new AttackStep(){ type = AttackType.Melee, cooldownMultiplier = 1f },
    };

    private int currentComboIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        spr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody>();
        isDashing = false;
        attackTimer = attackCooldown;
    }


    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        FlipX(horizontal);

        attackTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            TryExecuteCombo(leftAtkPoint, isRightSide: false);
        else if (Input.GetKeyDown(KeyCode.RightArrow))
            TryExecuteCombo(rightAtkPoint, isRightSide: true);

        if (Input.GetKeyDown(KeyCode.UpArrow))
            if (!isDashing)
                DoubleAttack(rightAtkPoint);
    }

    public void FlipX(float horizontal)
    {
        if (spr == null) return;
        if (horizontal > 0.01f) spr.flipX = false;
        else if (horizontal < -0.01f) spr.flipX = true;
    }

    private Collider GetClosest(Collider[] hits)
    {
        if (hits == null || hits.Length == 0) return null;
        Collider closest = hits[0];
        float bestDist = Vector3.SqrMagnitude(hits[0].transform.position - transform.position);
        for (int i = 1; i < hits.Length; i++)
        {
            float d = Vector3.SqrMagnitude(hits[i].transform.position - transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                closest = hits[i];
            }
        }
        return closest;
    }

    private void TryExecuteCombo(Transform atkPoint, bool isRightSide)
    {
        if (isDashing) return;
        if (comboSteps == null || comboSteps.Count == 0) return;

        AttackStep step = comboSteps[Mathf.Clamp(currentComboIndex, 0, comboSteps.Count - 1)];
        float stepCooldown = attackCooldown * Mathf.Max(0.01f, step.cooldownMultiplier);

        if (attackTimer < stepCooldown) return;

        spr.flipX = !isRightSide;
        ExecuteAttackType(step.type, atkPoint);

        // avança combo
        currentComboIndex++;
        if (currentComboIndex >= comboSteps.Count)
            currentComboIndex = 0;

        attackTimer = 0f;
    }

    private void ExecuteAttackType(AttackType type, Transform atkPoint)
    {
        switch (type)
        {
            case AttackType.Melee:
                Attack(atkPoint);
                break;
            case AttackType.DoubleMelee:
                DoubleAttack(atkPoint);
                break;
            case AttackType.Shoot:
                ShootProjectile(false);
                break;
            case AttackType.ShootPiercing:
                ShootProjectile(true);
                break;
            case AttackType.NoDebuffDash:
                NoDebuffAttack(atkPoint);
                break;
        }
    }

    // =======================
    //       ATAQUES
    // =======================
    public void Attack(Transform atkPoint)
    {
        if (atkPoint == null) return;
        Collider[] hits = Physics.OverlapSphere(atkPoint.position, atkRange, enemyLayer);
        Collider target = GetClosest(hits);
        if (target != null) StartCoroutine(DashAndHit(atkPoint, target));
        else StartCoroutine(DashAndMiss(atkPoint, true));
    }

    public void DoubleAttack(Transform atkPoint)
    {
        if (atkPoint == null) return;
        StartCoroutine(DoubleDashSequence(atkPoint));
    }

    private IEnumerator DoubleDashSequence(Transform atkPoint)
    {
        for (int i = 0; i < 2; i++)
        {
            Collider[] hits = Physics.OverlapSphere(atkPoint.position, atkRange, enemyLayer);
            Collider target = GetClosest(hits);
            if (target != null)
                yield return StartCoroutine(DashAndHit(atkPoint, target));
            else
                yield return StartCoroutine(DashAndMiss(atkPoint, false));
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void NoDebuffAttack(Transform atkPoint)
    {
        if (atkPoint == null) return;
        Collider[] hits = Physics.OverlapSphere(atkPoint.position, atkRange, enemyLayer);
        Collider target = GetClosest(hits);
        if (target != null) StartCoroutine(DashAndHit(atkPoint, target));
        else StartCoroutine(DashAndMiss(atkPoint, false));
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
        if (canDebuff) attackTimer = 0f;
        isDashing = false;
    }

    public void ShootProjectile(bool isPiercing)
    {
        GameObject prefab = isPiercing ? piercingProjectilePrefab : projectilePrefab;
        if (prefab == null) return;
        GameObject bullet = Instantiate(prefab, transform.position, Quaternion.identity);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            Vector3 direction = spr != null && spr.flipX ? Vector3.left : Vector3.right;
            bulletRb.linearVelocity = direction * shotSpeed;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0) Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (leftAtkPoint != null) Gizmos.DrawWireSphere(leftAtkPoint.position, atkRange);
        if (rightAtkPoint != null) Gizmos.DrawWireSphere(rightAtkPoint.position, atkRange);
    }

    public void InsertAttackType(int type)
    {
        switch (type)
        {
            case 1:
                {
                    var attackType = new AttackStep() { type = AttackType.Melee, cooldownMultiplier = 1f };

                    comboSteps.Add(attackType);

                    break;
                }

            case 2:
                {
                    var attackType = new AttackStep() { type = AttackType.DoubleMelee, cooldownMultiplier = 1f };
        

                    comboSteps.Add(attackType);

                    break;
                }

            case 3:
                {
                    var attackType = new AttackStep() { type = AttackType.Shoot, cooldownMultiplier = 1f };

                    comboSteps.Add(attackType);

                    break;
                }

            case 4:
                {
                    var attackType = new AttackStep() { type = AttackType.ShootPiercing, cooldownMultiplier = 1f };

                    comboSteps.Add(attackType);

                    break;
                }

            case 5:
                {
                    var attackType = new AttackStep() { type = AttackType.NoDebuffDash, cooldownMultiplier = 1f };

                    comboSteps.Add(attackType);

                    break;
                }

            default: break;
        }
    }
}
