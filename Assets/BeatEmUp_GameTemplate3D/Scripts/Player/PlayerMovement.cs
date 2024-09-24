using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(UnitState))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Linked Components")] private UnitAnimator animator;
    private Rigidbody rb;
    private UnitState playerState;
    private CapsuleCollider capsule;

    [Header("Settings")] public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float ZSpeed = 1.5f;
    public float JumpForce = 8f;
    public bool AllowDepthJumping;
    public float AirAcceleration = 3f;
    public float AirMaxSpeed = 3f;
    public float rotationSpeed = 15f;
    public float jumpRotationSpeed = 30f;
    public float lookAheadDistance = .2f;
    public float landRecoveryTime = .1f;
    public float landTime = 0;
    public LayerMask CollisionLayer;

    [Header("Audio")] public string jumpUpVoice = "";
    public string jumpLandVoice = "";

    [Header("Stats")] public DIRECTION currentDirection;
    public Vector2 inputDirection;
    public bool jumpInProgress;
    public GameObject ladderNearby;
    public bool haveLadder;

    private bool isDead = false;
    private bool JumpNextFixedUpdate;
    private float jumpDownwardsForce = .3f;

    //спсиок состояний, когда можно передвигатся
    private List<UNITSTATE> MovementStates = new List<UNITSTATE>
    {
        UNITSTATE.IDLE,
        UNITSTATE.WALK,
        UNITSTATE.RUN,
        UNITSTATE.JUMPING,
        UNITSTATE.UPSTAIRS,
        UNITSTATE.JUMPKICK,
        UNITSTATE.LAND,
        UNITSTATE.DEFEND,
    };

    //--

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

    void Start()
    {
        //поиск компонентов префаба
        if (!animator) animator = GetComponentInChildren<UnitAnimator>();
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!playerState) playerState = GetComponent<UnitState>();
        if (!capsule) capsule = GetComponent<CapsuleCollider>();

        //сообщения об ошибках, если чего-то не хватает
        if (!animator) Debug.LogError("No animator found inside " + gameObject.name);
        if (!rb) Debug.LogError("No Rigidbody component found on " + gameObject.name);
        if (!playerState) Debug.LogError("No UnitState component found on " + gameObject.name);
        if (!capsule) Debug.LogError("No Capsule Collider found on " + gameObject.name);
    }

    void FixedUpdate()
    {
        if (!MovementStates.Contains(playerState.currentState) || isDead) return;

        //блок
        if (playerState.currentState == UNITSTATE.DEFEND)
        {
            TurnToCurrentDirection();
            return;
        }

        //начало прыжка
        if (JumpNextFixedUpdate)
        {
            Jump();
            return;
        }
        

        //призимление после прыжка
        if (jumpInProgress && IsGrounded())
        {
            HasLanded();
            return;
        }

        //короткое время после приземления(для анимаций)
        if (playerState.currentState == UNITSTATE.LAND && Time.time - landTime > landRecoveryTime)
            playerState.SetState(UNITSTATE.IDLE);

        //движение в воздухе и на земле
        bool isGrounded = IsGrounded();
        animator.SetAnimatorBool("isGrounded", isGrounded);
        if (isGrounded) animator.SetAnimatorBool("Falling", false);

        if (isGrounded)
        {
            MoveGrounded();
        }
        else
        {
            MoveAirborne();
        }


        //всегда поворачиваемся в сторону движения
        TurnToCurrentDirection();
    }

    //движение на земле
    void MoveGrounded()
    {
        //ничего не делаем, когда приземлились
        if (playerState.currentState == UNITSTATE.LAND) return;

        //двигаться, когда перед нами нет стены и input обнаружен
        if (rb != null && (inputDirection.sqrMagnitude > 0 && !WallInFront()))
        {
            //установить скорость движения на скорость бега или скорость ходьбы в зависимости от текущего состояния
            float movementSpeed = playerState.currentState == UNITSTATE.RUN ? runSpeed : walkSpeed;

            rb.velocity = new Vector3(inputDirection.x * -movementSpeed,
                rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, inputDirection.y * -ZSpeed);
            if (animator) animator.SetAnimatorFloat("MovementSpeed", rb.velocity.magnitude);
        }
        else
        {
            //перестаём двигаться, но продолжаем применять гравитацию
            rb.velocity = new Vector3(0, rb.velocity.y + Physics.gravity.y * Time.fixedDeltaTime, 0);

            if (animator) animator.SetAnimatorFloat("MovementSpeed", 0);
            playerState.SetState(UNITSTATE.IDLE);
        }

        //устанавливаем состояние бега (для аниматоре) на true или false
        animator.SetAnimatorBool("Run", playerState.currentState == UNITSTATE.RUN);
    }

    //движение в воздухе
    void MoveAirborne()
    {
        //падание вниз
        if (rb.velocity.y < 0.1f && playerState.currentState != UNITSTATE.KNOCKDOWN)
            animator.SetAnimatorBool("Falling", true);

        if (!WallInFront())
        {
            //направление движения на основе текущего input
            int dir = Mathf.Clamp(Mathf.RoundToInt(-inputDirection.x), -1, 1);
            float xpeed = Mathf.Clamp(rb.velocity.x + AirMaxSpeed * dir * Time.fixedDeltaTime * AirAcceleration,
                -AirMaxSpeed, AirMaxSpeed);
            float downForce = rb.velocity.y > 0 ? 0 : jumpDownwardsForce; //добавляем маленькую силу падения при падении

            //применяем движение
            if (AllowDepthJumping)
            {
                rb.velocity = new Vector3(xpeed, rb.velocity.y - downForce, -inputDirection.y * ZSpeed);
            }
            else
            {
                rb.velocity = new Vector3(xpeed, rb.velocity.y - downForce, 0);
            }
        }
    }

    //выполняем прыжок
    void Jump()
    {
        playerState.SetState(UNITSTATE.JUMPING);
        JumpNextFixedUpdate = false;
        jumpInProgress = true;
        rb.velocity = Vector3.up * JumpForce;

        //проигрываем анимацию
        animator.SetAnimatorBool("JumpInProgress", true);
        animator.SetAnimatorBool("Run", false);
        animator.SetAnimatorTrigger("JumpUp");
        animator.ShowDustEffectJump();

        //играем sfx
        if (jumpUpVoice != "") GlobalAudioPlayer.PlaySFXAtPosition(jumpUpVoice, transform.position);
    }

    public void LadderInRange(GameObject ladder)
    {
        ladderNearby = ladder;
        UpStair();
    }

    public void LadderOutRange(GameObject ladder)
    {
        if (ladderNearby == ladder) ladderNearby = null;
    }

    //выполняем залез по лестнице
    void UpStair()
    {
        playerState.SetState(UNITSTATE.UPSTAIRS);
        JumpNextFixedUpdate = false;
        //jumpInProgress = true;
        rb.velocity = Vector3.up * runSpeed;

        //проигрываем анимацию
        animator.SetAnimatorBool("UpStairs", true);
        animator.SetAnimatorBool("Run", false);
        animator.SetAnimatorBool("JumpUp", false);
        animator.SetAnimatorFloat("UpStairs", 3f);


        //играем sfx
        if (jumpUpVoice != "") GlobalAudioPlayer.PlaySFXAtPosition(jumpUpVoice, transform.position);
    }

    //спрыгнуть с лестницы
    public void JumpUpStair()
    {
        playerState.SetState(UNITSTATE.JUMPING);
        rb.velocity = new Vector3(0, 10, 10);
    }

    //игрок приземлился после прыжка
    void HasLanded()
    {
        jumpInProgress = false;
        playerState.SetState(UNITSTATE.LAND);
        rb.velocity = Vector2.zero;
        landTime = Time.time;

        //установить свойства аниматора
        animator.SetAnimatorFloat("MovementSpeed", 0f);
        animator.SetAnimatorBool("JumpInProgress", false);
        animator.SetAnimatorBool("JumpKickActive", false);
        animator.SetAnimatorBool("Falling", false);
        animator.ShowDustEffectLand();

        //sfx
        GlobalAudioPlayer.PlaySFX("FootStep");
        if (jumpLandVoice != "") GlobalAudioPlayer.PlaySFXAtPosition(jumpLandVoice, transform.position);
    }

    #region controller input

    //установить текущее направление на направление input
    void OnDirectionInputEvent(Vector2 dir, bool doubleTapActive)
    {
        //игнорировать ввод, когда мы мертвы или когда это состояние не активно
        if (!MovementStates.Contains(playerState.currentState) || isDead) return;

        //установить текущее направление на основе входного вектора.
        //Mathf.sign используется, потому что мы хотим, чтобы игрок оставался в левом или правом направлении (при движении вверх/вниз)
        int dir2 = Mathf.RoundToInt(Mathf.Sign((float)-inputDirection.x));
        if (Mathf.Abs(inputDirection.x) > 0) SetDirection((DIRECTION)dir2);
        inputDirection = dir;

        //начало бега при двойном нажатии клавиши(лево/право)
        if (doubleTapActive && IsGrounded() && Mathf.Abs(dir.x) > 0) playerState.SetState(UNITSTATE.RUN);
    }

    //input actions
    void OnInputEvent(string action, BUTTONSTATE buttonState)
    {
        //игнорировать ввод, когда мы мертвы или когда это состояние не активно
        if (!MovementStates.Contains(playerState.currentState) || isDead) return;

        //начало прыжка
        if (action == "Jump" && buttonState == BUTTONSTATE.PRESS && IsGrounded() &&
            playerState.currentState != UNITSTATE.JUMPING) JumpNextFixedUpdate = true;

        //начало бега при двойном нажатии клавиши(лево/право)(или клавиша на геймпаде)
        if (action == "Run") playerState.SetState(UNITSTATE.RUN);
    }

    #endregion

    //прервать продолжающийся прыжок
    public void CancelJump()
    {
        jumpInProgress = false;
    }

    //устанавливаем текущее направление
    public void SetDirection(DIRECTION dir)
    {
        currentDirection = dir;
        if (animator) animator.currentDirection = currentDirection;
    }

    //возвращаем текущее направление
    public DIRECTION getCurrentDirection()
    {
        return currentDirection;
    }

    //возвращаем true, если игрок на тригере лестницы
    /* public bool IsLadderZone()
    {
        //проверка столкновений капсул со смещением вниз на 0,1 от коллайдера капсулы
        Vector3 bottomCapsulePos = transform.position + (Vector3.up) * (capsule.radius - 0.1f);
        return Physics.CheckCapsule(transform.position + capsule.center, bottomCapsulePos, capsule.radius, LadderLayer);
    }*/

    //возвращаем true, если игрок на земле
    public bool IsGrounded()
    {
        //проверка столкновений капсул со смещением вниз на 0,1 от коллайдера капсулы
        Vector3 bottomCapsulePos = transform.position + (Vector3.up) * (capsule.radius - 0.1f);
        return Physics.CheckCapsule(transform.position + capsule.center, bottomCapsulePos, capsule.radius,
            CollisionLayer);
    }

    //посмотреть (и повернуться) в сторону направления
    public void TurnToCurrentDirection()
    {
        if (currentDirection == DIRECTION.Right || currentDirection == DIRECTION.Left)
        {
            float turnSpeed = jumpInProgress ? jumpRotationSpeed : rotationSpeed;
            Vector3 newDir = Vector3.RotateTowards(transform.forward, Vector3.forward * -(int)currentDirection,
                turnSpeed * Time.fixedDeltaTime, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDir);
        }
    }

    //обновить направление на основе текущего input
    public void updateDirection()
    {
        TurnToCurrentDirection();
    }

    //игрок умэр
    void Death()
    {
        isDead = true;
        rb.velocity = Vector3.zero;
    }

    //возвращает true, если перед нами находится коллайдер окружающей среды(нужно для паркура)
    bool WallInFront()
    {
        var MovementOffset = new Vector3(inputDirection.x, 0, inputDirection.y) * lookAheadDistance;
        var c = GetComponent<CapsuleCollider>();
        Collider[] hitColliders =
            Physics.OverlapSphere(transform.position + Vector3.up * (c.radius + .1f) + -MovementOffset, c.radius,
                CollisionLayer);

        int i = 0;
        bool hasHitwall = false;
        while (i < hitColliders.Length)
        {
            if (CollisionLayer == (CollisionLayer | 1 << hitColliders[i].gameObject.layer)) hasHitwall = true;
            i++;
        }

        return hasHitwall;
    }

    //нарисовать перспективную сферу(зрение) в unity editor
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        var c = GetComponent<CapsuleCollider>();
        Gizmos.color = Color.yellow;
        Vector3 MovementOffset = new Vector3(inputDirection.x, 0, inputDirection.y) * lookAheadDistance;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * (c.radius + .1f) + -MovementOffset, c.radius);
    }
#endif
    public void SetZspread(float newZ)
    {
        ZSpeed = newZ;
    }
}


public enum DIRECTION
{
    Right = -1,
    Left = 1,
    Up = 2,
    Down = -2,
};