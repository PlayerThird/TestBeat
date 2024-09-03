using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


public class Ladder : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Animator anim;

    [SerializeField] private float speed = 0.1f;
    [SerializeField] public Transform teleportLock;
    [SerializeField] public Collider upStairs;

    [SerializeField] private bool inCoolider = false;
    [SerializeField] private bool isActive = false;

    [SerializeField] private bool oneTime = false;
    [SerializeField] public bool triggerEnd = false;
    private Rigidbody rb;

    private void OnTriggerEnter(Collider collider)
    {
        oneTime = true;
        inCoolider = true;
        player = collider.gameObject;

        rb = player.GetComponent<Rigidbody>();
        anim = player.GetComponentInChildren<Animator>();
    }

    //поворачиваем персонажа в сторону лестницы, когда он взаимодействует с ней
    private void OnTriggerStay(Collider other)
    {
        inCoolider = true;
        if (isActive) player.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    private void Update()
    {
        if (!inCoolider) return;
        if (isActive)
        {
            //при нажатии на пробел, поднимаем модель персонажа и задаём скорость для анимации подъема
            if (Input.GetKeyDown(KeyCode.Space))
            {
                anim.Play("UpStairs");
                anim.speed = 1;
                rb.velocity = Vector3.up * speed;
            }
            //при отжатии, останавливаем на месте
            if (Input.GetKeyUp(KeyCode.Space))
            {
                anim.Play("UpStairs");
                anim.speed = 0;
                rb.velocity = Vector3.zero;
            }
        }
//при нажатии на Е переключаем режим "лестницы"
        if (Input.GetKeyDown(KeyCode.E))
        {
            isActive = !isActive;

            if (isActive && inCoolider)
            {
                Enter();
            }
            else
            {
                Exit();
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        inCoolider = false;
        anim.SetBool("isGrounded", true);
        StartCoroutine(WaitSec());
        Exit();
        ClearInfo();
        oneTime = false;
    }

    private IEnumerator WaitSec()
    {
        yield return new WaitForSeconds(2);
    }

    private void Enter()
    {
        rb.velocity = Vector3.zero;
        anim.SetBool("Falling", false);
        player.GetComponent<UnitState>().SetState(UNITSTATE.UPSTAIRS);

       // телепорт на позицию телепорта
        player.transform.position = new Vector3(teleportLock.position.x, player.transform.position.y,
                teleportLock.position.z);
        
        //выключаем скрипты, контролирующие передвижение игрока
        player.GetComponent<PlayerMovement>().enabled = false;
        player.GetComponent<PlayerCombat>().enabled = false;

        rb.useGravity = false;

        anim.Play("UpStairs");
        anim.speed = 0;
    }

    //включаем обратно скрипты передвижения для игрока
    public void Exit()
    {
        player.GetComponent<PlayerMovement>().enabled = true;
        player.GetComponent<PlayerCombat>().enabled = true;

        if (!player.GetComponent<PlayerMovement>().IsGrounded() ||
            player.GetComponent<PlayerMovement>().IsGrounded() && inCoolider)
        {
            anim.SetBool("Falling", true);
        }
        anim.speed = 1;
        rb.useGravity = true;
        
        isActive = false;
    }

    //Чистим считанную информацию об игроке
    private void ClearInfo()
    {
        if (!inCoolider)
        {
            player = null;
            anim = null;
            rb = null;
        }
    }

    //тест
    public void ChangeBoolEnter()
    {
        triggerEnd = !triggerEnd;
    }
}