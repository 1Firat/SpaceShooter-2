using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float speed = 2.0f;
    public bool gameOver;
    public bool isFire;
    private bool ammoMaxed;
    private bool gamePaused;
    private int position = 550;
    public ParticleSystem hitEffect;
    public ParticleSystem collectAmmoBoxEffect;
    private AudioSource[] audioSources;
    void OnEnable()
    {
        GameEvent.RegisterListener(EventListener);
    }

    void OnDisable()
    {
        GameEvent.UnregisterListener(EventListener);
    }
    void Start()
    {
        audioSources = GetComponents<AudioSource>();
    }

    void EventListener(EventGame eg)
    {
        if (eg.type == Constant.gameTimeIsUP || eg.type == Constant.playerDeath)
        {
            gameOver = true;
        }
        if (eg.type == Constant.ammoMax)
        {
            ammoMaxed = true;
        }
        if (eg.type == Constant.ammoNotMax)
        {
            ammoMaxed = false;
        }
        if (eg.type == Constant.pauseGame)
        {
            gamePaused = true;
        }
        if (eg.type == Constant.resumeGame)
        {
            gamePaused = false;
        }

    }
    void Update()
    {
        if (gamePaused)
        {
            return;
        }

        if (gameOver)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(-90, 90, -180), Time.deltaTime * 5);
            return;
        }

        float horizontalInput = Input.GetAxis("Horizontal");
        // float horizontalInput = 0;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            // horizontalInput = 1;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(-40, 90, -180), Time.deltaTime * speed);
        }
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            // horizontalInput = -1;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(-130, 90, -180), Time.deltaTime * speed);
        }
        if (!Input.GetKey(KeyCode.LeftArrow) || !Input.GetKey(KeyCode.A) || !Input.GetKey(KeyCode.RightArrow) || !Input.GetKey(KeyCode.D))
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(-90, 90, -180), Time.deltaTime * 5);
        }
        Vector3 moveDirection = new Vector3(horizontalInput, 0f, 0f);
        transform.Translate(moveDirection * DifficultySelect.selected.playerSpeed * Time.deltaTime, 0f);

        // if (horizontalInput == 0)
        // {
        //     transform.Translate(Vector3.zero);
        // }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (ammoMaxed)
            {
                audioSources[1].Play();
                return;
            }
            StartFire();
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            StopFire();
        }

        if (transform.position.x < -position)
        {
            transform.position = new Vector3(-position, transform.position.y, transform.position.z);
        }

        if (transform.position.x > position)
        {
            transform.position = new Vector3(position, transform.position.y, transform.position.z);
        }

        if (gameOver)
        {
            Destroy(gameObject);
        }
    }

    public void FireBullet()
    {
        if (gameOver)
        {
            return;
        }
        GameObject bullet = ObjectPool.SharedInstance.GetPooledObject();
        if (bullet != null)
        {
            EventGame ammoUsed = new(Constant.useAmmo, 0, 0);
            GameEvent.Raise(ammoUsed);
            bullet.transform.position = transform.position;
            var script = bullet.GetComponent<Bullet>();
            script.GO(DifficultySelect.selected.bulletSpeed);
            audioSources[0].Play();
            bullet.SetActive(true);
        }
    }

    void StartFire()
    {
        if (!gameOver)
        {
            isFire = true;
            StartCoroutine(FireRoutine());
        }
    }

    void StopFire()
    {

        isFire = false;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("AmmoBox"))
        {
            collectAmmoBoxEffect.gameObject.SetActive(true);
            collectAmmoBoxEffect.Play();
            StartCoroutine(DestroyAfterEffect("ammo_box"));
            EventGame ammoBox = new(Constant.ammoBoxCollected, 0, 0);
            GameEvent.Raise(ammoBox);
        }
        if (other.gameObject.CompareTag("Enemy"))
        {
            PlayerHit();
            hitEffect.gameObject.SetActive(true);
            hitEffect.Play();
            StartCoroutine(DestroyAfterEffect("player_hit"));
        }
    }
    void PlayerHit()
    {
        EventGame playerGetHit = new(Constant.playerHit, 0, 0);
        GameEvent.Raise(playerGetHit);
    }

    IEnumerator FireRoutine()
    {
        while (isFire)
        {
            FireBullet();
            yield return new WaitForSeconds(DifficultySelect.selected.fireRate);
        }
    }

    private IEnumerator DestroyAfterEffect(string effectType)
    {
        if (effectType == "ammo_box")
        {
            float ammoBoxEffectDuration = collectAmmoBoxEffect.main.duration;

            yield return new WaitForSeconds(ammoBoxEffectDuration);
            collectAmmoBoxEffect.Stop();
            collectAmmoBoxEffect.gameObject.SetActive(false);
        }

        if (effectType == "player_hit")
        {
            float hitEffectDuration = hitEffect.main.duration;
            yield return new WaitForSeconds(hitEffectDuration);
            hitEffect.Stop();
            hitEffect.gameObject.SetActive(false);
        }

    }
}
