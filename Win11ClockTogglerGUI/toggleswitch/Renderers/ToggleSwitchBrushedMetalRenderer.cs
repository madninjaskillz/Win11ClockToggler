﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using ToggleSwitch;

namespace JCS
{
    public class ToggleSwitchBrushedMetalRenderer : ToggleSwitchRendererBase
    {
        #region Constructor

        public ToggleSwitchBrushedMetalRenderer()
        {
            BorderColor1 = Color.FromArgb(255, 145, 146, 149);
            BorderColor2 = Color.FromArgb(255, 227, 229, 232);
            BackColor1 = Color.FromArgb(255, 125, 126, 128);
            BackColor2 = Color.FromArgb(255, 224, 226, 228);
            UpperShadowColor1 = Color.FromArgb(150, 0, 0, 0);
            UpperShadowColor2 = Color.FromArgb(5, 0, 0, 0);
            ButtonNormalBorderColor = Color.FromArgb(255, 144, 144, 145);
            ButtonNormalSurfaceColor = Color.FromArgb(255, 251, 251, 251);
            ButtonHoverBorderColor = Color.FromArgb(255, 166, 167, 168);
            ButtonHoverSurfaceColor = Color.FromArgb(255, 238, 238, 238);
            ButtonPressedBorderColor = Color.FromArgb(255, 123, 123, 123);
            ButtonPressedSurfaceColor = Color.FromArgb(255, 184, 184, 184);

            UpperShadowHeight = 8;
        }

        #endregion Constructor

        #region Public Properties

        public Color BorderColor1 { get; set; }
        public Color BorderColor2 { get; set; }
        public Color BackColor1 { get; set; }
        public Color BackColor2 { get; set; }
        public Color UpperShadowColor1 { get; set; }
        public Color UpperShadowColor2 { get; set; }
        public Color ButtonNormalBorderColor { get; set; }
        public Color ButtonNormalSurfaceColor { get; set; }
        public Color ButtonHoverBorderColor { get; set; }
        public Color ButtonHoverSurfaceColor { get; set; }
        public Color ButtonPressedBorderColor { get; set; }
        public Color ButtonPressedSurfaceColor { get; set; }

        public int UpperShadowHeight { get; set; }

        #endregion Public Properties

        #region Render Method Implementations

        public override void RenderBorder(Graphics g, Rectangle borderRectangle)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;

            //Draw outer border
            using (GraphicsPath outerControlPath = GetRectangleClipPath(borderRectangle))
            {
                g.SetClip(outerControlPath);

                Color borderColor1 = (!ToggleSwitch.Enabled && ToggleSwitch.GrayWhenDisabled) ? BorderColor1.ToGrayScale() : BorderColor1;
                Color borderColor2 = (!ToggleSwitch.Enabled && ToggleSwitch.GrayWhenDisabled) ? BorderColor2.ToGrayScale() : BorderColor2;

                using (Brush borderBrush = new LinearGradientBrush(borderRectangle, borderColor1, borderColor2, LinearGradientMode.Vertical))
                {
                    g.FillPath(borderBrush, outerControlPath);
                }

                g.ResetClip();
            }

            //Draw inner background
            Rectangle innercontrolRectangle = new Rectangle(borderRectangle.X + 1, borderRectangle.Y + 1, borderRectangle.Width - 1, borderRectangle.Height - 2);

            using (GraphicsPath innerControlPath = GetRectangleClipPath(innercontrolRectangle))
            {
                g.SetClip(innerControlPath);

                Color backColor1 = (!ToggleSwitch.Enabled && ToggleSwitch.GrayWhenDisabled) ? BackColor1.ToGrayScale() : BackColor1;
                Color backColor2 = (!ToggleSwitch.Enabled && ToggleSwitch.GrayWhenDisabled) ? BackColor2.ToGrayScale() : BackColor2;

                using (Brush backgroundBrush = new LinearGradientBrush(borderRectangle, backColor1, backColor2, LinearGradientMode.Horizontal))
                {
                    g.FillPath(backgroundBrush, innerControlPath);
                }

                //Draw inner top shadow
                Rectangle upperShadowRectangle = new Rectangle(innercontrolRectangle.X, innercontrolRectangle.Y, innercontrolRectangle.Width, UpperShadowHeight);

                g.IntersectClip(upperShadowRectangle);

                using (Brush shadowBrush = new LinearGradientBrush(upperShadowRectangle, UpperShadowColor1, UpperShadowColor2, LinearGradientMode.Vertical))
                {
                    g.FillRectangle(shadowBrush, upperShadowRectangle);
                }

                g.ResetClip();
            }
        }

        public override void RenderLeftToggleField(Graphics g, Rectangle leftRectangle, int totalToggleFieldWidth)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;

            Rectangle innercontrolRectangle = new Rectangle(1, 1, ToggleSwitch.Width - 1, ToggleSwitch.Height - 2);

            using (GraphicsPath innerControlPath = GetRectangleClipPath(innercontrolRectangle))
            {
                g.SetClip(innerControlPath);

                //Draw image or text
                if (ToggleSwitch.OnSideImage != null || !string.IsNullOrEmpty(ToggleSwitch.OnText))
                {
                    RectangleF fullRectangle = new RectangleF(leftRectangle.X + 2 - (totalToggleFieldWidth - leftRectangle.Width), 2, totalToggleFieldWidth - 2, ToggleSwitch.Height - 4);

                    g.IntersectClip(fullRectangle);

                    if (ToggleSwitch.OnSideImage != null)
                    {
                        Size imageSize = ToggleSwitch.OnSideImage.Size;
                        Rectangle imageRectangle;

                        int imageXPos = (int)fullRectangle.X;

                        if (ToggleSwitch.OnSideScaleImageToFit)
                        {
                            Size canvasSize = new Size((int)fullRectangle.Width, (int)fullRectangle.Height);
                            Size resizedImageSize = ImageHelper.RescaleImageToFit(imageSize, canvasSize);

                            if (ToggleSwitch.OnSideAlignment == ToggleSwitch.ToggleSwitchAlignment.Center)
                            {
                                imageXPos = (int)((float)fullRectangle.X + (((float)fullRectangle.Width - (float)resizedImageSize.Width) / 2));
                            }
                            else if (ToggleSwitch.OnSideAlignment == ToggleSwitch.ToggleSwitchAlignment.Near)
                            {
                                imageXPos = (int)((float)fullRectangle.X + (float)fullRectangle.Width - (float)resizedImageSize.Width);
                            }

                            imageRectangle = new Rectangle(imageXPos, (int)((float)fullRectangle.Y + (((float)fullRectangle.Height - (float)resizedImageSize.Height) / 2)), resizedImageSize.Width, resizedImageSize.Height);

                            if (!ToggleSwitch.Enabled && ToggleSwitch.GrayWhenDisabled)
                                g.DrawImage(ToggleSwitch.OnSideImage, imageRectangle, 0, 0, ToggleSwitch.OnSideImage.Width, ToggleSwitch.OnSideImage.Height, GraphicsUnit.Pixel, ImageHelper.GetGrayscaleAttributes());
                            else
                                g.DrawImage(ToggleSwitch.OnSideImage, imageRectangle);
                        }
                        else
                        {
                            if (ToggleSwitch.OnSideAlignment == ToggleSwitch.ToggleSwitchAlignment.Center)
                            {
                                imageXPos = (int)((float)fullRectangle.X + (((float)fullRectangle.Width - (float)imageSize.Width) / 2));
                            }
                            else if (ToggleSwitch.OnSideAlignment == ToggleSwitch.ToggleSwitchAlignment.Near)
                            {
                                imageXPos = (int)((float)fullRectangle.X + (float)fullRectangle.Width - (float)imageSize.Width);
                            }

                            imageRectangle = new Rectangle(imageXPos, (int)((float)fullRectangle.Y + (((float)fullRectangle.Height - (float)imageSize.Height) / 2)), imageSize.Width, imageSize.Height);

                            if (!ToggleSwitch.Enabled && ToggleSwitch.GrayWhenDisabled)
                                g.DrawImage(ToggleSwitch.OnSideImage, imageRectangle, 0, 0, ToggleSwitch.OnSideImage.Width, ToggleSwitch.OnSideImage.Height, GraphicsUnit.Pixel, ImageHelper.GetGrayscaleAttributes());
                            else
                                g.DrawImageUnscaled(ToggleSwitch.OnSideImage, imageRectangle);
                        }
                    }
                    else if (!string.IsNullOrEmpty(ToggleSwitch.OnText))
                    {
                        SizeF textSize = g.MeasureString(ToggleSwitch.OnText, ToggleSwitch.OnFont);

                        float textXPos = fullRectangle.X;

                        if (ToggleSwitch.OnSideAlignment == ToggleSwitch.ToggleSwitchAlignment.Center)
                        {
                            textXPos = (float)fullRectangle.X + (((float)fullRectangle.Width - (float)textSize.Width) / 2);
                        }
                        else if (ToggleSwitch.OnSideAlignment == ToggleSwitch.ToggleSwitchAlignment.Near)
                        {
                            textXPos = (float)fullRectangle.X + (float)fullRectangle.Width - (float)textSize.Width;
                        }

                        RectangleF textRectangle = new RectangleF(textXPos, (float)fullRectangle.Y + (((float)fullRectangle.Height - (float)textSize.Height) / 2), textSize.Width, textSize.Height);

                        Color textForeColor = ToggleSwitch.OnForeColor;

                        if (!ToggleSwitch.Enabled && ToggleSwitch.GrayWhenDisabled)
                            textForeColor = textForeColor.ToGrayScale();

                        using (Brush textBrush = new SolidBrush(textForeColor))
                        {
                            g.DrawString(ToggleSwitch.OnText, ToggleSwitch.OnFont, textBrush, textRectangle);
                        }
                    }
                }

                g.ResetClip();
            }
        }

        public override void RenderRightToggleField(Graphics g, Rectangle rightRectangle, int totalToggleFieldWidth)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;

            Rectangle innercontrolRectangle = new Rectangle(1, 1, ToggleSwitch.Width - 1, ToggleSwitch.Height - 2);

            using (GraphicsPath innerControlPath = GetRectangleClipPath(innercontrolRectangle))
            {
                g.SetClip(innerControlPath);

                //Draw image or text
                if (ToggleSwitch.OffSideImage != null || !string.IsNullOrEmpty(ToggleSwitch.OffText))
                {
                    RectangleF fullRectangle = new RectangleF(rightRectangle.X, 2, totalToggleFieldWidth - 2, ToggleSwitch.Height - 4);

                    g.IntersectClip(fullRectangle);

                    if (ToggleSwitch.OffSideImage != null)
                    {
                        Size imageSize = ToggleSwitch.OffSideImage.Size;
                        Rectangle imageRectangle;

                        int imageXPos = (int)fullRectangle.X;

                        if (ToggleSwitch.OffSideScaleImageToFit)
                        {
                            Size canvasSize = new Size((int)fullRectangle.Width, (int)fullRectangle.Height);
                            Size resizedImageSize = ImageHelper.RescaleImageToFit(imageSize, canvasSize);

                            if (ToggleSwitch.OffSideAlignment == ToggleSwitch.ToggleSwitchAlignment.Center)
                            {
                                imageXPos = (int)((float)fullRectangle.X + (((float)fullRectangle.Width - (float)resizedImageSize.Width) / 2));
                            }
                            else if (ToggleSwitch.OffSideAlignment == ToggleSwitch.ToggleSwitchAlignment.Far)
                            {
                                imageXPos = (int)((float)fullRectangle.X + (float)fullRectangle.Width - (float)resizedImageSize.Width);
                            }

                            imageRectangle = new Rectangle(imageXPos, (int)((float)fullRectangle.Y + (((float)fullRectangle.Height - (float)resizedImageSize.Height) / 2)), resizedImageSize.Width, resizedImageSize.Height);

                            if (!ToggleSwitch.Enabled && ToggleSwitch.GrayWhenDisabled)
                                g.DrawImage(ToggleSwitch.OnSideImage, imageRectangle, 0, 0, ToggleSwitch.OnSideImage.Width, ToggleSwitch.OnSideImage.Height, GraphicsUnit.Pixel, ImageHelper.GetGrayscaleAttributes());
                            else
                                g.DrawImage(ToggleSwitch.OnSideImage, imageRectangle);
                        }
                        else
                        {
                            if (ToggleSwitch.OffSideAlignment == ToggleSwitch.ToggleSwitchAlignment.Center)
                            {
                                imageXPos = (int)((float)fullRectangle.X + (((float)fullRectangle.Width - (float)imageSize.Width) / 2));
                            }
                            else if (ToggleSwitch.OffSideAlignment == ToggleSwitch.ToggleSwitchAlignment.Far)
                            {
                                imageXPos = (int)((float)fullRectangle.X + (float)fullRectangle.Width - (float)imageSize.Width);
                            }

                            imageRectangle = new Rectangle(imageXPos, (int)((float)fullRectangle.Y + (((float)fullRectangle.Height - (float)imageSize.Height) / 2)), imageSize.Width, imageSize.Height);

                            if (!ToggleSwitch.Enabled && ToggleSwitch.GrayWhenDisabled)
                                g.DrawImage(ToggleSwitch.OnSideImage, imageRectangle, 0, 0, ToggleSwitch.OnSideImage.Width, ToggleSwitch.OnSideImage.Height, GraphicsUnit.Pixel, ImageHelper.GetGrayscaleAttributes());
                            else
                                g.DrawImageUnscaled(ToggleSwitch.OffSideImage, imageRectangle);
                        }
                    }
                    else if (!string.IsNullOrEmpty(ToggleSwitch.OffText))
                    {
                        SizeF textSize = g.MeasureString(ToggleSwitch.OffText, ToggleSwitch.OffFont);

                        float textXPos = fullRectangle.X;

                        if (ToggleSwitch.OffSideAlignment == ToggleSwitch.ToggleSwitchAlignment.Center)
                        {
                            textXPos = (float)fullRectangle.X + (((float)fullRectangle.Width - (float)textSize.Width) / 2);
                        }
                        else if (ToggleSwitch.OffSideAlignment == ToggleSwitch.ToggleSwitchAlignment.Far)
                        {
                            textXPos = (float)fullRectangle.X + (float)fullRectangle.Width - (float)textSize.Width;
                        }

                        RectangleF textRectangle = new RectangleF(textXPos, (float)fullRectangle.Y + (((float)fullRectangle.Height - (float)textSize.Height) / 2), textSize.Width, textSize.Height);

                        Color textForeColor = ToggleSwitch.OffForeColor;

                        if (!ToggleSwitch.Enabled && ToggleSwitch.GrayWhenDisabled)
                            textForeColor = textForeColor.ToGrayScale();

                        using (Brush textBrush = new SolidBrush(textForeColor))
                        {
                            g.DrawString(ToggleSwitch.OffText, ToggleSwitch.OffFont, textBrush, textRectangle);
                        }
                    }
                }

                g.ResetClip();
            }
        }

        public override void RenderButton(Graphics g, Rectangle buttonRectangle)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;

            g.SetClip(buttonRectangle);

            //Draw button surface
            Color buttonSurfaceColor = ButtonNormalSurfaceColor;

            if (ToggleSwitch.IsButtonPressed)
                buttonSurfaceColor = ButtonPressedSurfaceColor;
            else if (ToggleSwitch.IsButtonHovered)
                buttonSurfaceColor = ButtonHoverSurfaceColor;

            if (!ToggleSwitch.Enabled && ToggleSwitch.GrayWhenDisabled)
                buttonSurfaceColor = buttonSurfaceColor.ToGrayScale();

            using (Brush buttonSurfaceBrush = new SolidBrush(buttonSurfaceColor))
            {
                g.FillEllipse(buttonSurfaceBrush, buttonRectangle);
            }

            //Draw "metal" surface
            PointF centerPoint1 = new PointF(buttonRectangle.X + (buttonRectangle.Width / 2f), buttonRectangle.Y + 1.2f * (buttonRectangle.Height / 2f));

            using (PathGradientBrush firstMetalBrush = GetBrush(new Color[]
                                                                  {
                                                                      Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent,
                                                                      Color.Transparent, Color.Transparent, Color.Transparent, Color.FromArgb(255, 110, 110, 110), Color.Transparent, Color.Transparent,
                                                                      Color.Transparent
                                                                  }, buttonRectangle, centerPoint1))
            {
                g.FillEllipse(firstMetalBrush, buttonRectangle);
            }

            PointF centerPoint2 = new PointF(buttonRectangle.X + 0.8f * (buttonRectangle.Width / 2f), buttonRectangle.Y + (buttonRectangle.Height / 2f));

            using (PathGradientBrush secondMetalBrush = GetBrush(new Color[]
                                                                  {
                                                                      Color.FromArgb(255, 110, 110, 110), Color.Transparent,  Color.Transparent, Color.Transparent,
                                                                      Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent,
                                                                      Color.FromArgb(255, 110, 110, 110)
                                                                  }, buttonRectangle, centerPoint2))
            {
                g.FillEllipse(secondMetalBrush, buttonRectangle);
            }

            PointF centerPoint3 = new PointF(buttonRectangle.X + 1.2f * (buttonRectangle.Width / 2f), buttonRectangle.Y + (buttonRectangle.Height / 2f));

            using (PathGradientBrush thirdMetalBrush = GetBrush(new Color[]
                                                                  {
                                                                      Color.Transparent, Color.Transparent,  Color.Transparent, Color.Transparent,
                                                                      Color.FromArgb(255, 98, 98, 98), Color.Transparent, Color.Transparent, Color.Transparent,
                                                                      Color.Transparent
                                                                  }, buttonRectangle, centerPoint3))
            {
                g.FillEllipse(thirdMetalBrush, buttonRectangle);
            }

            PointF centerPoint4 = new PointF(buttonRectangle.X + 0.9f * (buttonRectangle.Width / 2f), buttonRectangle.Y + 0.9f * (buttonRectangle.Height / 2f));

            using (PathGradientBrush fourthMetalBrush = GetBrush(new Color[]
                                                                  {
                                                                      Color.Transparent, Color.FromArgb(255, 188, 188, 188), Color.FromArgb(255, 110, 110, 110), Color.Transparent, Color.Transparent, Color.Transparent,
                                                                      Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent, Color.Transparent,
                                                                      Color.Transparent
                                                                  }, buttonRectangle, centerPoint4))
            {
                g.FillEllipse(fourthMetalBrush, buttonRectangle);
            }

            //Draw button border
            Color buttonBorderColor = ButtonNormalBorderColor;

            if (ToggleSwitch.IsButtonPressed)
                buttonBorderColor = ButtonPressedBorderColor;
            else if (ToggleSwitch.IsButtonHovered)
                buttonBorderColor = ButtonHoverBorderColor;

            if (!ToggleSwitch.Enabled && ToggleSwitch.GrayWhenDisabled)
                buttonBorderColor = buttonBorderColor.ToGrayScale();

            using (Pen buttonBorderPen = new Pen(buttonBorderColor))
            {
                g.DrawEllipse(buttonBorderPen, buttonRectangle);
            }

            g.ResetClip();
        }

        #endregion Render Method Implementations

        #region Helper Method Implementations

        public GraphicsPath GetRectangleClipPath(Rectangle rect)
        {
            GraphicsPath borderPath = new GraphicsPath();
            borderPath.AddArc(rect.X, rect.Y, rect.Height, rect.Height, 90, 180);
            borderPath.AddArc(rect.Width - rect.Height, rect.Y, rect.Height, rect.Height, 270, 180);
            borderPath.CloseFigure();

            return borderPath;
        }

        public GraphicsPath GetButtonClipPath()
        {
            Rectangle buttonRectangle = GetButtonRectangle();

            GraphicsPath buttonPath = new GraphicsPath();

            buttonPath.AddArc(buttonRectangle.X, buttonRectangle.Y, buttonRectangle.Height, buttonRectangle.Height, 0, 360);

            return buttonPath;
        }

        public override int GetButtonWidth()
        {
            return ToggleSwitch.Height - 2;
        }

        public override Rectangle GetButtonRectangle()
        {
            int buttonWidth = GetButtonWidth();
            return GetButtonRectangle(buttonWidth);
        }

        public override Rectangle GetButtonRectangle(int buttonWidth)
        {
            Rectangle buttonRect = new Rectangle(ToggleSwitch.ButtonValue, 1, buttonWidth, buttonWidth);
            return buttonRect;
        }

        private PathGradientBrush GetBrush(Color[] Colors, RectangleF r, PointF centerPoint)
        {
            int i = Colors.Length - 1;
            PointF[] points = new PointF[i + 1];

            float a = 0f;
            int n = 0;
            float cx = r.Width / 2f;
            float cy = r.Height / 2f;

            int w = (int)(Math.Floor((180.0 * (i - 2.0) / i) / 2.0));
            double wi = w * Math.PI / 180.0;
            double faktor = 1.0 / Math.Sin(wi);

            float radx = (float)(cx * faktor);
            float rady = (float)(cy * faktor);

            while (a <= Math.PI * 2)
            {
                points[n] = new PointF((float)((cx + (Math.Cos(a) * radx))) + r.Left, (float)((cy + (Math.Sin(a) * rady))) + r.Top);
                n += 1;
                a += (float)(Math.PI * 2 / i);
            }

            points[i] = points[0];
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddLines(points);

            PathGradientBrush fBrush = new PathGradientBrush(graphicsPath);
            fBrush.CenterPoint = centerPoint;
            fBrush.CenterColor = Color.Transparent;
            fBrush.SurroundColors = new Color[] { Color.White };

            try
            {
                fBrush.SurroundColors = Colors;
            }
            catch (Exception ex)
            {
                throw new Exception("Too may colors!", ex);
            }

            return fBrush;
        }

        #endregion Helper Method Implementations
    }
}
