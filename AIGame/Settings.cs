using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIGame
{
    static class Settings
    {
        public static float TERRAIN_TEXTURE_SIZE = 512.0f;
        public static float CAMERA_HEIGHT_OVER_TERRAIN = 1000.0f;

        public static Color ENVIRONMENT_COLOR = Color.DarkOliveGreen;

        public static Vector3 LIGHT_SPECULAR_COLOR = new Vector3(0.8f, 0.9f, 1.0f);
        public static float LIGHT_SPECULAR_POWER_WORLD = 40;
        public static float LIGHT_SPECULAR_POWER_OBJECTS = 40;

        public static bool ENABLE_DEFAULT_LIGHTING = AI.AIParams.CONFIG_ENABLE_DEFAULT_LIGHTING;

        public static bool FOG_ENABLED = false;
        //public static FogMode FOG_MODE = FogMode.Linear;
        public static Color FOG_COLOR = ENVIRONMENT_COLOR;
        public static float FOG_START = 500f;
        public static float FOG_END = 800f;
        public static float FOG_DENSITY = 0.6f;

        public static float CAMERA_VELOCITY = 900f;
        public static float CAMERA_ACCELERATION = 1200f;

        public static float GAME_SPEED = 1f;
        public static float GameSpeed(float speed)
        {
            return speed * GAME_SPEED;
        }
        public static bool ACCELERATED_MODE = false;
        public static bool DRAW_STATICS_LABELS = false;
        public static bool DRAW_OBJECTS_LABELS = true;
        public static bool DRAW_BOUNDING_BOXES = false;
        public static bool DRAW_RANGE_CIRCLES = false;
        public static bool DRAW_GRAPH = false;

        public static float OBJECTS_MOVEMENT_SPEED = 12f;
        public static float OBJECTS_TURNING_SPEED = 0.3f;
    }
}
