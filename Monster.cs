using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{
    private Coroutine wanderCoroutine;
    private Animator animator;
    private MeshRenderer meshRenderer;
    private NavMeshAgent agent;

    private MonsterState currentState = MonsterState.Inactive; // 현재상태를 비활성상태로 지정

    // 플레이어의 위치
    public Transform player;

    // 이동속도 설정
    float wanderSpeed = 2.0f;       // 배회할 때 이동속도
    float chaseSpeed = 5.0f;        // 추격할 때 이동속도
    float rotationSpeed = 5.0f;     // 회전 속도
    float stoppingDistance = 1.5f;  // 정지 거리
    bool canAttack = true;
    float attackRange = 1.5f;       // 공격 거리
    float attackCooldown = 2f;      // 공격 간격

    // 몬스터 동작 상태
    enum MonsterState
    {
        Inactive,   // 아직 활성화되지 않음
        Wandering,  // 배회 상태
        Chasing,     // 플레이어 추격 상태
        Attacking   // 플레이어 공격
    }

    void Start()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position); // NavMesh 위로 위치 변경
        }

        // 만약 상태 초기화나 다른 로직이 필요하다면 여기서 진행
        agent.isStopped = false; // 초반에는 이동을 멈춘 상태로 설정
        agent.speed = wanderSpeed;
        ActivateMonster();
    }

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        agent = GetComponent<NavMeshAgent>(); // NavMeshAgent 가져오기
    }

    /// 몬스터를 활성화하여 배회 상태로 전환한다.
    void ActivateMonster()
    {
        currentState = MonsterState.Wandering;
        agent.isStopped = false;
        agent.speed = wanderSpeed;
        // 코루틴이 이미 실행중이면 Stop하고 다시 실행
        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
        }
        wanderCoroutine = StartCoroutine(WanderRoutine());
    }

    /// 몬스터를 플레이어 추격 상태로 전환한다.
    void EnableChase()
    {
        currentState = MonsterState.Chasing;
        agent.speed = chaseSpeed;

        if (wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
            wanderCoroutine = null;
        }
        // 추격 관련 코드
    }

    void Update()
    {
        // 현재 상태에 따라 행동을 실행
        switch (currentState)
        {
            case MonsterState.Wandering:
                if (wanderCoroutine == null) // 이미 실행 중이면 중복 실행 방지
                {
                    wanderCoroutine = StartCoroutine(WanderRoutine());
                }
                break;

            case MonsterState.Chasing:
                Chase();
                break;
            case MonsterState.Attacking:
                Attack();
                break;
        }
    }

    /// 배회 행동 로직 (코루틴 사용) / 추가해야될 내용) 장애물 회피 , 일정 범위 내에서 활동
    private IEnumerator WanderRoutine()
    {
        // 회전 속도와 회전 각도 제한 (필요에 따라 값 조절)
        float maxAngularDeviation = 45f;  // 현재 진행 방향 기준 ±45도 내에서 회전

        while (currentState == MonsterState.Wandering)
        {
            // 현재 회전 값에서 ±45도 범위 내의 작은 각도 변경을 계산합니다.
            float randomOffset = Random.Range(-maxAngularDeviation, maxAngularDeviation);
            Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y + randomOffset, 0);

            // 부드럽게 회전하기 위해 일정 시간 동안 Slerp로 보간합니다.
            float rotationTime = Random.Range(0.5f, 1f); // 회전하는 데 걸리는 시간
            float elapsed = 0f;
            while (elapsed < rotationTime)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsed / rotationTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
            // 보간 후 정확히 목표 회전값으로 맞춥니다.
            transform.rotation = targetRotation;

            // 현재 진행 방향을 기준으로 무작위 이동 거리 계산
            Vector3 randomDirection = transform.forward * Random.Range(2f, 5f);
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position + randomDirection, out hit, 5f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        
            // 이동 시간과 도착 후 대기 시간을 설정합니다.
            yield return new WaitForSeconds(Random.Range(2f, 4f)); // 이동 중 대기
            yield return new WaitForSeconds(Random.Range(0.5f, 1f)); // 이동 후 대기
        }
    }

    /// 플레이어를 향해 추격하는 로직
    void Chase()
    {
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance > stoppingDistance) //거리가 정지거리보다 멀면 추격 실행
            {
                agent.SetDestination(player.position);
                Vector3 direction = (player.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                Debug.Log("플레이어에게 도착! 공격 시작!");
                currentState = MonsterState.Attacking;

            }
        }
    }

    void Attack()
    {
        Debug.Log("공격");
        if (canAttack && player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= attackRange)
            {
                Debug.Log("플레이어를 공격");
                // 애니메이션 실행
                // GetComponent<Animator>().SetTrigger("Attack");

                // 플레이어에게 데미지 적용
                // player.GetComponent<PlayerHealth>().TakeDamge(10);

                canAttack = false;
                StartCoroutine(AttackCooldown()); // 공격후 쿨타임임
            }
            else
            {
                currentState = MonsterState.Chasing; // 공격 범위 밖이면 다시 추격
            }
        }

        // 공격 애니메이션 및 데미지 적용
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
}
