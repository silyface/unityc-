using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyStates{GUARD,PATROL,CHASE,DEAD}
[RequireComponent(typeof(NavMeshAgent))]

public class EnemyController : MonoBehaviour, IEndGameObserver
{
   private EnemyStates enemyStates;
   private NavMeshAgent agent;
   private Animator anim;
   private Collider coll;

   private CharacterStats characterStats;

   [Header("Basic Setting")]
   public float sightRadius;

   public bool isGuard;
   public float speed;

   private GameObject attackTarget;

   public float lookAtTime;

   private float remainLookAtTime;
   private float lastAttacktime;

   private Quaternion guardRotation;
   [Header("Patrol State")]

   public float patrolRang;
   private Vector3 wayPoint;

   private Vector3 guardPos;


   //bool状态设置
   bool isWalk;
   bool isChase;
   bool isFollow;
   bool isDeath;
   bool playerDead;


   void Awake()
   {
    agent = GetComponent<NavMeshAgent>();
    speed = agent.speed;
    anim = agent.GetComponent<Animator>();
    characterStats = GetComponent<CharacterStats>();
    coll = GetComponent<Collider>();

    guardPos = transform.position;
    guardRotation = transform.rotation;
    remainLookAtTime = lookAtTime;
   }

   void Start()
   {
      //TODO:这里可能有问题
      playerDead = false;
      if (isGuard)
      {
         enemyStates = EnemyStates.GUARD;

      }

      else
      {
         enemyStates = EnemyStates.PATROL;
         //给于巡逻敌人初始移动点位
         GetNewWayPoint();    
      }
       //FIXME:场景切换后修改
      GameManager.Instance.AddObserver(this);
      UnityEngine.Debug.Log ("已经加到名单");
   }
   /*
    void OnEnable()
    {
       GameManager.Instance.AddObserver(this);
    }
   */
   void OnDisable() 
   {
      //将敌人移除广播名单
      if(!GameManager.IsInitialized) return;
      GameManager.Instance.RemoveObserver(this);
   }
   void Update()
   {
      if(characterStats.CurrentHealth == 0)
         isDeath = true;

      if (!playerDead)
      {
         SwitchStates();
         SwitchAnimation();
         lastAttacktime -= Time.deltaTime;
      }
   }
   void SwitchAnimation()
   {
      anim.SetBool("Walk",isWalk);
      anim.SetBool("Chase",isChase);
      anim.SetBool("Follow",isFollow);
      anim.SetBool("Critical",characterStats.isCritical);
      anim.SetBool("Death",isDeath);
   }

   void SwitchStates()
   {
      if(isDeath)
      enemyStates = EnemyStates.DEAD;
      //如果发现player，切换到chase状态
      else if (FoundPlayer())
      {
         enemyStates =EnemyStates.CHASE;
         UnityEngine.Debug.Log ("找到player");
      }
    switch(enemyStates)
    {
      case EnemyStates.GUARD:
            isChase = false;
         if (transform.position != guardPos)
         {
            isWalk = true;
            agent.isStopped = false;
            agent.destination = guardPos;

            if(Vector3.SqrMagnitude(guardPos-transform.position)<=agent.stoppingDistance)
            //SqrMagnitude的性能消耗比Distance小
            {
               isWalk = false;
               transform.rotation = Quaternion.Lerp(transform.rotation,guardRotation,0.01f);
            }
         }
         break;

      case EnemyStates.PATROL:
         isChase =false;
         agent.speed = speed * 0.5f;
         if(Vector3.Distance(wayPoint,transform.position) <= agent.stoppingDistance)
         {
            isWalk =false;
            if(remainLookAtTime > 0)
                remainLookAtTime -= Time.deltaTime;
            else
            GetNewWayPoint();
         }
         else
         {
            isWalk = true;
            agent.destination = wayPoint;
         }
         break;
      case EnemyStates.CHASE:
         //追击player
         //拉脱回到上一个状态；
         //配合动画
         isWalk = false;
         isChase = true;
         agent.speed = speed;
         if(!FoundPlayer())
         {
            //拉脱回到上一个状态；
            isFollow = false;
            if(remainLookAtTime > 0)
            {
               //脱战后回归正常状态（别继续追着玩家跑）
               agent.destination = transform.position;
               remainLookAtTime-=Time.deltaTime;
            }
            else if(isGuard)
              enemyStates = EnemyStates.GUARD;
            else
              enemyStates = EnemyStates.PATROL;
         }
         else
         {
            //在攻击范围内则攻击
            isFollow = true;
            agent.isStopped = false;
            agent.destination = attackTarget.transform.position;
         }

         //在攻击范围内则攻击；
         if(TargetInAttackRange()||TargetInSkillRange())
         {
            isFollow = false;
            agent.isStopped = true;

            if(lastAttacktime < 0)
            {
               lastAttacktime = characterStats.attackData.coolDown;

               //暴击判断
               characterStats.isCritical = Random.value < characterStats.attackData.criticalChance;
               //执行攻击
               Attack();
            }
         }
         break;
      case EnemyStates.DEAD:
         coll.enabled = false;
         agent.enabled =false;
         Destroy(gameObject,2f);
         break;

    }
   }
   void Attack()
   {
      transform.LookAt(attackTarget.transform);
      if(TargetInAttackRange())
      {
         //近身攻击动画
         anim.SetTrigger("Attack");
      }
      if(TargetInSkillRange())
      {
         //技能攻击动画
         anim.SetTrigger("Skill");
      }

   }

   //bool值怪物判断周围是否有玩家
   bool FoundPlayer()
   {
      var colliders = Physics.OverlapSphere(transform.position,sightRadius);

      foreach  (var target in colliders)
      {
         if(target.CompareTag("Player"))
         {
            attackTarget = target.gameObject;
            return true;
         }
      }
      attackTarget = null;
      return false;
   }

   bool TargetInAttackRange()
   {
      if(attackTarget != null)
         return Vector3.Distance(attackTarget.transform.position,transform.position)<= characterStats.attackData.attackRange;
      else
         return false;
   }
   bool TargetInSkillRange()
   {
      if(attackTarget != null)
         return Vector3.Distance(attackTarget.transform.position,transform.position)<= characterStats.attackData.skillRange;
      else
         return false;
   }

   //在巡逻范围内，控制怪物随机移动
   void GetNewWayPoint()
   {
      remainLookAtTime = lookAtTime;

      float randomX =   Random.Range(-patrolRang,patrolRang);
      float randomZ =   Random.Range(-patrolRang,patrolRang);
      //保留y的当前位置是为了防止怪物悬浮移动
      Vector3 randomPoint = new Vector3(guardPos.x+randomX,transform.position.y,guardPos.z+randomZ);
      //三元运算符，判断随机点位是否为可行走点位
      NavMeshHit hit;
      wayPoint =NavMesh.SamplePosition(randomPoint,out hit, patrolRang, 1)? hit.position : transform.position;
   }

   void OnDrawGizmosSelected() 
   {
      Gizmos.color = Color.blue;
      Gizmos.DrawWireSphere(transform.position,sightRadius);
   }

   //Animation event

    void Hit()
    {
      if (attackTarget != null)
      {
        var targetStats = attackTarget.GetComponent<CharacterStats>();
        targetStats.TakeDamage(characterStats,targetStats);
      }
    }

     public void EndNotify()
    {
        //获胜动画
        //停止所有移动
        //停止Agent
         UnityEngine.Debug.Log ("已经收到玩家死亡讯息，开始执行庆祝操作");
        anim.SetBool("Win", true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;

    }
}
