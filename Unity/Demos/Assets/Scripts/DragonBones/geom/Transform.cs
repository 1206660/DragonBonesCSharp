﻿using System;

namespace DragonBones
{
    /**
     * @language zh_CN
     * 2D 变换。
     * @version DragonBones 3.0
     */
    public class Transform
    {
        /**
         * @private
         */
        internal static readonly float PI = 3.141593f;
        /**
         * @private
         */
        internal static readonly float PI_D = PI * 2.0f;
        /**
         * @private
         */
        internal static readonly float PI_H = PI / 2.0f;
        /**
         * @private
         */
        internal static readonly float PI_Q = PI / 4.0f;
        /**
         * @private
         */
        internal static readonly float RAD_DEG = 180.0f / PI;
        /**
         * @private
         */
        internal static readonly float DEG_RAD = PI / 180.0f;

        /**
         * @private
         */
        internal static float NormalizeRadian(float value)
        {
            value = (value + PI) % (PI * 2.0f);

           
            value += value > 0.0f ? -PI : PI;

            return value;
        }

        /**
         * 水平位移。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float x = 0.0f;
        /**
         * 垂直位移。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float y = 0.0f;
        /**
         * 倾斜。 (以弧度为单位)
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float skew = 0.0f;
        /**
         * 旋转。 (以弧度为单位)
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float rotation = 0.0f;
        /**
         * 水平缩放。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float scaleX = 1.0f;
        /**
         * 垂直缩放。
         * @version DragonBones 3.0
         * @language zh_CN
         */
        public float scaleY = 1.0f;

        public Transform()
        {
            
        }

        /**
         * @private
         */
        public override string ToString()
        {
            return "[object dragonBones.Transform] x:" + this.x + " y:" + this.y + " skewX:" + this.skew* 180.0 / PI + " skewY:" + this.rotation* 180.0 / PI + " scaleX:" + this.scaleX + " scaleY:" + this.scaleY;
        }

        /**
         * @private
         */
        public Transform CopyFrom(Transform value)
        {
            this.x = value.x;
            this.y = value.y;
            this.skew = value.skew;
            this.rotation = value.rotation;
            this.scaleX = value.scaleX;
            this.scaleY = value.scaleY;

            return this;
        }

        /**
         * @private
         */
        public Transform Identity()
        {
            this.x = this.y = 0.0f;
            this.skew = this.rotation = 0.0f;
            this.scaleX = this.scaleY = 1.0f;

            return this;
        }

        /**
         * @private
         */
        public Transform Add(Transform value)
        {
            this.x += value.x;
            this.y += value.y;
            this.skew += value.skew;
            this.rotation += value.rotation;
            this.scaleX *= value.scaleX;
            this.scaleY *= value.scaleY;

            return this;
        }

        /**
         * @private
         */
        public Transform Minus(Transform value)
        {
            this.x -= value.x;
            this.y -= value.y;
            this.skew -= value.skew;
            this.rotation -= value.rotation;
            this.scaleX /= value.scaleX;
            this.scaleY /= value.scaleY;

            return this;
        }

        /**
         * @private
         */
        public Transform FromMatrix(Matrix matrix)
        {
            var backupScaleX = this.scaleX;
            var backupScaleY = this.scaleY;
            var PI_Q = Transform.PI_Q;

            this.x = matrix.tx;
            this.y = matrix.ty;
            this.rotation = (float)Math.Atan(matrix.b / matrix.a);
            var skewX = (float)Math.Atan(-matrix.c / matrix.d);

            this.scaleX = (this.rotation > -PI_Q && this.rotation < PI_Q) ? matrix.a / (float)Math.Cos(this.rotation) : matrix.b / (float)Math.Sin(this.rotation);
            this.scaleY = (skewX > -PI_Q && skewX < PI_Q) ? matrix.d / (float)Math.Cos(skewX) : -matrix.c / (float)Math.Sin(skewX);

            if (backupScaleX >= 0.0f && this.scaleX < 0.0f)
            {
                this.scaleX = -this.scaleX;
                this.rotation = this.rotation - PI;
            }

            if (backupScaleY >= 0.0f && this.scaleY < 0.0f)
            {
                this.scaleY = -this.scaleY;
                skewX = skewX - PI;
            }

            this.skew = skewX - this.rotation;

            if ((int)(Math.Floor(Math.Abs(this.rotation) * 100)) == (int)(Math.Floor(PI * 100)))
            {
                this.rotation = 0.0f;
            }

            if ((int)(Math.Floor(Math.Abs(this.skew) * 100)) == (int)(Math.Floor(PI * 100)))
            {
                this.skew = 0.0f;
            }

            return this;
        }

        /**
         * @language zh_CN
         * 转换为矩阵。
         * @param 矩阵。
         * @version DragonBones 3.0
         */
        public Transform ToMatrix(Matrix matrix)
        {
            if (this.skew != 0.0f || this.rotation != 0.0f)
            {
                matrix.a = (float)Math.Cos(this.rotation);
                matrix.b = (float)Math.Sin(this.rotation);

                if (this.skew == 0.0f)
                {
                    matrix.c = -matrix.b;
                    matrix.d = matrix.a;
                }
                else
                {
                    matrix.c = -(float)Math.Sin(this.skew + this.rotation);
                    matrix.d = (float)Math.Cos(this.skew + this.rotation);
                }

                if (this.scaleX != 1.0f)
                {
                    matrix.a *= this.scaleX;
                    matrix.b *= this.scaleX;
                }

                if (this.scaleY != 1.0f)
                {
                    matrix.c *= this.scaleY;
                    matrix.d *= this.scaleY;
                }
            }
            else
            {
                matrix.a = this.scaleX;
                matrix.b = 0.0f;
                matrix.c = 0.0f;
                matrix.d = this.scaleY;
            }

            matrix.tx = this.x;
            matrix.ty = this.y;

            return this;
        }
    }
}
