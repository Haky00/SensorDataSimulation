namespace SensorDataSimulation;

public record FitnessScore
{
    public double Score { get; private set; } = 0;
    public double MaxScore {get; private set; } = 0;

    public Dictionary<string, (double Score, double Max)> IndividialScores = [];

    public void AddWeighedScoreLinear(string name, double score, double weight)
    {
        double weighedScore = score * Math.Clamp(weight, 0, 1);
        Score += weighedScore;
        IndividialScores[name] = (weighedScore, score);
        MaxScore += score;
    }

    public void AddWeighedScoreSqrt(string name, double score, double weight)
    {
        AddWeighedScoreLinear(name, score, Math.Sqrt(Math.Clamp(weight, 0, 1)));
    }

    public void AddScore(string name, double score)
    {
        Score += score;
        IndividialScores[name] = (score, score);
        MaxScore += score;
    }

    public void AddWeighedPenaltyLinear(string name, double score, double weight)
    {
        double weighedScore = score * Math.Clamp(weight, 0, 1);
        Score += -weighedScore;
        IndividialScores[name] = (-weighedScore, -score);
    }

    public void AddWeighedPenaltySqrt(string name, double score, double weight)
    {
        AddWeighedPenaltyLinear(name, score, Math.Sqrt(Math.Clamp(weight, 0, 1)));
    }

    public void AddPenalty(string name, double score)
    {
        Score -= score;
        IndividialScores[name] = (-score, -score);
    }
}
