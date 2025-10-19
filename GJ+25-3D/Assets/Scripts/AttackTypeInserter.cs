using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AttackTypeInserter : MonoBehaviour
{
   public List<Button> buttonList = new List<Button>();
   public List<Transform> buttonPos = new List<Transform>();

   public void Insert(int type)
   {
        var player = PlayerScript.Instance;

        player.InsertAttackType(type);
   }

   public void SpawnButtons()
    {
        for(int i = 0; i<buttonPos.Count; i++)
        {
            var button = Instantiate(buttonList[i], buttonPos[i].position, Quaternion.identity, buttonPos[i]);
        }
    }
}
