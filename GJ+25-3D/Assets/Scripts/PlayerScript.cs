using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Android;

[DisallowMultipleComponent]
public class PlayerScript : MonoBehaviour
{
    public static PlayerScript Instance { get; private set; }

    [Header("Vida / Status")]
    public int health = 3;
    public int maxHealth = 3;

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

    public bool isShielded;
    public bool hasDeadlyAttack;
    public int nCounter = 0;
    public GameObject deathHitbox;
    public bool canTakeDamage;
    private UpgradeManager upgradeManager;

    private SpriteRenderer spr;
    private Rigidbody rb;

    public Animator anim;

    // =======================
    //         COMBO
    // =======================
    public enum AttackType
    {
        Melee,
        DoubleMelee,
        Shoot,
        ShootPiercing,
        LongMelee,
        Howl
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
        canTakeDamage = true;
        attackTimer = attackCooldown;
        upgradeManager = GetComponent<UpgradeManager>();
    }


    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        FlipX(horizontal);

        attackTimer += Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftArrow) && attackTimer >= attackCooldown)
            TryExecuteCombo(leftAtkPoint, isRightSide: false);
        else if (Input.GetKeyDown(KeyCode.RightArrow) && attackTimer >= attackCooldown)
            TryExecuteCombo(rightAtkPoint, isRightSide: true);

        if(Input.GetKeyDown(KeyCode.Space))
        {
            UseSpecialAttack();
        }

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
            case AttackType.LongMelee:
                LongAttack(atkPoint, extraRange: 0.2f);
                break;
            case AttackType.Howl:
                Howl(range: 10f, duration: 2f);
                break;
            default:
                Attack(atkPoint);
                break;

        }
    }

    // =======================
    //       ATAQUES
    // =======================
    public void Attack(Transform atkPoint)
    {
        if (atkPoint == null) return;
        if(upgradeManager != null && upgradeManager.UseSpecialAttack())
        {
            UseSpecialAttack();
        }

        Collider[] hits = Physics.OverlapSphere(atkPoint.position, atkRange, enemyLayer);
        Collider target = GetClosest(hits);
        if (target != null)
        {
            StartCoroutine(DashAndHit(atkPoint, target));
        }
        else
        {
            StartCoroutine(DashAndMiss(atkPoint, true));
        }
    }

    public void UseSpecialAttack()
    {
        GameObject prefab = deathHitbox;
        if (prefab == null) return;
        GameObject bullet = Instantiate(prefab, transform.position, Quaternion.identity);
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        if (bulletRb != null)
        {
            Vector3 direction = spr != null && spr.flipX ? Vector3.left : Vector3.right;
            bulletRb.linearVelocity = direction * 10;
        }
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
        anim.SetTrigger("Attack");
        isDashing = true;
        Vector3 start = transform.position;
        Vector3 end = Vector3.zero;

        if (target.transform.position.x > transform.position.x)// inimigo na direita
        {
            end = target.transform.position + Vector3.left * .1f;
        }
        else if (target.transform.position.x < transform.position.x)// inimigo a esquerda
        {
            end = target.transform.position + Vector3.right * .1f;
        }

        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            Vector3 next = Vector3.Lerp(start, end, t);
            rb.MovePosition(next);
            Collider[] hits = Physics.OverlapSphere(atkPoint.position, atkRange, enemyLayer);
            if (hits.Length > 1)
            {
                //TimeManager.TimeInstance.ActivateSlowMotion(0.2f, 1f);
                //slowApplied = true;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        ScreenShake.Instance.Shake(0.1f, 0.01f);
        AudioManager.instance.PlaySFX(ChooseSFX());
        rb.MovePosition(end);
        var enemy = target.GetComponent<EnemyScript>();
        if (enemy != null) enemy.TakeDamage(1);
        rb.linearVelocity = Vector3.zero;
        StartCoroutine(IFrames());
        isDashing = false;

    }

    public string ChooseSFX()
    {
        int random = Random.Range(0, 2);
        switch (random)
        {
            case 0:
                return "LoboCorte1";
            case 1:
                return "LoboCorte2";
            default:
                return "LoboCorte3";
        }
    }

    private IEnumerator DashAndMiss(Transform atkPoint, bool canDebuff)
    {
        anim.SetTrigger("Attack");
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
        StartCoroutine(IFrames());
        isDashing = false;
        anim.SetTrigger("Miss");
    }

    public void ShootProjectile(bool isPiercing)
    {
        anim.SetTrigger("Shoot");
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
        AudioManager.instance.PlaySFX("LoboApanha");
        if (isShielded)
        {
            isShielded = false;
            return;
        }
        if(upgradeManager != null && upgradeManager.hasPendingPotion)
        {
            upgradeManager.ApplyPendingPotion();
        }
        Debug.Log("Player took damage!");
        FreezeAllEnemies(0.25f);
        ScreenShake.Instance.Shake(0.1f, 0.02f);
        if (health <= 0) Destroy(gameObject); //ao invés disso, volta pro main menu
    }

    public IEnumerator IFrames()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(0.5f);
        canTakeDamage = true;
    }

    public void FreezeAllEnemies(float value)
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            enemy.GetComponent<EnemyScript>().StartCoroutine(enemy.GetComponent<EnemyScript>().Paralyze(value));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (leftAtkPoint != null) Gizmos.DrawWireSphere(leftAtkPoint.position, atkRange);
        if (rightAtkPoint != null) Gizmos.DrawWireSphere(rightAtkPoint.position, atkRange);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && !isDashing && canTakeDamage)
        {
            TakeDamage(1);
            Destroy(collision.gameObject);
        }
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
                    var attackType = new AttackStep() { type = AttackType.LongMelee, cooldownMultiplier = 1f };

                    comboSteps.Add(attackType);

                    break;
                }
            case 6:
                {
                    var attackType = new AttackStep() { type = AttackType.Howl, cooldownMultiplier = 1f };
                    comboSteps.Add(attackType);
                    break;
                }

            default: break;
        }
    }

    public void GiveDeadlyAttack()
    {
        hasDeadlyAttack = true;
        nCounter = 0;
    }
}