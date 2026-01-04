namespace AiBackendDemo.Extensions
{
    public static class DoubleEmbeddingExtensions
    {
        // Cosine similarity for double[]
        public static double CosineSimilarity(this double[] a, double[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return double.NaN;

            double dot = 0.0;
            double mag1 = 0.0;
            double mag2 = 0.0;

            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                mag1 += a[i] * a[i];
                mag2 += b[i] * b[i];
            }

            return dot / (System.Math.Sqrt(mag1) * System.Math.Sqrt(mag2));
        }

        // Cosine similarity for float[]
        public static double CosineSimilarity(this float[]? embedding, float[]? other)
        {
            if (embedding == null || other == null || embedding.Length != other.Length)
                return double.NaN;

            double dot = 0.0;
            double mag1 = 0.0;
            double mag2 = 0.0;

            for (int i = 0; i < embedding.Length; i++)
            {
                dot += embedding[i] * other[i];
                mag1 += embedding[i] * embedding[i];
                mag2 += other[i] * other[i];
            }

            return dot / (System.Math.Sqrt(mag1) * System.Math.Sqrt(mag2));
        }
    }
}