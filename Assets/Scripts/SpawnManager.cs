using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemy;
    public GameObject ammoBox;
    public Transform enemySpawnPoint;
    private bool gameOver;
    private bool gamePaused;
    private int spawnPos = 450;
    private float ammoBoxSpawnTime;
    private float spawnCounter;
    private float spawnTime;

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
        spawnTime = DifficultySelect.selected.enemySpawnCD;
        ammoBoxSpawnTime = DifficultySelect.selected.ammoBoxSpawnTime;
        enemySpawnPoint = GameObject.FindWithTag("SpawnManager").transform;
    }

    private void spawnRoutine(float dt)
    {
        spawnCounter += dt;
        if (spawnCounter >= spawnTime)
        {
            Spawn();
            spawnCounter = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gamePaused)
        {
            return;
        }

        float dt = Time.deltaTime;
        spawnRoutine(dt);
        ammoBoxSpawnTime -= Time.deltaTime;

        if (ammoBoxSpawnTime <= 0)
        {
            AmmoBoxSpawn();
        }
    }

    void EventListener(EventGame eg)
    {
        if (eg.type == Constant.playerDeath || eg.type == Constant.gameTimeIsUP)
        {
            gameOver = true;
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

    private void AmmoBoxSpawn()
    {
        Vector3 randomPos = new Vector3(Random.Range(-spawnPos, spawnPos), 41.1f, 410);
        GameObject a = Instantiate(ammoBox, randomPos, enemySpawnPoint.rotation);
        EnemyMovement script = a.GetComponent<EnemyMovement>();
        script.enemySpeed = DifficultySelect.selected.enemySpeed;
        ammoBoxSpawnTime = DifficultySelect.selected.ammoBoxSpawnTime;
    }

    private void Spawn()
    {
        GameObject randomSpawnEnemy = enemy[Random.Range(0, enemy.Length)];
        Vector3 randomPos = new Vector3(Random.Range(-spawnPos, spawnPos), 32f, 410);

        GameObject o = Instantiate(randomSpawnEnemy, randomPos, enemySpawnPoint.rotation);
        EnemyMovement script = o.GetComponent<EnemyMovement>();
        script.enemySpeed = DifficultySelect.selected.enemySpeed;
    }
}
