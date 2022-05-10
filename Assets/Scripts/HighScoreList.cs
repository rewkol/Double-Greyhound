using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class HighScoreList
{
    public bool hideScores { get; set; }
    public List<HighScore> scores { get; set; }

    public HighScoreList()
    {
        hideScores = false;
        scores = new List<HighScore>();
    }
    
    public HighScoreList(bool hideScores, List<HighScore> scores)
    {
        this.hideScores = hideScores;
        this.scores = scores;
    }

    [System.Serializable]
    public class HighScore : IComparable
    {
        public string player { get; set; }
        public long score { get; set; }
        public bool hidden { get; set; }
        public long time { get; set; }

        public HighScore()
        {
            score = 0L;
            player = "AAAAAAAAAAAA";
            hidden = true;
            time = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
        }

        public HighScore(long score, string player, bool hidden)
        {
            this.score = score;
            this.player = player;
            this.hidden = hidden;
            time = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            HighScore otherScore = obj as HighScore;
            if (otherScore != null)
            {
                //Compare by scores, then by names, then by time
                if (this.score != otherScore.score)
                {
                    //Want scores in descending order to get high scores at the top
                    return this.score > otherScore.score ? -1 : (this.score < otherScore.score ? 1 : 0);
                }
                else if (this.player != otherScore.player)
                {
                    //For names actually want alphabetical order
                    return this.player.CompareTo(otherScore.player);
                }
                else
                {
                    //For times show the newest highest
                    return this.time > otherScore.time ? -1 : (this.time < otherScore.time ? 1 : 0);
                }
            }
            else
            {
                throw new ArgumentException("Compared score object was not a score");
            }
        }

        public string ToString()
        {
            string number = this.score.ToString();

            //Prefix it with 0s
            if (number.Length < 14)
            {
                int difference = 14 - number.Length;
                number = new string('0', difference) + number;
            }
            return this.player+" "+number;
        }
    }
}
