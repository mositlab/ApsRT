using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using ApsRT.Core.Partias.PartiasClasses;
using ApsRT.Core.Props;
using ApsRT.Core.Session;
using UniversalPack.Common.Drawing;
using static UniversalPack.Extensions.EnumExtensions;
using ApsRT.Core.PlanSimulator;

namespace ApsRTCustomComponents
{

    public class GanttChart : Panel
    {

        public struct DoubleID
        {
            public Uco Part1;
            public int Part2;

            public DoubleID(Uco part1, int part2)
            {
                Part1 = part1;
                Part2 = part2;
            }

            public override string ToString()
            {
                if (Part1.IsEmpty)
                {
                    return Part1.ToString();
                }

                return $"{Part1}.{Part2}";
            }

            public static bool operator ==(DoubleID left, DoubleID right) => left.Part1 == right.Part1 && left.Part2 == right.Part2;
            public static bool operator !=(DoubleID left, DoubleID right) => left.Part1 != right.Part1 || left.Part2 != right.Part2;

            public static explicit operator DoubleID(Uco a) => new DoubleID(a, -1);
        }

        /// <summary>
        /// Конструктор.
        /// </summary>
        public GanttChart()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, value: true);
        }

        #region Даты
        /// <summary>
        /// Дата начала.
        /// </summary>
        public DateTime From;

        /// <summary>
        /// Дата конца.
        /// </summary>
        public DateTime To;

        public Action<DateTime, DateTime> ChangedDatesDelegate;

        private int timelineHeight = 50;
        /// <summary>
        /// Высота полей с датами.
        /// </summary>
        public int TimelineHeight
        {
            get => timelineHeight;
            set
            {
                timelineHeight = value;
                UpdateScrollBarParams();
            }
        }

        /// <summary>
        /// Высота полей с датами
        /// </summary>
        public int DateTimeOffset = 5;

        /// <summary>
        /// Высота полей с временем
        /// </summary>
        public int TimeOffset = 30;

        /// <summary>
        /// Стандартный шрифт для отображения даты.
        /// </summary>
        public Font StandartDateTimeFont = new Font("Montserrat", 9f, FontStyle.Bold, GraphicsUnit.Point);

        /// <summary>
        /// Стандартный шрифт для отображения времени.
        /// </summary>
        public Font StandartTimeFont = new Font("Montserrat", 9f, FontStyle.Regular, GraphicsUnit.Point);

        /// <summary>
        /// Стандартный шрифт для отображения мелочей.
        /// </summary>
        public Font VerySmallFont = new Font("Montserrat", 5f, FontStyle.Regular, GraphicsUnit.Point);

        /// <summary>
        /// Отрисовать временную линию.
        /// </summary>
        public void DrawTimeline()
        {
            var bbrush = new SolidBrush(BackColor);
            var fbrush = new SolidBrush(ForeColor);
            graphics.FillRectangle(bbrush, new Rectangle(0, 0, Width, TimelineHeight));
            graphics.DrawLine(GridPen,
                new Point(0, TimelineHeight),
                new Point(Width, TimelineHeight));
            graphics.DrawLine(GridPen,
                new Point(0, TimelineHeight - 1),
                new Point(Width, TimelineHeight - 1));
            var StartPos = RowTitleSize.X;

            var timeSpanBeg = TimeSpan.FromDays(1) - new TimeSpan(From.Hour, From.Minute, From.Second); // начальный тайм-спан
            var totaldays = (int)(To - From).TotalDays - 1;//(timeSpanBeg.TotalDays >= 1 ? 1 : 0)
            var timeSpanFin = new TimeSpan(To.Hour, To.Minute, To.Second); // ещё тайм-спан
            var daysAll = timeSpanBeg.TotalDays + timeSpanFin.TotalDays + totaldays;
            var allL = Width - StartPos;
            var one = allL / daysAll;
            var j = 0.0;
            var wasLast = 0.0;
            var itsFirst = true;

            void drawDateTime(double off)
            {
                var from = From.AddDays(j);
                var interval = (int)(one * j);
                var strtint = StartPos + interval;

                void draw()
                {
                    graphics.DrawLine(GridPen,
                        new Point(strtint, DateTimeOffset),
                        new Point(strtint, RowTitleSize.Y * RowTitles.Count + YOffset));
                    strtint++;
                    graphics.DrawLine(GridPen,
                        new Point(strtint, DateTimeOffset),
                        new Point(strtint, RowTitleSize.Y * RowTitles.Count + YOffset));
                    strtint++;
                }

                var interval_Curr = one * off; // это текущий интервал
                var curr = interval_Curr + wasLast;
                if (curr > 50)
                {
                    draw();
                    var format = "{0:dd.MM}";
                    if (curr > 80)
                    {
                        format = "{0:dd.MM (ddd)}";
                    }

                    if (curr > 120)
                    {
                        format = "{0:dd.MM.yyyy (ddd)}";
                    }

                    graphics.DrawString(string.Format(format, from), StandartDateTimeFont, fbrush,
                        new Point(strtint, DateTimeOffset));
                    wasLast = 0;
                }
                else
                {
                    if (itsFirst)
                    {
                        draw();
                    }
                    else
                    {
                        wasLast += interval_Curr;
                    }
                }
                itsFirst = false;

                // теперь рисуем само время
                var count = (int)(interval_Curr / 50); // сколько можно вместить
                var off_part = off / count; // это поделенное итоговое смещение. в цикле ща будет добавляться, для времени.
                var offpx = interval_Curr / count; // смещение в пикселях
                if (count > 1)
                {
                    for (var i = 0; i < count; i++)
                    {
                        var x = StartPos + interval + (int)(i * offpx);
                        graphics.DrawLine(GridPen,
                            new Point(x, TimeOffset),
                            new Point(x, RowTitleSize.Y * RowTitles.Count + YOffset));
                        graphics.DrawString(string.Format("{0:HH:mm}", from.AddDays(off_part * i)), StandartTimeFont, fbrush,
                        new Point(x + 2, TimeOffset));
                    }
                }

                j += off;
            }

            drawDateTime(timeSpanBeg.TotalDays);
            for (var i = 0; i < totaldays; i++)
            {
                drawDateTime(1);
            }

            drawDateTime(timeSpanFin.TotalDays);
        }

        #endregion

        #region Тайтлы строк
        private int titleRowWidth = 200;
        /// <summary>
        /// Устанавливает значение ширины для тайтлов. -1 означает требование найти автоматически нужную ширину.
        /// </summary>
        public int TitleRowWidth
        {
            get => titleRowWidth;
            set
            {
                if (titleRowWidth != value)
                {
                    titleRowWidth = value;
                    UpdateTitleSize();
                }
            }
        }

        private int titleRowHeight = -1;
        /// <summary>
        /// Устанавливает значение высоты для тайтлов. -1 означает требование найти автоматически нужную высоту.
        /// </summary>
        public int TitleRowHeight
        {
            get => titleRowHeight;
            set
            {
                if (titleRowHeight != value)
                {
                    titleRowHeight = value;
                    UpdateTitleSize();
                }
            }
        }

        /// <summary>
        /// Обновить размер ячейки текста тайтла.
        /// </summary>
        private void UpdateTitleSize()
        {
            RowTitleSize = GetMaxNeededLengthForTitles();
            if (TitleRowWidth > -1)
            {
                RowTitleSize.X = TitleRowWidth;
            }

            if (TitleRowHeight > -1)
            {
                RowTitleSize.Y = TitleRowHeight;
            }

            if (AssignedScrollBar != null)
            {
                AssignedScrollBar.LargeChange = RowTitleSize.Y * 2;
                AssignedScrollBar.SmallChange = RowTitleSize.Y;
            }
        }

        /// <summary>
        /// В этой переменной хранится информация о размере ячейки.
        /// </summary>
        private Point RowTitleSize = Point.Empty;

        public class RowTitle
        {
            public TextInfo[] Texts;
            public Uco CenterID, SubID;//DoubleID

            public RowTitle(TextInfo[] texts, Uco centerID, Uco subID)
            {
                Texts = texts;
                CenterID = centerID;
                SubID = subID;
            }
        }

        /// <summary>
        /// В этой структуре содержится информация о тексте для тайтла строки - где расположен текст, и какой у него шрифт.
        /// </summary>
        public struct TextInfo
        {
            public string Text;
            public Point LocalPosition;
            public Font UsedFont; // Каким шрифтом это писать?
            public Color Color;

            public TextInfo(string text, Font usedFont = null)
            {
                Text = text;
                LocalPosition = new Point(0, 0);
                UsedFont = usedFont;
                Color = Color.Black;
            }

            public TextInfo(string text, Point localPosition, Font usedFont = null) : this(text, usedFont)
            {
                LocalPosition = localPosition;
            }

            public TextInfo(string text, Point localPosition, Font usedFont, Color color) : this(text, localPosition, usedFont)
            {
                Color = color;
            }
        }

        /// <summary>
        /// Список тайтлов непосредственно.
        /// </summary>
        List<RowTitle> SavedRowTitles = new List<RowTitle>(0);
        List<RowTitle> RowTitles = new List<RowTitle>(0);
        Dictionary<int, int> GlobalToLocalRows = null; // превращение из локальных в глобальные

        /// <summary>
        /// Обновляет список тайтлов.
        /// </summary>
        /// <param name="rowTitles">Список.</param>
        public void SetRowTitles(List<RowTitle> rowTitles)
        {
            SavedRowTitles = rowTitles;
            RowTitles = new(SavedRowTitles);
            UpdateTitleSize();
            UpdateScrollBarParams();
        }

        /// <summary>
        /// Стандартный шрифт для отображения тайтла.
        /// </summary>
        public Font StandartRowTitleFont = new Font("Montserrat", 9f, FontStyle.Regular, GraphicsUnit.Point);

        /// <summary>
        /// Рисует наименования.
        /// </summary>
        public void DrawRowTitles()
        {
            if (RowTitles.Count == 0)
            {
                return;
            }

            var brush = new SolidBrush(Color.Black);
            for (var i = 0; i < RowTitles.Count; i++)
            {
                for (var j = 0; j < RowTitles[i].Texts.Length; j++)
                {
                    var titl = RowTitles[i].Texts[j];
                    brush.Color = titl.Color;
                    graphics.DrawString(titl.Text, titl.UsedFont ?? StandartRowTitleFont, brush,
                        new Point(titl.LocalPosition.X, YOffset + titl.LocalPosition.Y + RowTitleSize.Y * i));
                    var y = YOffset + RowTitleSize.Y * (i + 1);
                    graphics.DrawLine(GridPen, new Point(0, y), new Point(Width, y));
                }
            }
        }

        /// <summary>
        /// Получает максимальную требуемую ширину и высоту для текста на строке.
        /// </summary>
        /// <returns>Размер прямоугольника, способного вместить все требуемые тексты.</returns>
        public Point GetMaxNeededLengthForTitles()
        {
            if (SavedRowTitles.Count == 0)
            {
                return Point.Empty;
            }

            var graphics = CreateGraphics();
            var max = Point.Empty;
            for (var i = 0; i < SavedRowTitles.Count; i++)
            {
                for (var j = 0; j < SavedRowTitles[i].Texts.Length; j++)
                {
                    var titl = SavedRowTitles[i].Texts[j];
                    var size = graphics.MeasureString(titl.Text, titl.UsedFont ?? StandartRowTitleFont);
                    var s = (int)(titl.LocalPosition.X + size.Width);
                    if (s > max.X)
                    {
                        max.X = s;
                    }

                    s = (int)(titl.LocalPosition.Y + size.Height);
                    if (s > max.Y)
                    {
                        max.Y = s;
                    }
                }
            }
            return max;
        }

        #endregion

        #region Бары
        /// <summary>
        /// Класс для собственно бара.
        /// </summary>
        public class Bar
        {
            /// <summary>
            /// Дата начала.
            /// </summary>
            public DateTime From;

            /// <summary>
            /// Дата конца.
            /// </summary>
            public DateTime To;

            /// <summary>
            /// Цвет бара.
            /// </summary>
            public Color Color;

            /// <summary>
            /// Цвет при наведении на бар.
            /// </summary>
            public Color HoverColor;

            /// <summary>
            /// Дополнительные цвета бара.
            /// </summary>
            public Color? SubColor1, SubColor2, SubColor3, SubColor4, SubColor5, SubColor6;

            /// <summary>
            /// Информация, появляющаяся при наведении на бар.
            /// </summary>
            public string Description;

            /// <summary>
            /// На какой строке его рендерить.
            /// </summary>
            public int RowIndex;

            /// <summary>
            /// Индекс операции, имеющей отношение к тому бару. Если -1, то это не операция.
            /// </summary>
            public int OperID;

            /// <summary>
            /// Смещение по Y
            /// </summary>
            public float OffsetK;

            /// <summary>
            /// Размер по Y
            /// </summary>
            public float SizeK;

            /// <summary>
            /// Наладочная длина
            /// </summary>
            public float NaladLength;

            public List<(DateTime, DateTime)> fixingBars;

            public Bar(DateTime from, DateTime to, Color color, Color hoverColor, string description, int rowIndex, float offset, float size, int operId, float firstNaladLength, Color? subColor1 = null, Color? subColor2 = null, Color? subColor3 = null, Color? subColor4 = null, Color? subColor5 = null, Color? subColor6 = null)
            {
                From = from;
                To = to;
                Color = color;
                HoverColor = hoverColor;
                SubColor1 = subColor1;
                SubColor2 = subColor2;
                SubColor3 = subColor3;
                SubColor4 = subColor4;
                SubColor5 = subColor5;
                SubColor6 = subColor6;
                Description = description;
                OperID = operId;
                RowIndex = rowIndex;
                OffsetK = offset;
                SizeK = size;
                NaladLength = firstNaladLength;
            }
        }

        /// <summary>
        /// Список ползунков
        /// </summary>
        List<Bar> Bars = new List<Bar>();

        public Dictionary<int, Bar> IndexToBar = new(); // мелкая копия для быстрого поиска

        public void ClearBars()
        {
            Bars.Clear();
            IndexToBar.Clear();
        }

        public void AddBar(Bar bar, int operN = -1)
        {
            if (operN > -1)
            {
                if (IndexToBar.TryGetValue(operN, out var barGot))
                {
                    if (barGot.From > bar.From)
                    {
                        barGot.From = bar.From;
                    }

                    if (barGot.To < bar.To)
                    {
                        barGot.To = bar.To;
                    }

                    //IndexToBarDT[operN] = dates;
                }
                else
                {
                    IndexToBar.Add(operN, bar);
                }
            }
            Bars.Add(bar);
        }

        /// <summary>
        /// На сколько уменьшать верхние границы баров.
        /// </summary>
        public int BarBorderMinus = 8;

        /// <summary>
        /// Текущий наведённый бар
        /// </summary>
        public int CurrentHoveredBar = -1;

        /// <summary>
        /// Рисует бары.
        /// </summary>
        void DrawBars()
        {
            var cp = PointToClient(Cursor.Position);
            //Point best = new PointF(float.MaxValue, float.MaxValue);

            var NearestBarDistance = float.MaxValue;
            NearestBarToInsert = -1;
            var NearestBarToInsertRow = -1;

            var L = Width - RowTitleSize.X;

            RectangleF GetRect(Bar bar, int rowind)
            {
                var totalS = (To - From).TotalSeconds;
                var needF = (bar.From - From).TotalSeconds;
                var needT = (bar.To - From).TotalSeconds;
                var sX = (float)(RowTitleSize.X + Math.Max(0, L * (needF / totalS)));
                var sY = (float)(RowTitleSize.X + L * (needT / totalS));
                float pY = YOffset + RowTitleSize.Y * (rowind + bar.OffsetK) + BarBorderMinus * (bar.SizeK == 1 ? 1 : 0);
                float finalYSize = RowTitleSize.Y - BarBorderMinus * 2 - Math.Max(0, TimelineHeight - pY);
                return new RectangleF(sX,
                   Math.Max(TimelineHeight, pY),
                   sY - sX,
                   finalYSize * bar.SizeK);
            }

            RectangleF GetRectSub((DateTime From, DateTime To) bar, int rowind)
            {
                var totalS = (To - From).TotalSeconds;
                var needF = (bar.From - From).TotalSeconds;
                var needT = (bar.To - From).TotalSeconds;
                var sX = (float)(RowTitleSize.X + Math.Max(0, L * (needF / totalS)));
                var sY = (float)(RowTitleSize.X + L * (needT / totalS));
                float pY = YOffset + RowTitleSize.Y * (rowind + 0) + BarBorderMinus * 1;
                float finalYSize = RowTitleSize.Y - BarBorderMinus * 2 - Math.Max(0, TimelineHeight - pY);
                return new RectangleF(sX,
                   Math.Max(TimelineHeight, pY),
                   sY - sX,
                   2);
            }

            CurrentHoveredBar = -1;
            var brush = new SolidBrush(Color.Blue);
            var pen = new Pen(Color.Black);
            var pen2 = new Pen(Color.Black, 2.0f);
            for (var i = 0; i < Bars.Count; i++)
            {
                var bar = Bars[i];
                if (bar.From > To || bar.To < From)
                {
                    continue;
                }

                var rowind = bar.RowIndex;
                if (GlobalToLocalRows == null || GlobalToLocalRows.TryGetValue(bar.RowIndex, out rowind))
                {
                    var rect = GetRect(bar, rowind);
                    if (CurrentHoveredBar == -1 && cp.X >= rect.X && cp.X < rect.X + rect.Width && cp.Y >= rect.Y && cp.Y < rect.Y + rect.Height)
                    {
                        CurrentHoveredBar = i;
                        brush.Color = bar.HoverColor;
                    }
                    else
                    {
                        brush.Color = MainOper == -1 || bar.OperID == -2 ? (false/*bar.fixingBars != null && bar.fixingBars.Count > 0*/ ? Color.DarkBlue : bar.Color) :

                            bar.OperID == -1 ? Color.Gray :
                                bar.OperID == MainOper ? (bar.fixingBars != null && bar.fixingBars.Count > 0 ? Color.DarkBlue : bar.Color) :

                                            AsPrevs.Contains(bar.OperID) ? Color.Aquamarine :

                                                AsNexts.Contains(bar.OperID) ? Color.HotPink : Color.Gray



                            ;
                    }
                    graphics.FillRectangle(brush, rect);
                    if (CurrentPressedBar == i)
                    {
                        graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
                    }

                    if (IsDragging && PossibleOpersForClicked != null && PossibleOpersForClicked.Contains(bar.OperID))
                    {
                        graphics.DrawLine(pen2, new PointF(rect.X, rect.Y - 1), new PointF(rect.X, rect.Y + rect.Height + 1));
                        var dist = MathF.Sqrt(MathF.Pow(cp.X - rect.X, 2) + MathF.Pow(cp.Y - rect.Y, 2));
                        if (dist < NearestBarDistance)
                        {
                            NearestBarDistance = dist;
                            NearestBarToInsert = i;
                            NearestBarToInsertRow = rowind;
                        }
                    }

                    var morecolorn = (float)rect.Height / 6;

                    if (ShowFixedColors && bar.SubColor1.HasValue)
                    {
                        brush.Color = bar.SubColor1.Value;
                        graphics.FillRectangle(brush, new RectangleF(rect.X, rect.Y + morecolorn * 5, rect.Width, morecolorn));
                    }
                    if (ShowAltRCsColors && bar.SubColor2.HasValue)
                    {
                        brush.Color = bar.SubColor2.Value;
                        graphics.FillRectangle(brush, new RectangleF(rect.X, rect.Y + morecolorn * 4, rect.Width, morecolorn));
                    }
                    if (ShowFreeColors && bar.SubColor3.HasValue)
                    {
                        brush.Color = bar.SubColor3.Value;
                        graphics.FillRectangle(brush, new RectangleF(rect.X, rect.Y + morecolorn * 3, rect.Width, morecolorn));
                    }
                    if (DrawMoreModColors)
                    {
                        if (bar.SubColor4.HasValue)
                        {
                            brush.Color = bar.SubColor4.Value;
                            graphics.FillRectangle(brush, new RectangleF(rect.X, rect.Y + morecolorn * 2, rect.Width, morecolorn));
                        }
                        if (bar.SubColor6.HasValue)
                        {
                            brush.Color = bar.SubColor6.Value;
                            graphics.FillRectangle(brush, new RectangleF(rect.X, rect.Y, rect.Width, morecolorn));
                        }
                    }
                    if (DrawMoreOperColors && bar.SubColor5.HasValue)
                    {
                        brush.Color = bar.SubColor5.Value;
                        graphics.FillRectangle(brush, new RectangleF(rect.X, rect.Y + morecolorn, rect.Width, morecolorn));
                    }

                    if (rect.Width > 10)
                    {
                        brush.Color = Color.Black;
                        graphics.DrawString(bar.OperID.ToString(), VerySmallFont, brush, rect.X, rect.Y);

                    }

                    if (bar.fixingBars != null)
                        foreach (var sub in bar.fixingBars)
                        {
                            var rct = GetRectSub(sub, rowind);
                            brush.Color = Color.Violet;
                            graphics.FillRectangle(brush, rct);
                        }

                    if (bar.OperID > -1 && bar.NaladLength > 0)
                    {
                        brush.Color = Color.Orange;
                        var naladRect = new RectangleF(rect.X, rect.Y - BarBorderMinus, MathF.Min(rect.Width, 30), BarBorderMinus);
                        graphics.FillRectangle(brush, naladRect);
                        if (rect.Width > 10)
                        {
                            brush.Color = Color.Black;
                            graphics.DrawString(bar.NaladLength.ToString(), VerySmallFont, brush, naladRect.X, naladRect.Y);
                        }
                    }
                }

            }
            if (NearestBarToInsert > -1)
            {
                var rect = GetRect(Bars[NearestBarToInsert], NearestBarToInsertRow);
                brush.Color = Color.Black;

                PointF[] curvePoints = { new PointF(rect.X - 8, rect.Y - 8), new PointF(rect.X + 8, rect.Y - 8), new PointF(rect.X, rect.Y) };

                graphics.FillPolygon(brush, curvePoints);

                var bar = Bars[CurrentPressedBar];
                brush.Color = bar.HoverColor;
                rect = GetRect(bar, 0);
                graphics.FillRectangle(brush, cp.X, cp.Y, rect.Width, rect.Height);
            }
        }
        #endregion

        #region Вертикальный ползунок
        /// <summary>
        /// Ссылка на вертикальный скролл-бар для удобного управления
        /// </summary>
        VScrollBar AssignedScrollBar;

        void UpdateScrollBarParams()
        {
            if (AssignedScrollBar == null)
            {
                return;
            }

            var size = RowTitleSize.Y * RowTitles.Count;
            var size2 = size;
            var total = Height - TimelineHeight;
            size -= total;
            size += RowTitleSize.Y * 2;
            if (size < 0)
            {
                size = 0;
            }

            AssignedScrollBar.Maximum = size;
            if (RowTitles.Count == 0)
            {
                return;
            }

            size2 -= TimelineHeight;
            size2 += YOffset;
            var h = Height - TimelineHeight;
            var final = h - size2;
            if (final < 0)
            {
                final = 0;
            }

            if (AssignedScrollBar.Value >= final)
            {
                AssignedScrollBar.Value -= final;
                UpdateYOffset();
            }
        }

        public void AssignScrollBar(VScrollBar bar)
        {
            AssignedScrollBar = bar;
            bar.Minimum = 0;
            bar.Value = 0;
            UpdateScrollBarParams();
            bar.LargeChange = RowTitleSize.Y * 2;
            bar.SmallChange = RowTitleSize.Y;
            bar.ValueChanged += delegate
            {
                UpdateYOffset();
                DrawChart();
            };
        }

        /// <summary>
        /// На сколько вверх подвинут график.
        /// </summary>
        int YOffset = 0;

        /// <summary>
        /// Обновить текущее смещение из-за скроллбара.
        /// </summary>
        void UpdateYOffset()
        {
            if (AssignedScrollBar != null)
            {
                YOffset = TimelineHeight - AssignedScrollBar.Value;
            }
        }

        #endregion

        #region Данные для показа

        /// <summary>
        /// Может ли двигать операции вместо сокрытия лишних
        /// </summary>
        public bool CanMoveOperations = false;

        /// <summary>
        /// Сохранённый список операций конкретно этой диаграммы. Его можно заюзать 
        /// </summary>
        public List<PlanSimulatorBlock> SavedOpersLink;
        public int PoolIndex;

        List<int> PossibleOpersForClicked;

        /// <summary>
        /// Выбранная операция для показа.
        /// </summary>
        int MainOper = -1;

        /// <summary>
        /// Предыдущие операции.
        /// </summary>
        HashSet<int> AsPrevs;

        /// <summary>
        /// Следующие операции.
        /// </summary>
        HashSet<int> AsNexts;

        /// <summary>
        /// Дата-пак для выбора.
        /// </summary>
        public Dictionary<int, (List<int> prevs, List<int> nexts, Uco cent, List<Uco> possibleCents)> assignedDataPack;

        /// <summary>
        /// Выбрать какую-то операцию.
        /// </summary>
        public void FindAllPrevsHere(int SelectedOperID, bool ShowAltRcs, bool SelectWithBar = false)
        {
            MainOper = -1;
            if (SelectedOperID == -10) // я тыкнул мимо графика
            {
                AsPrevs = AsNexts = null;
                RowTitles = new(SavedRowTitles);
                GlobalToLocalRows = null;
                DrawChart();
                return;
            }

            if (SelectedOperID < 0)
            {
                DrawChart();
                return;
            }

            //select with bar подразумевает, что это команда от поиска пришла, а значит там нужен только один рц
            if (SelectWithBar)
            {
                CurrentPressedBar = Bars.FindIndex(x => x.OperID == SelectedOperID);
            }

            if (assignedDataPack != null)
            {
                HashSet<Uco> cents = new()
                {
                    assignedDataPack[SelectedOperID].cent
                };
                if (ShowAltRcs && assignedDataPack[SelectedOperID].possibleCents != null)
                {
                    foreach (var center in assignedDataPack[SelectedOperID].possibleCents)
                    {
                        cents.Add(center);
                    }
                }
                AsPrevs = assignedDataPack[SelectedOperID].prevs.ToHashSet();
                AsNexts = assignedDataPack[SelectedOperID].nexts.ToHashSet();
                cents = AsPrevs.Union(AsNexts)
                    .Where(x => assignedDataPack.ContainsKey(x))
                    .Select(x => assignedDataPack[x].cent)
                    .Union(cents).ToHashSet();
                //cents.Add(assignedDataPack[SelectedOperID].cent);
                //HashSet<int> prevs = new();
                //HashSet<int> nexts = new();
                //Queue<int> toCheckPrev = new();
                //foreach (var prev in assignedDataPack[SelectedOperID].prevs)
                //{
                //    toCheckPrev.Enqueue(prev);
                //}

                //Queue<int> toCheckNext = new();
                //foreach (var next in assignedDataPack[SelectedOperID].nexts)
                //{
                //    toCheckNext.Enqueue(next);
                //}

                //HashSet<int> already = new();
                //while (toCheckPrev.Count > 0 || toCheckNext.Count > 0)
                //{

                //    void add(int N, bool asnext)
                //    {
                //        if (asnext)
                //        {
                //            nexts.Add(N);
                //        }
                //        else
                //        {
                //            prevs.Add(N);
                //        }

                //        cents.Add(assignedDataPack[N].cent);
                //        foreach (var prev in assignedDataPack[N].prevs)
                //        {
                //            if (!toCheckPrev.Contains(prev) && !already.Contains(prev))
                //            {
                //                toCheckPrev.Enqueue(prev);
                //                already.Add(prev);
                //            }
                //        }
                //        foreach (var next in assignedDataPack[N].nexts)
                //        {
                //            if (!toCheckNext.Contains(next) && !already.Contains(next))
                //            {
                //                toCheckNext.Enqueue(next);
                //                already.Add(next);
                //            }
                //        }
                //    }

                //    if (toCheckPrev.Count > 0)
                //    {
                //        add(toCheckPrev.Dequeue(), false);
                //    }

                //    if (toCheckNext.Count > 0)
                //    {
                //        add(toCheckNext.Dequeue(), true);
                //    }
                //}
                //AsPrevs = prevs;
                //AsNexts = nexts;

                if (AsPrevs.Count > 0 || AsNexts.Count > 0 || SelectWithBar) //
                {
                    MainOper = SelectedOperID;
                    //if (cents.Contains(SavedRowTitles[i].CenterID))

                    GlobalToLocalRows = new Dictionary<int, int>();
                    RowTitles.Clear();
                    var j = 0;
                    for (var i = 0; i < SavedRowTitles.Count; i++)
                    {
                        if (SelectWithBar && SavedRowTitles[i].CenterID == assignedDataPack[SelectedOperID].cent || !SelectWithBar && cents.Contains(SavedRowTitles[i].CenterID))
                        {
                            RowTitles.Add(SavedRowTitles[i]);
                            GlobalToLocalRows.Add(i, j);
                            j++;
                            if (SelectWithBar)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    GlobalToLocalRows = null;
                    RowTitles = new(SavedRowTitles);
                }
                DrawChart();
            }
        }
        #endregion

        #region Отрисовка
        /// <summary>
        /// Текущая графика.
        /// </summary>
        Graphics graphics;

        /// <summary>
        /// Цвет решётки.
        /// </summary>
        public Pen GridPen = new Pen(Color.Gray);

        /// <summary>
        /// Отрисовать весь график.
        /// </summary>
        public void DrawChart() => Invalidate();

        /// <summary>
        /// Рисовать ли цвета операций.
        /// </summary>
        bool DrawMoreOperColors = false;
        /// <summary>
        /// Рисовать ли цвета моделирований.
        /// </summary>
        bool DrawMoreModColors = false;
        /// <summary>
        /// Рисовать ли цвета фиксированных операций.
        /// </summary>
        bool ShowFixedColors = true;
        /// <summary>
        /// Рисовать ли цвета свободных операций.
        /// </summary>
        bool ShowFreeColors = true;
        /// <summary>
        /// Рисовать ли цвета операций на альт. РЦ.
        /// </summary>
        bool ShowAltRCsColors = true;

        /// <summary>
        /// Перерисовать график с учётом настроек цветов.
        /// </summary>
        public void UpdateColorsInfoAndPaint(bool moreOpColors, bool moreModColors, bool Fixed, bool Free, bool altRC)
        {
            DrawMoreOperColors = moreOpColors;
            DrawMoreModColors = moreModColors;
            ShowFixedColors = Fixed;
            ShowFreeColors = Free;
            ShowAltRCsColors = altRC;
            DrawChart();
        }

        /// <summary>
        /// Провоцирует отрисовку только баров (должен был). Но по так нельзя, и он нужен, чтобы при движении мыши менялись цвета.
        /// </summary>
        bool assigned = false;
        int last = -1;
        Point lastMousePos;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (CurrentHoveredBar > -1)
            {
                if (!assigned || last != CurrentHoveredBar)
                {
                    assigned = true;
                    last = CurrentHoveredBar;
                    var bar = Bars[CurrentHoveredBar];
                    //Infimum.Form1.This.ganttTip.SetToolTip(this, );
                    //Infimum.Form1.This.ganttTip.ToolTipTitle = "???";

                    //Form1.This.ganttTip.Hide(this);
                    //Form1.This.ganttTip.Show($"{bar.Description}\r\nПериод: С {bar.From} по {bar.To} (длительность {bar.To - bar.From})", this, 60000);
                }
            }
            else
            {
                if (assigned)
                {
                    //Form1.This.ganttTip.Hide(this);
                    assigned = false;
                }
            }
            if (CanMoveOperations && CurrentPressedBar > -1)
            {
                if (IsMouseDown && e.Location != lastMousePos && !IsDragging)
                {
                    IsDragging = true;
                    var index = Bars[CurrentPressedBar].OperID;
                    if (index > -1)
                    {
                        var opers0 = SavedOpersLink.Find(y => y.Lists.Any(x => x.StagesAsNormal.Any(z => z.Index == index)));
                        var opers = opers0.Lists[0];// тут по идее можно оставить 0, т.к. все равно рц один
                        var center = opers0.GetCenterData(PoolIndex).Center;
                        var possible_centers = opers.StagesAsNormal[0].PossibleCenters;
                        if (possible_centers == null)
                        {
                            possible_centers = new List<Uco>();
                        }

                        foreach (var oper in opers.StagesAsNormal)
                        {
                            if (oper.PossibleCenters == null)
                            {
                                continue;
                            }

                            if (possible_centers.Count == 0)
                            {
                                possible_centers.AddRange(oper.PossibleCenters);
                            }
                            else
                            {
                                possible_centers = possible_centers.Intersect(oper.PossibleCenters).ToList();
                            }
                        }
                        if (possible_centers.Count == 0)
                        {
                            possible_centers = null;
                        }

                        var ops = SavedOpersLink//x.List[0].Oper.PossibleCenters == null ? (x.List[0].ActualRC == center) : x.List[0].Oper.PossibleCenters.Contains(center)
                            .Where(x => possible_centers == null ?
                            x.GetCenterData(PoolIndex).Center == center
                            : possible_centers.Contains(x.GetCenterData(PoolIndex).Center))
                            .SelectMany(x => x.Lists[0].StagesAsNormal) // тут вместо 0 имеет смысл ставить все рц, которые можно выбрать
                            .ToList();

                        var allPrevs = opers.StagesAsNormal.SelectMany(x => x.Prevs).Distinct().ToList();

                        if (allPrevs.Count > 0)
                        {
                            var mindat = allPrevs.Max(x => IndexToBar[x].To);
                            ops = ops.Where(x => IndexToBar[x.Index].From >= mindat).ToList();
                        }

                        var allNexts = opers.StagesAsNormal.SelectMany(x => x.Nexts).Distinct().ToList();

                        if (allNexts.Count > 0)
                        {
                            var mindat = allNexts.Min(x => IndexToBar[x].From);
                            ops = ops.Where(x => IndexToBar[x.Index].To <= mindat).ToList();
                        }

                        PossibleOpersForClicked = ops.Select(x => x.Index).ToList();
                    }
                    else
                    {
                        PossibleOpersForClicked = null;
                    }
                }
            }
            Invalidate();
        }

        /// <summary>
        /// При изменении размера срабатывает форма.
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateScrollBarParams();
            Invalidate();
        }

        /// <summary>
        /// Функция отрисовки.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            try
            {
                graphics = pe.Graphics;
                graphics.Clear(BackColor);
                if (RowTitleSize.IsEmpty)
                {
                    UpdateTitleSize();
                }

                UpdateYOffset();
                DrawRowTitles();
                DrawTimeline();
                DrawBars();
            }
            catch
            {

            }
        }
        #endregion

        #region Управление

        /// <summary>
        /// Ссылка на текстбокс для информации.
        /// </summary>
        public TextBox AssignedTextBox;

        /// <summary>
        /// Текущий нажатый бар.
        /// </summary>
        int CurrentPressedBar = -1;

        bool IsDragging = false;
        bool IsMouseDown = false;

        /// <summary>
        /// Перед каким баром можно вставить операцию
        /// </summary>
        int NearestBarToInsert = -1;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            IsMouseDown = true;
            lastMousePos = e.Location;
            CurrentPressedBar = CurrentHoveredBar;
            FindAllPrevsHere(CurrentHoveredBar == -1 ? -10 : Bars[CurrentHoveredBar].OperID, CanMoveOperations);
            if (AssignedTextBox != null)
            {
                if (CurrentHoveredBar == -1)
                {
                    AssignedTextBox.Text = "";
                }
                else
                {
                    var bar = Bars[CurrentHoveredBar];
                    AssignedTextBox.Text = bar.Description + $"\r\nПериод: С {bar.From} по {bar.To} (длительность {bar.To - bar.From})";
                }
            }
        }

        public event Action<int, int, Uco> OnAnalysis;

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            IsDragging = false;
            IsMouseDown = false;
            if (NearestBarToInsert > -1 && NearestBarToInsert > -1 && CurrentPressedBar > -1 && NearestBarToInsert != CurrentPressedBar)
            {
                // Обновляем это дело
                var index = Bars[NearestBarToInsert].OperID;
                var res1 = SavedOpersLink.Find(x => x.Lists.Any(y => y.StagesAsNormal.Any(z => z.Index == index))).GetCenterData(PoolIndex).Center;
                var ind = Bars[CurrentPressedBar].OperID;
                OnAnalysis?.Invoke(ind, index, res1);
                CurrentPressedBar = Bars.FindIndex(x => x.OperID == ind);
            }
            //CurrentHoveredBar = -1;
            //CurrentPressedBar = -1;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
            {
                var diff = (To - From).TotalDays;
                var fin = To;
                fin = fin.AddDays(diff / 20f * (e.Delta > 0 ? 1f : -1f));
                if (fin < From)
                {
                    fin = From.AddMinutes(60);
                }

                ChangedDatesDelegate?.Invoke(From, fin);
            }
            else
            {
                if (Control.ModifierKeys == Keys.Shift)
                {
                    var diff = (To - From).TotalDays;
                    var val = diff / 20f * (e.Delta > 0 ? 1f : -1f);
                    ChangedDatesDelegate?.Invoke(From.AddDays(val), To.AddDays(val));
                }
                else
                {
                    if (AssignedScrollBar != null)
                    {
                        var val = AssignedScrollBar.Value;
                        val += e.Delta / -4;
                        if (val < AssignedScrollBar.Minimum)
                        {
                            val = AssignedScrollBar.Minimum;
                        }

                        if (val > AssignedScrollBar.Maximum)
                        {
                            val = AssignedScrollBar.Maximum;
                        }

                        AssignedScrollBar.Value = val;
                        UpdateYOffset();
                        UpdateScrollBarParams();
                    }
                }
            }
        }
        #endregion
    }
}
