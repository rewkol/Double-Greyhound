using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwarmController : MonoBehaviour
{
    public SeabeeController seabee;

    private Transform transform;
    private UIController ui;
    private int health;
    private SeabeeController[] bees;
    private bool attack;
    private int pattern;
    private int[] lastPattern;
    private int[] attackMap;
    private int[] attackOrder;
    private int[] attackDelay;

    private float leftX;
    private float topY;
    private float rightX;
    private float bottomY;
    private float centreX;
    private float centreY;

    // Start is called before the first frame update
    void Start()
    {
        transform = GetComponent<Transform>();
        ui = GameObject.FindObjectsOfType<UIController>()[0];
        health = 50;
        attack = false;
        pattern = Random.Range(0, 8);
        lastPattern = new int[2];
        lastPattern[0] = Random.Range(0, 8);
        int temp = Random.Range(0, 8);
        while (temp == lastPattern[0])
        {
            temp = Random.Range(0, 8);
        }
        lastPattern[1] = temp;

        leftX = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        rightX = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        topY = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).y;
        bottomY = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, transform.position.z - Camera.main.transform.position.z)).y;
        centreX = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, transform.position.z - Camera.main.transform.position.z)).x;
        centreY = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, transform.position.z - Camera.main.transform.position.z)).y;

        bees = new SeabeeController[5];
        for (int i = 0; i < 5; i++)
        {
            bees[i] = Instantiate(seabee, new Vector3(rightX + 5.0f, centreY, -1.0f), transform.rotation);
        }
        attackMap = new int[5];
        attackOrder = new int[5];
        attackDelay = new int[5];

        //Need to put it separate so that the seabees can properly instantiate first
        StartCoroutine(SetupRoutine());
    }

    private IEnumerator SetupRoutine()
    {
        yield return new WaitForFixedUpdate();
        
        //By some stroke of luck this positions the bees perfectly for the intro
        for (int i = 0; i < 5; i++)
        {
            bees[i].SetParent(transform);
            bees[i].SetNewTarget(leftX + 3.0f + (i * 3.0f), centreY + 2.0f);
        }

        ui.PanCamera();

        //Movement of Boss onto screen through the cutscene
        for (int i = 0; i < 100; i++)
        {
            transform.position = transform.position + new Vector3(-0.025f, 0.0f, 0.0f);
            if (i == 50)
            {
                ui.DisplayDialogue("SwarmHeadshot", "We are Swarm for we are many;|Welcome to our hive.|You may have defeated our sisters|but if we fight together we are unstoppable!");
                ui.BossEntrance(health, "SWARM");
            }
            yield return new WaitForFixedUpdate();
        }

        //Need new camera coordinates
        leftX = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        rightX = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).x;
        topY = Camera.main.ViewportToWorldPoint(new Vector3(0.0f, 0.0f, transform.position.z - Camera.main.transform.position.z)).y;
        bottomY = Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 1.0f, transform.position.z - Camera.main.transform.position.z)).y;
        centreX = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, transform.position.z - Camera.main.transform.position.z)).x;
        // Adding the DEFAULT_HEIGHT from Seabee because I have removed it from Seabee
        centreY = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, transform.position.z - Camera.main.transform.position.z)).y + 1.32f;
        
        //Post dialogue target
        for (int i = 0; i < 5; i++)
        {
            bees[i].SetParent(transform);
            bees[i].SetNewTarget(leftX + 3.0f + (i * 3.0f), centreY + 2.0f);
        }

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Ready())
        {
            //If attack then trigger based on the primed pattern
            if (attack)
            {
                TriggerAttacks();
                SetTargets();

                for (int i = 0; i < 5; i++)
                {
                    bees[i].SetCooldown(attackDelay[i]);
                }
            }
            //If not attack then randomly choose the next attack and prepare everyone for it
            else
            {
                while (pattern == lastPattern[0] || pattern == lastPattern[1])
                {
                    pattern = Random.Range(0, 8);
                }
                PrepAttack();
                lastPattern[0] = lastPattern[1];
                lastPattern[1] = pattern;
            }

            //Flip
            attack = !attack;
        }
        //Not really much to do if they aren't ready. The Seabees handle that logic
    }

    private bool Ready()
    {
        bool ready = true;
        for (int i = 0; i < 5; i++)
        {
            ready = bees[i].IsInPosition();
            if (!ready)
            {
                break;
            }
        }
        return ready;
    }

    private void PrepAttack()
    {
        switch (pattern)
        {
            //All five swoop left from right
            case 0:
                {
                    for (int i = 0; i < 5; i++)
                    {
                        bees[i].SetNewTarget(centreX + 1.5f + (i * 1.7f), centreY + ((4 - i) * 0.7f));
                        bees[i].SetAttackDistance(13.0f);
                        attackMap[i] = 0;
                        attackOrder[i] = i;
                        attackDelay[i] = (i * 19) + 10;
                    }
                    break;
                }
            //All five swoop right from left
            case 1:
                {
                    for (int i = 0; i < 5; i++)
                    {
                        bees[i].SetNewTarget(centreX - 1.5f - (i * 1.7f), centreY + ((4 - i) * 0.7f));
                        bees[i].SetAttackDistance(13.0f);
                        attackMap[i] = 0;
                        attackOrder[i] = i;
                        attackDelay[i] = (i * 19) + 10;
                    }
                    break;
                }
            //All five drop from top in random order
            case 2:
                {
                    RandomOrder();
                    for (int i = 0; i < 5; i++)
                    {
                        bees[i].SetNewTarget(leftX + 1.5f + (i * 3.6f), centreY + 3.0f);
                        bees[i].SetAttackDistance(7.0f);
                        attackMap[i] = 1;
                        attackDelay[attackOrder[i]] = (i * 19) + 10 + (i >= 3 ? 15 : 0);
                    }
                    break;
                }
            //Three swoop left from right, other two drop on left
            case 3:
                {
                    //Swoopers
                    for (int i = 0; i < 5; i+=2)
                    {
                        bees[i].SetNewTarget(centreX + 3.0f + (i * 1.2f), centreY + 0.8f + ((4 - i) * 0.3f));
                        bees[i].SetAttackDistance(13.0f);
                        attackMap[i] = 0;
                        attackOrder[i] = i;
                        attackDelay[i] = (i * 19) + 10;
                    }
                    //Droopers
                    for (int i = 1; i < 5; i += 2)
                    {
                        bees[i].SetNewTarget(leftX + 0.7f + (i * 1.1f), centreY + 3.0f);
                        bees[i].SetAttackDistance(7.0f);
                        attackMap[i] = 1;
                        attackOrder[i] = i;
                        attackDelay[i] = (i * 19) + 10;
                    }
                    break;
                }
            //Three swoop right from left, other two drop on right
            case 4:
                {
                    //Swoopers
                    for (int i = 0; i < 5; i += 2)
                    {
                        bees[i].SetNewTarget(centreX - 3.0f - (i * 1.2f), centreY + 0.8f + ((4 - i) * 0.3f));
                        bees[i].SetAttackDistance(13.0f);
                        attackMap[i] = 0;
                        attackOrder[i] = i;
                        attackDelay[i] = (i * 19) + 10;
                    }
                    //Droopers
                    for (int i = 1; i < 5; i += 2)
                    {
                        bees[i].SetNewTarget(rightX - 0.7f - (i * 1.1f), centreY + 3.0f);
                        bees[i].SetAttackDistance(7.0f);
                        attackMap[i] = 1;
                        attackOrder[i] = i;
                        attackDelay[i] = (i * 19) + 10;
                    }
                    break;
                }
            //Two swoop right from left, three swoop left from right
            case 5:
                {
                    //Right to Left
                    for (int i = 0; i < 5; i += 2)
                    {
                        bees[i].SetNewTarget(centreX + 1.5f + (i * 1.7f), centreY + ((4 - i) * 0.7f));
                        bees[i].SetAttackDistance(13.0f);
                        attackMap[i] = 0;
                        attackOrder[i] = i;
                        attackDelay[i] = (i * 19) + 10;
                    }
                    //Left to Right
                    for (int i = 1; i < 5; i += 2)
                    {
                        bees[i].SetNewTarget(centreX - 1.5f - (i * 1.7f), centreY + ((4 - i) * 0.7f));
                        bees[i].SetAttackDistance(13.0f);
                        attackMap[i] = 0;
                        attackOrder[i] = i;
                        attackDelay[i] = (i * 19) + 10;
                    }
                    break;
                }
            //Two swoop left from right, three swoop right from left
            case 6:
                {
                    //Left to Right
                    for (int i = 1; i < 5; i += 2)
                    {
                        bees[i].SetNewTarget(centreX + 1.5f + (i * 1.7f), centreY + ((4 - i) * 0.7f));
                        bees[i].SetAttackDistance(13.0f);
                        attackMap[i] = 0;
                        attackOrder[i] = i;
                        attackDelay[i] = (i * 19) + 10;
                    }
                    //Right to Left
                    for (int i = 0; i < 5; i += 2)
                    {
                        bees[i].SetNewTarget(centreX - 1.5f - (i * 1.7f), centreY + ((4 - i) * 0.7f));
                        bees[i].SetAttackDistance(13.0f);
                        attackMap[i] = 0;
                        attackOrder[i] = i;
                        attackDelay[i] = (i * 19) + 10;
                    }
                    break;
                }
            //Three Drop two Swoop
            case 7:
                {
                    //
                    for (int i = 1; i < 4; i ++)
                    {
                        bees[i].SetNewTarget(centreX - 14.4f + (i * 7.2f), centreY + 3.0f);
                        bees[i].SetAttackDistance(7.0f);
                        attackMap[i] = 1;
                        attackOrder[i] = i;
                        attackDelay[i] = 35;
                    }
                    //Outer Swoopers
                    bees[0].SetNewTarget(centreX - 3.6f, centreY + 2.3f);
                    bees[0].SetAttackDistance(13.0f);
                    attackMap[0] = 0;
                    attackOrder[0] = 0;
                    attackDelay[0] = 10;

                    bees[4].SetNewTarget(centreX + 3.6f, centreY + 2.3f);
                    bees[4].SetAttackDistance(13.0f);
                    attackMap[4] = 0;
                    attackOrder[4] = 4;
                    attackDelay[4] = 10;
                    break;
                }
        }

    }

    private void TriggerAttacks()
    {
        switch (pattern)
        {
            case 0:
                {
                    for (int i = 0; i < 5; i++)
                    {
                        bees[i].SetDirection(true);
                        StartCoroutine("DelayedSwoopRoutine", i);
                    }
                    break;
                }
            case 1:
                {
                    for (int i = 0; i < 5; i++)
                    {
                        bees[i].SetDirection(false);
                        StartCoroutine("DelayedSwoopRoutine", i);
                    }
                    break;
                }
            case 2:
                {
                    for (int i = 0; i < 5; i++)
                    {
                        StartCoroutine("DelayedDropRoutine", i);
                    }
                    break;
                }
            case 3:
                {
                    for (int i = 0; i < 5; i+=2)
                    {
                        bees[i].SetDirection(true);
                        StartCoroutine("DelayedSwoopRoutine", i);
                    }
                    for (int i = 1; i < 5; i+=2)
                    {
                        StartCoroutine("DelayedDropRoutine", i);
                    }
                    break;
                }
            case 4:
                {
                    for (int i = 0; i < 5; i += 2)
                    {
                        bees[i].SetDirection(false);
                        StartCoroutine("DelayedSwoopRoutine", i);
                    }
                    for (int i = 1; i < 5; i += 2)
                    {
                        StartCoroutine("DelayedDropRoutine", i);
                    }
                    break;
                }
            case 5:
                {
                    for (int i = 0; i < 5; i += 2)
                    {
                        bees[i].SetDirection(true);
                        StartCoroutine("DelayedSwoopRoutine", i);
                    }
                    for (int i = 1; i < 5; i += 2)
                    {
                        bees[i].SetDirection(false);
                        StartCoroutine("DelayedSwoopRoutine", i);
                    }
                    break;
                }
            case 6:
                {
                    for (int i = 1; i < 5; i += 2)
                    {
                        bees[i].SetDirection(true);
                        StartCoroutine("DelayedSwoopRoutine", i);
                    }
                    for (int i = 0; i < 5; i += 2)
                    {
                        bees[i].SetDirection(false);
                        StartCoroutine("DelayedSwoopRoutine", i);
                    }
                    break;
                }
            case 7:
                {
                    bees[0].SetDirection(false);
                    StartCoroutine("DelayedSwoopRoutine", 0);
                    bees[4].SetDirection(true);
                    StartCoroutine("DelayedSwoopRoutine", 4);
                    for (int i = 1; i < 4; i ++)
                    {
                        StartCoroutine("DelayedDropRoutine", i);
                    }
                    break;
                }
        }
    }

    private void SetTargets()
    {
        switch (pattern)
        {
            case 0:
                {
                    for (int i = 0; i < 5; i++)
                    {
                        bees[i].SetNewTarget(leftX - 6.0f, centreY - 9.0f);
                    }
                    break;
                }
            case 1:
                {
                    for (int i = 0; i < 5; i++)
                    {
                        bees[i].SetNewTarget(rightX + 6.0f, centreY - 9.0f);
                    }
                    break;
                }
            case 2:
                {
                    bees[0].SetNewTarget(leftX - 6.0f, centreY - 2.0f);
                    bees[1].SetNewTarget(leftX - 6.0f, centreY - 2.0f);
                    bees[2].SetNewTarget(Random.Range(0.0f, 1.0f) > 0.5f ? leftX - 6.0f : rightX + 6.0f, centreY - 2.0f);
                    bees[3].SetNewTarget(rightX + 6.0f, centreY - 2.0f);
                    bees[4].SetNewTarget(rightX + 6.0f, centreY - 2.0f);
                    break;
                }
            case 3:
                {
                    for (int i = 0; i < 5; i+=2)
                    {
                        bees[i].SetNewTarget(leftX - 6.0f, centreY - 9.0f);
                    }
                    for (int i = 1; i < 5; i += 2)
                    {
                        bees[i].SetNewTarget(leftX - 6.0f, centreY - 2.0f);
                    }
                    break;
                }
            case 4:
                {
                    for (int i = 0; i < 5; i += 2)
                    {
                        bees[i].SetNewTarget(rightX + 6.0f, centreY - 9.0f);
                    }
                    for (int i = 1; i < 5; i += 2)
                    {
                        bees[i].SetNewTarget(rightX + 6.0f, centreY - 2.0f);
                    }
                    break;
                }
            case 5:
                {
                    for (int i = 0; i < 5; i+=2)
                    {
                        bees[i].SetNewTarget(leftX - 6.0f, centreY - 9.0f);
                    }
                    for (int i = 1; i < 5; i+=2)
                    {
                        bees[i].SetNewTarget(rightX + 6.0f, centreY - 9.0f);
                    }
                    break;
                }
            case 6:
                {
                    for (int i = 1; i < 5; i += 2)
                    {
                        bees[i].SetNewTarget(leftX - 6.0f, centreY - 9.0f);
                    }
                    for (int i = 0; i < 5; i += 2)
                    {
                        bees[i].SetNewTarget(rightX + 6.0f, centreY - 9.0f);
                    }
                    break;
                }
            case 7:
                {
                    bees[0].SetNewTarget(rightX + 6.0f, centreY - 2.0f);
                    bees[1].SetNewTarget(leftX - 6.0f, centreY - 2.0f);
                    bees[2].SetNewTarget(Random.Range(0.0f, 1.0f) > 0.5f ? leftX - 6.0f : rightX + 6.0f, centreY - 2.0f);
                    bees[3].SetNewTarget(rightX + 6.0f, centreY - 2.0f);
                    bees[4].SetNewTarget(leftX - 6.0f, centreY - 2.0f);
                    break;
                }
        }
    }

    private void RandomOrder()
    {
        List<int> order = new List<int> {0,1,2,3,4};
        for(int i = 0; i < 5; i++)
        {
            int index = Random.Range(0, order.Count);
            attackOrder[i] = order[index];
            order.RemoveAt(index);
        }
    }

    private IEnumerator DelayedSwoopRoutine(int bee)
    {
        for (int i = 0; i < attackDelay[bee]; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        bees[bee].TriggerSwoop();
    }

    private IEnumerator DelayedDropRoutine(int bee)
    {
        for (int i = 0; i < attackDelay[bee]; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        bees[bee].TriggerDrop();
    }

    public void Hurt(int dmg)
    {
        health -= dmg;
        ui.BossHealthBar(health);
        if (health <= 0)
        {
            StartCoroutine(DeathRoutine());
        }
    }

    private IEnumerator DeathRoutine()
    {
        ui.SetPlayerInvincible(true);
        for (int i = 0; i < 51; i++)
        {
            if (i == 50)
            {
                ui.UpdateScore(5000L);
                ui.DisplayDialogue("SwarmHeadshot", "Our Queen, we have failed you.|Mother, we leave Captain Greyhound|to you...");
                ui.BossExit();
                while (!ui.GameActive())
                {
                    yield return new WaitForFixedUpdate();
                }
            }
            yield return new WaitForFixedUpdate();
        }
        ui.SetPlayerInvincible(false);
        Destroy(gameObject);
    }

    public void SpawnRandom()
    {
        //suppress warnings
    }
}
