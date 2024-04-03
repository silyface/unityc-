using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class PlayController : MonoBehaviour
{
    private NavMeshAgent agent;

    private Animator anim;
    private CharacterStats characterStats;

    private GameObject attackTarget;

    private float lastAttacktime;
    private bool isDeath;

    void Awake() 
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();
    }
    void Start() 
    {
        MouseManager.Instance.OnMouseClicked+=MoveToTarget;
        MouseManager.Instance.OnEnemyClicked+=EventAttack;
        characterStats.MaxHealth = 2;
        
    }


    void Update() 
    {
        isDeath = characterStats.CurrentHealth == 0;
       SwitchAnimation() ;

       lastAttacktime -= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed",agent.velocity.sqrMagnitude);
        anim.SetBool("Death",isDeath);
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        agent.isStopped = false;
        agent.destination=target;
       
    }
    
    private void EventAttack(GameObject target)
    {
      if(target!=null)
      {
        attackTarget = target;
        //计算攻击是否是暴击
        characterStats.isCritical = UnityEngine.Random.value <characterStats.attackData.criticalChance;
        StartCoroutine(MoveToAttackTarget());
      }
    }

    IEnumerator  MoveToAttackTarget()
    {
        //根据距离判断角色和敌人距离，并持续移动到敌人面前
        agent.isStopped = false;
        transform.LookAt(attackTarget.transform);
        //修改攻击范围参数
        while (Vector3.Distance(attackTarget.transform.position,transform.position)>characterStats.attackData.attackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }
        agent.isStopped =true;
        //attack
        if(lastAttacktime<0)
        {
            anim.SetTrigger("Attack");
            anim.SetBool("Critical",characterStats.isCritical);
            //重置冷却时间
            lastAttacktime = characterStats.attackData.coolDown;
        }
    }
    
    //Animation event

    void Hit()
    {
        var targetStats = attackTarget.GetComponent<CharacterStats>();
        targetStats.TakeDamage(characterStats,targetStats);
    }
   
}
