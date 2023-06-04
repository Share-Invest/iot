﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Iot.Device.Graphics.SkiaSharpAdapter
{
    /// <summary>
    /// Contains extension methods that operate on <see cref="IGraphics"/>
    /// </summary>
    public static class SkiaSharpGraphicsExtensions
    {
        /// <summary>
        /// Resizes an image. Extension method for SkiaSharp
        /// </summary>
        /// <param name="image">The image to resize</param>
        /// <param name="size">The new size</param>
        /// <returns>A new image. The old image is unaffected</returns>
        /// <exception cref="NotSupportedException">The image is not a SkiaSharpBitmap, meaning more than one image connector is in scope</exception>
        public static BitmapImage Resize(this BitmapImage image, Size size)
        {
            if (image is SkiaSharpBitmap img)
            {
                var resized = img.WrappedBitmap.Resize(new SKSizeI(size.Width, size.Height), SKFilterQuality.Medium);
                return new SkiaSharpBitmap(resized, image.PixelFormat);
            }

            throw new NotSupportedException("This overload can only Resize SkiaSharpImage instances");
        }

        /// <summary>
        /// Draws text to the bitmap
        /// </summary>
        public static void DrawText(this IGraphics graphics, string text, string fontFamilyName, int size, Color color, Point position)
        {
            var canvas = GetCanvas(graphics);
            SKFont fnt = new SKFont(SKTypeface.FromFamilyName(fontFamilyName), size);
            var paint = new SKPaint(fnt);
            paint.Color = new SKColor((uint)color.ToArgb());
            paint.TextAlign = SKTextAlign.Left;
            paint.TextEncoding = SKTextEncoding.Utf16;
            int lineSpacing = size + 2;
            SKPoint currentPosition = new SKPoint(position.X, position.Y + size); // drawing begins to the right and above the given point.
            var texts = text.Split(new char[]
            {
                '\r', '\n'
            }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

            // The DrawText implementation of SkiaSharp does not work with line breaks, so do that manually.
            foreach (var t in texts)
            {
                canvas.DrawText(t, currentPosition, paint);
                currentPosition.Y += lineSpacing;
            }
        }

        /// <summary>
        /// Draws another image into this one, at the given position and without scaling
        /// </summary>
        /// <param name="graphics">The target bitmap</param>
        /// <param name="source">The source bitmap</param>
        /// <param name="x">The x coordinate of the source bitmap in the resulting target bitmap</param>
        /// <param name="y">The y coordinate of the source bitmap in the resulting target bitmap</param>
        public static void DrawImage(this IGraphics graphics, BitmapImage source, int x, int y)
        {
            var sourceBmp = (SkiaSharpBitmap)source;
            var targetCanvas = GetCanvas(graphics);
            targetCanvas.DrawBitmap(sourceBmp.WrappedBitmap, x, y, null);
        }

        /// <summary>
        /// Draws another image into this one, at the given position and without scaling
        /// </summary>
        /// <param name="graphics">The target bitmap</param>
        /// <param name="source">The source bitmap</param>
        /// <param name="sourceRectangle">Rectangle in source image from where to draw</param>
        /// <param name="targetRectangle">Rectangle in target image where to draw to</param>
        public static void DrawImage(this IGraphics graphics, BitmapImage source, Rectangle sourceRectangle, Rectangle targetRectangle)
        {
            var sourceBmp = (SkiaSharpBitmap)source;
            var targetCanvas = GetCanvas(graphics);
            targetCanvas.DrawBitmap(sourceBmp.WrappedBitmap, new SKRect(sourceRectangle.Left, sourceRectangle.Top, sourceRectangle.Right, sourceRectangle.Bottom),
                new SKRect(targetRectangle.Left, targetRectangle.Top, targetRectangle.Right, targetRectangle.Bottom));
        }

        /// <summary>
        /// Get the internal SKCanvas instance, to manually draw to the target bitmap.
        /// </summary>
        /// <param name="graphics">The reference to the image</param>
        /// <returns>An instance that can be used in calls for drawing methods</returns>
        public static SKCanvas GetCanvas(this IGraphics graphics)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (graphics is SkiaSharpBitmap.BitmapCanvas bitmapCanvas)
            {
                return bitmapCanvas.Canvas;
            }

            throw new ArgumentException("These extension methods can only be used on SkiaSharpBitmap instances", nameof(graphics));
        }
    }
}
