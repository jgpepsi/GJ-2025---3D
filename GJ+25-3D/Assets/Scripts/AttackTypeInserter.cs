using UnityEngine;

public class AttackTypeInserter : MonoBehaviour
{
   public void Insert(int type)
   {
        var player = PlayerScript.Instance;

        player.InsertAttackType(type);
   }
}
