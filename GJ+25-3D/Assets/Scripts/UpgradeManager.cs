using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    private PlayerScript player;
    void Start()
    {
        player = GetComponent<PlayerScript>();
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
        if (player.health < 3)
        {
            player.health++;
        }
        else
        {

        }
    }
}
