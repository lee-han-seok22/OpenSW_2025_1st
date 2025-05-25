using System.Collections;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    // 몬스터 프리팹과 스폰 지점을 에디터에서 할당
    public GameObject monsterPrefab;
    public Transform spawnPoint;
    public GameObject monsterInstance;

    // 게임 시작 후 몬스터 생성 및 행동 시작 시간 (초 단위)
    private float spawnDelay = 10f; // 4분 후 몬스터 생성
    // private float chaseDelay = 420f; // 7분 후 플레이어 추격 시작

    private void Start()
    {
        Debug.Log("30초 대기");
        if (monsterInstance != null)
        {
            monsterInstance.SetActive(false);
            Debug.Log("기존 몬스터 비활성화");
        }
        // 30초 후 활성화하는 코루틴 실행
            StartCoroutine(InitializeMonster());

    }

    private IEnumerator InitializeMonster()
    {
        // 4분 대기 후 몬스터 생성
        yield return new WaitForSeconds(spawnDelay);

        if (monsterInstance != null)
        {
            // 기존 몬스터를 spawnpoint 위치로 이동 후 활성화화
            monsterInstance.transform.position = spawnPoint.position;
            monsterInstance.transform.rotation = spawnPoint.rotation;
            monsterInstance.SetActive(true);
            Debug.Log("기존 몬스터 활성화");
        }
        else
        {
            // 새로운 몬스터 생성성
            monsterInstance = Instantiate(monsterPrefab, spawnPoint.position, spawnPoint.rotation);
        }   

        // Monster 스크립트를 가져오고, 플레이어 Transform 할당
        Monster monsterScript = monsterInstance.GetComponent<Monster>(); // 생성된 몬스터 오브젝트에서 Monster스크립트를 가져옴
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player"); // "Player"태그가 설정된 오브젝트를 찾음음
        if (playerObject != null)
        {
            monsterScript.player = playerObject.transform;
        }
        // 몬스터를 배회 상태로 활성화
        monsterScript.ActivateMonster();
    }
}
