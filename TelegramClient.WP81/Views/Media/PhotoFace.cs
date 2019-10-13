// 
// This is the source code of Telegram for Windows Phone v. 3.x.x.
// It is licensed under GNU GPL v. 2 or later.
// You should have received a copy of the license in this archive (see LICENSE).
// 
// Copyright Evgeny Nadymov, 2013-present.
// 
using System;
using System.Windows.Media.Imaging;
using Windows.Foundation;
using Microsoft.Xna.Framework;
using OpenCVComponent;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace TelegramClient.Views.Media
{
    public class PhotoFace
    {
        public float Width { get; protected set; }

        public float Angle { get; protected set; }

        public Point? ForeheadPoint { get; protected set; }

        public Point? LeftEyePoint { get; protected set; }

        public Point? RightEyePoint { get; protected set; }

        public Point? EyesCenterPoint { get; protected set; }

        public float EyesDistance { get; protected set; }

        public Point? MouthPoint { get; protected set; }

        public Point? ChinPoint { get; protected set; }

        public PhotoFace(Face face, BitmapSource sourceBitmap, Size targetSize, bool sideward)
        {
            Point? leftMouthPoint;
            Point? rightMouthPoint;

            GetPoints(out leftMouthPoint, out rightMouthPoint, face, sourceBitmap, targetSize, sideward);

            if (LeftEyePoint != null && RightEyePoint != null)
            {
                EyesCenterPoint = new Point(0.5f * LeftEyePoint.Value.X + 0.5f * RightEyePoint.Value.X, 0.5f * LeftEyePoint.Value.Y + 0.5f * RightEyePoint.Value.Y);
                EyesDistance = (float)Math.Sqrt(Math.Pow(RightEyePoint.Value.X - LeftEyePoint.Value.X, 2.0) + Math.Pow(RightEyePoint.Value.Y - LeftEyePoint.Value.Y, 2.0));
                Angle = MathHelper.ToDegrees((float)(Math.Atan2(RightEyePoint.Value.Y - LeftEyePoint.Value.Y, RightEyePoint.Value.X - LeftEyePoint.Value.X)));

                Width = EyesDistance * 2.35f;

                var foreheadHeight = 0.7f * EyesDistance;
                var upAngle = MathHelper.ToRadians(Angle - 90.0f);
                ForeheadPoint = new Point(EyesCenterPoint.Value.X + foreheadHeight * (float)Math.Cos(upAngle), EyesCenterPoint.Value.Y + foreheadHeight * (float)Math.Sin(upAngle));
            }

            if (leftMouthPoint != null && rightMouthPoint != null)
            {
                MouthPoint = new Point(0.5f * leftMouthPoint.Value.X + 0.5f * rightMouthPoint.Value.X, 0.5f * leftMouthPoint.Value.Y + 0.5f * rightMouthPoint.Value.Y);

                var chinDepth = 0.6f * EyesDistance;
                var downAngle = MathHelper.ToRadians(Angle + 90.0f);
                ChinPoint = new Point(MouthPoint.Value.X + chinDepth * (float)Math.Cos(downAngle), MouthPoint.Value.Y + chinDepth * (float)Math.Sin(downAngle));
            }
        }

        public override string ToString()
        {
            return string.Format("Foregead={0}\nLeftEye={1}\nRightEye={2}\nMouth={3}\nChin={4}", ForeheadPoint, LeftEyePoint, RightEyePoint, MouthPoint, ChinPoint);
        }

        private void GetPoints(out Point? leftMouthPoint, out Point? rightMouthPoint, Face face, BitmapSource sourceBitmap, Size targetSize, bool sideward)
        {
            Rect? leftEye = null;
            Rect? rightEye = null;
            foreach (var eye in face.Eye)
            {
                if (leftEye == null || leftEye.Value.X > eye.X)
                {
                    leftEye = eye;
                }
                if (rightEye == null || rightEye.Value.X < eye.X)
                {
                    rightEye = eye;
                }
            }

            // two eyes
            if (leftEye != rightEye)
            {
                LeftEyePoint = TransposePoint(new Point(leftEye.Value.X + leftEye.Value.Width / 2.0, leftEye.Value.Y + leftEye.Value.Height / 2.0), sourceBitmap, targetSize, sideward);
                RightEyePoint = TransposePoint(new Point(rightEye.Value.X + rightEye.Value.Width / 2.0, rightEye.Value.Y + rightEye.Value.Height / 2.0), sourceBitmap, targetSize, sideward);
            }
            // only one eye was detected
            else if (leftEye != null)
            {
                if (leftEye.Value.X < face.Position.X + face.Position.Width/2.0)
                {
                    var displX = leftEye.Value.X - face.Position.X;
                    rightEye = new Rect(face.Position.X + face.Position.Width - leftEye.Value.Width - displX, leftEye.Value.Y, leftEye.Value.Width, leftEye.Value.Height);
                }
                else
                {
                    rightEye = leftEye;
                    var displX = face.Position.X + face.Position.Width - rightEye.Value.X - rightEye.Value.Width;
                    leftEye = new Rect(face.Position.X + displX, rightEye.Value.Y, rightEye.Value.Width, rightEye.Value.Height);
                }

                LeftEyePoint = TransposePoint(new Point(leftEye.Value.X + leftEye.Value.Width / 2.0, leftEye.Value.Y + leftEye.Value.Height / 2.0), sourceBitmap, targetSize, sideward);
                RightEyePoint = TransposePoint(new Point(rightEye.Value.X + rightEye.Value.Width / 2.0, rightEye.Value.Y + rightEye.Value.Height / 2.0), sourceBitmap, targetSize, sideward);
            }
            // no eyes
            else
            {
                LeftEyePoint = null;
                RightEyePoint = null;
            }

            Windows.Foundation.Rect? leftMouth = null;
            Windows.Foundation.Rect? rightMouth = null;
            foreach (var mouth in face.Mouth)
            {
                if (leftMouth == null || leftMouth.Value.X > mouth.X)
                {
                    leftMouth = mouth;
                }
                if (rightMouth == null || rightMouth.Value.X < mouth.X)
                {
                    rightMouth = mouth;
                }
            }

            if (leftMouth != null)
            {
                leftMouthPoint = TransposePoint(new Point(leftMouth.Value.X, leftMouth.Value.Y + leftMouth.Value.Height / 4.0), sourceBitmap, targetSize, sideward);
                rightMouthPoint = TransposePoint(new Point(rightMouth.Value.X + rightMouth.Value.Width, rightMouth.Value.Y + rightMouth.Value.Height / 4.0), sourceBitmap, targetSize, sideward);
            }
            else
            {
                leftMouthPoint = null;
                rightMouthPoint = null;
            }
        }

        public bool IsSufficient()
        {
            return EyesCenterPoint != null;
        }

        private Point TransposePoint(Point point, BitmapSource sourceBitmap, Size targetSize, bool sideward)
        {
            float bitmapW = sideward ? sourceBitmap.PixelHeight : sourceBitmap.PixelWidth;
            float bitmapH = sideward ? sourceBitmap.PixelWidth : sourceBitmap.PixelHeight;

            return new Point(targetSize.Width / bitmapW * point.X, targetSize.Height / bitmapH * point.Y);
        }

        public Point? GetPointForAnchor(int anchor)
        {
            switch (anchor)
            {
                case 0:
                    {
                        return ForeheadPoint;
                    }

                case 1:
                    {
                        return EyesCenterPoint;
                    }

                case 2:
                    {
                        return MouthPoint;
                    }

                case 3:
                    {
                        return ChinPoint;
                    }

                default:
                    {
                        return null;
                    }
            }
        }

        public float GetWidthForAnchor(int anchor)
        {
            if (anchor == 1)
                return EyesDistance;

            return Width;
        }

        public float GetAngle()
        {
            return Angle;
        }
    }
}
