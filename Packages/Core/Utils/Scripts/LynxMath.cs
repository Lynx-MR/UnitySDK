//   ==============================================================================
//   | Lynx Interfaces (2023)                                                     |
//   |======================================                                      |
//   | Vector Methods                                                             |
//   | This class contains some vectors methods.                                  |
//   ==============================================================================

using UnityEngine;

namespace Lynx
{
    public class LynxMath
    {
        /// <summary>
        /// Call this function to inverse a Vector3.
        /// </summary>
        /// <param name="vector">Vector3 to inverse.</param>
        public static Vector3 InverseVector(Vector3 vector)
        {
            Vector3 inversedVector = new Vector3();
            inversedVector.x = 1f / vector.x;
            inversedVector.y = 1f / vector.y;
            inversedVector.z = 1f / vector.z;
            return inversedVector;
        }


        #region EASINGS

        /// <summary>
        /// Different type of easing
        /// </summary>
        public enum easingType
        {
            SinIn,
            SinOut,
            SinInOut,

            QuadIn,
            QuadOut,
            QuadInOut,

            CubicIn,
            CubicOut,
            CubicInOut,

            QuartIn,
            QuartOut,
            QuartInOut,

            QuintIn,
            QuintOurt,
            QuintInOut,

            ExpoIn,
            ExpoOut,
            ExpoInOut,

            CircIn,
            CircOut,
            CircInOut,

            BackIn,
            BackOut,
            BackInOut,

            ElasticIn,
            ElasticOut,
            ElasticInOut,

            BounceIn,
            BounceOut,
            BounceInOut
        };


        /// <summary>
        /// Ease a value (between 0 and 1) 
        /// </summary>
        /// <param name="x">Value to ease</param>
        /// <param name="type">Type of easing</param>
        /// <returns></returns>
        public static float Ease(float x, easingType type)
        {
            switch (type)
            {
                case easingType.SinIn:
                    return 1 - Mathf.Cos((x * Mathf.PI) / 2);
                case easingType.SinOut:
                    return Mathf.Sin((x * Mathf.PI) / 2);
                case easingType.SinInOut:
                    return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;


                case easingType.QuadIn:
                    return x * x;
                case easingType.QuadOut:
                    return 1 - (1 - x) * (1 - x);
                case easingType.QuadInOut:
                    return x < 0.5 ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2;


                case easingType.CubicIn:
                    return x * x * x;
                case easingType.CubicOut:
                    return 1 - Mathf.Pow(1 - x, 3);
                case easingType.CubicInOut:
                    return x < 0.5 ? 4 * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 3) / 2;


                case easingType.QuartIn:
                    return x * x * x * x;
                case easingType.QuartOut:
                    return 1 - Mathf.Pow(1 - x, 4);
                case easingType.QuartInOut:
                    return x < 0.5 ? 8 * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 4) / 2;


                case easingType.QuintIn:
                    return x * x * x * x * x;
                case easingType.QuintOurt:
                    return 1 - Mathf.Pow(1 - x, 5);
                case easingType.QuintInOut:
                    return x < 0.5 ? 16 * x * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2;


                case easingType.ExpoIn:
                    return x == 0 ? 0 : Mathf.Pow(2, 10 * x - 10);
                case easingType.ExpoOut:
                    return x == 1 ? 1 : 1 - Mathf.Pow(2, -10 * x);
                case easingType.ExpoInOut:
                    return x == 0 ? 0 : x == 1 ? 1 : x < 0.5 ? Mathf.Pow(2, 20 * x - 10) / 2 : (2 - Mathf.Pow(2, -20 * x + 10)) / 2;


                case easingType.CircIn:
                    return 1 - Mathf.Sqrt(1 - Mathf.Pow(x, 2));
                case easingType.CircOut:
                    return Mathf.Sqrt(1 - Mathf.Pow(x - 1, 2));
                case easingType.CircInOut:
                    return x < 0.5 ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * x, 2))) / 2 : (Mathf.Sqrt(1 - Mathf.Pow(-2 * x + 2, 2)) + 1) / 2;


                case easingType.BackIn:
                    return 2.70158f * x * x * x - 1.70158f * x * x;
                case easingType.BackOut:
                    return 1 + 2.70158f * Mathf.Pow(x - 1, 3) + 1.70158f * Mathf.Pow(x - 1, 2);
                case easingType.BackInOut:
                    return x < 0.5
                        ? (Mathf.Pow(2 * x, 2) * ((3.22658f + 1) * 2 * x - 3.22658f)) / 2
                        : (Mathf.Pow(2 * x - 2, 2) * ((3.22658f + 1) * (x * 2 - 2) + 3.22658f) + 2) / 2;


                case easingType.ElasticIn:
                    return x == 0
                          ? 0
                          : x == 1
                          ? 1
                          : -Mathf.Pow(2, 10 * x - 10) * Mathf.Sin((x * 10 - 10.75f) * 2.09439f);
                case easingType.ElasticOut:
                    return x == 0
                          ? 0
                          : x == 1
                          ? 1
                          : Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * 2.09439f) + 1;
                case easingType.ElasticInOut:
                    return x == 0
                          ? 0
                          : x == 1
                          ? 1
                          : x < 0.5
                          ? -(Mathf.Pow(2, 20 * x - 10) * Mathf.Sin((20 * x - 11.125f) * 1.396263f)) / 2
                          : (Mathf.Pow(2, -20 * x + 10) * Mathf.Sin((20 * x - 11.125f) * 1.396263f)) / 2 + 1;


                case easingType.BounceIn:
                    {
                        float n1 = 7.5625f;
                        float d1 = 2.75f;
                        float r;
                        float w = 1 - x;
                        if (x < 1 / d1)
                        {
                            r = n1 * w * w;
                        }
                        else if (w < 2 / d1)
                        {
                            r = n1 * (w -= 1.5f / d1) * w + 0.75f;
                        }
                        else if (x < 2.5 / d1)
                        {
                            r = n1 * (w -= 2.25f / d1) * w + 0.9375f;
                        }
                        else
                        {
                            r = n1 * (w -= 2.625f / d1) * w + 0.984375f;
                        }
                        return 1 - r;
                    };
                case easingType.BounceOut:
                    {
                        float n1 = 7.5625f;
                        float d1 = 2.75f;

                        if (x < 1 / d1)
                        {
                            return n1 * x * x;
                        }
                        else if (x < 2 / d1)
                        {
                            return n1 * (x -= 1.5f / d1) * x + 0.75f;
                        }
                        else if (x < 2.5 / d1)
                        {
                            return n1 * (x -= 2.25f / d1) * x + 0.9375f;
                        }
                        else
                        {
                            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
                        }
                    }
                case easingType.BounceInOut:
                    {
                        float n1 = 7.5625f;
                        float d1 = 2.75f;
                        if (x < 0.5)
                        {
                            float r;
                            float w = 1-2*x;
                            if (x < 1 / d1)
                            {
                                r = n1 * w * w;
                            }
                            else if (w < 2 / d1)
                            {
                                r = n1 * (w -= 1.5f / d1) * w + 0.75f;
                            }
                            else if (x < 2.5 / d1)
                            {
                                r = n1 * (w -= 2.25f / d1) * w + 0.9375f;
                            }
                            else
                            {
                                r = n1 * (w -= 2.625f / d1) * w + 0.984375f;
                            }
                            return (1 - r)/2;
                        }
                        else
                        {
                            float r;
                            float w = 1*x -1;
                            if (x < 1 / d1)
                            {
                                r = n1 * w * w;
                            }
                            else if (w < 2 / d1)
                            {
                                r = n1 * (w -= 1.5f / d1) * w + 0.75f;
                            }
                            else if (x < 2.5 / d1)
                            {
                                r = n1 * (w -= 2.25f / d1) * w + 0.9375f;
                            }
                            else
                            {
                                r = n1 * (w -= 2.625f / d1) * w + 0.984375f;
                            }
                            return (1 + r) / 2;
                        }
                    };
                default:
                    return x;
            }
        }
        #endregion
    }
}
