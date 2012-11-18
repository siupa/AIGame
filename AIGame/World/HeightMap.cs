//-----------------------------------------------------------------------------
// Copyright (c) 2008 dhpoware. All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIGame
{
    public class HeightMap
    {
        #region Fields
        private int size;
        private int gridSpacing;
        private float minHeight;
        private float maxHeight;
        private float[] heights;
        private Random random;

        #endregion

        #region Properties
        public int Size
        {
            get { return size; }
        }

        public int RealSize
        {
            get { return size * gridSpacing; }
        }

        public int GridSpacing
        {
            get { return gridSpacing; }
        }

        public float[] Heights
        {
            get { return heights; }
        }

        public float MaxHeight
        {
            get { return maxHeight; }
        }

        public float MinHeight
        {
            get { return minHeight; }
        }
        #endregion

        #region Public Methods
        public HeightMap(int size, int gridSpacing, float minHeight, float maxHeight)
        {
            this.size = size;
            this.gridSpacing = gridSpacing;
            this.minHeight = minHeight;
            this.maxHeight = maxHeight;

            heights = null;
            heights = new float[size * size];

            random = new Random();
        }

        /// <summary>
        /// Generates a fractal height field using the diamond-square (midpoint
        /// displacement) algorithm. Note that only square height fields work with
        /// this algorithm.
        /// <para>
        /// Based on article and associated code:
        /// "Fractal Terrain Generation - Midpoint Displacement" by Jason Shankel
        /// (Game Programming Gems I, pp.503-507).
        /// </para>
        /// </summary>
        /// <param name="roughness">
        /// Small roughness values will result in quite flat height maps. Whilst
        /// larger roughness values will result in more mountainous and hilly
        /// height maps. A good default value is 1.2.
        /// </param>
        public void GenerateDiamondSquareFractal(float roughness)
        {
            System.Array.Clear(heights, 0, heights.Length);

            int p1 = 0;
            int p2 = 0;
            int p3 = 0;
            int p4 = 0;
            int mid = 0;
            float dH = size * 0.5f;
            float dHFactor = (float)Math.Pow(2.0, -roughness);
            float minH = 0.0f;
            float maxH = 0.0f;
            float t = 0.0f;

            for (int w = size; w > 0; dH *= dHFactor, w /= 2)
            {
                // Diamond step.
                for (int z = 0; z < size; z += w)
                {
                    for (int x = 0; x < size; x += w)
                    {
                        p1 = HeightIndexAt(x, z);
                        p2 = HeightIndexAt(x + w, z);
                        p3 = HeightIndexAt(x + w, z + w);
                        p4 = HeightIndexAt(x, z + w);
                        mid = HeightIndexAt(x + w / 2, z + w / 2);

                        heights[mid] = random.Next((int)-dH, (int)dH) +
                                       (heights[p1] + heights[p2] +
                                       heights[p3] + heights[p4]) * 0.25f;

                        minH = Math.Min(minH, heights[mid]);
                        maxH = Math.Max(maxH, heights[mid]);
                    }
                }

                // Square step.
                for (int z = 0; z < size; z += w)
                {
                    for (int x = 0; x < size; x += w)
                    {
                        p1 = HeightIndexAt(x, z);
                        p2 = HeightIndexAt(x + w, z);
                        p3 = HeightIndexAt(x + w / 2, z - w / 2);
                        p4 = HeightIndexAt(x + w / 2, z + w / 2);
                        mid = HeightIndexAt(x + w / 2, z);

                        heights[mid] = random.Next((int)-dH, (int)dH) +
                                       (heights[p1] + heights[p2] +
                                       heights[p3] + heights[p4]) * 0.25f;

                        minH = Math.Min(minH, heights[mid]);
                        maxH = Math.Max(maxH, heights[mid]);

                        p1 = HeightIndexAt(x, z);
                        p2 = HeightIndexAt(x, z + w);
                        p3 = HeightIndexAt(x + w / 2, z + w / 2);
                        p3 = HeightIndexAt(x - w / 2, z + w / 2);
                        mid = HeightIndexAt(x, z + w / 2);

                        heights[mid] = random.Next((int)-dH, (int)dH) +
                                       (heights[p1] + heights[p2] +
                                       heights[p3] + heights[p4]) * 0.25f;

                        minH = Math.Min(minH, heights[mid]);
                        maxH = Math.Max(maxH, heights[mid]);
                    }
                }
            }

            Smooth();

            // Normalize heights to range [minHeight, maxHeight].
            for (int i = 0; i < size * size; ++i)
            {
                t = (heights[i] - minH) / (maxH - minH);
                heights[i] = minHeight + (maxHeight - minHeight) * t;
            }
        }

        public void GenerateFromBitmap(Texture2D heightMap, float bumpiness)
        {
            //float min = 100000.0f;
            //float max = -100000.0f;
            //float t = 0.0f;

            size = heightMap.Width;
            heights = new float[size * size];

            Color[] heightMapColors = new Color[size * size];
            heightMap.GetData(heightMapColors);

            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                {
                    heights[x + y * size] = heightMapColors[x + y * size].R * bumpiness;
                    if (heights[x + y * size] > maxHeight)
                        heights[x + y * size] = maxHeight;
                    //min = Math.Min(min, heights[x + y * size]);
                    //max = Math.Max(max, heights[x + y * size]);

                }
            Smooth();

            //// Normalize heights to range [minHeight, maxHeight].
            //for (int i = 0; i < size * size; ++i)
            //{
            //    t = (heights[i] - min) / (max - min);
            //    heights[i] = minHeight + (maxHeight - minHeight) * t;
            //}
        }

        /// <summary>
        /// Given a (x, z) world position on the height map this method
        /// calculates the exact height of the height map at that (x, z)
        /// position using bilinear interpolation.
        /// </summary>
        /// <param name="x">The world x position on the height map.</param>
        /// <param name="z">The world z position on the height map.</param>
        /// <returns>The interpolated height at the (x, z) position.</returns>
        public float HeightAt(float x, float z)
        {
            
            x /= (float)gridSpacing;
            z /= (float)gridSpacing;

            //Debug.Assert(x >= 0.0f && x < (float)size);
            //Debug.Assert(z >= 0.0f && z < (float)size);

            int ix = (int)x;
            int iz = (int)z;
            try
            {
                float topLeft = heights[HeightIndexAt(ix, iz)];
                float topRight = heights[HeightIndexAt(ix + 1, iz)];
                float bottomLeft = heights[HeightIndexAt(ix, iz + 1)];
                float bottomRight = heights[HeightIndexAt(ix + 1, iz + 1)];
                float percentX = x - (float)ix;
                float percentZ = z - (float)iz;

                return topLeft * ((1.0f - percentX) * (1.0f - percentZ)) +
                       topRight * (percentX * (1.0f - percentZ)) +
                       bottomLeft * (percentZ * (1.0f - percentX)) +
                       bottomRight * (percentX * percentZ);
            }
            catch
            {
                return 0;
            }
        }

        public float HeightAtPixel(int x, int z)
        {
            return heights[z * size + x];
        }

        /// <summary>
        /// Given a (x, z) world position on the height map this method
        /// calculates the exact normal of the height map at that (x, z)
        /// position using bilinear interpolation.
        /// </summary>
        /// <param name="x">The world x position on the height map.</param>
        /// <param name="z">The world z position on the height map.</param>
        /// <param name="n">The normal at position (x, z).</param>
        public void NormalAt(float x, float z, out Vector3 n)
        {
            x /= (float)gridSpacing;
            z /= (float)gridSpacing;

            Debug.Assert(x >= 0.0f && x < (float)size);
            Debug.Assert(z >= 0.0f && z < (float)size);

            int ix = (int)x;
            int iz = (int)z;
            float percentX = x - (float)ix;
            float percentZ = z - (float)iz;

            Vector3 topLeft;
            Vector3 topRight;
            Vector3 bottomLeft;
            Vector3 bottomRight;

            NormalAtPixel(ix, iz, out topLeft);
            NormalAtPixel(ix + 1, iz, out topRight);
            NormalAtPixel(ix, iz + 1, out bottomLeft);
            NormalAtPixel(ix + 1, iz + 1, out bottomRight);

            n = topLeft * ((1.0f - percentX) * (1.0f - percentZ)) +
                topRight * (percentX * (1.0f - percentZ)) +
                bottomLeft * (percentZ * (1.0f - percentX)) +
                bottomRight * (percentX * percentZ);

            n.Normalize();
        }

        /// <summary>
        /// Returns the normal at the specified pixel location on the height map.
        /// The normal is calculated using the properties of the height map.
        /// This approach is much quicker and more elegant than triangulating
        /// the height map and averaging triangle surface normals.
        /// </summary>
        /// <param name="x">The x pixel location on the height map.</param>
        /// <param name="z">The z pixel location on the height map.</param>
        /// <param name="n">The normal at pixel location (x, z).</param>
        public void NormalAtPixel(int x, int z, out Vector3 n)
        {
            n = new Vector3();
            if (x > 0 && x < size - 1)
                n.X = HeightAtPixel(x - 1, z) - HeightAtPixel(x + 1, z);
            else if (x > 0)
                n.X = 2.0f * (HeightAtPixel(x - 1, z) - HeightAtPixel(x, z));
            else
                n.X = 2.0f * (HeightAtPixel(x, z) - HeightAtPixel(x + 1, z));

            if (z > 0 && z < size - 1)
                n.Z = HeightAtPixel(x, z - 1) - HeightAtPixel(x, z + 1);
            else if (z > 0)
                n.Z = 2.0f * (HeightAtPixel(x, z - 1) - HeightAtPixel(x, z));
            else
                n.Z = 2.0f * (HeightAtPixel(x, z) - HeightAtPixel(x, z + 1));

            n.Y = 2.0f * gridSpacing;
            n.Normalize();
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Returns the heights index for the specified (x, z) pixel location
        /// on the height map. This method wraps around for pixel locations
        /// larger than the height map's size.
        /// </summary>
        /// <param name="x">The x pixel location on the height map.</param>
        /// <param name="z">The z pixel location on the height map.</param>
        /// <returns>The heights index at pixel location (x, z).</returns>
        private int HeightIndexAt(int x, int z)
        {
            return (((x + size) % size) + ((z + size) % size) * size);
        }

        /// <summary>
        /// Applies a box filter to the height map to smooth it out.
        /// </summary>
        private void Smooth()
        {
            float[] source = new float[heights.Length];
            float value = 0.0f;
            float cellAverage = 0.0f;
            int i = 0;
            int bounds = size * size;

            System.Array.Copy(heights, source, heights.Length);

            for (int y = 0; y < size; ++y)
            {
                for (int x = 0; x < size; ++x)
                {
                    value = 0.0f;
                    cellAverage = 0.0f;

                    i = (y - 1) * size + (x - 1);
                    if (i >= 0 && i < bounds)
                    {
                        value += source[i];
                        cellAverage += 1.0f;
                    }

                    i = (y - 1) * size + x;
                    if (i >= 0 && i < bounds)
                    {
                        value += source[i];
                        cellAverage += 1.0f;
                    }

                    i = (y - 1) * size + (x + 1);
                    if (i >= 0 && i < bounds)
                    {
                        value += source[i];
                        cellAverage += 1.0f;
                    }

                    i = y * size + (x - 1);
                    if (i >= 0 && i < bounds)
                    {
                        value += source[i];
                        cellAverage += 1.0f;
                    }

                    i = y * size + x;
                    if (i >= 0 && i < bounds)
                    {
                        value += source[i];
                        cellAverage += 1.0f;
                    }

                    i = y * size + (x + 1);
                    if (i >= 0 && i < bounds)
                    {
                        value += source[i];
                        cellAverage += 1.0f;
                    }

                    i = (y + 1) * size + (x - 1);
                    if (i >= 0 && i < bounds)
                    {
                        value += source[i];
                        cellAverage += 1.0f;
                    }

                    i = (y + 1) * size + x;
                    if (i >= 0 && i < bounds)
                    {
                        value += source[i];
                        cellAverage += 1.0f;
                    }

                    i = (y + 1) * size + (x + 1);
                    if (i >= 0 && i < bounds)
                    {
                        value += source[i];
                        cellAverage += 1.0f;
                    }

                    heights[y * size + x] = value / cellAverage;
                }
            }
        }
        #endregion
    }
}
