using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UniversalPack.Common;
using UniversalPack.Extensions;

namespace ApsRTCustomComponents
{
    public static class DrawingExtensions
    {
        /// <summary>
        /// Пережимает картинку из потока в JPEG с указанным качеством <paramref name="quality"/>, в случае получения меньшего размера файла пишет данные в тот же поток и возвращает true
        /// </summary>
        /// <param name="stream">Поток изображения</param>
        /// <param name="quality">Качество JPEG [0..100]</param>
        public static bool CompressImage(this Stream stream, byte quality)
        {
            if (!stream.CanRead || !stream.CanSeek || !stream.CanWrite)
            {
                throw new ArgumentException("Stream is not accessible", nameof(stream));
            }

            Image img;

            try
            {
                stream.Position = 0;
                img = Image.FromStream(stream);
            }
            catch
            {
                return false;
            }

            using (var temp = new MemoryStream())
            using (img)
            {
                img.CompressImage(quality, temp);

                if (temp.Length < stream.Length)
                {
                    temp.Position = 0;
                    stream.SetLength(0);
                    temp.CopyTo(stream);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Пережимает картинку в JPEG с указанным качеством <paramref name="quality"/>
        /// </summary>
        /// <param name="image">Изображение</param>
        /// <param name="quality">Качество JPEG [0..100]</param>
        /// <param name="output">Выходной поток</param>
        public static void CompressImage(this Image image, byte quality, Stream output)
        {
            if (!output.CanSeek || !output.CanWrite)
            {
                throw new ArgumentException("Stream is inaccessible", nameof(output));
            }
            if (quality > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(quality));
            }

            var enc = new EncoderParameters
            {
                Param = new[]
                {
                    new EncoderParameter(Encoder.Quality, (long)quality)
                }
            };

            output.Position = 0;

            using (var bitmap = new Bitmap(image))
            {
                bitmap.Save(output, ImageFormat.Jpeg.GetEncoder(), enc);
            }
        }

        /// <summary>
        /// Получает <see cref="ImageCodecInfo"/>, соответствующий <see cref="ImageCodecInfo"/>.
        /// </summary>
        public static ImageCodecInfo GetEncoder(this ImageFormat format)
        {
            var codecs = ImageCodecInfo.GetImageEncoders();

            return codecs.FirstOrDefault(e => e.FormatID == format.Guid);
        }

        /// <summary>
        /// Масштабировать размер к заданной величине по минимальной стророне
        /// </summary>
        /// <param name="size"></param>
        /// <param name="minSide"></param>
        /// <returns></returns>
        public static Size FitToMin(this Size size, int minSide)
        {
            if (size.Width > size.Height)
            {
                return size.FitTo(height: minSide);
            }
            else
            {
                return size.FitTo(width: minSide);
            }
        }

        /// <summary>
        /// Масштабировать размер к заданномй величине
        /// </summary>
        /// <param name="size"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Size FitTo(this Size size, int width = 0, int height = 0)
        {
            if (width > 0 && size.Width > width)
            {
                var mul = width / (double)size.Width;

                size.Width = (int)(size.Width * mul);
                size.Height = (int)(size.Height * mul);
            }
            if (height > 0 && size.Height > height)
            {
                var mul = height / (double)size.Height;

                size.Width = (int)(size.Width * mul);
                size.Height = (int)(size.Height * mul);
            }

            return size;
        }

        /// <summary>
        /// Поворот на 90 градусов
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Size Rotate(this Size size)
        {
            return new Size(size.Height, size.Width);
        }

        /// <summary>
        /// Вычисляет максимальные отступы
        /// </summary>
        /// <param name="paddings"></param>
        /// <returns></returns>
        public static Padding Max(this IEnumerable<Padding> paddings)
        {
            return new Padding(
                paddings.SafeMax(e => e.Left),
                paddings.SafeMax(e => e.Top),
                paddings.SafeMax(e => e.Right),
                paddings.SafeMax(e => e.Bottom));
        }

        /// <summary>
        /// Вычитает поля из размера
        /// </summary>
        /// <param name="size"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static Size Substract(this Size size, Padding padding)
        {
            return new Size(size.Width - padding.Left - padding.Right, size.Height - padding.Top - padding.Bottom);
        }

        /// <summary>
        /// Вычитает поля из размера
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public static Rectangle Substract(this Rectangle rect, Padding padding)
        {
            return Rectangle.FromLTRB(
                rect.Left + padding.Left,
                rect.Top + padding.Top,
                rect.Right - padding.Right,
                rect.Bottom - padding.Bottom);
        }

        /// <summary>
        /// Поворачивает прямоугольник вокруг центра
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static RectangleF RotateCenter(this RectangleF rect)
        {
            return new RectangleF(rect.X + rect.Width / 2 - rect.Height / 2, rect.Y - rect.Width / 2 + rect.Height / 2, rect.Height, rect.Width);
        }

        /// <summary>
        /// Определяет, пересекает ли прямоугольник размера <paramref name="size"/> в точке <paramref name="location"/> макет <paramref name="rect"/>.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="location"></param>
        /// <param name="size"></param>
        public static bool IntersectsWith(this Rectangle rect, Point location, Size size) =>
            location.X <= rect.Right && rect.X <= location.X + size.Width &&
            location.Y <= rect.Bottom && rect.Y <= location.Y + size.Height;

        /// <summary>
        /// Сохраняет все картинки в папку
        /// </summary>
        /// <param name="images"></param>
        /// <param name="path"></param>
        public static void SaveAll(this IEnumerable<Image> images, string path)
        {
            foreach (var image in images.AsIndexValueEnumerable())
            {
                using (image.Value)
                {
                    var file = Path.Combine(path, $"{image.Index}.png");
                    Tools.EnsurePathExists(file);
                    image.Value.Save(file);
                }
            }
        }

        /// <summary>
        /// Делает вертикальный разрез, разделяя область пополам
        /// </summary>
        /// <param name="rectangle">Разрезаемая область</param>
        /// <param name="position">Позиция разреза</param>
        /// <returns></returns>
        public static IEnumerable<Rectangle> SliceVertical(this Rectangle rectangle, int position)
        {
            // левая половина
            yield return Rectangle.FromLTRB(
                rectangle.Left,
                rectangle.Top,
                position,
                rectangle.Bottom);

            // правая половина
            yield return Rectangle.FromLTRB(
                position,
                rectangle.Top,
                rectangle.Right,
                rectangle.Bottom);
        }

        /// <summary>
        /// Делает горизонтальный разрез, разделяя область пополам
        /// </summary>
        /// <param name="rectangle">Разрезаемая область</param>
        /// <param name="position">Позиция разреза</param>
        /// <returns></returns>
        public static IEnumerable<Rectangle> SliceHorizontal(this Rectangle rectangle, int position)
        {
            // верхняя половина
            yield return Rectangle.FromLTRB(
                rectangle.Left,
                rectangle.Top,
                rectangle.Right,
                position);

            // нижняя половина
            yield return Rectangle.FromLTRB(
                rectangle.Left,
                position,
                rectangle.Right,
                rectangle.Bottom);
        }

        /// <summary>
        /// Fills image with specified color.
        /// </summary>
        /// <param name="image">Source image.</param>
        /// <param name="color">Target color.</param>
        public static void FillWithColor(this Image image, Color color)
        {
            using (var graphics = Graphics.FromImage(image))
            {
                graphics.Clear(color);
            }
        }

        /// <summary>
        /// Returns pixel bounds of image.
        /// </summary>
        /// <param name="image">Source image.</param>
        /// <returns></returns>
        public static Rectangle GetBounds(this Image image)
        {
            return new Rectangle
            {
                Width = image.Width,
                Height = image.Height
            };
        }

        /// <summary>
        /// Получает скругленный прямоугольник
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static GraphicsPath RoundedRect(this RectangleF bounds, int radius)
        {
            var diameter = radius * 2;
            var size = new Size(diameter, diameter);
            var arc = new RectangleF(bounds.Location, size);
            var path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Рисование скругленного прямоугольника
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="pen"></param>
        /// <param name="bounds"></param>
        /// <param name="cornerRadius"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, RectangleF bounds, int cornerRadius)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (pen == null)
            {
                throw new ArgumentNullException(nameof(pen));
            }

            using (var path = bounds.RoundedRect(cornerRadius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        /// <summary>
        /// Заливка скругленного прямоугольника
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="brush"></param>
        /// <param name="bounds"></param>
        /// <param name="cornerRadius"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, RectangleF bounds, int cornerRadius)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (brush == null)
            {
                throw new ArgumentNullException(nameof(brush));
            }

            using (var path = bounds.RoundedRect(cornerRadius))
            {
                graphics.FillPath(brush, path);
            }
        }
    }
}
