public class Score
{
    private int _score;
    public int GetScore()
    {
        return _score;
    }
    public void SetScore(int score)
    {
        _score = score;
    }
    public int CalculateScore(int pointSameColor)
    {
        return pointSameColor * 10 ;
    }

    public int CalculateScoreFallingDown(int pointFall)
    {
        return pointFall;
    }
}
