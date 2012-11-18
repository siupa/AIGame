//======================================================================
// XNA Terrain Editor
// Copyright (C) 2008 Eric Grossinger
// http://psycad007.spaces.live.com/
//======================================================================
using System;
using System.Collections.Generic;
using System.Text;
//using System.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace AIGame.ScreenOutput
{
    public class HUD
    {
        #region Fields
        private SpriteBatch _spriteBatch;
        private SpriteFont _textFont;
        private SpriteFont _textFontSmall;

        //private Timer _timer;
        private int _fpsCount = 0;
        private int _fps = 0;
        private bool _update;
        private BasicPrimitives _graphAvg = new BasicPrimitives(AIGame.device);
        private BasicPrimitives _graphMax = new BasicPrimitives(AIGame.device);
        #endregion

        #region Constructor
        public HUD()
        {
            _textFont = AIGame.content.Load<SpriteFont>(@"Fonts\tahoma");
            _textFontSmall = AIGame.content.Load<SpriteFont>(@"Fonts\tahoma_small");
            _spriteBatch = new SpriteBatch(AIGame.graphics.GraphicsDevice);

            //_timer = new Timer(500);
            //_timer.Elapsed += new ElapsedEventHandler(timer_tick);
            //_timer.Enabled = true;
        } 
        #endregion

        #region Private Methods
        //private void timer_tick(Object obj, ElapsedEventArgs v_args)
        //{
        //    _fps = _fpsCount;
        //    _fpsCount = 0;
        //    _update = true;
        //}

        private bool AreListsEqual(List<double> list1, List<double> list2)
        {
            if (list1.Count != list2.Count)
                return false;
            for (int i = 0; i < list1.Count; i++)
                if (list1[i] != list2[i])
                    return false;
            return true;
        } 
        #endregion

        #region Public Methods
        public void PrintTextInLocation(string text, Vector3 location3D, Color color)
        {
            // check if the object is on a proper side of the view matrix
            Vector3 v1 = AIGame.camera.ViewDirection;
            Vector3 v2 = location3D - AIGame.camera.Position;
            v1.Normalize();
            v2.Normalize();
            if (Vector3.Dot(v1, v2) < 0)
                return;
            Vector3 pos2d = AIGame.graphics.GraphicsDevice.Viewport.Project(
                 location3D,
                 AIGame.camera.ProjectionMatrix,
                 AIGame.camera.ViewMatrix,
                 Matrix.Identity);

            // NOTE: migration to XNA 4.0 _spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate, SaveStateMode.SaveState);
            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);

            // NOTE: migration to XNA 4.0 _spriteBatch.GraphicsDevice.RenderState.FillMode = FillMode.Solid;

            _spriteBatch.DrawString(_textFont, text, new Vector2(pos2d.X, pos2d.Y), color);
            _spriteBatch.End();
        }

        public void Draw()
        {
            _fpsCount++;
            AI.AIObject aiobj = AIGame.AIContainer.SelectedObject;
            AI.AITeam aiteam = AIGame.AIContainer.SelectedTeam;

            // NOTE: migration to XNA 4.0 _spriteBatch.Begin(SpriteSpriteBlendMode.AlphaBlend);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // NOTE: migration to XNA 4.0 _spriteBatch.GraphicsDevice.RenderState.FillMode = FillMode.Solid;

            #region DRAW GRAPH
            if (Settings.DRAW_GRAPH)
            {
                float h = AIGame.device.Viewport.Height;
                float w = AIGame.device.Viewport.Width;

                _graphAvg.ClearVectors();
                _graphMax.ClearVectors();

                _graphAvg.AddVector(new Vector2(0, h));
                _graphMax.AddVector(new Vector2(0, h));
                float index = 1;
                for (int i = 0; i < AIGame.AIContainer.FitnessListAvg.Count; i++)
                {
                    Vector2 pointAvg = new Vector2(
                        w / (AIGame.AIContainer.FitnessListAvg.Count / index),
                        h - (float)AIGame.AIContainer.FitnessListAvg[i] / (AI.AIParams.UPDATE_EVOLUTION_TIME_PERIOD_FRAMES * 2.0f) * h
                        );
                    Vector2 pointMax = new Vector2(
                        w / (AIGame.AIContainer.FitnessListMax.Count / index),
                        h - (float)AIGame.AIContainer.FitnessListMax[i] / (AI.AIParams.UPDATE_EVOLUTION_TIME_PERIOD_FRAMES * 2.0f) * h
                        );

                    _graphAvg.AddVector(pointAvg);
                    _graphMax.AddVector(pointMax);
                    index++;
                }
                _graphAvg.Colour = Color.LightPink;
                _graphAvg.Thickness = 0.5f;
                _graphMax.Colour = Color.LightSalmon;
                _graphMax.Thickness = 0.5f;

                _graphAvg.RenderLinePrimitive(_spriteBatch);
                _graphMax.RenderLinePrimitive(_spriteBatch);
            } 
            #endregion

            #region RIGHT UP CORNER
            _spriteBatch.DrawString(_textFont, "FPS:" + _fps, new Vector2(AIGame.graphics.GraphicsDevice.Viewport.Width - 100, 5f), Color.Gold);
            if (AI.AIParams.FITNESS_INCREASE_WITH_TIME_ENABLE)
            {
                _spriteBatch.DrawString(_textFont, "FF's:" + AIGame.AIContainer.FramesCounterForFitness + "/" + AI.AIParams.FITNESS_INCREASE_WITH_TIME_PERIOD_FRAMES, new Vector2(AIGame.graphics.GraphicsDevice.Viewport.Width - 100, 20f), Color.Coral);
                _spriteBatch.DrawString(_textFont, "Still alive:" + AIGame.AIContainer.NumberAIObjStillAlive, new Vector2(AIGame.graphics.GraphicsDevice.Viewport.Width - 100, 35f), Color.Coral);
            }

            if (AI.AIParams.UPDATE_EVOLUTION_ENABLED)
            {
                _spriteBatch.DrawString(_textFont, "FE's:" + AIGame.AIContainer.FramesCounterForEvolution + "/" + AI.AIParams.UPDATE_EVOLUTION_TIME_PERIOD_FRAMES, new Vector2(AIGame.graphics.GraphicsDevice.Viewport.Width - 100, 50f), Color.Coral);
            }
            _spriteBatch.DrawString(_textFont, "Camera Mode: " + AIGame.camera.CurrentBehavior.ToString(), new Vector2(5f, 5f), Color.Gold);

            #endregion

            #region DUMP ANN INPUTS AND OUTPUTS
            if (aiobj != null && AI.AIParams.CONFIG_DUMP_ANN_VECTORS)
            {
                // dumpa ann information
                List<double> temp = new List<double>();
                temp.AddRange(aiobj.AINeuralBrain.Inputs);
                temp.AddRange(aiobj.AINeuralBrain.Outputs);

                if (Utilities.DoubleCache.Count == 0)
                    Utilities.DoubleCache.Add(temp);
                if (!AreListsEqual(Utilities.DoubleCache[Utilities.DoubleCache.Count - 1], temp))
                    Utilities.DoubleCache.Add(temp);

                if (Utilities.DoubleCache.Count > AI.AIParams.CONFIG_DUMP_ANN_VECTORS_NUM_PROBES)
                {
                    Utilities.WriteToFile(Utilities.DoubleCache, AI.AIParams.CONFIG_DUMP_ANN_VECTORS_FILE_PATH, true);
                    Utilities.DoubleCache.Clear();
                }

                // draw ann inputs and outputs
                StringBuilder text = new StringBuilder();
                text.AppendLine(aiobj.Identifier);
                text.AppendLine("           TDis     CDis    BDis     MDis    Lf        ShAt");// + 
                //"                 A/B      TC       Med     W");
                string inputs_outputs = "Inputs:";
                foreach (double input in aiobj.AINeuralBrain.Inputs)
                    inputs_outputs += " " + string.Format("{0:0.0000}", input);
                inputs_outputs += " OutputsBefSig:";
                foreach (double outputBS in aiobj.AINeuralBrain.OutputsBeforeSig)
                    inputs_outputs += " " + string.Format("{0:0.0000}", outputBS);
                inputs_outputs += " Outputs:";
                foreach (double output in aiobj.AINeuralBrain.Outputs)
                    inputs_outputs += " " + string.Format("{0:0.0000}", output);

                text.AppendLine(inputs_outputs);

                int j = 0;
                foreach (string line in text.ToString().Split('\n'))
                    _spriteBatch.DrawString(_textFont, line, new Vector2(5f, AIGame.Console.Height + j++ * 15f), Color.PaleGreen);

                _spriteBatch.DrawString(_textFont, "Probing ANN states:" + Utilities.DoubleCache.Count + "/" + AI.AIParams.CONFIG_DUMP_ANN_VECTORS_NUM_PROBES, new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - 245), Color.Coral);

            } 
            #endregion

            #region AIOBJECTS LIST
            if (AI.AIParams.CONFIG_DUMP_ANN_VECTORS)
            {
                int k = 0;
                int intend = 0;
                foreach (AI.AITeam ait in AIGame.AIContainer.Teams)
                {
                    foreach (AI.AIObject aio in ait.Values)
                    {
                        _spriteBatch.DrawString(_textFontSmall,
                            aio.Identifier + " (" +
                            aio.AIFitnessesListLife.Count + ", " +
                            Math.Round(aio.AIFitnessLife, 1) + "-" +
                            Math.Round(aio.AIFitnessPenaltyCounter, 1) + "+" +
                            Math.Round(aio.AIFitnessExp, 1) + "+" +
                            Math.Round(aio.AIFitnessCustom, 1) + "=" +
                            Math.Round(aio.AIFitness, 1) + ", " +
                            aio.ActualAction.ToString().Substring(0, 3) + ")"
                            ,
                            new Vector2(AIGame.graphics.GraphicsDevice.Viewport.Width - 850 + intend, 200f + k * 15f), Color.PaleGreen);
                        k++;
                    }
                    intend += 300;
                    k = 0;
                }
            } 
            #endregion

            #region LEFT BOTTOM CORNER
            int locationy = 230;
            _spriteBatch.DrawString(_textFont, "Fittest AIObject: " + AIGame.AIContainer.GenethicAlgorithm.FittestObject.Identifier, new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - locationy), Color.Coral);
            _spriteBatch.DrawString(_textFont, "Generation: " + AIGame.AIContainer.Generation, new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - locationy + 15), Color.Coral);
            _spriteBatch.DrawString(_textFont, "FitnessScaling: " + AI.AIParams.FitnessScalling.ToString(), new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - locationy + 30), Color.Coral);
            _spriteBatch.DrawString(_textFont, "Best fit : " + Math.Round(AIGame.AIContainer.GenethicAlgorithm.BestFitness, 2), new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - locationy + 45), Color.Coral);
            _spriteBatch.DrawString(_textFont, "Average fitness: " + Math.Round(AIGame.AIContainer.GenethicAlgorithm.AverageFitness, 2), new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - locationy + 60), Color.Coral);
            _spriteBatch.DrawString(_textFont, "Worst fitness: " + Math.Round(AIGame.AIContainer.GenethicAlgorithm.WorstFitness, 2), new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - locationy + 75), Color.Coral);

            if (AIGame.camera.CurrentBehavior == Camera.Behavior.FreeView)
            {
                _spriteBatch.DrawString(_textFont, "Mouse X: " + Mouse.GetState().X.ToString(), new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - 125), Color.Gold);
                _spriteBatch.DrawString(_textFont, "Mouse Y: " + Mouse.GetState().Y.ToString(), new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - 110), Color.Gold);

                _spriteBatch.DrawString(_textFont, "Cursor X: " + Math.Round(AIGame.terrain.groundCursorPosition.X, 5) + " (" + +Math.Round(AIGame.terrain.groundCursorPosition.X, 5) * Settings.TERRAIN_TEXTURE_SIZE + ")", new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - 95), Color.Gold);
                _spriteBatch.DrawString(_textFont, "Cursor Y: " + Math.Round(AIGame.terrain.groundCursorPosition.Y, 5) + " (" + +Math.Round(AIGame.terrain.groundCursorPosition.Y, 5) + ")", new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - 80), Color.Gold);
                _spriteBatch.DrawString(_textFont, "Cursor Z: " + Math.Round(AIGame.terrain.groundCursorPosition.Z, 5) + " (" + +Math.Round(AIGame.terrain.groundCursorPosition.Z, 5) * Settings.TERRAIN_TEXTURE_SIZE + ")", new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - 65), Color.Gold);
            }

            _spriteBatch.DrawString(_textFont, "Camera X: " + Math.Round(AIGame.camera.Position.X, 5), new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - 50), Color.Gold);
            _spriteBatch.DrawString(_textFont, "Camera Y: " + Math.Round(AIGame.camera.Position.Y, 5), new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - 35), Color.Gold);
            _spriteBatch.DrawString(_textFont, "Camera Z: " + Math.Round(AIGame.camera.Position.Z, 5), new Vector2(5f, AIGame.graphics.GraphicsDevice.Viewport.Height - 20), Color.Gold);

            #endregion

            #region CONSOLE INFO
            if (AIGame.AIContainer.SelectedTeam != null &&
                   AIGame.AIContainer.SelectedObject != null &&
                   _update &&
                   AIGame.Console.State == Console.ConsoleState.Opened)
            {
                AIGame.Console.Clear();
                AIGame.Console.Add("Team: " + AIGame.AIContainer.SelectedTeam.TeamType +
                         " Objective: " + AIGame.AIContainer.SelectedTeam.Objective +
                         " OAAU: " + AIGame.AIContainer.SelectedTeam.OperateAsAUnit +
                         " KF: " + AIGame.AIContainer.SelectedTeam.KeepFormation +
                         " Formation: " + AIGame.AIContainer.SelectedTeam.Formation);

                AIGame.Console.Add("ID: <" + AIGame.AIContainer.SelectedObject.Identifier + ">" +
                         " Leader: <" + AIGame.AIContainer.SelectedObject.IsLeader + ">" +
                         " Role: <" + AIGame.AIContainer.SelectedObject.Role + ">" +
                         " Sight range: <" + AIGame.AIContainer.SelectedObject.SightRange + ">" +
                         " Status: <" + AIGame.AIContainer.SelectedObject.Status + "(" + AIGame.AIContainer.SelectedObject.MovementSpeed + ")" + ">" +
                         " Life: <" + AIGame.AIContainer.SelectedObject.Life * 100 + "%" + ">" +
                         " Experience: <" + AIGame.AIContainer.SelectedObject.Experience + " pt" + ">" +
                         " HF: <" + AIGame.AIContainer.SelectedObject.HoldFire + ">" +
                         " Target: <" + (AIGame.AIContainer.SelectedObject.Target == null ? "none" : AIGame.AIContainer.SelectedObject.Target.Identifier + " (" + (aiobj.Target.Position - aiobj.Position).Length() + ")") + ">" +
                         (AIGame.AIContainer.SelectedObject.IsShooting == true ? " Shooting!" : "") +
                         (AIGame.AIContainer.SelectedObject.InCover == true ? " In Cover!" : ""));

                string whos_seen = string.Empty;
                foreach (AI.AIObject.SeenAIObject seen_aiobj in AIGame.AIContainer.SelectedObject.SightList)
                    whos_seen += seen_aiobj.aiobj.Identifier + " ";

                AIGame.Console.Add("See list (" + AIGame.AIContainer.SelectedObject.SightList.Count + "): " + whos_seen);

                string net_text = AI.AIParams.NumInputs.ToString();
                foreach (AI.ANN.NeuronLayer layer in aiobj.AINeuralBrain.Layers)
                    net_text += " " + layer.NumOfNeurons;
                AIGame.Console.Add("ANN: " + net_text + " Fitness: " + Math.Round(aiobj.AIFitness, 1) +
                    " Actions: a:" + aiobj.ActionCounter[AI.AIAction.Attack] +
                    " pb:" + aiobj.ActionCounter[AI.AIAction.PUBonus] +
                    " pm:" + aiobj.ActionCounter[AI.AIAction.PUMed] +
                    " tc:" + aiobj.ActionCounter[AI.AIAction.TakeCover] +
                    " w:" + aiobj.ActionCounter[AI.AIAction.Wander] +
                    ", " +
                    " ACTUAL ACTION: " + aiobj.ActualAction.ToString() +
                    " (" + aiobj.IsOccupied.ToString() + ")"
                    );

                _update = false;
            }
            #endregion

            _spriteBatch.End();
        } 
        #endregion
    }
}
