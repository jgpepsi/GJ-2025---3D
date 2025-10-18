using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ComboManager : MonoBehaviour
{
    public enum AttackType
    {
        Melee,              // PlayerScript.Attack
        DoubleMelee,        // PlayerScript.DoubleAttack
        Shoot,              // PlayerScript.ShootProjectile(false)
        ShootPiercing,      // PlayerScript.ShootProjectile(true)
        NoDebuffDash        // Executa NoDebuffAttack (sem penalidade)
    }

    [Serializable]
    public class AttackStep
    {
        public AttackType type = AttackType.Melee;

        [Tooltip("Multiplica o cooldown base do Player (1 = sem alteração).")]
        public float cooldownMultiplier = 1f;

        [Tooltip("Repetições desta etapa antes de avançar para a próxima.")]
        public int repeatCount = 1;

        [Tooltip("Rótulo opcional para depuração/UI.")]
        public string label = "";
    }

    [Header("Combo (ordem fixa por enquanto)")]
    [SerializeField] private List<AttackStep> combo = new List<AttackStep>();

    [Header("Input")]
    [SerializeField] private KeyCode leftKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode rightKey = KeyCode.RightArrow;

    [Header("Debug")]
    [SerializeField] private int currentIndex = 0;
    [SerializeField] private int repeatsLeftOnCurrent = 0;

    public event Action<int, AttackStep> OnComboAdvanced;

    private PlayerScript player;
    private Transform leftPoint, rightPoint;
    private float basePlayerCooldown;

    private void Awake()
    {
        player = GetComponent<PlayerScript>();
        if (player == null)
        {
            Debug.LogError("[ComboManager] PlayerScript não encontrado no mesmo GameObject.");
        }
    }

    private void Start()
    {
        // cria combo padrão caso a lista esteja vazia
        if (combo == null || combo.Count == 0)
        {
            combo = new List<AttackStep>
            {
                new AttackStep { type = AttackType.Melee, label = "Melee 1" },
                new AttackStep { type = AttackType.Shoot, label = "Tiro" },
                new AttackStep { type = AttackType.DoubleMelee, label = "Duplo" },
                new AttackStep { type = AttackType.Melee, label = "Melee 2" },
                new AttackStep { type = AttackType.ShootPiercing, label = "Piercing" }
            };
        }

        leftPoint = player.leftAtkPoint;
        rightPoint = player.rightAtkPoint;

        currentIndex = Mathf.Clamp(currentIndex, 0, combo.Count - 1);
        repeatsLeftOnCurrent = GetRepeatForCurrent();

        basePlayerCooldown = player.attackCooldown;
    }

    private void Update()
    {
        if (player == null) return;
        if (player.isDashing) return;

        // respeita cooldown global
        if (player.attackTimer < player.attackCooldown) return;

        bool leftPressed = Input.GetKeyDown(leftKey);
        bool rightPressed = Input.GetKeyDown(rightKey);
        if (!leftPressed && !rightPressed) return;

        Transform atkPoint = leftPressed ? leftPoint : rightPoint;

        var step = combo[currentIndex];
        bool executed = ExecuteStep(step, atkPoint);
        if (!executed) return;

        // reseta o timer do Player
        player.attackTimer = 0f;

        // aplica multiplicador de cooldown (temporário)
        if (Mathf.Abs(step.cooldownMultiplier - 1f) > 0.001f)
        {
            player.attackCooldown = basePlayerCooldown * step.cooldownMultiplier;
            StartCoroutine(RestoreCooldownNextFrame());
        }

        // consome repetição e avança combo
        repeatsLeftOnCurrent--;
        if (repeatsLeftOnCurrent <= 0)
        {
            AdvanceComboIndex();
        }
    }

    private IEnumerator RestoreCooldownNextFrame()
    {
        yield return null;
        player.attackCooldown = basePlayerCooldown;
    }

    private int GetRepeatForCurrent()
    {
        if (combo == null || combo.Count == 0) return 1;
        int r = combo[currentIndex].repeatCount;
        return Mathf.Max(1, r);
    }

    private void AdvanceComboIndex()
    {
        currentIndex = (currentIndex + 1) % combo.Count;
        repeatsLeftOnCurrent = GetRepeatForCurrent();
        OnComboAdvanced?.Invoke(currentIndex, combo[currentIndex]);

#if UNITY_EDITOR
        var step = combo[currentIndex];
        Debug.Log($"[Combo] Avançou para {currentIndex} ({step.label} | {step.type}) x{repeatsLeftOnCurrent}");
#endif
    }

    // --- EXECUÇÃO DE ATAQUES ---
    private bool ExecuteStep(AttackStep step, Transform atkPoint)
    {
        if (step == null) return false;

        // ✅ Atualiza a direção visual antes do ataque
        // Se o ponto for o esquerdo, flipX = true → mira pra esquerda
        SpriteRenderer spr = player.GetComponent<SpriteRenderer>();
        if (spr != null)
            spr.flipX = (atkPoint == player.leftAtkPoint);

        switch (step.type)
        {
            case AttackType.Melee:
                player.Attack(atkPoint);
                return true;

            case AttackType.DoubleMelee:
                player.DoubleAttack(atkPoint);
                return true;

            case AttackType.Shoot:
                player.ShootProjectile(false);
                return true;

            case AttackType.ShootPiercing:
                player.ShootProjectile(true);
                return true;

            case AttackType.NoDebuffDash:
                StartCoroutine(RunNoDebuff(atkPoint));
                return true;

            default:
                return false;
        }
    }

    // --- WRAPPER PARA NoDebuffAttack QUE É void ---
    private IEnumerator RunNoDebuff(Transform atkPoint)
    {
        player.NoDebuffAttack(atkPoint);

        // Aguarda o dash terminar observando a flag isDashing
        float safety = 5f; // failsafe para evitar travas
        while (player.isDashing && safety > 0f)
        {
            safety -= Time.deltaTime;
            yield return null;
        }
    }

    // --- MÉTODOS PÚBLICOS DE EXTENSÃO ---
    public void AppendStep(AttackStep step)
    {
        if (step == null) return;
        combo.Add(step);
    }

    public void AppendStep(AttackType type, float cooldownMult = 1f, int repeatCount = 1, string label = "")
    {
        combo.Add(new AttackStep
        {
            type = type,
            cooldownMultiplier = cooldownMult,
            repeatCount = Mathf.Max(1, repeatCount),
            label = label
        });
    }

    public (int index, AttackStep step) GetCurrent() => (currentIndex, combo[currentIndex]);
}
