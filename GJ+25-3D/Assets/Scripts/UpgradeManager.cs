using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    private PlayerScript player;
    public bool specialUpgradeActive = false;
    private int specialAttackCount = 0;
    public int specialAttackMax = 3;
    public bool hasPendingPotion = false;
    public bool hasCard = false;
    void Start()
    {
        player = GetComponent<PlayerScript>();
    }

    void Update()
    {
        
    }

    public void AddZombieShield()
    {
        Debug.Log("Shield Added");
        player.isShielded = true;
        hasCard = true;
    }

    public void AddWitchPotion()
    {
        Debug.Log("Potion Added");
        hasCard = true;
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
        Debug.Log("Special Attack Added");
        specialUpgradeActive = true;
        specialAttackCount = 0;
        hasCard = true;
    }

    public bool UseSpecialAttack()
    {
        if (specialUpgradeActive && specialAttackCount < specialAttackMax)
        {
            specialAttackCount++;
            if (specialAttackCount >= specialAttackMax)
            {
                specialUpgradeActive = false;
                hasCard = false;
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
            hasCard = false;
        }
    }

    public void GetRandomUpgrade()
    {
        int random = Random.Range(0, 3);
        switch (random)
        {
            case 0:
                AddZombieShield();
                break;
            case 1:
                AddWitchPotion();
                break;
            case 2:
                AddSpecialAttack();
                break;
                

        }
    }


}
