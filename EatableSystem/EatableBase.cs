using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]
public enum EatableObjectClassification
{
    Dog, // ����
    Human, // �ΰ�
    Inanimate // ������
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
/// ���� �� �ִ� ��� ������Ʈ�� ���� ���̽� Ŭ����
/// �� ������Ʈ�� ��� �� ����, ���, ����Ʈ ���� ó���ϸ�
/// AchievementsManager�� �̺�Ʈ�� �����
/// </summary>
public abstract class EatableBase : MonoBehaviour, IEatable
{
    public EatableObjectData data;

    public bool isBeingEaten;
    public bool hasGoldReward;
    public bool isMale; //���� �� -> �Ҹ� �ٸ�
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
        //�������� �Դ� ��� �̵���Ű��
        //PlayerMouth -> OntriggerEnter �� ���� Mouth ���޹���
        if (!isBeingEaten)
        {
            return;
        }

        transform.localPosition = playerMouthCollider.center + gettingEatenPositionOffset;
        transform.localRotation = Quaternion.Euler(gettingEatenRotation);
    }

    //ĳ�ñ׸��� ��ų GoldRushSkill���� ȣ��    
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
        //����, �ù� -> ���� �浹 ����
        if (other.gameObject.CompareTag("EatableObject"))
        {
            Physics.IgnoreCollision(transform.GetComponent<Collider>(), other);
        }
    }

    /// <summary>
    /// ��� ������ �Ϲ� ���� �� ����� ���赵 ���� ���� �̺�Ʈ�� ����.
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
        //�׸��� Ȱ��ȭ - �ù�, ������
        Transform shadowTr = transform.Find("Shadow");
        if (shadowTr != null)
        {
            shadowTr.gameObject.SetActive(true);
        }
    }

    public void InitNavMeshAgent()
    {
        //NavMeshAgent �ʱ�ȭ
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
        //����Ʈ �̺�Ʈ ������ add
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
        //����Ʈ �̺�Ʈ ������ remove
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
    /// ������Ʈ�� ������ �ִϸ��̼� �� ���¸� ����.
    /// HP/EXP ����, ����Ʈ �̺�Ʈ ����, �ִϸ��̼� ���� �� ����.
    /// </summary>
    public void StartDeathSequence()
    {
        // �״� ���� - HP�� 0�� �� TakeDamage���� ȣ�� 
        player.GainHP(data.hpRewarded);
        player.GainEXP(data.expRewarded);

        if (transform.TryGetComponent(out Collider collider))
        {
            collider.enabled = false;
        }

        HideOutline();
        transform.localScale = gettingEatenScale; //������ ��ü ������ ����
        OnObjectOfTypeEaten.Invoke(objectType); //����Ʈ, �̼� �̺�Ʈ ����
        FirstRespond(); //->�߻� Ŭ���� ��ӹ��� ���� Ŭ���� �̺�Ʈ ����
        ObjectEatenAnimation(); 
        SetEatenObjectPosition(); 
        SetObjectEatenBool(); 
    }

    /// <summary>
    /// �Ա� �ִϸ��̼��� ���� ����(�ִϸ��̼� �̺�Ʈ)�� ȣ���
    /// ����Ʈ ó��, ���� �ݿ�, ����Ʈ �̺�Ʈ, ���� ���Ǿ� ����, ������Ʈ ��Ȱ��ȭ�� ����
    /// Skill�� ���� �׾����� ���ο� ���� ����Ʈ ������ �Ű������� ����
    /// </summary>
    /// <param name="isInPlayerMouth">������Ʈ�� �÷��̾� �� �ȿ� �ִ� �������� ����</param>
    /// <param name="healingSphereColor">��ų�� �׾��� ��� ������ ���� ����Ʈ�� ����</param>
    public void DestroyEatableObject(bool isInPlayerMouth, Color healingSphereColor)
    {
        //Player Eating �ִϸ��̼ǰ� ����ȭ

        Debug.Log("Finished eating object");

        ReturnEatenObjectPosition(); //����

        if (transform.TryGetComponent(out Collider collider))
        {
            collider.enabled = true;
        }

        HandleDeathBySkill(isInPlayerMouth, healingSphereColor); //��ų�� ���� �׾��� ��� ����Ʈ ȿ�� �ߵ�
        SecondRespond(); //��ӹ��� Ŭ�������� ������
        NotifyEatableObjectDeath(); //���ھ� ���
        gameObject.SetActive(false); // ���ֱ�
    }

    private void SetObjectEatenBool()
    {
        //isBeingSucked = false; //ĳ�ñ׸��� üũ
        isBeingEaten = true;
    }    

    private void SetEatenObjectPosition()
    {
        //������ �� ��ü ��ġ �巡�� ������ ����
        originalParent = transform.parent;
        transform.SetParent(playerMouthCollider.transform);

        //Agent�� NavMesh���� ����� �� ���ֱ�
        if (transform.TryGetComponent(out NavMeshAgent agent) && agent.isOnNavMesh)
        {
            Debug.Log($"Is agent active: {agent.enabled}");
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.isStopped = true;
        }

        //�巡���� ������ rotation���� ���� ����
        transform.localPosition = playerMouthCollider.center + gettingEatenPositionOffset;
        transform.localRotation = Quaternion.Euler(gettingEatenRotation);
    }

    private void ReturnEatenObjectPosition()
    {
        transform.SetParent(originalParent); //Ǯ�� �ٽ� �ֱ�
    }

    public void ShowGreenOutline()
    {
        //�ʷϻ� �ƿ����� ���̴� Ȱ��ȭ
        Outline outline = transform.GetComponent<Outline>();
        outline.OutlineColor = Color.green;
        outline.enabled = true;
    }

    public void HideOutline()
    {
        //�ƿ����� ���̴� ��Ȱ��ȭ
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
    /// ������Ʈ�� ������ ù ������ �߻��ϴ� ������ �����ϴ� �߻� �޼���.
    /// ���, �׸��� ����, ���� ó�� �� ���� ������Ʈ�� �°� �������̵��.
    /// </summary>
    protected abstract void FirstRespond();

    /// <summary>
    /// �Դ� �ִϸ��̼� ���� ������ �߻��ϴ� ��ó�� �̺�Ʈ.
    /// ���� VFX, ���� �� �� ������Ʈ�� �ð��� ���� ����.
    /// </summary>
    protected abstract void SecondRespond();
    #endregion
}
