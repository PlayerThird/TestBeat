using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnitState))]
public class PlayerCombat : MonoBehaviour, IDamagable<DamageObject>
{
    [Header("Linked Components")] public Transform weaponBone; //кость, от которой будет создано оружие
    private UnitAnimator animator; //ссылка на компонент аниматора
    private UnitState playerState; //состояние игрока
    private Rigidbody rb;

    [Header("Attack Data & Combos")] public float hitZRange = 2f; //диапозон атаки по Z
    private int attackNum = -1; //текущий номер комбо атаки
    [Space(5)] public List<DamageObject> AnyCombo; //список из кастомных комбо
    [SerializeField] private int maxValCombo; //Максимальный индекс комбо в списке комбо-атак
    public DamageObject JumpKickData; //атака в прыжке с ноги
    public DamageObject GroundPunchData; //Наземная атака с руки
    public DamageObject GroundKickData; //Наземная атака с ноги
    public DamageObject RunningPunch; //анимация удара руки с разбега
    public DamageObject RunningKick; //анимация удара ноги с разбега
    private DamageObject lastAttack; //данные о последней произошедшей атаке

    [Header("Settings")] public bool blockAttacksFromBehind = false; //блокировка атаки противника сзади
    public bool comboContinueOnHit = true; //продолжение комбо только тогда, когда предыдущая атака была успешной
    public bool resetComboChainOnChangeCombo; //перезапустить комбо при переключении на другую цепочку комбо
    public bool invulnerableDuringJump = false; //проверка, можно ли ударить игрока во время прыжка
    public float hitRecoveryTime = .4f; //время, необходимое для восстановления после удара
    public float hitThreshold = .2f; //время, прежде чем нас снова смогут ударить
    public float hitKnockBackForce = 1.5f; //сила отбрасывания, когда нас бьют

    public float
        GroundAttackDistance = 1.5f; //расстояние от противника, на котором может быть осуществлена наземная атака

    public int knockdownHitCount = 3; //число, сколько раз игрока можно ударить, прежде чем он будет сбит с ног
    public float KnockdownTimeout = 0; //время, прежде чем мы встанем после нокдауна
    public float KnockdownUpForce = 5; //Up сила нокдауна
    public float KnockbackForce = 4; //горизонтальная сила нокдауна
    public float KnockdownStandUpTime = .8f; //время, необходимое для завершения анимации вставания

    [Header("Audio")] public string knockdownVoiceSFX = "";
    public string hitVoiceSFX = "";
    public string deathVoiceSFX = "";
    public string defenceHitSFX = "";
    public string dropSFX = "";

    [Header("Stats")] public DIRECTION currentDirection; //текущее направление
    public GameObject itemInRange; //предмет, который в настоящее время находится в диапазоне взаимодействия
    private Weapon currentWeapon; //текущее оружие, которое держит игрок
    private DIRECTION defendDirection; //направление во время защиты
    private bool continueCombo; //true, если комбинацию ударов нужно продолжать
    private float lastAttackTime = 0; //время, последней атаки
    [SerializeField] private bool targetHit; //true, если последний удар поразил цель
    private int hitKnockDownCount = 0; //количество ударов по игроку подряд
    private int hitKnockDownResetTime = 2; //время до сброса счетчика попаданий по игроку
    private float LastHitTime = 0; //время, последний раз когда нас ударили
    private bool isDead = false; //true, если игрок умер
    private int EnemyLayer; //слой, врагов
    private int DestroyableObjectLayer; //слой, уничтожаемых предметов
    private int EnvironmentLayer; //слой, окружения
    private LayerMask HitLayerMask; //список, всех попадаемых объектов
    private bool isGrounded;
    private Vector3 fixedVelocity;
    private bool updateVelocity;
    private string lastAttackInput;

    private DIRECTION lastAttackDirection;

    //Эксперимент
    private List<string> inputBuffer = new List<string>();
    private float inputBufferTime = 0.5f; // Время, в течение которого сохраняются нажатия
    private ComboManager comboManager;
    public ComboList comboList;
    private float nextTime;
    private bool timeLastAttack;


    //список состояний, когда игрок может атаковать
    private List<UNITSTATE> AttackStates = new List<UNITSTATE>
    {
        UNITSTATE.IDLE,
        UNITSTATE.WALK,
        UNITSTATE.RUN,
        UNITSTATE.JUMPING,
        UNITSTATE.PUNCH,
        UNITSTATE.KICK,
        UNITSTATE.DEFEND,
    };

    //список состояний, когда игрок может получить урон
    private List<UNITSTATE> HitableStates = new List<UNITSTATE>
    {
        UNITSTATE.DEFEND,
        UNITSTATE.HIT,
        UNITSTATE.IDLE,
        UNITSTATE.LAND,
        UNITSTATE.PUNCH,
        UNITSTATE.KICK,
        UNITSTATE.THROW,
        UNITSTATE.WALK,
        UNITSTATE.RUN,
        UNITSTATE.GROUNDKICK,
        UNITSTATE.GROUNDPUNCH,
    };

    //список состояний, когда игрок может использовать блок
    private List<UNITSTATE> DefendStates = new List<UNITSTATE>
    {
        UNITSTATE.IDLE,
        UNITSTATE.DEFEND,
        UNITSTATE.WALK,
        UNITSTATE.RUN,
    };

    //список состояний, в которых игрок может изменить направление
    private List<UNITSTATE> MovementStates = new List<UNITSTATE>
    {
        UNITSTATE.IDLE,
        UNITSTATE.WALK,
        UNITSTATE.RUN,
        UNITSTATE.JUMPING,
        UNITSTATE.JUMPKICK,
        UNITSTATE.LAND,
        UNITSTATE.DEFEND,
    };

    //---

    void OnEnable()
    {
        InputManager.onInputEvent += OnInputEvent;
        InputManager.onDirectionInputEvent += OnDirectionInputEvent;
    }

    void OnDisable()
    {
        InputManager.onInputEvent -= OnInputEvent;
        InputManager.onDirectionInputEvent -= OnDirectionInputEvent;
    }

    //awake
    void Start()
    {
        animator = GetComponentInChildren<UnitAnimator>();
        playerState = GetComponent<UnitState>();
        rb = GetComponent<Rigidbody>();

        //назначить слои и маски слоев
        EnemyLayer = LayerMask.NameToLayer("Enemy");
        DestroyableObjectLayer = LayerMask.NameToLayer("DestroyableObject");
        EnvironmentLayer = LayerMask.NameToLayer("Environment");
        HitLayerMask = (1 << EnemyLayer) | (1 << DestroyableObjectLayer);

        //отобразить сообщения об ошибках для отсутствующих компонентов
        if (!animator) Debug.LogError("No player animator found inside " + gameObject.name);
        if (!playerState) Debug.LogError("No playerState component found on " + gameObject.name);
        if (!rb) Debug.LogError("No rigidbody component found on " + gameObject.name);

        //сделать неуязвимым во время прыжка
        if (!invulnerableDuringJump)
        {
            HitableStates.Add(UNITSTATE.JUMPING);
            HitableStates.Add(UNITSTATE.JUMPKICK);
        }

        //Доделать чтобы список доступных комбо выпадал, или несколько комбо в одну записать
//        comboList.LoadFromJson("Resources/ComboLists/FirstComboList.json");

        foreach (var obj in AnyCombo)
        {
            if (obj.numInCombo > maxValCombo)
            {
                maxValCombo = obj.numInCombo;
                Debug.Log("MaxVal= " + maxValCombo);
            }
        }
    }

    void Update()
    {
        //проверка, игрок сталкивается с землей
        if (animator) isGrounded = animator.animator.GetBool("isGrounded");

        //обновлять состояние каждый кадр
        Defend(InputManager.defendKeyDown);
        timeLastAttack = Time.time >= nextTime;
    }

    //обновление физики
    void FixedUpdate()
    {
        if (updateVelocity)
        {
            rb.velocity = fixedVelocity;
            updateVelocity = false;
        }
    }

    //late Update
    void LateUpdate()
    {
        //применить любые смещения корневого движения к родителю
        if (animator && animator.GetComponent<Animator>().applyRootMotion &&
            animator.transform.localPosition != Vector3.zero)
        {
            Vector3 offset = animator.transform.localPosition;
            animator.transform.localPosition = Vector3.zero;
            transform.position += offset * -(int)currentDirection;
        }
    }

    //установите скорость в следующем FixedUpdate
    void SetVelocity(Vector3 velocity)
    {
        fixedVelocity = velocity;
        updateVelocity = true;
    }

    //движения input event
    void OnDirectionInputEvent(Vector2 inputVector, bool doubleTapActive)
    {
        if (!MovementStates.Contains(playerState.currentState)) return;
        int dir = Mathf.RoundToInt(Mathf.Sign((float)-inputVector.x));
        if (Mathf.Abs(inputVector.x) > 0) currentDirection = (DIRECTION)dir;
    }

    #region Combat Input Events

    //combat input event
    private void OnInputEvent(string action, BUTTONSTATE buttonState)
    {
        if (AttackStates.Contains(playerState.currentState) && !isDead && timeLastAttack)
        {
            //удар рукой при беге
            if (action == "Punch" && buttonState == BUTTONSTATE.PRESS && playerState.currentState == UNITSTATE.RUN &&
                isGrounded)
            {
                animator.SetAnimatorBool("Run", false);
                if (RunningPunch.animTrigger.Length > 0) doAttack(RunningPunch, UNITSTATE.ATTACK, "Punch");
                return;
            }

            //удар ногой при беге
            if (action == "Kick" && buttonState == BUTTONSTATE.PRESS && playerState.currentState == UNITSTATE.RUN &&
                isGrounded)
            {
                animator.SetAnimatorBool("Run", false);
                if (RunningKick.animTrigger.Length > 0) doAttack(RunningKick, UNITSTATE.ATTACK, "Kick");
                return;
            }

            //подобрать предмет
            if (action == "Punch" && buttonState == BUTTONSTATE.PRESS && itemInRange != null && isGrounded &&
                currentWeapon == null)
            {
                interactWithItem();
                return;
            }

            //использовать предмет
            if (action == "Punch" && buttonState == BUTTONSTATE.PRESS && isGrounded && currentWeapon != null)
            {
                useCurrentWeapon();
                return;
            }

            //удар с руки на землю
            if (action == "Punch" && buttonState == BUTTONSTATE.PRESS &&
                (playerState.currentState != UNITSTATE.PUNCH && NearbyEnemyDown()) && isGrounded)
            {
                if (GroundPunchData.animTrigger.Length > 0) doAttack(GroundPunchData, UNITSTATE.GROUNDPUNCH, "Punch");
                return;
            }

            //удар ноги на землю
            if (action == "Kick" && buttonState == BUTTONSTATE.PRESS &&
                (playerState.currentState != UNITSTATE.KICK && NearbyEnemyDown()) && isGrounded)
            {
                if (GroundKickData.animTrigger.Length > 0) doAttack(GroundKickData, UNITSTATE.GROUNDKICK, "Kick");
                return;
            }

            //удар руки или ноги в прыжке
            if (action == "Punch" || action == "Kick" && playerState.currentState == UNITSTATE.JUMPING &&
                buttonState == BUTTONSTATE.PRESS && !isGrounded)
            {
                if (JumpKickData.animTrigger.Length > 0)
                {
                    doAttack(JumpKickData, UNITSTATE.JUMPKICK, "Kick");
                    StartCoroutine(JumpKickInProgress());
                }

                return;
            }

            //кастомные комбо - атаки
            if (action == "Punch" || action == "Kick" && buttonState == BUTTONSTATE.PRESS && isGrounded)
            {
                //перейдите к следующей атаке, если время находится внутри комбо-окна
                bool insideComboWindow = (lastAttack != null &&
                                          (Time.time <
                                           (lastAttackTime + lastAttack.duration + lastAttack.comboResetTime)));
                if (insideComboWindow && !continueCombo && (attackNum < maxValCombo))
                {
                    attackNum += 1;
                }
                else
                {
                    attackNum = 0;
                }

                if (AnyCombo != null)
                {
                    DamageObject obj =
                        AnyCombo.FirstOrDefault(obj => obj.numInCombo == attackNum && obj.GetAtackState() == action);
                    if (!(obj.animTrigger.Length > 0))
                    {
                        Debug.Log("Animation no found or some error in:" + obj.actionTrigger);
                    }

                    doAttack(obj, obj.playerState, obj.GetAtackState());
                    return;
                }
            }

            nextTime = lastAttack != null ? Time.time + lastAttack.duration : 0;
        }
    }

    #endregion

    #region Combat functions

    private void doAttack(DamageObject damageObject, UNITSTATE state, string inputAction)
    {
        animator.SetAnimatorTrigger(damageObject.animTrigger);
        playerState.SetState(state);

        //сохранить данные об атаке
        lastAttack = damageObject;
        lastAttack.inflictor = gameObject;
        lastAttackTime = Time.time;
        lastAttackInput = inputAction;
        lastAttackDirection = currentDirection;

        //повернуть в сторону текущего направления ввода
        TurnToDir(currentDirection);

        if (isGrounded) SetVelocity(Vector3.zero);
        if (damageObject.forwardForce > 0) animator.AddForce(damageObject.forwardForce);

        if (state == UNITSTATE.JUMPKICK) return;
        Invoke("Ready", damageObject.duration);
    }

    //использовать текущее экипированное оружие
    void useCurrentWeapon()
    {
        playerState.SetState(UNITSTATE.USEWEAPON);
        TurnToDir(currentDirection);
        SetVelocity(Vector3.zero);

        //сохранить информацию об атаке
        lastAttackInput = "WeaponAttack";
        lastAttackTime = Time.time;
        lastAttack = currentWeapon.damageObject;
        lastAttack.inflictor = gameObject;
        lastAttackDirection = currentDirection;

        if (!string.IsNullOrEmpty(currentWeapon.damageObject.animTrigger))
            animator.SetAnimatorTrigger(currentWeapon.damageObject.animTrigger);
        if (!string.IsNullOrEmpty(currentWeapon.useSound)) GlobalAudioPlayer.PlaySFX(currentWeapon.useSound);
        Invoke("Ready", currentWeapon.damageObject.duration);

        //"деградация" оружия
        if (currentWeapon.degenerateType == DEGENERATETYPE.DEGENERATEONUSE) currentWeapon.useWeapon();
        if (currentWeapon.degenerateType == DEGENERATETYPE.DEGENERATEONUSE && currentWeapon.timesToUse == 0)
            StartCoroutine(destroyCurrentWeapon(currentWeapon.damageObject.duration));
        if (currentWeapon.degenerateType == DEGENERATETYPE.DEGENERATEONHIT && currentWeapon.timesToUse == 1)
            StartCoroutine(destroyCurrentWeapon(currentWeapon.damageObject.duration));
    }

    //удалить текущее оружие
    IEnumerator destroyCurrentWeapon(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (currentWeapon.degenerateType == DEGENERATETYPE.DEGENERATEONUSE)
            GlobalAudioPlayer.PlaySFX(currentWeapon.breakSound);
        Destroy(currentWeapon.playerHandPrefab);
        currentWeapon.BreakWeapon();
        currentWeapon = null;
    }

    //возвращает текущее оружие
    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    //удар в прыжке в процессе
    IEnumerator JumpKickInProgress()
    {
        animator.SetAnimatorBool("JumpKickActive", true);

        //список врагов, которых мы ударили
        List<GameObject> enemieshit = new List<GameObject>();

        //небольшая задержка, чтобы анимация успела воспроизвестися
        yield return new WaitForSeconds(.1f);

        //проверка на удар
        while (playerState.currentState == UNITSTATE.JUMPKICK)
        {
            //рисуем хитбокс перед персонажем, чтобы увидеть, с какими объектами он сталкивается
            Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight) +
                                  Vector3.right * ((int)currentDirection * lastAttack.collDistance);
            Vector3 boxSize = new Vector3(lastAttack.CollSize / 2, lastAttack.CollSize / 2, hitZRange / 2);
            Collider[] hitColliders = Physics.OverlapBox(boxPosition, boxSize, Quaternion.identity, HitLayerMask);

            //бьём врага, который есть в списке врагов
            foreach (Collider col in hitColliders)
            {
                if (!enemieshit.Contains(col.gameObject))
                {
                    enemieshit.Add(col.gameObject);

                    //бьем "ударяемые" обьекты
                    IDamagable<DamageObject> damagableObject =
                        col.GetComponent(typeof(IDamagable<DamageObject>)) as IDamagable<DamageObject>;
                    if (damagableObject != null)
                    {
                        damagableObject.Hit(lastAttack);

                        //качание камеры
                        CamShake camShake = Camera.main.GetComponent<CamShake>();
                        if (camShake != null) camShake.Shake(.1f);
                    }
                }
            }

            yield return null;
        }
    }

    //вкл/выкл защиту
    private void Defend(bool defend)
    {
        if (!DefendStates.Contains(playerState.currentState)) return;
        animator.SetAnimatorBool("Defend", defend);
        if (defend)
        {
            TurnToDir(currentDirection);
            SetVelocity(Vector3.zero);
            playerState.SetState(UNITSTATE.DEFEND);
            animator.SetAnimatorBool("Run", false); //выключить бег
        }
        else
        {
            if (playerState.currentState == UNITSTATE.DEFEND) playerState.SetState(UNITSTATE.IDLE);
        }
    }

    #endregion

    #region Check For Hit

    //проерка, ударяем ли мы, что-то(кого-то) (Animation Event)
    public void CheckForHit()
    {
        //рисуем хитбокс перед персонажем, чтобы увидеть, с какими объектами он сталкивается
        Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight) +
                              Vector3.right * ((int)lastAttackDirection * lastAttack.collDistance);
        Vector3 boxSize = new Vector3(lastAttack.CollSize / 2, lastAttack.CollSize / 2, hitZRange / 2);
        Collider[] hitColliders = Physics.OverlapBox(boxPosition, boxSize, Quaternion.identity, HitLayerMask);

        int i = 0;
        while (i < hitColliders.Length)
        {
            //бьем "ударяемые" обьекты
            IDamagable<DamageObject> damagableObject =
                hitColliders[i].GetComponent(typeof(IDamagable<DamageObject>)) as IDamagable<DamageObject>;
            if (damagableObject != null)
            {
                damagableObject.Hit(lastAttack);

                //мы ударяем что-то(кого-то)
                targetHit = true;
            }

            i++;
        }

        //ничего не ударили
        if (hitColliders.Length == 0) targetHit = false;

        //при попадании оружия
        if (lastAttackInput == "WeaponAttack" && targetHit) currentWeapon.onHitSomething();
    }

    //Показ хит-бокса в Unity Editor (Debug)
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (lastAttack != null && (Time.time - lastAttackTime) < lastAttack.duration)
        {
            Gizmos.color = Color.red;
            Vector3 boxPosition = transform.position + (Vector3.up * lastAttack.collHeight) +
                                  Vector3.right * ((int)lastAttackDirection * lastAttack.collDistance);
            Vector3 boxSize = new Vector3(lastAttack.CollSize, lastAttack.CollSize, hitZRange);
            Gizmos.DrawWireCube(boxPosition, boxSize);
        }
    }
#endif

    #endregion

    #region We Are Hit

    //мы получили удар
    public void Hit(DamageObject d)
    {
        //проверь, сможем ли мы получить удар еще раз
        if (Time.time < LastHitTime + hitThreshold) return;

        //проверь, находимся ли мы в состоянии попадания
        if (HitableStates.Contains(playerState.currentState))
        {
            CancelInvoke();

            //тряска камеры
            CamShake camShake = Camera.main.GetComponent<CamShake>();
            if (camShake != null) camShake.Shake(.1f);

            //защита от приближающейся атаки сзади
            if (playerState.currentState == UNITSTATE.DEFEND && !d.DefenceOverride &&
                (isFacingTarget(d.inflictor) || blockAttacksFromBehind))
            {
                Defend(d);
                return;
            }
            else
            {
                animator.SetAnimatorBool("Defend", false);
            }

            //нас ударили
            UpdateHitCounter();
            LastHitTime = Time.time;

            //показать эффект удара
            animator.ShowHitEffect();

            //вычитываем здоровье
            HealthSystem healthSystem = GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.SubstractHealth(d.damage);
                if (healthSystem.CurrentHp == 0)
                    return;
            }

            //проверяем на knockdown
            if ((hitKnockDownCount >= knockdownHitCount || !IsGrounded() || d.knockDown) &&
                playerState.currentState != UNITSTATE.KNOCKDOWN)
            {
                hitKnockDownCount = 0;
                StopCoroutine("KnockDownSequence");
                StartCoroutine("KnockDownSequence", d.inflictor);
                GlobalAudioPlayer.PlaySFXAtPosition(d.hitSFX, transform.position + Vector3.up);
                GlobalAudioPlayer.PlaySFXAtPosition(knockdownVoiceSFX, transform.position + Vector3.up);
                return;
            }

            //стандартный удар
            int i = Random.Range(1, 3);
            animator.SetAnimatorTrigger("Hit" + i);
            SetVelocity(Vector3.zero);
            playerState.SetState(UNITSTATE.HIT);

            //добавили маленькую силу для импакта
            if (isFacingTarget(d.inflictor))
            {
                animator.AddForce(-1.5f);
            }
            else
            {
                animator.AddForce(1.5f);
            }

            //SFX
            GlobalAudioPlayer.PlaySFXAtPosition(d.hitSFX, transform.position + Vector3.up);
            GlobalAudioPlayer.PlaySFXAtPosition(hitVoiceSFX, transform.position + Vector3.up);

            Invoke("Ready", hitRecoveryTime);
        }
    }

    //обновить счётчик ударов
    void UpdateHitCounter()
    {
        if (Time.time - LastHitTime < hitKnockDownResetTime)
        {
            hitKnockDownCount += 1;
        }
        else
        {
            hitKnockDownCount = 1;
        }

        LastHitTime = Time.time;
    }

    //защита от приближающейся атаки
    void Defend(DamageObject d)
    {
        //показать эфект защиты
        animator.ShowDefendEffect();

        //сыграть sfx
        GlobalAudioPlayer.PlaySFXAtPosition(defenceHitSFX, transform.position + Vector3.up);

        //добавили маленькую силу для импакта
        if (isFacingTarget(d.inflictor))
        {
            animator.AddForce(-hitKnockBackForce);
        }
        else
        {
            animator.AddForce(hitKnockBackForce);
        }
    }

    #endregion

    #region Item interaction

    //предмет в радиусе действия
    public void ItemInRange(GameObject item)
    {
        itemInRange = item;
    }

    //предмет вне радиуса действия
    public void ItemOutOfRange(GameObject item)
    {
        if (itemInRange == item) itemInRange = null;
    }

    //взаимодействуем с предметом в радиусе действия
    public void interactWithItem()
    {
        if (itemInRange != null)
        {
            animator.SetAnimatorTrigger("Pickup");
            playerState.SetState(UNITSTATE.PICKUPITEM);
            SetVelocity(Vector3.zero);
            Invoke("Ready", .3f);
            Invoke("pickupItem", .2f);
        }
    }

    //подобрать предмет
    void pickupItem()
    {
        if (itemInRange != null)
            itemInRange.SendMessage("OnPickup", gameObject, SendMessageOptions.DontRequireReceiver);
    }

    //экипировать текущее оружие
    public void equipWeapon(Weapon weapon)
    {
        currentWeapon = weapon;
        currentWeapon.damageObject.inflictor = gameObject;

        //добавить игроку ручное оружие
        if (weapon.playerHandPrefab != null)
        {
            GameObject PlayerWeapon = GameObject.Instantiate(weapon.playerHandPrefab, weaponBone) as GameObject;
            currentWeapon.playerHandPrefab = PlayerWeapon;
        }
    }

    #endregion

    #region KnockDown Sequence

    //последовательность knockDown
    public IEnumerator KnockDownSequence(GameObject inflictor)
    {
        playerState.SetState(UNITSTATE.KNOCKDOWN);
        animator.StopAllCoroutines();
        yield return new WaitForFixedUpdate();

        //посмотрите в направлении приближающейся атаки
        int dir = inflictor.transform.position.x > transform.position.x ? 1 : -1;
        currentDirection = (DIRECTION)dir;
        TurnToDir(currentDirection);

        //обновить playermovement
        var pm = GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.CancelJump();
            pm.SetDirection(currentDirection);
        }

        //добавить силу отбрасывания
        animator.SetAnimatorTrigger("KnockDown_Up");
        while (IsGrounded())
        {
            SetVelocity(new Vector3(KnockbackForce * -dir, KnockdownUpForce, 0));
            yield return new WaitForFixedUpdate();
        }

        //направляемся вверх
        while (rb.velocity.y >= 0) yield return new WaitForFixedUpdate();

        //направляемся вниз
        animator.SetAnimatorTrigger("KnockDown_Down");
        while (!IsGrounded()) yield return new WaitForFixedUpdate();

        //ударились об землю
        animator.SetAnimatorTrigger("KnockDown_End");
        CamShake camShake = Camera.main.GetComponent<CamShake>();
        if (camShake != null) camShake.Shake(.3f);
        animator.ShowDustEffectLand();

        //sfx
        GlobalAudioPlayer.PlaySFXAtPosition(dropSFX, transform.position);

        //скольжение по земле
        float t = 0;
        float speed = 2;
        Vector3 fromVelocity = rb.velocity;
        while (t < 1)
        {
            SetVelocity(Vector3.Lerp(
                new Vector3(fromVelocity.x, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, fromVelocity.z),
                new Vector3(0, rb.velocity.y, 0), t));
            t += Time.deltaTime * speed;
            yield return null;
        }

        //таймаут knockDown
        SetVelocity(Vector3.zero);
        yield return new WaitForSeconds(KnockdownTimeout);

        //встаём
        animator.SetAnimatorTrigger("StandUp");
        playerState.currentState = UNITSTATE.STANDUP;

        yield return new WaitForSeconds(KnockdownStandUpTime);
        playerState.currentState = UNITSTATE.IDLE;
    }

    #endregion

    //возвращает true, если ближайший враг находится в "сбитый с ног" состоянии
    bool NearbyEnemyDown()
    {
        float distance = GroundAttackDistance;
        GameObject closestEnemy = null;
        foreach (GameObject enemy in EnemyManager.activeEnemies)
        {
            //праверка, на врагов перед нами
            if (isFacingTarget(enemy))
            {
                //ищем ближайшего противника
                float dist2enemy = (enemy.transform.position - transform.position).magnitude;
                if (dist2enemy < distance)
                {
                    distance = dist2enemy;
                    closestEnemy = enemy;
                }
            }
        }

        if (closestEnemy != null)
        {
            EnemyAI AI = closestEnemy.GetComponent<EnemyAI>();
            if (AI != null && AI.enemyState == UNITSTATE.KNOCKDOWNGROUNDED)
            {
                return true;
            }
        }

        return false;
    }

    //атака завершена и игрок готов к новым действиям
    public void Ready()
    {
        //продолжаем комбо только тогда, когда мы во что-то попали
        if (comboContinueOnHit && !targetHit)
        {
            continueCombo = false;
            lastAttackTime = 0;
        }

        playerState.SetState(UNITSTATE.IDLE);
    }

    //возвращает true, если игрок сталкивается с GameObject
    public bool isFacingTarget(GameObject g)
    {
        return ((g.transform.position.x > transform.position.x && currentDirection == DIRECTION.Left) ||
                (g.transform.position.x < transform.position.x && currentDirection == DIRECTION.Right));
    }

    //возвращает true, если игрок на земле
    public bool IsGrounded()
    {
        CapsuleCollider c = GetComponent<CapsuleCollider>();
        float colliderSize = c.bounds.extents.y;
#if UNITY_EDITOR
        Debug.DrawRay(transform.position + c.center, Vector3.down * colliderSize, Color.red);
#endif
        return Physics.Raycast(transform.position + c.center, Vector3.down, colliderSize + .1f, 1 << EnvironmentLayer);
    }

    //повернуться в сторону, куда идём
    public void TurnToDir(DIRECTION dir)
    {
        transform.rotation = Quaternion.LookRotation(Vector3.forward * -(int)dir);
    }

    //игрок умэр
    void Death()
    {
        if (!isDead)
        {
            isDead = true;
            StopAllCoroutines();
            animator.StopAllCoroutines();
            CancelInvoke();
            SetVelocity(Vector3.zero);
            GlobalAudioPlayer.PlaySFXAtPosition(deathVoiceSFX, transform.position + Vector3.up);
            animator.SetAnimatorBool("Death", true);
            EnemyManager.PlayerHasDied();
            StartCoroutine(ReStartLevel());
        }
    }

    //рестарт этого уровня
    IEnumerator ReStartLevel()
    {
        yield return new WaitForSeconds(2);
        float fadeoutTime = 1.3f;

        UIManager UI = GameObject.FindObjectOfType<UIManager>();
        if (UI != null)
        {
            //исчезание UI
            UI.UI_fader.Fade(UIFader.FADE.FadeOut, fadeoutTime, 0);
            yield return new WaitForSeconds(fadeoutTime);

            //показать "game over"
            UI.DisableAllScreens();
            UI.ShowMenu("GameOver");
        }
    }

// Отладка
    public float GetTimeCombo()
    {
        return Time.time - (lastAttackTime + lastAttack.duration + lastAttack.comboResetTime);
    }

    public float GetTime()
    {
        return Time.time;
    }

    public bool GetComboWindow()
    {
        return timeLastAttack;
    }
}