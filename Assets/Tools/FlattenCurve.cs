using System;
using UnityEngine;

namespace Tools
{
    ///
    ///  We start from a point on 1 axis that have been flattened but badly ( i.e. the farther points are closer together and the closer to the center point are farther apart)
    ///
    /// We want to have points spaced from each other according to the circle center they may have belong originally.
    ///
    /// To do that we will proceed in 5 steps :
    /// - 0. We have the point for which we are searching the correct value, called P2(x) and we want the corrected point, called P1(x)
    /// - 1. Specify the supposed center of the circle. We'll call it P0'(x,y), P0(x) being the point on the original X axis
    /// - 2. Get the equation of the corresponding circle, we'll assume that the original X axis is tangent to the circle we are searching. We'll call it C1
    ///       C1(x,y) : (x - P0'x)² + (y - P0'y)² = P0'y² 
    /// - 3. From C1, find the point P2'(x,y) where P2 should have been located originally. For that, search the vertical line (x = P2x) where the equation of the circle = 1 and x >= P0'x
    ///       P2'(x,y) : (P2'x - P0'x)² + (P2'y - P0'y)² = P0'y²
    ///              with P2'y >= P0'y
    ///              with P2'x = P2x
    ///
    ///       Simplified : P2'y = sqrt(P0'y²-(P2x-P0'x)²) + P0'y
    /// 
    /// - 4. Get the linear equation of the line going from P0' to P2', we'll call it Fc(x)
    ///       Fc(x) : ax+b
    ///              with a = ((P2'y-P0'y)/(P2'x-P0'x))
    ///              with b = P0'y
    /// - 5. We'll obtain P1(x) by resolving Fc(x) = 0
    ///      Simplified =  P1(x) = -P0'y / ((P2'y - P0'y)/(P2x-P0'x))
    ///
    /// We need to do this for the X axis and th Y axis for each point at the loading of the dataset and we're done ! 
    ///
    public static class FlattenCurve
    {
        public static float GetFlattenedOneDimensionPoint(float p2, float[] p0Prime)
        {
            return p2;
            //TODO make this work
            
            // step 1 + 2
            float[] p2Prime = new float[2];
            p2Prime[0] = p2;
            // step 3
            p2Prime[1] = (float) Math.Sqrt(Math.Pow(p0Prime[1], 2) - Math.Pow(p2 - p0Prime[0], 2)) + p0Prime[1];
            // step 4 
            float coeffA = (p0Prime[1] - p2Prime[1]) / (p0Prime[0] - p2Prime[0]);
            float coeffB = p0Prime[1];

            // step 5
            float res = coeffB / coeffA;

            return res;
        }

        /// <summary>
        ///  We start from a point on 1 axis that have been flattened but badly ( i.e. the farther points are closer together and the closer to the center point are farther apart)
        ///
        /// We want to have points spaced from each other according to the circle center they may have belong originally.
        /// </summary>
        /// <param name="p2">2d point to flatten</param>
        /// <param name="p0Prime"> Assumption about the original 3d point of the circle , must be float[3] x,y,z</param>
        /// <returns>The re-flattened point</returns>
        public static float[] GetFlattenedTwoDimensionPoint(float[] p2, float[] p0Prime)
        {
            float[] res = new float[2];
            res[0] = GetFlattenedOneDimensionPoint(p2[0], new[] {p0Prime[0], p0Prime[2]});
            res[1] = GetFlattenedOneDimensionPoint(p2[1], new[] {p0Prime[1], p0Prime[2]});
            return res;
        }
    }
}