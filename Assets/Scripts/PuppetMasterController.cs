using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetMasterController : MonoBehaviour
{
    // TODO: public references to the three boss controllers (CHief, Queen, Supreme Saint)
    // TODO: public references to prefabs for the puppets (vikingMale, vikingFemale, seabee, saint, greyhound)
    public PuppetVikingController maleViking;
    public PuppetVikingController femaleViking;
    public PuppetSeabeeController seabee;
    public PuppetSaintController saint;
    public PuppetShadowController shadow;

    public PuppetBossController chief;
    public PuppetBossController queen;
    public PuppetBossController supreme;

    private PuppetMasterController master;
    private UIController ui;
    private PlayerController player;

    private List<IPuppet> allies;
    private List<IPuppet> enemies;
    private List<PuppetBossController> bosses;
    private bool start;
    private int special;

    private const int AVG_PUPPETS = 25;
    private const int MIN_PUPPETS = 20;
    private const int MAX_PUPPETS = 30;

    // Start is called before the first frame update
    void Start()
    {
        ui = GameObject.FindObjectsOfType<UIController>()[0];
        player = GameObject.FindObjectsOfType<PlayerController>()[0];
        allies = new List<IPuppet>();
        enemies = new List<IPuppet>();
        bosses = new List<PuppetBossController>();
        bosses.Add(null);
        // Their indices match their special id
        bosses.Add(chief);
        bosses.Add(queen);
        bosses.Add(supreme);

        start = false;
        special = player.GetSpecial();

        master = GetComponent<PuppetMasterController>();
    }

    void FixedUpdate()
    {
        if (start)
        {
            if (allies.Count < MIN_PUPPETS)
            {
                IPuppet ally = SpawnAlly();
                ally.StartBattle();
            }
            if (enemies.Count < MIN_PUPPETS)
            {
                IPuppet enemy = SpawnShadow();
                enemy.StartBattle();
            }

            // When player isDead stop spawning new puppets for either side. Trigger exit of boss and no new returns
            if (player.IsDead())
            {
                foreach(IPuppet enemy in enemies)
                {
                    ((PuppetShadowController) enemy).Victory();
                }
                bosses[this.special].Exit();
                start = false;
            }
        }
    }

    private IEnumerator EntranceDialogueRoutine()
    {
        // This shouldn't be changeable in a cutscene, but being absolutely sure here
        int specialSelected = special;
        switch (specialSelected)
        {
            case 1:
                {
                    ui.DisplayDialogue("GodHeadshot", "You're strong enough to take them all|but even still...");
                    break;
                }
            case 2:
                {
                    ui.DisplayDialogue("GodHeadshot", "It is quite peculiar that he would|claim you to be all alone...");
                    break;
                }
            case 3:
                {
                    ui.DisplayDialogue("GodHeadshot", "Do not let his provocations get to you!|You know you can do this...");
                    break;
                }
        }

        while (!ui.GameActive())
        {
            yield return new WaitForFixedUpdate();
        }

        PuppetBossController boss = this.bosses[specialSelected];
        boss.Enter();
        ui.StartManualCutscene();
        while (!boss.HasEntered())
        {
            yield return new WaitForFixedUpdate();
        }
        for (int i = 0; i < AVG_PUPPETS; i++)
        {
            SpawnAlly();
        }

        ui.EndManualCutscene();
        switch (specialSelected)
        {
            case 1:
                {
                    ui.DisplayDialogue("ChiefHeadshot", "You have my axe Greyhound!|Focus on him, we have these dogs.");
                    break;
                }
            case 2:
                {
                    ui.DisplayDialogue("QueenHeadshot", "Despite all of us being here.|We will handle this swarm for you.");
                    break;
                }
            case 3:
                {
                    ui.DisplayDialogue("SupremeHeadshot", "And so do we!|Let us disperse these shadows.");
                    break;
                }
        }

        while (!ui.GameActive())
        {
            yield return new WaitForFixedUpdate();
        }

        this.start = true;
        StartCoroutine(TrackSpecialRoutine());
        foreach (IPuppet enemy in enemies)
        {
            enemy.StartBattle();
        }
        foreach (IPuppet ally in allies)
        {
            ally.StartBattle();
        }
    }

    private IPuppet SpawnMaleViking()
    {
        float y = Random.Range(0.25f, 2.80f);
        PuppetVikingController vikingMale = Instantiate(maleViking, new Vector3(50.0f, y, -0.8f + (y * 0.01f)), transform.rotation);
        vikingMale.SetParent(master);
        allies.Add(vikingMale);
        return vikingMale;
    }

    private IPuppet SpawnFemaleViking()
    {
        float y = Random.Range(0.25f, 2.80f);
        PuppetVikingController vikingFemale = Instantiate(femaleViking, new Vector3(50.0f, y, -0.8f + (y * 0.01f)), transform.rotation);
        vikingFemale.SetParent(master);
        allies.Add(vikingFemale);
        return vikingFemale;
    }

    private IPuppet SpawnSeabee()
    {
        float y = Random.Range(0.25f, 2.80f);
        PuppetSeabeeController bee = Instantiate(seabee, new Vector3(50.0f, y, -0.8f + (y * 0.01f)), transform.rotation);
        bee.SetParent(master);
        allies.Add(bee);
        return bee;
    }

    private IPuppet SpawnSaint()
    {
        float y = Random.Range(0.25f, 2.80f);
        PuppetSaintController super = Instantiate(saint, new Vector3(50.0f, y, -0.8f + (y * 0.01f)), transform.rotation);
        super.SetParent(master);
        allies.Add(super);
        return super;
    }

    private IPuppet SpawnShadow()
    {
        float y = Random.Range(0.25f, 2.80f);
        PuppetShadowController greyhound = Instantiate(shadow, new Vector3(85.0f, y, -0.8f + (y * 0.01f)), transform.rotation);
        greyhound.SetParent(master);
        enemies.Add(greyhound);
        return greyhound;
    }

    private IPuppet SpawnAlly()
    {
        IPuppet ally = null;
        switch (this.special)
        {
            case 1:
                {
                    if (Random.value < 0.5f)
                    {
                        ally = SpawnMaleViking();
                    }
                    else
                    {
                        ally = SpawnFemaleViking();
                    }
                    break;
                }
            case 2:
                {
                    ally = SpawnSeabee();
                    break;
                }
            case 3:
                {
                    ally = SpawnSaint();
                    break;
                }
        }
        return ally;
    }

    public void EnemyEntrance()
    {
        for(int i = 0; i < AVG_PUPPETS; i++)
        {
            SpawnShadow();
        }
    }

    public void AllyEntrance()
    {
        this.special = player.GetSpecial();
        StartCoroutine(EntranceDialogueRoutine());
    }

    private IEnumerator TrackSpecialRoutine()
    {
        // Want to guarantee that the player has decided on a special
        int specialPrior = player.GetSpecial();
        int specialNow = 0;
        int specialFuture = 0;

        for (int i = 0; i < 15; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        specialNow = player.GetSpecial();
        if (specialPrior != specialNow && this.start)
        {
            StartCoroutine(TrackSpecialRoutine());
            yield break;
        }
        for (int i = 0; i < 15; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        specialFuture = player.GetSpecial();
        // They are consistent with their choice
        if (specialPrior == specialFuture && specialNow == specialFuture)
        {
            // If the choice has changed we need to switch bosses
            if (this.special != specialFuture)
            {
                // Can only switch bosses if most recent one has finished entering yet
                if (bosses[this.special].HasEntered())
                {
                    while (!bosses[this.special].Exit())
                    {
                        // Need to wait incase boss was trying to emote
                        yield return new WaitForFixedUpdate();
                    }
                    this.special = specialFuture;
                    bosses[this.special].Enter();

                    while (this.allies.Count < MAX_PUPPETS)
                    {
                        IPuppet ally = SpawnAlly();
                        ally.StartBattle();
                    }
                }
            }
        }

        if (this.start)
        {
            StartCoroutine(TrackSpecialRoutine());
        }
    }

    private IEnumerator DisappearRoutine()
    {
        start = false;
        while (allies.Count > 0 || enemies.Count > 0)
        {
            if (allies.Count > 0 && Random.value < 0.5f)
            {
                allies[0].Disappear();
                allies.RemoveAt(0);
            }
            else if (enemies.Count > 0)
            {
                enemies[0].Disappear();
                enemies.RemoveAt(0);
            }
            // Why this condition again? In case all enemies are gone and random chance didn't work out
            else if (allies.Count > 0)
            {
                allies[0].Disappear();
                allies.RemoveAt(0);
            }
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForFixedUpdate();
            }
        }
        chief.Disappear();
        queen.Disappear();
        supreme.Disappear();
    }

    public void CueDestruction()
    {
        StartCoroutine(DisappearRoutine());
    }

    public void RemoveAlly(IPuppet child)
    {
        allies.Remove(child);
    }

    public void RemoveEnemy(IPuppet enemy)
    {
        enemies.Remove(enemy);
    }
}
