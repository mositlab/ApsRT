using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsCustomComponents
{
    public class CustomProgressBar : Control
    {
        private static readonly StringFormat _stringFormat;

        static CustomProgressBar()
        {
            _stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisWord
            };
        }

        private int _value;
        private int _maximum = 100;
        private int _minimum = 0;
        private bool _isInfinite;
        private Rectangle _barRect;
        private Brush _paintBrush;
        private Padding _barPadding;

        /// <summary>
        /// Цвет бара
        /// </summary>
        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                base.ForeColor = value;
                _paintBrush?.Dispose();
                _paintBrush = new SolidBrush(value);
            }
        }

        /// <summary>
        /// Текущее значение
        /// </summary>
        public int Value
        {
            get => _value;
            set
            {
                if (_isInfinite)
                {
                    _value = value;
                }
                else if (value < _minimum)
                {
                    _value = _minimum;
                }
                else if (value > _maximum)
                {
                    _value = _maximum;
                }
                else
                {
                    _value = value;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Максимальное значение
        /// </summary>
        public int Maximum
        {
            get => _maximum;
            set
            {
                if (value < _minimum)
                {
                    _maximum = _minimum;
                }
                else
                {
                    _maximum = value;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Минимальное значение
        /// </summary>
        public int Minimum
        {
            get => _minimum;
            set
            {
                if (value > _maximum)
                {
                    _minimum = _maximum;
                }
                else
                {
                    _minimum = value;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Конец прогресса неизвестен
        /// </summary>
        /// <remarks>
        /// В этом случае прогресс будет показан текстом
        /// </remarks>
        public bool IsInfinite
        {
            get => _isInfinite;
            set
            {
                _isInfinite = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Текущий прогресс в процентном соотношении
        /// </summary>
        public double Progress
        {
            get
            {
                if (_maximum == 0)
                {
                    return 0;
                }

                if (_value >= _minimum && _value <= _maximum)
                {
                    var range = Math.Abs(_maximum - _minimum);
                    var value = Math.Abs(_minimum + _value);

                    return (double)value / range;
                }

                return 0;
            }
        }

        /// <summary>
        /// Высота шрифта в пикселях
        /// </summary>
        public int FontSizePixels
        {
            get
            {
                if (_barRect.Height < 8)
                {
                    return 8;
                }
                if (_barRect.Height > 32)
                {
                    return 32;
                }

                return _barRect.Height;
            }
        }

        /// <summary>
        /// Паддинги полоски бара
        /// </summary>
        public Padding BarPadding
        {
            get => _barPadding;
            set
            {
                _barPadding = value;
                _barRect = Substract(ClientRectangle, BarPadding);
                Font = new Font("Montserrat", FontSizePixels, FontStyle.Bold, GraphicsUnit.Pixel);
                Invalidate();
            }
        }

        public CustomProgressBar() : base()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, value: true);
            DoubleBuffered = true;
            BarPadding = new Padding(2);
            ForeColor = Color.Gold;
        }

        public Rectangle Substract(Rectangle rect, Padding padding)
        {
            return Rectangle.FromLTRB(
                rect.Left + padding.Left,
                rect.Top + padding.Top,
                rect.Right - padding.Right,
                rect.Bottom - padding.Bottom);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            _barRect = Substract(ClientRectangle, BarPadding);
            Font = new Font("Montserrat", FontSizePixels, FontStyle.Bold, GraphicsUnit.Pixel);
            base.OnSizeChanged(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_paintBrush == null) _paintBrush = new SolidBrush(ForeColor);

            base.OnPaint(e);

            if (_isInfinite)
            {
                e.Graphics.DrawString(_value.ToString(), Font, _paintBrush, _barRect, _stringFormat);
            }
            else
            {
                var filledRect = _barRect;
                filledRect.Width = (int)(filledRect.Width * Progress);
                e.Graphics.FillRectangle(_paintBrush, filledRect);
            }
        }
    }
}
