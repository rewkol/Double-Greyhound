[System.Serializable]
public class GameState
{
    public long score { get; set; }
    public int health { get; set; }
    public int special { get; set; }
    public int unlocked { get; set; }
    public int run { get; set; }
    public bool firstRun { get; set; }

    public GameState()
    {
        score = 0L;
        health = 20;
        special = 0;
        unlocked = 3;
        run = 0;
        firstRun = true;
    }

    public GameState(long score, int health, int special, int unlocked, int run, bool firstRun)
    {
        this.score = score;
        this.health = health;
        this.special = special;
        this.unlocked = unlocked;
        this.run = run;
        this.firstRun = firstRun;
    }

}
