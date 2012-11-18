using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace AIGame
{
    public class World : DrawableGameComponent
    {
        private ModelHandler town = new ModelHandler();
        private Model terrain;// = new Model();
        private Sky sky;


        public World(Game game) : base(game)
        {
        }

        public void Load(ContentManager content)
        {
            //terrain = content.Load<Model>("terrain");
            //sky = content.Load<Sky>("sky");
            //town.Initialize(content.Load<Model>("Models/town"));
        }

        public void DrawWorld(Matrix view, Matrix projection)
        {
            //DrawTerrain(view, projection);

            //town.Draw(new GameTime(), projection, view);
            sky.Draw(view, projection);
        }

        /// <summary>
        /// Helper for drawing the terrain model.
        /// </summary>
        private void DrawTerrain(Matrix view, Matrix projection)
        {
            foreach (ModelMesh mesh in terrain.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.View = view;
                    effect.Projection = projection;

                    if (Settings.ENABLE_DEFAULT_LIGHTING)
                    {
                        effect.EnableDefaultLighting();
                        effect.SpecularColor = Settings.LIGHT_SPECULAR_COLOR;
                        effect.SpecularPower = Settings.LIGHT_SPECULAR_POWER_WORLD;
                    }

                    // Set the fog to match the distant mountains
                    // that are drawn into the sky texture.
                    effect.FogEnabled = Settings.FOG_ENABLED;
                    effect.FogColor = Settings.FOG_COLOR.ToVector3();
                    effect.FogStart = Settings.FOG_START;
                    effect.FogEnd = Settings.FOG_END;
                }

                mesh.Draw();
            }
        }
    }
}
