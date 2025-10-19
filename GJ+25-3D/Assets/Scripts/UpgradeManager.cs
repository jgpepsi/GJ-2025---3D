using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    private PlayerScript player;
    public bool specialUpgradeActive = false;
    private int specialAttackCount = 0;
    public int specialAttackMax = 3;
    public bool hasPendingPotion = false;
    void Start()
    {
        player = GetComponent<PlayerScript>();
        AddSpecialAttack();
    }

    void Update()
    {
        
    }

    public void AddZombieShield()
    {
        player.isShielded = true;
    }

    public void AddWitchPotion()
    {
        if (player.health < player.maxHealth)
        {
            player.health++;
        }
        else
        {
            hasPendingPotion = true;
        }
    }

    public void AddSpecialAttack()
    {
        specialUpgradeActive = true;
        specialAttackCount = 0;
    }

    public bool UseSpecialAttack()
    {
        if (specialUpgradeActive && specialAttackCount < specialAttackMax)
        {
            specialAttackCount++;
            if (specialAttackCount >= specialAttackMax)
            {
                specialUpgradeActive = false;
            }
            return true;
        }
        return false;
    }

    public void ApplyPendingPotion()
    {
        if (hasPendingPotion && player.health < 3)
        {
            player.health++;
            hasPendingPotion = false;
        }
    }


}
