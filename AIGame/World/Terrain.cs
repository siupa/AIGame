using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AIGame
{
    public class Terrain
    {
        #region Nested types
        public struct Region
        {
            public float Min;
            public float Max;
            public Texture2D ColorMap;
        }
        public enum DrawingTechnique
        {
            Texture,
            Wireframe,
            TextureAndWireframe
        }
        #endregion

        #region Fields
        private GraphicsDevice graphicsDevice;
        private ContentManager content;
        private HeightMap heightMap;
        private List<Region> regions;
        private VertexPositionNormalTexture[] vertices;
        private ushort[] indices16Bit;
        private uint[] indices32Bit;
        private IndexBuffer indexBuffer;
        // NOTE: migration to XNA 4.0 private DynamicVertexBuffer vertexBuffer;
        private DynamicVertexBuffer vertexBuffer;
        // NOTE: migration to XNA 4.0 private VertexDeclaration vertexDeclaration;
        private BoundingBox boundingBox;
        private BoundingSphere boundingSphere;

        //Triangles used for the ray intersection detection
        public Tri[] triangle;
        public struct Tri
        {
            public int id;
            public Vector3 p1;
            public Vector3 p2;
            public Vector3 p3;
            public Vector3 normal;
        }

        private Effect effect;
        public static DrawingTechnique currentTechnique = DrawingTechnique.Texture;

        private Vector3 sunlightDirection;
        private Vector4 sunlightColor;
        private Vector4 terrainAmbient;
        private Vector4 terrainDiffuse;
        private float terrainTilingFactor;

        public Vector3 groundCursorPosition = Vector3.Zero;
        public Vector3 lastGroundCursorPos = Vector3.Zero;
        private Texture2D groundCursorTex;
        public int groundCursorSize = 10;
        public int groundCursorStrength = 20;
        public bool showGroundCursor = true;
        public bool isGroundCursorActivated = true;
        #endregion

        #region Properties
        public Vector3 SunlightDirection
        {
            get { return sunlightDirection; }
            set { sunlightDirection = value; }
        }

        public Vector4 SunlightColor
        {
            get { return sunlightColor; }
            set { sunlightColor = value; }
        }

        public Vector4 TerrainAmbient
        {
            get { return terrainAmbient; }
            set { terrainAmbient = value; }
        }

        public BoundingBox TerrainBoundingBox
        {
            get { return boundingBox; }
        }

        public BoundingSphere TerrainBoundingSphere
        {
            get { return boundingSphere; }
        }

        public Vector3 TerrainCenter
        {
            get
            {
                Vector3 center = new Vector3();
                float worldSize = heightMap.Size * heightMap.GridSpacing;

                center.X = worldSize * 0.5f;
                center.Z = worldSize * 0.5f;
                center.Y = heightMap.HeightAt(center.X, center.Z);

                return center;
            }
        }
        public Vector3 TerrainCornerS
        {
            get { return new Vector3(0, heightMap.HeightAt(0, 0), 0); }
        }
        public Vector3 TerrainCornerW
        {
            get { return new Vector3(0, heightMap.HeightAt(0, heightMap.RealSize), heightMap.RealSize); }
        }
        public Vector3 TerrainCornerE
        {
            get { return new Vector3(heightMap.RealSize, heightMap.HeightAt(heightMap.RealSize, 0), 0); }
        }
        public Vector3 TerrainCornerN
        {
            get { return new Vector3(heightMap.RealSize, heightMap.HeightAt(heightMap.RealSize, heightMap.RealSize), heightMap.RealSize); }
        }

        public Vector4 TerrainDiffuse
        {
            get { return terrainDiffuse; }
            set { terrainDiffuse = value; }
        }

        public HeightMap TerrainHeightMap
        {
            get { return heightMap; }
        }

        public List<Region> TerrainRegions
        {
            get { return regions; }
        }

        public float TerrainSize
        {
            get { return heightMap.Size * heightMap.GridSpacing; }
        }

        public float TerrainTilingFactor
        {
            get { return terrainTilingFactor; }
            set { terrainTilingFactor = value; }
        }

        /// <summary>
        /// Get or set property indicating if the ground cursor is visible or not.
        /// </summary>
        public bool ShowGroundCursor
        {
            get { return showGroundCursor; }
            set { showGroundCursor = value; }
        }
        #endregion

        #region Public methods
        public Terrain(GraphicsDevice graphicsDevice, ContentManager content)
        {
            this.graphicsDevice = graphicsDevice;
            this.content = content;

            regions = new List<Region>();

            sunlightDirection = new Vector3(1, 1, 1);// Vector3.Down;
            sunlightColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

            terrainAmbient = new Vector4(0.3f, 0.3f, 0.3f, 1.0f);
            terrainDiffuse = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
            terrainTilingFactor = 12.0f;

            groundCursorTex = content.Load<Texture2D>(@"Textures\Icons\groundCursor");
        }

        public Vector3? IsIntersected(Ray ray)
        {
            float rayLength = 0f;
            for (int i = 0; i < triangle.Length; i++)
            {
                Tri thisTri = triangle[i];
                if (MathExtra.Intersects(ray, thisTri.p1, thisTri.p3, thisTri.p2, thisTri.normal, false, true, out rayLength))
                {
                    Vector3 rayTarget = ray.Position + ray.Direction * rayLength;
                    return rayTarget;
                }
            }
            return null;
        }
        public void ToggleDrawingTechnique()
        {
            if (currentTechnique == DrawingTechnique.Texture)
                currentTechnique = DrawingTechnique.TextureAndWireframe;
            else if (currentTechnique == DrawingTechnique.TextureAndWireframe)
                currentTechnique = DrawingTechnique.Wireframe;
            else if (currentTechnique == DrawingTechnique.Wireframe)
                currentTechnique = DrawingTechnique.Texture;
        }

        //public Ray GetPickRay()
        //{
        //    MouseState mouseState = Mouse.GetState();

        //    int mouseX = mouseState.X;
        //    int mouseY = mouseState.Y;

        //    float width = AIGame.graphics.GraphicsDevice.Viewport.Width;
        //    float height = AIGame.graphics.GraphicsDevice.Viewport.Height;

        //    double screenSpaceX = ((float)mouseX / (width / 2) - 1.0f) * AIGame.camera.AspectRatio;
        //    double screenSpaceY = (1.0f - (float)mouseY / (height / 2)) ;

        //    double viewRatio = Math.Tan(AIGame.camera.Fovx / 2);


        //    screenSpaceX = screenSpaceX * viewRatio;
        //    screenSpaceY = screenSpaceY * viewRatio;

        //    Vector3 cameraSpaceNear = new Vector3((float)(screenSpaceX * AIGame.camera.NearPlane), (float)(screenSpaceY * AIGame.camera.NearPlane), (float)(-AIGame.camera.NearPlane));
        //    Vector3 cameraSpaceFar = new Vector3((float)(screenSpaceX * AIGame.camera.FarPlane), (float)(screenSpaceY * AIGame.camera.FarPlane), (float)(-AIGame.camera.FarPlane));

        //    Matrix invView = Matrix.Invert(AIGame.camera.ViewMatrix);
        //    Vector3 worldSpaceNear = Vector3.Transform(cameraSpaceNear, invView);
        //    Vector3 worldSpaceFar = Vector3.Transform(cameraSpaceFar, invView);

        //    Ray pickRay = new Ray(worldSpaceNear, worldSpaceFar - worldSpaceNear);

        //    return new Ray(pickRay.Position, Vector3.Normalize(pickRay.Direction));
        //}


        public void AddRegion(float min, float max, string colorMapAssetName)
        {
            // Add the new terrain region.

            Region region = new Region();

            region.Min = min;
            region.Max = max;
            region.ColorMap = content.Load<Texture2D>(colorMapAssetName);

            if (region.ColorMap.Width != Settings.TERRAIN_TEXTURE_SIZE || region.ColorMap.Height != Settings.TERRAIN_TEXTURE_SIZE)
            {
                throw new Exception(
                    "Invalid texture (" + region.ColorMap.Name + ") size!" +
                    "Valid size is " + Settings.TERRAIN_TEXTURE_SIZE + "x" + Settings.TERRAIN_TEXTURE_SIZE + ".");
            }

            regions.Add(region);

            // Calculate the new terrain tiling factor.

            float averageColorMapWidth = 0.0f;
            float averageColorMapHeight = 0.0f;

            foreach (Region r in regions)
            {
                averageColorMapWidth += r.ColorMap.Width;
                averageColorMapHeight += r.ColorMap.Height;
            }

            averageColorMapWidth /= regions.Count;
            averageColorMapHeight /= regions.Count;

            terrainTilingFactor = (float)Math.Min(averageColorMapWidth, averageColorMapHeight);
            terrainTilingFactor = TerrainSize / terrainTilingFactor;
        }

        public enum HeightMapGenerationMethod
        {
            DiamondSquareFractal,
            FromBitMap
        }

        public void Create(int size, int gridSpacing, float minHeight, float maxHeight, HeightMapGenerationMethod method, float bumpiness)
        {
            Texture2D terrain_bitmap = null;
            if (method == HeightMapGenerationMethod.FromBitMap)
            {
                terrain_bitmap = content.Load<Texture2D>("terrain");
                size = terrain_bitmap.Width;
            }

            heightMap = new HeightMap(size, gridSpacing, minHeight, maxHeight);

            // Generate the terrain vertices.

            int totalVertices = size * size;
            //int totalIndices = (size - 1) * (size * 2 + 1);
            int totalIndices = (size - 1) * (size - 1) * 6;


            // NOTE: migration to XNA 4.0
            // NOTE: vertexDeclaration = new VertexDeclaration(graphicsDevice,
            // NOTE:                     VertexPositionNormalTexture.VertexElements);
            // NOTE: 
            // NOTE: vertices = new VertexPositionNormalTexture[totalVertices];
            // NOTE: 
            // NOTE: vertexBuffer = new DynamicVertexBuffer(graphicsDevice,
            // NOTE:                VertexPositionNormalTexture.SizeInBytes * vertices.Length,
            // NOTE:                BufferUsage.WriteOnly);
            // NOTE: 
            vertices = new VertexPositionNormalTexture[totalVertices];

            vertexBuffer = new DynamicVertexBuffer(graphicsDevice, typeof(VertexPositionNormalTexture), vertices.Length, BufferUsage.WriteOnly);
            //vertexBuffer.ContentLost += new EventHandler(vertexBuffer_ContentLost);
            //vertexBuffer.SetData(particles);

            //vertexBuffer = new VertexBuffer(graphicsDevice, typeof(VertexBuffer), vertices.Length, BufferUsage.WriteOnly);

            switch (method)
            {
                case HeightMapGenerationMethod.DiamondSquareFractal:
                    GenerateUsingDiamondSquareFractal(1.2f);
                    break;
                case HeightMapGenerationMethod.FromBitMap:
                    GenerateFromBitmap(terrain_bitmap, bumpiness);
                    break;
                default:
                    GenerateUsingDiamondSquareFractal(1.2f);
                    break;
            }

            // Generete the terrain triangle indices.
            if (Using16BitIndices())
            {
                indices16Bit = new ushort[totalIndices];
                GenerateIndices();

                indexBuffer = new IndexBuffer(graphicsDevice,
                              typeof(ushort),
                              indices16Bit.Length,
                              BufferUsage.WriteOnly);

                indexBuffer.SetData(indices16Bit);
            }
            else
            {
                indices32Bit = new uint[totalIndices];
                GenerateIndices();

                indexBuffer = new IndexBuffer(graphicsDevice,
                              typeof(uint),
                              indices32Bit.Length,
                              BufferUsage.WriteOnly);

                indexBuffer.SetData(indices32Bit);
            }

            // Calculate the bounding box and bounding sphere for the terrain.

            Vector3 min = new Vector3(), max = new Vector3();

            min.X = 0.0f;
            min.Y = heightMap.MinHeight;
            min.Z = 0.0f;

            max.X = heightMap.Size * heightMap.GridSpacing;
            max.Y = heightMap.MaxHeight;
            max.Z = heightMap.Size * heightMap.GridSpacing;

            boundingBox = new BoundingBox(min, max);
            boundingSphere = BoundingSphere.CreateFromBoundingBox(boundingBox);

            // Load the effect file used to render the terrain.

            try
            {
                effect = content.Load<Effect>(@"Shaders\TerrainShader");
            }
            catch (ContentLoadException)
            {
                try
                {
                    effect = content.Load<Effect>("TerrainShader");
                }
                catch (ContentLoadException)
                {
                }
            }
        }

        public void Update()
        {
            Ray pickRay = MathExtra.CalculateCursorRay(AIGame.camera.ProjectionMatrix, AIGame.camera.ViewMatrix);
            float rayLength = 0f;
            for (int i = 0; i < triangle.Length; i++)
            {
                Tri thisTri = triangle[i];
                if (MathExtra.Intersects(pickRay, thisTri.p1, thisTri.p3, thisTri.p2, thisTri.normal, false, true, out rayLength))
                {
                    Vector3 rayTarget = pickRay.Position + pickRay.Direction * rayLength;
                    groundCursorPosition.X =
                        rayTarget.X / (
                        //heightMap.Size * (heightMap.GridSpacing - 1)) *
                        //(((float)heightMap.Size * ((float)heightMap.GridSpacing - 1)) / 
                        Settings.TERRAIN_TEXTURE_SIZE);
                    groundCursorPosition.Y = rayTarget.Y;
                    groundCursorPosition.Z =
                        rayTarget.Z / (
                        // heightMap.Size * (heightMap.GridSpacing - 1)) *
                        //(((float)heightMap.Size * ((float)heightMap.GridSpacing - 1)) /
                        Settings.TERRAIN_TEXTURE_SIZE);
                }
            }
            lastGroundCursorPos = groundCursorPosition;
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            // NOTE migration to XNA 4.0 if (vertexBuffer.IsContentLost)
            vertexBuffer.SetData(vertices);

            // NOTE: migration to XNA 4.0
            // NOTE: graphicsDevice.VertexDeclaration = vertexDeclaration;
            // NOTE: graphicsDevice.Vertices[0].SetSource(vertexBuffer, 0,
            // NOTE:     VertexPositionNormalTexture.SizeInBytes);
            // NOTE: graphicsDevice.Indices = indexBuffer;
            // NOTE: Update();
            // NOTE: UpdateEffect(world, view, projection);
            // NOTE: effect.Begin();
            // NOTE: 
            // NOTE: foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            // NOTE: {
            // NOTE:     pass.Begin();
            // NOTE: 
            // NOTE:     graphicsDevice.DrawIndexedPrimitives(
            // NOTE:         PrimitiveType.TriangleList, //PrimitiveType.TriangleStrip,
            // NOTE:         0, 0, vertices.Length, 0,
            // NOTE:         Using16BitIndices() ? indices16Bit.Length / 3 //indices16Bit.Length - 2 
            // NOTE:                             : indices32Bit.Length / 3 //indices32Bit.Length - 2
            // NOTE:     );
            // NOTE: 
            // NOTE:     pass.End();
            // NOTE: }
            // NOTE: 
            // NOTE: effect.End();

            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;

            Update();
            UpdateEffect(world, view, projection);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList, //PrimitiveType.TriangleStrip,
                    0, 0, vertices.Length, 0,
                    Using16BitIndices() ? indices16Bit.Length / 3 //indices16Bit.Length - 2 
                                        : indices32Bit.Length / 3 //indices32Bit.Length - 2
                );

            }

            //AIGame.hud.PrintTextInLocation("South Corner (" + TerrainCornerS.X + ", " + TerrainCornerS.Z + ")", TerrainCornerS, Color.AliceBlue);
            //AIGame.hud.PrintTextInLocation("East Corner (" + TerrainCornerE.X + ", " + TerrainCornerE.Z + ")", TerrainCornerE, Color.AliceBlue);
            //AIGame.hud.PrintTextInLocation("West Corner (" + TerrainCornerW.X + ", " + TerrainCornerW.Z + ")", TerrainCornerW, Color.AliceBlue);
            //AIGame.hud.PrintTextInLocation("North Corner (" + TerrainCornerN.X + ", " + TerrainCornerN.Z + ")", TerrainCornerN, Color.AliceBlue);

            // NOTE: migration to XNA 4.0
            // NOTE: graphicsDevice.Indices = null;
            // NOTE: graphicsDevice.Vertices[0].SetSource(null, 0, 0);
            // NOTE: graphicsDevice.VertexDeclaration = null;
            graphicsDevice.Indices = null;
            graphicsDevice.SetVertexBuffer(null);
        }

        public void GenerateUsingDiamondSquareFractal(float roughness)
        {
            heightMap.GenerateDiamondSquareFractal(roughness);
            GenerateVertices();
        }

        public void GenerateFromBitmap(Texture2D tex, float bumpiness)
        {
            heightMap.GenerateFromBitmap(tex, bumpiness);
            GenerateVertices();
        }


        #endregion

        #region Private methods
        private void GenerateIndices()
        {
            int index = 0;
            int currentVertex = 0;
            int size = heightMap.Size;

            triangle = new Tri[(size - 1) * (size - 1) * 2];
            int triangleID = 0;

            if (Using16BitIndices())
            {
                for (int z = 0; z < size - 1; ++z)
                {
                    for (int x = 0; x < size - 1; ++x)
                    {
                        currentVertex = z * size + x;

                        indices16Bit[index++] = (ushort)(currentVertex);
                        indices16Bit[index++] = (ushort)(currentVertex + 1);
                        indices16Bit[index++] = (ushort)(currentVertex + size);

                        indices16Bit[index++] = (ushort)(currentVertex + 1);
                        indices16Bit[index++] = (ushort)(currentVertex + size + 1);
                        indices16Bit[index++] = (ushort)(currentVertex + size);
                        SetUpCollision(index, triangleID, x, z);
                        triangleID += 2;
                    }
                }
            }
            else
            {
                for (int z = 0; z < size - 1; ++z)
                {
                    for (int x = 0; x < size - 1; ++x)
                    {
                        currentVertex = z * size + x;

                        indices32Bit[index++] = (uint)(currentVertex);
                        indices32Bit[index++] = (uint)(currentVertex + 1);
                        indices32Bit[index++] = (uint)(currentVertex + size);

                        indices32Bit[index++] = (uint)(currentVertex + 1);
                        indices32Bit[index++] = (uint)(currentVertex + size + 1);
                        indices32Bit[index++] = (uint)(currentVertex + size);

                        SetUpCollision(index, triangleID, x, z);
                        triangleID += 2;
                    }
                }
            }
        }

        private void SetUpCollision(int indiceID, int tID, int x, int y)
        {
            triangle[tID] = new Tri();
            triangle[tID].p1 = vertices[x + y * heightMap.Size].Position;
            triangle[tID].p2 = vertices[x + (y + 1) * heightMap.Size].Position;
            triangle[tID].p3 = vertices[(x + 1) + y * heightMap.Size].Position;
            triangle[tID].normal = MathExtra.GetNormal(triangle[tID].p1, triangle[tID].p2, triangle[tID].p3);
            triangle[tID].id = indiceID / 6 - 1;

            triangle[tID + 1] = new Tri();
            triangle[tID + 1].p1 = vertices[(x + 1) + y * heightMap.Size].Position;
            triangle[tID + 1].p2 = vertices[x + (y + 1) * heightMap.Size].Position;
            triangle[tID + 1].p3 = vertices[(x + 1) + (y + 1) * heightMap.Size].Position;
            triangle[tID + 1].normal = MathExtra.GetNormal(triangle[tID + 1].p1, triangle[tID + 1].p2, triangle[tID + 1].p3);
            triangle[tID + 1].id = indiceID / 6;
        }

        private void GenerateIndicesTriangleStrip()
        {
            if (Using16BitIndices())
            {
                int index = 0;
                int size = heightMap.Size;

                for (int z = 0; z < size - 1; ++z)
                {
                    if (z % 2 == 0)
                    {
                        for (int x = 0; x < size; ++x)
                        {
                            indices16Bit[index++] = (ushort)(x + (z + 1) * size);
                            indices16Bit[index++] = (ushort)(x + z * size);
                        }
                        // Add degenerate triangles to stitch strips together.
                        indices16Bit[index++] = (ushort)((size - 1) + (z + 1) * size);
                    }
                    else
                    {
                        for (int x = size - 1; x >= 0; --x)
                        {
                            indices16Bit[index++] = (ushort)(x + (z + 1) * size);
                            indices16Bit[index++] = (ushort)(x + z * size);
                        }
                        // Add degenerate triangles to stitch strips together.
                        indices16Bit[index++] = (ushort)((z + 1) * size);
                    }
                }
            }
            else
            {
                int index = 0;
                int size = heightMap.Size;

                for (int z = 0; z < size - 1; ++z)
                {
                    if (z % 2 == 0)
                    {
                        for (int x = 0; x < size; ++x)
                        {
                            indices32Bit[index++] = (uint)(x + (z + 1) * size);
                            indices32Bit[index++] = (uint)(x + z * size);
                        }
                        // Add degenerate triangles to stitch strips together.
                        indices32Bit[index++] = (uint)((size - 1) + (z + 1) * size);
                    }
                    else
                    {
                        for (int x = size - 1; x >= 0; --x)
                        {
                            indices32Bit[index++] = (uint)(x + (z + 1) * size);
                            indices32Bit[index++] = (uint)(x + z * size);
                        }
                        // Add degenerate triangles to stitch strips together.
                        indices32Bit[index++] = (uint)((z + 1) * size);
                    }
                }
            }
        }

        private void GenerateVertices()
        {
            int index = 0;
            int size = heightMap.Size;
            int gridSpacing = heightMap.GridSpacing;
            Vector3 position;
            Vector3 normal;
            Vector2 texCoord;

            for (int z = 0; z < size; ++z)
            {
                for (int x = 0; x < size; ++x)
                {
                    index = z * size + x;

                    position.X = (float)(x * gridSpacing);
                    position.Y = heightMap.HeightAtPixel(x, z);
                    position.Z = (float)(z * gridSpacing);

                    heightMap.NormalAtPixel(x, z, out normal);

                    texCoord.X = (float)x / (float)size;
                    texCoord.Y = (float)z / (float)size;

                    vertices[index] = new VertexPositionNormalTexture(position, normal, texCoord);

                }
            }
            vertexBuffer.SetData(vertices);
        }

        private void UpdateEffect(Matrix world, Matrix view, Matrix projection)
        {
            Matrix worldInvTrans = Matrix.Transpose(Matrix.Invert(world));
            Matrix worldViewProj = world * view * projection;

            effect.Parameters["world"].SetValue(world);
            effect.Parameters["worldInvTrans"].SetValue(worldInvTrans);
            effect.Parameters["worldViewProjection"].SetValue(worldViewProj);

            effect.Parameters["sunlightDir"].SetValue(sunlightDirection);
            effect.Parameters["sunlightColor"].SetValue(sunlightColor);

            effect.Parameters["terrainAmbient"].SetValue(terrainAmbient);
            effect.Parameters["terrainDiffuse"].SetValue(terrainDiffuse);

            effect.Parameters["terrainTilingFactor"].SetValue(terrainTilingFactor);

            effect.Parameters["terrainRegion1"].SetValue(new Vector2(regions[0].Min, regions[0].Max));
            effect.Parameters["region1ColorMapTexture"].SetValue(regions[0].ColorMap);

            effect.Parameters["terrainRegion2"].SetValue(new Vector2(regions[1].Min, regions[1].Max));
            effect.Parameters["region2ColorMapTexture"].SetValue(regions[1].ColorMap);

            effect.Parameters["terrainRegion3"].SetValue(new Vector2(regions[2].Min, regions[2].Max));
            effect.Parameters["region3ColorMapTexture"].SetValue(regions[2].ColorMap);

            //effect.Parameters["terrainRegion4"].SetValue(new Vector2(regions[3].Min, regions[3].Max));
            //effect.Parameters["region4ColorMapTexture"].SetValue(regions[3].ColorMap);

            //effect.Parameters["terrainRegion5"].SetValue(new Vector2(regions[4].Min, regions[4].Max));
            //effect.Parameters["region5ColorMapTexture"].SetValue(regions[4].ColorMap);

            //effect.Parameters["terrainRegion6"].SetValue(new Vector2(regions[5].Min, regions[5].Max));
            //effect.Parameters["region6ColorMapTexture"].SetValue(regions[5].ColorMap);

            effect.Parameters["groundCursorPosition"].SetValue(groundCursorPosition);
            effect.Parameters["groundCursorTex"].SetValue(groundCursorTex);
            effect.Parameters["groundCursorSize"].SetValue(groundCursorSize);
            effect.Parameters["bShowCursor"].SetValue(showGroundCursor);

            effect.CurrentTechnique = effect.Techniques[currentTechnique.ToString()];
        }

        private bool Using16BitIndices()
        {
            return vertices.Length <= ushort.MaxValue;
        }
        #endregion
    }
}