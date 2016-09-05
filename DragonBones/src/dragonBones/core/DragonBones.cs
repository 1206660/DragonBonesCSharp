﻿using System.Diagnostics;

namespace dragonBones
{
    /**
     * @private
     */
    public enum ArmatureType 
    {
        Armature = 0, 
        MovieClip = 1, 
        Stage = 2
    }

    /**
     * @private
     */
    public enum DisplayType 
    {
        Image = 0, 
        Armature = 1, 
        Mesh = 2
    }

    /**
     * @private
     */
    public enum ExtensionType
    {
        FFD = 0,
        AdjustColor = 10,
        BevelFilter = 11,
        BlurFilter = 12,
        DropShadowFilter = 13,
        GlowFilter = 14,
        GradientBevelFilter = 15,
        GradientGlowFilter = 16
    }

    /**
     * @private
     */
    public enum EventType
    {
        Frame = 10,
        Sound = 11
    }

    /**
     * @private
     */
    public enum ActionType
    {
        Play = 0,
        Stop = 1,
        GotoAndPlay = 2,
        GotoAndStop = 3,
        FadeIn = 4,
        FadeOut = 5
    }

    /**
     * @private
     */
    public enum BlendMode
    {
        Normal = 0,
        Add = 1,
        Alpha = 2,
        Darken = 3,
        Difference = 4,
        Erase = 5,
        HardLight = 6,
        Invert = 7,
        Layer = 8,
        Lighten = 9,
        Multiply = 10,
        Overlay = 11,
        Screen = 12,
        Subtract = 13
    }

    public class DragonBones  
    {
        public const float PI = 3.14159265358979323846f;

        /**
         * @private
         */
        public const float PI_D = PI * 2.0f;

        /**
         * @private
         */
        public const float PI_H = PI / 2.0f;

        /**
         * @private
         */
        public const float PI_Q = PI / 4.0f;

        /**
         * @private
         */
        public const float ANGLE_TO_RADIAN = PI / 180.0f;

        /**
         * @private
         */
        public const float RADIAN_TO_ANGLE = 180.0f / PI;

        /**
         * @private
         */
        public const float SECOND_TO_MILLISECOND = 1000.0f;

        /**
         * @private
         */
        public const float NO_TWEEN = 100.0f;

        public const string VSESION = "4.7.2";

        /**
         * @private
         */
        public static bool debug = false;

        /**
         * @private
         */
        public static bool debugDraw = false;

        public static void assert(string message)
        {
            Debug.Assert(true, message);
        }
    }
}