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
    private bool isDead;
    private float stopDistance;

    void Awake() 
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStats = GetComponent<CharacterStats>();

         stopDistance = agent.stoppingDistance;
    }

    
    void Start() 
    {
        MouseManager.Instance.OnMouseClicked+=MoveToTarget;
        MouseManager.Instance.OnEnemyClicked+=EventAttack;
  
        GameManager.Instance.RigisterPlayerr(characterStats);
    }

    void Update()
    {
        isDead = characterStats.CurrentHealth == 0;

        if (isDead)
            GameManager.Instance.NotifyObservers();

        // KeyboardControl();
        // ActionAttack();

        SwitchAnimation();

        lastAttacktime-= Time.deltaTime;
    }

    private void SwitchAnimation()
    {
        anim.SetFloat("Speed",agent.velocity.sqrMagnitude);
        anim.SetBool("Death",isDead);
    }

    public void MoveToTarget(Vector3 target)
    {
        StopAllCoroutines();
        if(isDead) return;
        agent.isStopped = false;
        agent.destination=target;
       
    }
    
    private void EventAttack(GameObject target)
    {
        if(isDead) return;

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
           
            anim.SetBool("Critical",characterStats.isCritical);
            anim.SetTrigger("Attack");
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
