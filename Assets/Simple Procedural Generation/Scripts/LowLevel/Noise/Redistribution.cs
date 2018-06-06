using UnityEngine;

namespace SurvivalKit.PCG
{
    [System.Serializable]
    public class Redistribution
    {
        public enum RedistributionType { None, Power, Sin, Cos, Atan, OneOver, ExponentialGrowth, ExponentialDecay }

        [SerializeField]
        private RedistributionType m_RedistributionType;

        [SerializeField]
        private float m_Value;

        [SerializeField]
        private float m_TerraceValue;

        public float Get(float nValue)
        {
            float newV = nValue;

            //Smooth cliffs.
            switch (m_RedistributionType)
            {
                case RedistributionType.Power:
                    newV = Mathf.Pow(nValue, m_Value);
                    break;
                case RedistributionType.Sin:
                    newV = Mathf.Sin(nValue * m_Value);
                    break;
                case RedistributionType.Cos:
                    newV = Mathf.Cos(nValue * m_Value);
                    break;
                case RedistributionType.Atan:
                    newV = Mathf.Atan(nValue * m_Value);
                    break;
                case RedistributionType.OneOver:
                    newV = 1 / (nValue * m_Value);
                    break;
                case RedistributionType.ExponentialGrowth:
                    newV = Mathf.Pow(m_Value, nValue);
                    break;
                case RedistributionType.ExponentialDecay:
                    newV = Mathf.Pow(m_Value, -nValue);
                    break;
            }

            if(m_TerraceValue != 0)
                newV = Mathf.Round(newV * m_TerraceValue) * m_TerraceValue;

            return newV;
        }
    }
}