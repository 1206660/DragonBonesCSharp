﻿using System.Collections.Generic;
using System.Diagnostics;

namespace DragonBones
{
    /**
     * @private
     */
    public enum ArmatureType
    {
        None = -1,
        Armature = 0,
        MovieClip = 1,
        Stage = 2
    }
    /**
     * @private
     */
    public enum DisplayType
    {
        None = -1,
        Image = 0,
        Armature = 1,
        Mesh = 2,
        BoundingBox = 3
    }
    /**
     * 包围盒类型。
     * @version DragonBones 5.0
     * @language zh_CN
     */
    public enum BoundingBoxType
    {
        None = -1,
        Rectangle = 0,
        Ellipse = 1,
        Polygon = 2
    }
    /**
     * @private
     */
    public enum ActionType
    {
        Play = 0,
        Frame = 10,
        Sound = 11
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

    /**
     * @private
     */
    public enum TweenType
    {
        None = 0,
        Line = 1,
        Curve = 2,
        QuadIn = 3,
        QuadOut = 4,
        QuadInOut = 5
    }

    /**
     * @private
     */
    public enum TimelineType
    {
        Action = 0,
        ZOrder = 1,

        BoneAll = 10,
        BoneTranslate = 11,
        BoneRotate = 12,
        BoneScale = 13,

        SlotDisplay = 20,
        SlotColor = 21,
        SlotFFD = 22,

        AnimationTime = 40,
        AnimationWeight = 41
    }
    /**
     * @private
     */
    public enum OffsetMode
    {
        None,
        Additive,
        Override
    }
    /**
     * 动画混合的淡出方式。
     * @version DragonBones 4.5
     * @language zh_CN
     */
    public enum AnimationFadeOutMode
    {
        /**
         * 不淡出动画。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        None = 0,
        /**
         * 淡出同层的动画。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        SameLayer = 1,
        /**
         * 淡出同组的动画。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        SameGroup = 2,
        /**
         * 淡出同层并且同组的动画。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        SameLayerAndGroup = 3,
        /**
         * 淡出所有动画。
         * @version DragonBones 4.5
         * @language zh_CN
         */
        All = 4,
        /**
         * 不替换同名动画。
         * @version DragonBones 5.1
         * @language zh_CN
         */
        Single = 5
    }
    public class DragonBones
    {
        public const float SECOND_TO_MILLISECOND = 1000.0f;

        public static bool yDown = true;
        public static bool debug = false;
        public static bool debugDraw = false;
        public static bool webAssembly = false;
        public static readonly string VERSION = "5.5.0";

        /**
         * @private
         */
        public static void Assert(bool condition, string message)
        {
            Debug.Assert(condition, message);
        }
        /**
         * @private
         */
        public static void ResizeList<T>(List<T> list, int count, T value)
        {
            if (list.Count == count)
            {
                return;
            }

            if (list.Count > count)
            {
                list.RemoveRange(count, list.Count - count);
            }
            else
            {
                list.Capacity = count;
                for (int i = list.Count, l = count; i < l; ++i)
                {
                    list.Add(value);
                }
            }
        }

        private readonly WorldClock _clock = new WorldClock();
        private readonly List<EventObject> _events = new List<EventObject>();
        private readonly List<BaseObject> _objects = new List<BaseObject>();
        private IEventDispatcher<EventObject> _eventManager = null;

        public DragonBones(IEventDispatcher<EventObject> eventManager)
        {
            this._eventManager = eventManager;
        }

        public void AdvanceTime(float passedTime)
        {
            if (this._objects.Count > 0)
            {
                for (int i = 0; i < this._objects.Count; ++i)
                {
                    var obj = this._objects[i];
                    obj.ReturnToPool();
                }

                this._objects.Clear();
            }

            this._clock.AdvanceTime(passedTime);

            if (this._events.Count > 0)
            {
                for (int i = 0; i < this._events.Count; ++i)
                {
                    var eventObject = this._events[i];
                    //var armature = eventObject.ar
                }
            }
        }
    }
}