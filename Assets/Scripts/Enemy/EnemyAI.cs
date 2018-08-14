using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    private Transform targetTransform;
    private Vector3 targetBlock;
    private Rigidbody2D rb;

    public float speed;

    public float damage;
    public float attackRate;
    private bool canAttack;

    public bool isPlayerChaser;

    public LayerMask directionFinderMask;

    public Animator animator;
    private bool isWalkingLeft = false;
    private bool isWalkingRight = false;
    private bool isMoving = false;

    public AudioSource grunt1;
    public AudioSource grunt2;
    public AudioSource grunt3;

    List<PlayerController> playerControllers;

    void Start()
    {
        playerControllers = new List<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        foreach (var player in WorldManager.singleton.players)
        {
            //Debug.Log(player.GetComponent<PlayerController>() == null);
            playerControllers.Add(player.GetComponent<PlayerController>());
        }

        // 20% chance to attack heaters, attack players otherwise
        isPlayerChaser = Random.Range(0.0f, 1.0f) > 0.25f;

        // Picks closest player
        if (isPlayerChaser)
        {
            chaseClosestPlayer();
            EnemySpawnManager.singleton.playerEnemies.Add(this);
        }
        else
        {
            attackRandomHeater();
            EnemySpawnManager.singleton.heaterEnemies.Add(this);
        }

        targetBlock = targetTransform.position;
        canAttack = true;

        speed += Random.Range(-1.0f, 1.0f);
    }

    void Update()
    {
        // Animation
        animator.SetBool("isWalkingLeft", isWalkingLeft);
        animator.SetBool("isWalkingRight", isWalkingRight);
        animator.SetBool("isMoving", isMoving);

        /**
        if (targetTransform == null && !isPlayerChaser) // Heater is dead
            attackRandomHeater();
        else if (isPlayerChaser &&
                 (targetTransform == null || targetTransform.GetComponent<PlayerController>().isDowned))
            chaseClosestPlayer(); // Player they were chasing is dead
        */

        if (targetTransform == null || (targetTransform.GetComponent<PlayerController>() != null &&
                                        targetTransform.GetComponent<PlayerController>().isDowned))
        {
            //its target is invalid
            targetTransform = null;
            if (isPlayerChaser)
            {
                chaseClosestPlayer();
            }
            else
            {
                attackRandomHeater();
            }
        }
        else
        {
        }

        if (targetTransform == null)
        {
            //should only happen when all players are dead and is a player chaser
            if (!isPlayerChaser)
            {
                Debug.LogError("a heater chaser failed to find a target");
            }

            return;
        }

        if ((targetTransform.position - transform.position).magnitude < 0.75)
        {
            if (canAttack)
            {
                attack();
                StartCoroutine(attackTimer());
                // Play attack effects
                if (isPlayerChaser)
                {
                    int gruntType = Random.Range(0, 2);
                    if (gruntType == 0)
                    {
                        grunt1.Play();
                    }
                    else if (gruntType == 1)
                    {
                        grunt2.Play();
                    }
                    else
                    {
                        grunt3.Play();
                    }
                }
            }

            // Stop moving
            if ((targetTransform.position - transform.position).magnitude < 0.5)
                return;
        }


        if (isPlayerChaser)
        {
            chaseClosestPlayer();
            avoidObstacles();
        }

        // Move towards target
        rb.MovePosition
        (
            transform.position - (transform.position - targetBlock).normalized * Time.deltaTime * speed *
            (WorldManager.singleton.checkIfInSnow(transform.position) ? 1.6f : 1f)
        );

        balanceNumEnemies();

        //NB for Animation
        isMoving = Math.Abs((transform.position - targetBlock).magnitude) > 0.1;

        Vector2 direction = (transform.position - targetBlock).normalized;
        if (Vector2.Dot(direction, Vector2.right) < 0)
        {
            isWalkingLeft = false;
            isWalkingRight = true;
        }
        else
        {
            isWalkingRight = false;
            isWalkingLeft = true;
        }
    }

    void balanceNumEnemies()
    {
        var numEnemies = EnemySpawnManager.singleton.spawner.livingEnemies;
        if (numEnemies <= 6 && WorldManager.singleton.heaters.Count > 0)
            return;

        for (int i = 0; i < numEnemies / 4 - EnemySpawnManager.singleton.heaterEnemies.Count; i++)
        {
            var switchingEnemy = EnemySpawnManager.singleton.playerEnemies[0];
            switchingEnemy.isPlayerChaser = false;
            EnemySpawnManager.singleton.heaterEnemies.Add(switchingEnemy);
            EnemySpawnManager.singleton.playerEnemies.Remove(switchingEnemy);

            switchingEnemy.attackRandomHeater();
        }
    }

    void chaseClosestPlayer()
    {
        float distance = float.MaxValue;
        foreach (var player in playerControllers)
        {
            if (!player.isDowned)
            {
                //player is alive
                float currentDist = Mathf.Abs(Vector2.Distance(player.transform.position, transform.position));
                if (currentDist < distance)
                {
                    distance = currentDist;
                    targetTransform = player.transform;
                }
            }
        }
    }

    void attackRandomHeater()
    {
        var heaters = WorldManager.singleton.heaters;
        if (heaters.Count == 0)
        {
            chaseClosestPlayer();
            isPlayerChaser = true;
        }
        else
        {
            //Debug.Log("Chosen new random heater");
            targetTransform = heaters.ElementAt(Random.Range(0, heaters.Count)).Value.transform;
            targetBlock = targetTransform.position;
        }
    }

    void avoidObstacles()
    {
        RaycastHit2D lookForPlayer = Physics2D.Raycast(
            transform.position,
            targetTransform.position - transform.position,
            1000,
            directionFinderMask);

        Debug.DrawRay(transform.position, targetTransform.position - transform.position);

        if (lookForPlayer.collider == null)
            return;

        // If enemy can see player then set target transform to player
        if (lookForPlayer.collider.transform == targetTransform)
            targetBlock = targetTransform.position;

        if (!lookForPlayer.collider.name.Contains("Heater")) return;

        // If heater in the way
        var heaterPos = lookForPlayer.collider.gameObject.GetComponent<Heater>().attachedTileIndex;
        var neighbours = WorldManager.singleton.getCellNeighboursIterator(heaterPos);

        // Finding the neighbour with the minimum distance to the player that also is not obscured by other obstacles
        float minDistanceToPoint = float.MaxValue;
        foreach (var position in neighbours)
        {
            var pos = WorldManager.singleton.tilemap.CellToWorld(
                WorldManager.singleton.tilemap.origin + new Vector3Int(position.x, position.y, 0));

            float distanceToNextPoint = Mathf.Abs(Vector2.Distance(pos, targetTransform.position));
            if (!(distanceToNextPoint < minDistanceToPoint)) continue;

            // If the distance is of this block is less than the distance of the minimum distance block
            // Then check if there is anything between the enemy and the new target block
            RaycastHit2D newTargetBlockToEnemy = Physics2D.Raycast(
                transform.position,
                pos - transform.position,
                Mathf.Abs(Vector3.Distance(pos, transform.position)),
                directionFinderMask);

            Debug.DrawRay(
                transform.position,
                pos - transform.position,
                Color.blue);


            if (newTargetBlockToEnemy.collider != null) continue;

            // If new ray hit nothing then there is nothing in the way so set the target to the new block
            minDistanceToPoint = distanceToNextPoint;
            targetBlock = pos;
        }
    }

    void attack()
    {
        targetTransform.gameObject.GetComponent<Health>().damage(damage);
    }

    IEnumerator attackTimer()
    {
        canAttack = false;
        yield return new WaitForSeconds(1 / attackRate);
        canAttack = true;
    }

    private void OnDestroy()
    {
        EnemySpawnManager.singleton.spawner.onEnemyKilled();
        if (isPlayerChaser)
            EnemySpawnManager.singleton.playerEnemies.Remove(this);
        else
            EnemySpawnManager.singleton.heaterEnemies.Remove(this);
    }
}