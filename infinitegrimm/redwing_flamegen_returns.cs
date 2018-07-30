using UnityEngine;
using Random = System.Random;

namespace infinitegrimm
{
    public class redwing_flamegen_returns
    {
        public readonly Texture2D[] firePillars = new Texture2D[3];
        private readonly Random rng = new Random();
        
        //private const double OPACITY_MASK = 1.0;

        // What are the fire colors anyway?
        private readonly Color[] flameIntensityCurve = { Color.red, new Color(1f, 0.63f, 0.26f), Color.white, Color.white };
        
        // At what point do you switch from color X to color Y.
        private readonly double[] flameIntensityThresholds = { 0.4, 1.4, 2.0, 2.6 };


        public redwing_flamegen_returns(int pillarWidth, int pillarHeight, int interpolatePx)
        {
            firePillars[0] = generateFirePillar(pillarWidth, pillarHeight, interpolatePx, false);
            firePillars[1] = generateFirePillar(pillarWidth, pillarHeight, interpolatePx, true);
            firePillars[2] = generateFireCeiling(pillarHeight, pillarWidth, interpolatePx, true);
            infinite_globals.log("Generated fire textures for NKG hardmode. but /waj/");
        }
        
        
        
        private Texture2D generateFirePillar(int width, int height, int interpolatePx, bool mirrored)
        {
            Texture2D fp = new Texture2D(width, height);
            double[] horzIntensity150 = new double[height];
            double[] horzOpacity150 = new double[height];
            // RNG phase
            for (int i = 0; i < height; i++)
            {
                if (i % interpolatePx != 0) continue;
                horzIntensity150[i] = rng.NextDouble();
                horzOpacity150[i] = rng.NextDouble();

                // because c# sucks NextDouble can't return arbitrary numbers
                // so apply a transformation to map verticalIntensity150 -> 0-0.2
                // and verticalOpacity150 -> -1 - 0
                horzOpacity150[i] = horzOpacity150[i] * 0.2 - 0.2;

                horzIntensity150[i] = (horzIntensity150[i] * 0.2);
            }

            // Interpolation phase
            for (int i = 0; i < height - interpolatePx; i++)
            {
                if (i % interpolatePx == 0) continue;
                int offset = i % interpolatePx;
                double avgWeighting = (double)offset / (double)interpolatePx;

                horzIntensity150[i] = horzIntensity150[i - offset + interpolatePx] * avgWeighting + horzIntensity150[i - offset] * (1.0 - avgWeighting);
                horzOpacity150[i] = horzOpacity150[i - offset + interpolatePx] * avgWeighting + horzOpacity150[i - offset] * (1.0 - avgWeighting);
            }

            // Interpolation phase pt 2 (for wrap around)
            for (int i = height - interpolatePx; i < height; i++)
            {
                if (i % interpolatePx == 0) continue;
                int offset = i % interpolatePx;
                double avgWeighting = (double)offset / (double)interpolatePx;

                horzIntensity150[i] = horzIntensity150[0] * avgWeighting + horzIntensity150[i - offset] * (1.0 - avgWeighting);
                horzOpacity150[i] = horzOpacity150[0] * avgWeighting + horzOpacity150[i - offset] * (1.0 - avgWeighting);
            }
            // Actually set the colors
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!mirrored)
                    {
                        fp.SetPixel(x, y, getFireColor
                        ((x), horzIntensity150[y], horzOpacity150[y],
                            width, width * 5, 2.4, 9.0));
                    }
                    else
                    {
                        fp.SetPixel(x, y, getFireColor
                        ((width - x), horzIntensity150[y], horzOpacity150[y],
                            width, width * 5, 2.4, 9.0));
                    }
                }
            }
            return fp;
        }
        
        private Texture2D generateFireCeiling(int width, int height, int interpolatePx, bool mirrored)
        {
            Texture2D fp = new Texture2D(width, height);
            double[] horzIntensity150 = new double[width];
            double[] horzOpacity150 = new double[width];
            // RNG phase
            for (int i = 0; i < width; i++)
            {
                if (i % interpolatePx != 0) continue;
                horzIntensity150[i] = rng.NextDouble();
                horzOpacity150[i] = rng.NextDouble();

                // because c# sucks NextDouble can't return arbitrary numbers
                // so apply a transformation to map verticalIntensity150 -> 0-0.2
                // and verticalOpacity150 -> -1 - 0
                horzOpacity150[i] = horzOpacity150[i] * 0.2 - 0.2;

                horzIntensity150[i] = (horzIntensity150[i] * 0.2);
            }

            // Interpolation phase
            for (int i = 0; i < width - interpolatePx; i++)
            {
                if (i % interpolatePx == 0) continue;
                int offset = i % interpolatePx;
                double avgWeighting = (double)offset / (double)interpolatePx;

                horzIntensity150[i] = horzIntensity150[i - offset + interpolatePx] * avgWeighting + horzIntensity150[i - offset] * (1.0 - avgWeighting);
                horzOpacity150[i] = horzOpacity150[i - offset + interpolatePx] * avgWeighting + horzOpacity150[i - offset] * (1.0 - avgWeighting);
            }

            // Interpolation phase pt 2 (for wrap around)
            for (int i = width - interpolatePx; i < width; i++)
            {
                if (i % interpolatePx == 0) continue;
                int offset = i % interpolatePx;
                double avgWeighting = (double)offset / (double)interpolatePx;

                horzIntensity150[i] = horzIntensity150[0] * avgWeighting + horzIntensity150[i - offset] * (1.0 - avgWeighting);
                horzOpacity150[i] = horzOpacity150[0] * avgWeighting + horzOpacity150[i - offset] * (1.0 - avgWeighting);
            }
            // Actually set the colors
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!mirrored)
                    {
                        fp.SetPixel(x, y, getFireColor
                        ((y), horzIntensity150[x], horzOpacity150[x],
                            width, width * 5, 2.4, 9.0));
                    }
                    else
                    {
                        fp.SetPixel(x, y, getFireColor
                        ((width - y), horzIntensity150[x], horzOpacity150[x],
                            width, width * 5, 2.4, 9.0));
                    }
                }
            }
            return fp;
        }
        
        
        
        private Color getFireColor(double distance, double intensity400, double opacity400, int intensInterpolateDist,
            double opacInterpDist, double intensWighting, double opacSharpness)
        {
            double intensity = getRealIntensity(distance, intensity400, intensInterpolateDist, intensWighting);
            // ReSharper disable once UseObjectOrCollectionInitializer because it looks dumb
            Color c = new Color();

            c.a = (float)getRealOpacity(distance, opacity400, opacInterpDist, opacSharpness);

            if (intensity < flameIntensityThresholds[1])
            {
                double intensitySeparation = flameIntensityThresholds[1] - flameIntensityThresholds[0];
                double intens1 = (intensity - flameIntensityThresholds[0]) / intensitySeparation;
                double intens2 = 1.0 - intens1;
                c.r = (flameIntensityCurve[0].r * (float)intens2) + (flameIntensityCurve[1].r * (float)intens1);
                c.g = (flameIntensityCurve[0].g * (float)intens2) + (flameIntensityCurve[1].g * (float)intens1);
                c.b = (flameIntensityCurve[0].b * (float)intens2) + (flameIntensityCurve[1].b * (float)intens1);
            } else if (intensity < flameIntensityThresholds[2])
            {
                double intensitySeparation = flameIntensityThresholds[2] - flameIntensityThresholds[1];
                double intens1 = (intensity - flameIntensityThresholds[1]) / intensitySeparation;
                double intens2 = 1.0 - intens1;
                c.r = (flameIntensityCurve[1].r * (float)intens2) + (flameIntensityCurve[2].r * (float)intens1);
                c.g = (flameIntensityCurve[1].g * (float)intens2) + (flameIntensityCurve[2].g * (float)intens1);
                c.b = (flameIntensityCurve[1].b * (float)intens2) + (flameIntensityCurve[2].b * (float)intens1);
            } else
            {
                double intensitySeparation = flameIntensityThresholds[3] - flameIntensityThresholds[2];
                double intens1 = (intensity - flameIntensityThresholds[2]) / intensitySeparation;
                double intens2 = 1.0 - intens1;
                c.r = (flameIntensityCurve[2].r * (float)intens2) + (flameIntensityCurve[3].r * (float)intens1);
                c.g = (flameIntensityCurve[2].g * (float)intens2) + (flameIntensityCurve[3].g * (float)intens1);
                c.b = (flameIntensityCurve[2].b * (float)intens2) + (flameIntensityCurve[3].b * (float)intens1);
            }
            return c;
        }

        private static double getRealOpacity(double distance, double opacity400, double interpolateDistance, double opacSharpness)
        {
            if (distance < 0.0)
            {
                distance = -distance;
            }
            double averageWeighting = distance / interpolateDistance;
            double opactReal = opacity400 * averageWeighting + ((1.0 - averageWeighting) * opacSharpness);
            if (opactReal < 0.0)
            {
                opactReal = 0.0;
            } else if (opactReal >= 1.0)
            {
                opactReal = 1.0;
            }

/*
            opactReal *= OPACITY_MASK;
*/

            return 1.0;
        }

        private static double getRealIntensity(double distance, double intensity400, int interpolateDistance,
            double intensityWeighting)
        {
            if (distance < 0.0)
            {
                distance = -distance;
            }
            double averageWeighting = distance / (double)interpolateDistance;
            double intenReal = intensity400 * averageWeighting + ((1.0 - averageWeighting) * intensityWeighting);
            if (intenReal < 0.0)
            {
                intenReal = 0.0;
            } else if (intenReal >= 3.0)
            {
                intenReal = 3.0;
            }
            return intenReal;
        }
    }
}