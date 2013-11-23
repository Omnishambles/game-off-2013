using UnityEngine;
using System.Collections;

public static class Score {

    private static int _score;
    public static GameObject scoreText = null;

    private static int getScore()
    {
        return _score;
    }
    public static void updateScore(int addValue)
    {
        _score = _score + addValue;
        if(scoreText == null)
        {
            scoreText = GameObject.Find("ScoreValue");
        }
        scoreText.guiText.text = "" + _score;
    }

}
