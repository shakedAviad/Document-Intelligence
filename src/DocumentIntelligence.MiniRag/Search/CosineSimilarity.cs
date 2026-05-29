namespace DocumentIntelligence.MiniRag.Search;

public static class CosineSimilarity
{
    public static double Calculate(IReadOnlyList<float> first, IReadOnlyList<float> second)
    {
        if (first.Count == 0 || second.Count == 0)
        {
            throw new ArgumentException("Vectors must not be empty.");
        }

        if (first.Count != second.Count)
        {
            throw new ArgumentException("Vectors must have the same length.");
        }

        double dot = 0;
        double magnitudeFirst = 0;
        double magnitudeSecond = 0;

        for (int i = 0; i < first.Count; i++)
        {
            dot += first[i] * second[i];
            magnitudeFirst += first[i] * first[i];
            magnitudeSecond += second[i] * second[i];
        }

        magnitudeFirst = Math.Sqrt(magnitudeFirst);
        magnitudeSecond = Math.Sqrt(magnitudeSecond);

        if (magnitudeFirst == 0 || magnitudeSecond == 0)
        {
            return 0;
        }

        return dot / (magnitudeFirst * magnitudeSecond);
    }
}
