using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterStats: MonoBehaviour
{
   public CharacterData_SO templateDate;
   public CharacterData_SO characterData;

   public AttackData_SO attackData;

   [HideInInspector]
   public bool isCritical;

//
   void Awake() 
   {
      if(templateDate != null)
      characterData = Instantiate(templateDate);
      
   }

//

   #region  Read from Data_SO

   public int MaxHealth
   {
    get
    { if(characterData!=null) return characterData.maxHealth; else return 0;}
    set{characterData.maxHealth = value;}
   }

   public int CurrentHealth
   {
    get
    { if(characterData!=null) return characterData.currentHealth; else return 0;}
    set{characterData.currentHealth = value;}
   }   
   public int BaseDefence
   {
    get
    { if(characterData!=null) return characterData.baseDefence; else return 0;}
    set{characterData.baseDefence = value;}
   }
   public int CurrentDefence
   {
    get
    { if(characterData!=null) return characterData.currentDefence; else return 0;}
    set{characterData.currentDefence = value;}
   }
   #endregion

   
      #region Character Combat
   public void TakeDamage(CharacterStats attacker,CharacterStats defender)
   {
      //造成伤害，并防止伤害为正数
      int damage =Mathf.Max(attacker.CurrentDamage() - defender.CurrentDefence,0);
      CurrentHealth = Mathf.Max(CurrentHealth - damage,0);

      if(isCritical)
      {
         //TODO:当前受击动作状态机有问题
         defender.GetComponent<Animator>().SetTrigger("Hit");
      }
      //TODO:update ui
      //TODO:经验升级
   }



    private int CurrentDamage()
    {
       float coreDamage = UnityEngine.Random.Range(attackData.minDamge,attackData.maxDamge);

       if(isCritical)
       {
         coreDamage *=attackData.criticalMultiplier;
         Debug.Log("暴击："+ coreDamage);
       }
       return (int) coreDamage;
    }

    #endregion

};


