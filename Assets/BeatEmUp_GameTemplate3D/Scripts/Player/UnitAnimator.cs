using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class UnitAnimator : MonoBehaviour
{
    [HideInInspector] public DIRECTION currentDirection;

    [Header("Effects")] public GameObject DustEffectLand;
    public GameObject DustEffectJump;
    public GameObject HitEffect;
    public GameObject DefendEffect;
    [HideInInspector] public Animator animator;
    private bool isplayer;

    //awake
    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        isplayer = transform.parent.CompareTag("Player");
        currentDirection = DIRECTION.Right;
    }

    //play an animation
    public void SetAnimatorTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    //устанавливаем bool в animator
    public void SetAnimatorBool(string name, bool state)
    {
        animator.SetBool(name, state);
    }

    //устанавливаем float в animator
    public void SetAnimatorFloat(string name, float value)
    {
        animator.SetFloat(name, value);
    }

    //устанавливаем направление
    public void SetDirection(DIRECTION dir)
    {
        currentDirection = dir;
    }

    //------------------------
    //--- Animation Events ---
    //-------------------------

    //Animation закончила проигрывать анимацию, unit готов для следующего input
    public void Ready()
    {
        if (isplayer)
        {
            transform.parent.GetComponent<PlayerCombat>().Ready();
        }
        else
        {
            transform.parent.GetComponent<EnemyAI>().Ready();
        }
    }

    //проверяем, не ударили ли что-то(кого-то)
    public void Check4Hit()
    {
        //проверяем, ударил ли что-то игрок
        if (isplayer)
        {
            PlayerCombat playerCombat = transform.parent.GetComponent<PlayerCombat>();
            if (playerCombat != null)
            {
                playerCombat.CheckForHit();
            }
            else
            {
                Debug.Log("no player combat component found on gameObject '" + transform.parent.name + "'.");
            }
        }
        else
        {
            //проверяем, не ударил ли враг что-нибудь
            EnemyAI AI = transform.parent.GetComponent<EnemyAI>();
            if (AI != null)
            {
                AI.CheckForHit();
            }
            else
            {
                Debug.Log("no enemy AI component found on gameObject '" + transform.parent.name + "'.");
            }
        }
    }

    //показать эффект попадания
    public void ShowHitEffect()
    {
        float unitHeight = 1.6f;
        GameObject.Instantiate(HitEffect, transform.position + Vector3.up * unitHeight, Quaternion.identity);
    }

    //показать эффект блока
    public void ShowDefendEffect()
    {
        GameObject.Instantiate(DefendEffect, transform.position + Vector3.up * 1.3f, Quaternion.identity);
    }

    //показать эффект песка
    public void ShowDustEffectLand()
    {
        GameObject.Instantiate(DustEffectLand, transform.position + Vector3.up * .13f, Quaternion.identity);
    }

    //показать эффект песка при прыжке
    public void ShowDustEffectJump()
    {
        GameObject.Instantiate(DustEffectJump, transform.position + Vector3.up * .13f, Quaternion.identity);
    }

    //play audio
    public void PlaySFX(string sfxName)
    {
        GlobalAudioPlayer.PlaySFXAtPosition(sfxName, transform.position + Vector3.up);
    }

    //добавить небольшую силу отталкивания для данного unit
    public void AddForce(float force)
    {
        StartCoroutine(AddForceCoroutine(force));
    }


    //добавить небольшую силу с течением времени
    IEnumerator AddForceCoroutine(float force)
    {
        DIRECTION startDir = currentDirection;
        Rigidbody rb = transform.parent.GetComponent<Rigidbody>();
        float speed = 8f;
        float t = 0;

        while (t < 1)
        {
            yield return new WaitForFixedUpdate();
            rb.velocity = Vector2.right * (int)startDir *
                          Mathf.Lerp(force, rb.velocity.y, MathUtilities.Sinerp(0, 1, t));
            t += Time.fixedDeltaTime * speed;
            yield return null;
        }
    }

    //эффект мерцания(при ударах)
    public IEnumerator FlickerCoroutine(float delayBeforeStart)
    {
        yield return new WaitForSeconds(delayBeforeStart);

        //находим все рендеры в данном gameObject
        Renderer[] CharRenderers = GetComponentsInChildren<Renderer>();

        if (CharRenderers.Length > 0)
        {
            float t = 0;
            while (t < 1)
            {
                float speed = Mathf.Lerp(15, 35, MathUtilities.Coserp(0, 1, t));
                float i = Mathf.Sin(Time.time * speed);
                foreach (Renderer r in CharRenderers)
                    r.enabled = i > 0;
                t += Time.deltaTime / 2;
                yield return null;
            }

            foreach (Renderer r in CharRenderers)
                r.enabled = false;
        }

        Destroy(transform.parent.gameObject);
    }

    //тряска камеры
    public void CamShake(float intensity)
    {
        CamShake camShake = Camera.main.GetComponent<CamShake>();
        if (camShake != null)
            camShake.Shake(intensity);
    }

    //спаун projectile
    public void SpawnProjectile(string name)
    {
        PlayerCombat playerCombat = transform.parent.GetComponent<PlayerCombat>();
        if (playerCombat)
        {
            //ищем собственную позицию появления, если она есть. В противном случае используйте руку с оружием в качестве позиции появления.
            Vector3 spawnPos = playerCombat.weaponBone.transform.position;
            ProjectileSpawnPos customSpawnPos = playerCombat.weaponBone.GetComponentInChildren<ProjectileSpawnPos>();
            if (customSpawnPos) spawnPos = customSpawnPos.transform.position;

            //спаун projectile с позиции спауна
            GameObject projectilePrefab =
                GameObject.Instantiate(Resources.Load(name), spawnPos, Quaternion.identity) as GameObject;
            if (!projectilePrefab) return;

            //устанавливаем projectile по направлению движения
            Projectile projectileComponent = projectilePrefab.GetComponent<Projectile>();
            if (projectileComponent)
            {
                projectileComponent.direction = playerCombat.currentDirection;
                Weapon currentWeapon = playerCombat.GetCurrentWeapon();
                if (currentWeapon != null) projectileComponent.SetDamage(playerCombat.GetCurrentWeapon().damageObject);
            }
        }
    }
}