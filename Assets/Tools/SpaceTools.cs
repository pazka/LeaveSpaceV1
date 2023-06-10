using DataProcessing.AllGP;
using UnityEngine;

namespace Tools
{
    public static class SpaceTools
    {
        public static (float x, float y) GetXYFromAllGpJsonData(AllGpJsonData data)
        {
            var meanMotion = float.Parse(data.MEAN_MOTION);
            var eccentricity = float.Parse(data.ECCENTRICITY);
            var inclination = float.Parse(data.INCLINATION);
            var raOfAscNode = float.Parse(data.RA_OF_ASC_NODE);
            var argOfPericenter = float.Parse(data.ARG_OF_PERICENTER);
            var meanAnomaly = float.Parse(data.MEAN_ANOMALY);
            var semiMajorAxis = float.Parse(data.SEMIMAJOR_AXIS);
            var periapsis = float.Parse(data.PERIAPSIS);

            var x = (int)(semiMajorAxis * Mathf.Cos(meanAnomaly));
            var y = (int)(semiMajorAxis * Mathf.Sin(meanAnomaly));

            return (x * eccentricity, y * eccentricity);
        }
    }
}