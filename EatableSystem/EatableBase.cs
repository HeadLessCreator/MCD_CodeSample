using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]
public enum EatableObjectClassification
{
    Dog, // 동물
    Human, // 인간
    Inanimate // 무생물
}

[System.Serializable]
public enum EatableObjectType
{
    Unclassified,
    Civilian,
    Police,
    Car,
    PoliceCar,
    House,
    Apartment,
    Skyscraper,
    Hummer,
    APC,
    Tank,
    Helicopter,
    FighterJet,
    Tree,
    Bridge,
    TreasureChest,
    GoldKing,
    GoldPharaoh,
    MushroomCivilian,
    PresentCivilian
}

/// <summary>
/// 먹을 수 있는 모든 오브젝트의 공통 베이스 클래스
/// 각 오브젝트는 사망 시 점수, 골드, 이펙트 등을 처리하며
/// AchievementsManager와 이벤트로 통신함
/// </summary>
public abstract class EatableBase : MonoBehaviour, IEatable
{
    public EatableObjectData data;

    public bool isBeingEaten;
    public bool hasGoldReward;
    public bool isMale; //성별 비교 -> 소리 다름
    public EatableObjectType objectType;

    [SerializeField]
    private Vector3 gettingEatenPositionOffset;

    [SerializeField]
    private Vector3 gettingEatenRotation;

    [SerializeField]
    private Vector3 gettingEatenScale;

    public GameObject[] onEatenVFXOptions;

    [SerializeField]
    private Vector3 scoreVFXPositionOffset;

    [SerializeField]
    private float scoreUpdateTimeOffset;

    [SerializeField]
    private GameObject healingSpheresVFXPrefab;

    public UnityEvent<int, Vector3> OnScoreAwarded;
    public UnityEvent<float> OnGoldRushScoreAwarded;
    public UnityEvent<int, Vector3> OnGoldAwarded;
    public UnityEvent OnHumanKilled;
    public UnityEvent OnBuildingDestroyed;
    public UnityEvent OnVehicleDestroyed;
    public UnityEvent OnOtherObjectDestroyed;
    public UnityEvent<int> OnDangerPointsUpdated;
    public UnityEvent<EatableObjectType> OnObjectOfTypeEaten;

    public Player player;
    public BoxCollider playerMouthCollider;
    private Animator animator;
    private float gettingEatenTimer;
    private Transform originalParent;
    public bool isEatingDone;
    public bool isBeingSucked = false;
    public float magneticForceMoveTime;

    private bool isCoroutineRunning;

    private int currentHP;
    private float magneticForceTimer;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        RemoveEventListeners();
    }

    private void FixedUpdate()
    {
        MoveToPlayerMouth();
    }

    private void Init()
    {
        transform.GetComponent<Outline>().enabled = false;
        isBeingEaten = false;
        isBeingSucked = false;
        magneticForceTimer = 0f;
        isEatingDone = false;
        currentHP = data.maxHP;

        ActivateShadow();
        InitNavMeshAgent();
        AddEventListeners();
    }

    private void MoveToPlayerMouth()
    {
        //입쪽으로 먹는 대상 이동시키기
        //PlayerMouth -> OntriggerEnter 을 통해 Mouth 전달받음
        if (!isBeingEaten)
        {
            return;
        }

        transform.localPosition = playerMouthCollider.center + gettingEatenPositionOffset;
        transform.localRotation = Quaternion.Euler(gettingEatenRotation);
    }

    //캐시그리버 스킬 GoldRushSkill에서 호출    
    public void StartSuction(Vector3 mouthPosition, float duration)
    {     
        if (isBeingSucked == true) return;
        StartCoroutine(SuctionCoroutine(mouthPosition, duration));
    }

    private IEnumerator SuctionCoroutine(Vector3 targetPos, float duration)
    {
        isBeingSucked = true;
        float timer = 0f;
        Vector3 startPos = transform.position;

        while (timer < duration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        isBeingSucked = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //차량, 시민 -> 서로 충돌 무시
        if (other.gameObject.CompareTag("EatableObject"))
        {
            Physics.IgnoreCollision(transform.GetComponent<Collider>(), other);
        }
    }

    /// <summary>
    /// 골드 러쉬나 일반 점수 등 보상과 위험도 레벨 등을 이벤트로 발행.
    /// </summary>
    public void NotifyEatableObjectDeath()
    {
        OnDangerPointsUpdated.Invoke(data.dangerPointsAwarded);
        OnScoreAwarded.Invoke(data.scoreRewarded, transform.TransformPoint(scoreVFXPositionOffset));
        OnGoldRushScoreAwarded.Invoke(data.goldRushPointsAwarded);

        if (hasGoldReward)
        {
            OnGoldAwarded.Invoke(data.goldRewarded, transform.position);
        }
    }

    public void HandleDeathBySkill(bool isInPlayerMouth, Color healingSphereColor)
    {
        if (!isInPlayerMouth)
        {
            player.GainHP(data.hpRewarded);
            player.GainEXP(data.expRewarded);

            HealingSpheresController healingSpheresController = Instantiate(healingSpheresVFXPrefab, transform.position, Quaternion.identity).GetComponent<HealingSpheresController>();
            healingSpheresController.Init(player, data.hpRewarded, data.expRewarded, healingSphereColor);
            healingSpheresController.MoveHealingSpheresToPlayer();
        }
    }

    public void ActivateShadow()
    {
        //그림자 활성화 - 시민, 강아지
        Transform shadowTr = transform.Find("Shadow");
        if (shadowTr != null)
        {
            shadowTr.gameObject.SetActive(true);
        }
    }

    public void InitNavMeshAgent()
    {
        //NavMeshAgent 초기화
        if (transform.TryGetComponent(out NavMeshAgent agent))
        {
            if (agent.isOnNavMesh)
            {
                agent.updatePosition = true;
                agent.updateRotation = true;
                agent.nextPosition = transform.position;
                agent.isStopped = false;
            }
        }
    }

    public void AddEventListeners()
    {
        //퀘스트 이벤트 리스너 add
        if (AchievementsManager.Instance != null)
        {
            OnHumanKilled.AddListener(AchievementsManager.Instance.IncreaseNumberOfHumansKilled);
            OnBuildingDestroyed.AddListener(AchievementsManager.Instance.IncreaseNumberOfBuildingsDestroyed);
            OnVehicleDestroyed.AddListener(AchievementsManager.Instance.IncreaseNumberOfVehiclesDestroyed);
            OnOtherObjectDestroyed.AddListener(AchievementsManager.Instance.IncreaseNumberOfOtherObjectsDestroyed);
            OnObjectOfTypeEaten.AddListener(AchievementsManager.Instance.IncrementAchievementsOfEatableObjectType);
        }
    }

    public void RemoveEventListeners()
    {
        //퀘스트 이벤트 리스너 remove
        if (AchievementsManager.Instance != null)
        {
            OnHumanKilled.RemoveListener(AchievementsManager.Instance.IncreaseNumberOfHumansKilled);
            OnBuildingDestroyed.RemoveListener(AchievementsManager.Instance.IncreaseNumberOfBuildingsDestroyed);
            OnVehicleDestroyed.RemoveListener(AchievementsManager.Instance.IncreaseNumberOfVehiclesDestroyed);
            OnOtherObjectDestroyed.RemoveListener(AchievementsManager.Instance.IncreaseNumberOfOtherObjectsDestroyed);
            OnObjectOfTypeEaten.RemoveListener(AchievementsManager.Instance.IncrementAchievementsOfEatableObjectType);
        }
    }

    public bool IsHPLeft()
    {
        return currentHP > 0;
    }

    /// <summary>
    /// 오브젝트가 먹히는 애니메이션 및 상태를 시작.
    /// HP/EXP 보상, 퀘스트 이벤트 발행, 애니메이션 실행 등 포함.
    /// </summary>
    public void StartDeathSequence()
    {
        // 죽는 로직 - HP가 0일 때 TakeDamage에서 호출 
        player.GainHP(data.hpRewarded);
        player.GainEXP(data.expRewarded);

        if (transform.TryGetComponent(out Collider collider))
        {
            collider.enabled = false;
        }

        HideOutline();
        transform.localScale = gettingEatenScale; //먹히는 물체 사이즈 조절
        OnObjectOfTypeEaten.Invoke(objectType); //퀘스트, 미션 이벤트 실행
        FirstRespond(); //->추상 클래스 상속받은 개별 클래스 이벤트 실행
        ObjectEatenAnimation(); 
        SetEatenObjectPosition(); 
        SetObjectEatenBool(); 
    }

    /// <summary>
    /// 먹기 애니메이션이 끝난 시점(애니메이션 이벤트)에 호출됨
    /// 이펙트 처리, 점수 반영, 퀘스트 이벤트, 힐링 스피어 생성, 오브젝트 비활성화를 수행
    /// Skill에 의해 죽었는지 여부와 힐링 이펙트 색상을 매개변수로 받음
    /// </summary>
    /// <param name="isInPlayerMouth">오브젝트가 플레이어 입 안에 있는 상태인지 여부</param>
    /// <param name="healingSphereColor">스킬로 죽었을 경우 생성할 힐링 이펙트의 색상</param>
    public void DestroyEatableObject(bool isInPlayerMouth, Color healingSphereColor)
    {
        //Player Eating 애니메이션과 동기화

        Debug.Log("Finished eating object");

        ReturnEatenObjectPosition(); //공용

        if (transform.TryGetComponent(out Collider collider))
        {
            collider.enabled = true;
        }

        HandleDeathBySkill(isInPlayerMouth, healingSphereColor); //스킬에 의해 죽었을 경우 이펙트 효과 발동
        SecondRespond(); //상속받은 클래스에서 구현됨
        NotifyEatableObjectDeath(); //스코어 계산
        gameObject.SetActive(false); // 꺼주기
    }

    private void SetObjectEatenBool()
    {
        //isBeingSucked = false; //캐시그리버 체크
        isBeingEaten = true;
    }    

    private void SetEatenObjectPosition()
    {
        //먹혔을 때 객체 위치 드래곤 하위로 조정
        originalParent = transform.parent;
        transform.SetParent(playerMouthCollider.transform);

        //Agent가 NavMesh에서 벗어났을 때 꺼주기
        if (transform.TryGetComponent(out NavMeshAgent agent) && agent.isOnNavMesh)
        {
            Debug.Log($"Is agent active: {agent.enabled}");
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.isStopped = true;
        }

        //드래곤의 포지션 rotation값에 맞춰 조절
        transform.localPosition = playerMouthCollider.center + gettingEatenPositionOffset;
        transform.localRotation = Quaternion.Euler(gettingEatenRotation);
    }

    private void ReturnEatenObjectPosition()
    {
        transform.SetParent(originalParent); //풀에 다시 넣기
    }

    public void ShowGreenOutline()
    {
        //초록색 아웃라인 쉐이더 활성화
        Outline outline = transform.GetComponent<Outline>();
        outline.OutlineColor = Color.green;
        outline.enabled = true;
    }

    public void HideOutline()
    {
        //아웃라인 쉐이더 비활성화
        Outline outline = transform.GetComponent<Outline>();
        outline.enabled = false;
    }

    public void ObjectEatenAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("IsGettingEaten", true);
        }
    }

    #region interface/abstract class
    public void OnEaten(int damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            StartDeathSequence();
        }
    }

    /// <summary>
    /// 오브젝트가 먹히는 첫 순간에 발생하는 반응을 정의하는 추상 메서드.
    /// 비명, 그림자 제거, 업적 처리 등 개별 오브젝트에 맞게 오버라이드됨.
    /// </summary>
    protected abstract void FirstRespond();

    /// <summary>
    /// 먹는 애니메이션 종료 시점에 발생하는 후처리 이벤트.
    /// 폭죽 VFX, 사운드 등 각 오브젝트별 시각적 연출 구현.
    /// </summary>
    protected abstract void SecondRespond();
    #endregion
}
