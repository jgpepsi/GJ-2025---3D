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
        NoDebuffDash        // PlayerScript.NoDebuffAttack (sem penalidade)
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
    [SerializeField] private bool handleInput = true; // <- controla se este script lê teclas
    [SerializeField] private KeyCode leftKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode rightKey = KeyCode.RightArrow;

    [Header("Cooldown")]
    [SerializeField] private bool useComboCooldownMultipliers = false;

    [Header("Debug")]
    [SerializeField] private bool verboseLogs = false;
    [SerializeField] private int currentIndex = 0;
    [SerializeField] private int repeatsLeftOnCurrent = 0;

    public event Action<int, AttackStep> OnComboAdvanced;

    private PlayerScript player;
    private Transform leftPoint, rightPoint;
    private SpriteRenderer spr;
    private float basePlayerCooldown;

    private void Awake()
    {
        player = GetComponent<PlayerScript>();
        if (player == null)
        {
            Debug.LogError("[ComboManager] PlayerScript não encontrado no mesmo GameObject!");
            enabled = false;
            return;
        }

        spr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // cria combo padrão se a lista estiver vazia
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

        if (leftPoint == null || rightPoint == null)
        {
            Debug.LogError("[ComboManager] leftAtkPoint/rightAtkPoint não atribuídos no PlayerScript!");
            enabled = false;
            return;
        }

        basePlayerCooldown = player.attackCooldown;
        currentIndex = 0;
        repeatsLeftOnCurrent = GetRepeatForCurrent();
    }

    private void Update()
    {
        if (!handleInput) return; // outro script pode acionar manualmente

        if (Input.GetKeyDown(leftKey)) TriggerLeft();
        if (Input.GetKeyDown(rightKey)) TriggerRight();
    }

    // --- Métodos públicos caso queira acionar de outro script ---
    public void TriggerLeft() => TryPerformStep(leftPoint);
    public void TriggerRight() => TryPerformStep(rightPoint);

    // --- Controle principal do combo ---
    private void TryPerformStep(Transform atkPoint)
    {
        if (player == null || !isActiveAndEnabled) return;
        if (player.isDashing) return; // não ataca durante dash
        if (player.attackTimer < player.attackCooldown) return; // respeita cooldown

        var step = combo[currentIndex];

        // Atualiza a orientação visual antes do ataque
        if (spr != null)
            spr.flipX = (atkPoint == player.leftAtkPoint);

        bool executed = ExecuteStep(step, atkPoint);
        if (!executed) return;

        // reseta cooldown do player
        player.attackTimer = 0f;

        // aplica multiplicador de cooldown se habilitado
        if (useComboCooldownMultipliers && Mathf.Abs(step.cooldownMultiplier - 1f) > 0.001f)
        {
            player.attackCooldown = basePlayerCooldown * step.cooldownMultiplier;
            StartCoroutine(RestoreCooldownNextFrame());
        }

        // consome repetição e avança combo
        repeatsLeftOnCurrent--;
        if (repeatsLeftOnCurrent <= 0)
            AdvanceComboIndex();

        if (verboseLogs)
            Debug.Log($"[Combo] Step {currentIndex} -> {step.type} ({step.label}) x{repeatsLeftOnCurrent}");
    }

    private IEnumerator RestoreCooldownNextFrame()
    {
        yield return null;
        player.attackCooldown = basePlayerCooldown;
    }

    private int GetRepeatForCurrent()
    {
        if (combo == null || combo.Count == 0) return 1;
        return Mathf.Max(1, combo[currentIndex].repeatCount);
    }

    private void AdvanceComboIndex()
    {
        currentIndex = (currentIndex + 1) % combo.Count;
        repeatsLeftOnCurrent = GetRepeatForCurrent();
        OnComboAdvanced?.Invoke(currentIndex, combo[currentIndex]);

        if (verboseLogs)
        {
            var step = combo[currentIndex];
            Debug.Log($"[Combo] Avançou para {currentIndex} ({step.label} | {step.type}) x{repeatsLeftOnCurrent}");
        }
    }

    // --- Executa cada tipo de ataque ---
    private bool ExecuteStep(AttackStep step, Transform atkPoint)
    {
        if (step == null) return false;

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
                StartCoroutine(RunNoDebuff(atkPoint)); // NoDebuffAttack é void
                return true;

            default:
                return false;
        }
    }

    // --- Wrapper para NoDebuffAttack (void no PlayerScript) ---
    private IEnumerator RunNoDebuff(Transform atkPoint)
    {
        player.NoDebuffAttack(atkPoint);

        float safety = 5f;
        while (player.isDashing && safety > 0f)
        {
            safety -= Time.deltaTime;
            yield return null;
        }
    }

    // --- Métodos auxiliares (adicionar ataques dinamicamente) ---
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
