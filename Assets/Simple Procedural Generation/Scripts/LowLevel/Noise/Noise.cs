using UnityEngine;

namespace SurvivalKit.ProceduralGeneration
{
    [System.Serializable]
    public class Noise
    {
        public enum NoiseType { Simplex, Perlin, Voronoi, Worley }

        [SerializeField]
        private NoiseType m_NoiseType;

        [Header("Permutation")]

        [SerializeField]
        private int m_Size = 1024;

        [SerializeField]
        private int m_Max = 255;

        private PermutationTable m_PermutTable;

        private const float K = 1.0f / 7.0f;
        private const float Ko = 3.0f / 7.0f;

        private static readonly float[] OFFSET_F = new float[] { -0.5f, 0.5f, 1.5f };

        public float Get(float x, float y)
        {
            m_PermutTable = new PermutationTable(m_Size, m_Max, 1);

            switch(m_NoiseType)
            {
                case NoiseType.Simplex:
                    return Simplex(x, y);
                case NoiseType.Perlin:
                    return Mathf.PerlinNoise(x, y);
                case NoiseType.Voronoi:
                    return Voronoi(x, y);
                case NoiseType.Worley:
                    return Worley(x, y);
            }

            return 0f;
        }

        private float Simplex(float x, float y)
        {
            //The 0.5 is to make the scale simliar to the other noise algorithms
            x = x * 0.5f;
            y = y * 0.5f;

            const float F2 = 0.366025403f; // F2 = 0.5*(sqrt(3.0)-1.0)
            const float G2 = 0.211324865f; // G2 = (3.0-Math.sqrt(3.0))/6.0

            float n0, n1, n2; // Noise contributions from the three corners

            // Skew the input space to determine which simplex cell we're in
            float s = (x + y) * F2; // Hairy factor for 2D
            float xs = x + s;
            float ys = y + s;
            int i = (int)Mathf.Floor(xs);
            int j = (int)Mathf.Floor(ys);

            float t = (i + j) * G2;
            float X0 = i - t; // Unskew the cell origin back to (x,y) space
            float Y0 = j - t;
            float x0 = x - X0; // The x,y distances from the cell origin
            float y0 = y - Y0;

            // For the 2D case, the simplex shape is an equilateral triangle.
            // Determine which simplex we are in.
            int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
            if (x0 > y0) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
            else { i1 = 0; j1 = 1; }      // upper triangle, YX order: (0,0)->(0,1)->(1,1)

            // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
            // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
            // c = (3-sqrt(3))/6

            float x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
            float y1 = y0 - j1 + G2;
            float x2 = x0 - 1.0f + 2.0f * G2; // Offsets for last corner in (x,y) unskewed coords
            float y2 = y0 - 1.0f + 2.0f * G2;

            // Calculate the contribution from the three corners
            float t0 = 0.5f - x0 * x0 - y0 * y0;
            if (t0 < 0.0)
                n0 = 0.0f;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Grad(m_PermutTable[i, j], x0, y0);
            }

            float t1 = 0.5f - x1 * x1 - y1 * y1;
            if (t1 < 0.0)
                n1 = 0.0f;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Grad(m_PermutTable[i + i1, j + j1], x1, y1);
            }

            float t2 = 0.5f - x2 * x2 - y2 * y2;
            if (t2 < 0.0)
                n2 = 0.0f;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Grad(m_PermutTable[i + 1, j + 1], x2, y2);
            }

            // Add contributions from each corner to get the final noise value.
            // The result is scaled to return values in the interval [-1,1].
            return 40.0f * (n0 + n1 + n2);
        }

        private float Voronoi(float x, float y)
        {
            //The 0.75 is to make the scale simliar to the other noise algorithms
            x = x * 0.75f;
            y = y * 0.75f;

            int lastRandom, numberFeaturePoints;
            float randomDiffX, randomDiffY;
            float featurePointX, featurePointY;
            int cubeX, cubeY;

            var distanceArray = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

            //1. Determine which cube the evaluation point is in
            var evalCubeX = (int)Mathf.Floor(x);
            var evalCubeY = (int)Mathf.Floor(y);

            for (int i = -1; i < 2; ++i)
            {
                for (int j = -1; j < 2; ++j)
                {
                    cubeX = evalCubeX + i;
                    cubeY = evalCubeY + j;

                    //2. Generate a reproducible random number generator for the cube
                    lastRandom = m_PermutTable[cubeX, cubeY];

                    //3. Determine how many feature points are in the cube
                    numberFeaturePoints = ProbLookup(lastRandom * m_PermutTable.Inverse);

                    //4. Randomly place the feature points in the cube
                    for (int l = 0; l < numberFeaturePoints; ++l)
                    {
                        lastRandom = m_PermutTable[lastRandom];
                        randomDiffX = lastRandom * m_PermutTable.Inverse;

                        lastRandom = m_PermutTable[lastRandom];
                        randomDiffY = lastRandom * m_PermutTable.Inverse;

                        featurePointX = randomDiffX + cubeX;
                        featurePointY = randomDiffY + cubeY;

                        //5. Find the feature point closest to the evaluation point. 
                        //This is done by inserting the distances to the feature points into a sorted list
                        distanceArray = Insert(distanceArray, Distance2(x, y, featurePointX, featurePointY));
                    }

                    //6. Check the neighboring cubes to ensure their are no closer evaluation points.
                    // This is done by repeating steps 1 through 5 above for each neighboring cube
                }
            }

            return distanceArray.x;
        }

        private float Worley(float x, float y)
        {
            int Pi0 = (int)Mathf.Floor(x);
            int Pi1 = (int)Mathf.Floor(y);

            float Pf0 = Frac(x);
            float Pf1 = Frac(y);

            Vector3 pX = new Vector3();
            pX[0] = m_PermutTable[Pi0 - 1];
            pX[1] = m_PermutTable[Pi0];
            pX[2] = m_PermutTable[Pi0 + 1];

            float d0, d1, d2;
            float F0 = float.PositiveInfinity;
            float F1 = float.PositiveInfinity;
            float F2 = float.PositiveInfinity;

            int px, py, pz;
            float oxx, oxy, oxz;
            float oyx, oyy, oyz;

            for (int i = 0; i < 3; i++)
            {
                px = m_PermutTable[(int)pX[i], Pi1 - 1];
                py = m_PermutTable[(int)pX[i], Pi1];
                pz = m_PermutTable[(int)pX[i], Pi1 + 1];

                oxx = Frac(px * K) - Ko;
                oxy = Frac(py * K) - Ko;
                oxz = Frac(pz * K) - Ko;

                oyx = Mod(Mathf.Floor(px * K), 7.0f) * K - Ko;
                oyy = Mod(Mathf.Floor(py * K), 7.0f) * K - Ko;
                oyz = Mod(Mathf.Floor(pz * K), 7.0f) * K - Ko;

                d0 = Distance2(Pf0, Pf1, OFFSET_F[i] + 1 * oxx, -0.5f + 1 * oyx);
                d1 = Distance2(Pf0, Pf1, OFFSET_F[i] + 1 * oxy, 0.5f + 1 * oyy);
                d2 = Distance2(Pf0, Pf1, OFFSET_F[i] + 1 * oxz, 1.5f + 1 * oyz);

                if (d0 < F0) { F2 = F1; F1 = F0; F0 = d0; }
                else if (d0 < F1) { F2 = F1; F1 = d0; }
                else if (d0 < F2) { F2 = d0; }

                if (d1 < F0) { F2 = F1; F1 = F0; F0 = d1; }
                else if (d1 < F1) { F2 = F1; F1 = d1; }
                else if (d1 < F2) { F2 = d1; }

                if (d2 < F0) { F2 = F1; F1 = F0; F0 = d2; }
                else if (d2 < F1) { F2 = F1; F1 = d2; }
                else if (d2 < F2) { F2 = d2; }

            }

            return F0;
        }

        private static float Mod(float x, float y)
        {
            return x - y * Mathf.Floor(x / y);
        }

        private static float Frac(float v)
        {
            return v - Mathf.Floor(v);
        }

        private static float Distance2(float p1x, float p1y, float p2x, float p2y)
        {
            return (p1x - p2x) * (p1x - p2x) + (p1y - p2y) * (p1y - p2y);
        }

        /// <summary>
        /// Given a uniformly distributed random number this function returns the number of feature points in a given cube.
        /// </summary>
        /// <param name="value">a uniformly distributed random number</param>
        /// <returns>The number of feature points in a cube.</returns>
        private static int ProbLookup(float value)
        {
            //Poisson Distribution
            if (value < 0.0915781944272058) return 1;
            if (value < 0.238103305510735) return 2;
            if (value < 0.433470120288774) return 3;
            if (value < 0.628836935299644) return 4;
            if (value < 0.785130387122075) return 5;
            if (value < 0.889326021747972) return 6;
            if (value < 0.948866384324819) return 7;
            if (value < 0.978636565613243) return 8;

            return 9;
        }

        /// <summary>
        /// Inserts value into array using insertion sort. If the value is greater than the largest value in the array
        /// it will not be added to the array.
        /// </summary>
        /// <param name="arr">The array to insert the value into.</param>
        /// <param name="value">The value to insert into the array.</param>
        private static Vector3 Insert(Vector3 arr, float value)
        {
            float temp;
            for (int i = 3 - 1; i >= 0; i--)
            {
                if (value > arr[i]) break;
                temp = arr[i];
                arr[i] = value;
                if (i + 1 < 3) arr[i + 1] = temp;
            }

            return arr;
        }

        private float Grad(int hash, float x, float y)
        {
            int h = hash & 7;           // Convert low 3 bits of hash code
            float u = h < 4 ? x : y;     // into 8 simple gradient directions,
            float v = h < 4 ? y : x;     // and compute the dot product with (x,y).
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
        }
    }
}