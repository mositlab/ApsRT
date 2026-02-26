using Uco = UniversalPack.Common.CodedObject<System.Guid, string>;
using ApsRT.Core.Partias.PartiasClasses;
using ApsRT.Core.Props;
using ApsRT.Core.Session;
using UniversalPack.Common.Drawing;
using static UniversalPack.Extensions.EnumExtensions;
using ApsRT.Core.Taras.TaraSearchEngine;
using UniversalPack.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApsRTCustomComponents
{
    public class CustomTreeView : Panel
    {

        public Session SessionLink;

        public CustomTreeView() : base()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, value: true);
        }

        internal class Node
        {
            //internal string Label;
            internal List<(string Label, Color color, double Count)> OpersData;
            //internal double TotalCount;
            internal double MaxCount;
            internal (double, double, double) NaladLengths;
            internal Uco UsedTara;
            internal string Data;
            internal HashSet<int> IDs;
            internal bool IsVspom;
            internal Color[] ZadelsColors;
            internal Color TextColor;
        }

        SortedList<int, List<Node>> Nodes = new();

        internal Dictionary<Node, List<Node>> Previous = new();

        Dictionary<Node, (int X, int Y)> NodesPositions = new();

        internal int BarabanasCount = 0;
        internal float K = 0;
        internal float GPsNow = 0;
        internal float MaxLongestCount = 0;

        public void SetThisTree(Dictionary<int, ApsRT.Core.NaladLengths.GottenNaladLengthStruct> NaladLengthsData, params PartiaClass[] partiaClasses)
        {
            RemoveAll();
            BarabanasCount = partiaClasses.Sum(x => x.OpersHere.Sum(y => y.Count));
            K = partiaClasses[0].LastK;
            GPsNow = partiaClasses[0].GPsCountNow;
            MaxLongestCount = partiaClasses[0].MaxThereCountForLongestOpers;


            Dictionary<int, Node> prevs = new();

            Dictionary<int, Color> HashCodeToColor = new();
            Color GetColorHere(int hash)
            {
                if (!HashCodeToColor.ContainsKey(hash))
                {
                    HashCodeToColor.Add(hash, Colors.NewRandomColor());
                }
                return HashCodeToColor[hash];
            }

            foreach (var partiaClass1 in partiaClasses)
            {
                PartiaModelling maxSdvig = partiaClass1.OpersHere.SelectMany(x => x).MaxBy(y => y.Opers[0].UsesVspomTara || y.ZadelCode != 0 || y.Opers[0].IsVspom || y.Opers[0].IsLast ? int.MinValue : y.Opers[0].LongestDuration);

                    var offset = partiaClass1.OpersHere.Count / 2;
                for (var i = 0; i < partiaClass1.OpersHere.Count; i++)
                {
                    for (var j = 0; j < partiaClass1.OpersHere[i].Count; j++)
                    {
                        //var naladLengthIndex = partiaClass1.OpersHere[i][j].Opers[0].Index;

                        var n1 = partiaClass1.OpersHere[i][j].Opers.Select(op => NaladLengthsData != null && NaladLengthsData.TryGetValue(op.Index, out var nres) ? (nres.NaladLengthHere.Value, nres.prevKoeff) : (0, 1));

                        var curr = n1.Max(x => x.Item1);

                        var sumMod = partiaClass1.OpersHere[i][j].Opers.Sum(x => x.Count);

                        var k = n1.Min(x => x.Item2);

                        var next = k * sumMod - sumMod;

                        var node = new Node
                        {
                            //node.Label = string.Join("\r\n", partiaClass1.OpersHere[i][j].Opers.Select(x => $"{x.ID} ({(groups == null ? "" : (groups.TryGetValue(x.ID, out var val) ? val : 0))}): {Operations.OperationsIntToStr[x.Oper]} | {x.SpecificationCode}")).ToString();

                            UsedTara = partiaClass1.OpersHere[i][j].Tara,

                            MaxCount = partiaClass1.OpersHere[i][j].Max,

                            NaladLengths = (curr, next, k),

                            OpersData = partiaClass1.OpersHere[i][j].Opers
                            .Select(x => ($"{Math.Round(x.Count, 3)}\r\n{x.Nomenklature}", x.Color, x.Count)).ToList()
                        };

                        //node.Label = string.Join("\r\n",
                        //    partiaClass1.OpersHere[i][j].Opers
                        //    .Select(x => $"{x.Count}\r\n{x.NomenName}");
                        //.Select(x => $"{x.ID} ({(groups == null ? "" : (groups.TryGetValue(x.ID, out var val) ? val : 0))}): {Operations.OperationsIntToStr[x.Oper]} | {x.SpecificationCode}")).ToString();


                        var sum = TaraSearchEngine.SumIsPossible(partiaClass1.OpersHere[i][j], out var res, out _, new SumIsPossibleOptions(false));

                        var opers = partiaClass1.OpersHere[i][j].Opers;
                        var justSum = /*opers[0].Modelling != Guid.Empty ? opers.GroupBy(x => x.Modelling).Sum(x => x.First().Count) : */opers.Sum(x => x.Count);
                        node.Data = $"{partiaClass1.OpersHere[i][j].Opers.Count} шт\r\n" +
                            //$"не менять тару: {notch}\r\n" +
                            $"\r\nПоставлена: {partiaClass1.OpersHere[i][j].Tara} ({partiaClass1.OpersHere[i][j].Max} {partiaClass1.OpersHere[i][j].Format.DimensionUnitToString()}) на РЦ {partiaClass1.OpersHere[i][j].RC}"


                        + $"\r\nпросто сумма: {justSum}"
                        + $"\r\nMES сумма: {partiaClass1.OpersHere[i][j].Opers.Sum(x => x.CountMES)}" +
                        $"\r\nНаладочные длины: [{string.Join(";", n1.Select(x => x.Item1))}] [{next}] << [{string.Join(";", n1.Select(x => x.Item2))}]"//[{curr}] [{next}] << [{k}]
                        + $"\r\nКонеч. тара: {partiaClass1.OpersHere[i][j].Opers[0].EndTara}"
                        //+ $"\r\nКгКм сумма: {partiaClass1.OpersHere[i][j].Opers.Sum(x => x.Count * x.KgKm / 1000.0)}"
                        + $"\r\nНезависимо, Сумма возможна: {sum} ('{res.TaraName}', {res.Max} {res.used_format.DimensionUnitToString()} на РЦ {(res.FinalRC.IsEmpty ? "---" : res.FinalRC.ToString())})\r\n"
                        + $"Множитель кратности этапа: {partiaClass1.OpersHere[i][j].PlacesCountMultiplierToRewrite}\r\n\r\n"
                            + $"код задела: {partiaClass1.OpersHere[i][j].ZadelCode}, заделы: {(partiaClass1.OpersHere[i][j].ZadelsGUIDs == null ? "нет" : string.Join(", ", partiaClass1.OpersHere[i][j].ZadelsGUIDs))}\r\n\r\n"
                            + string.Join("\r\n\r\n", partiaClass1.OpersHere[i][j].Opers.Select(x =>
                            {
                                string livCodeFinal = SessionLink.MasterDataLinks.StagesMasterData.SavedOpersLivenCode.TryGetValue(x.Index, out var livCode) ? livCode.GetHashCode().ToString() : "нету";
                                return $"{x} - {x.Registrator}" +
                                $"\r\n{x.OrderLink.OrderID}" +// [Отражение факта: {Orders.GlobalOrders[x.OrderN].IsFact}, Ручной: {Orders.GlobalOrders[x.OrderN].Ruchnoe}, ВПЗ: {Orders.GlobalOrders[x.OrderN].VPZ}, Фикс.: {Orders.GlobalOrders[x.OrderN].Fixed}]" +
                                $"\r\nПродукция: {x.Production}" +
                                $"\r\nНоменклатура: {x.Nomenklature}" +
                                $"\r\nХарактеристика: {x.Characteristics}" +
                                $"\r\nТех операция вид: {x.OperationVid}" +
                                $"\r\nОриг. моделирование: {x.MesModelling}" +
                                $"\r\nКгКм: {x.KgKm}" +
                                $"\r\nКоличество: {x.Count} | МЕС: {x.CountMES}" + // | с КгКм: {x.Count * x.KgKm / 1000.0}" +
                                $"\r\nЕд. изм.: {x.EdIzm.DimensionUnitToString()}" +
                                $"\r\nМетод расчёта: {x.UsedCalcMethod.DimensionUnitToString(true)}" +
                                $"\r\nУровень: {x.Level}" +
                                $"\r\nЛивенштейна: {livCodeFinal}" +
                                $"\r\nНомер повива: {x.Poviv}" +
                                "\r\n" + string.Join("\r\n", SessionLink.MasterDataLinks.PropsMasterData.OpersPropNumberToIndex.Select(z => $"{z.Key}: {x.NumberProps[z.Value]}")) +
                                "\r\n" + string.Join("\r\n", SessionLink.MasterDataLinks.PropsMasterData.OpersPropGuidToIndex.Select(z => $"{z.Key}: {x.GuidProps[z.Value][0]}")) +
                                "\r\n" + string.Join("\r\n", SessionLink.MasterDataLinks.PropsMasterData.OpersPropStringToIndex.Select(z => $"{z.Key}: {x.StringProps[z.Value]}")) +
                                $"\r\nПодразделение: {x.Subdivision}" +
                                $"\r\nКоличество мест: {x.PlacesCount}" +
                                //$"\r\nКгКм: {x.KgKm}" +
                                $"\r\nВид раб. центра: {(x.RCRoute.IsEmpty ? "нету" : x.RCRoute.ToString())}" +
                                $"\r\nСписок РЦ: {(x.PossibleCenters == null ? x.OriginalCenter : string.Join(", ", x.PossibleCenters.Select(x => x.ToString())))}" +
                                $"\r\nНаОдномРЦ: {x.OnOneRC}" +
                                $"\r\nНеОбъединять: {x.IsLast}" +
                                $"\r\nНеМенятьТару: {x.NoChangeTara}" +
                                $"\r\nВспом. тару: {x.UsesVspomTara}" +
                                $"\r\nКод спецификации: {x.Specification}" +
                                $"\r\n{x.StageMaterialsInfo}";
                        //$"\r\nПредыдущие: {string.Join(", ", x.Prevs.Select(y => y.ToString()))}"
                        // $"\r\nВид металла: {x.MetalType}" +
                        //$"\r\nКод Лив.: {(Livenshtein.SavedOpersLivenCode.TryGetValue(x.ID, out var val) ? string.Join("_", val.Select(x => (int)x)) : "")}" +
                            }


                        ));

                        node.IsVspom = partiaClass1.OpersHere[i][j].Opers.Any(x => x.IsVspom);

                        //node.Multiple = partiaClass1.OpersHere[i][j].Opers.Count > 1;

                        //node.TotalCount = partiaClass1.OpersHere[i][j].Opers.Sum(x => x.Count);

                        node.IDs = new HashSet<int>(partiaClass1.OpersHere[i][j].Opers.Select(x => x.Index));

                        node.ZadelsColors = partiaClass1.OpersHere[i][j].ZadelsGUIDs != null && partiaClass1.OpersHere[i][j].ZadelsGUIDs.Count > 0 ?
                            //(MainLoader.StaticColors[Math.Abs(partiaClass1.OpersHere[i][j].ZadelGUID.Value.GetHashCode()) % MainLoader.StaticColors.Length]) 
                            partiaClass1.OpersHere[i][j].ZadelsGUIDs.Select(x => GetColorHere(x.GetHashCode())).ToArray()
                            : (partiaClass1.OpersHere[i][j].ZadelCode != 0 ? new[] { GetColorHere(partiaClass1.OpersHere[i][j].ZadelCode) } : null); // дибильный способ как-нибудь выбрать цвет

                        node.TextColor = node.IsVspom ? Color.White : (maxSdvig == partiaClass1.OpersHere[i][j] ? Color.Blue : Color.Black);//Color.Black;
                        //var opHere = partiaClass1.OpersHere[i][j].Opers[0];
                        //var order = OperationsDataSet.ALL[opHere.ID].OrderN;
                        //if (Orders.GlobalOrders[order].PartiaPF.HasValue)
                        //{
                        //    var guid = Orders.GlobalOrders[order].PartiaPF.Value;
                        //    if (Orders.PFPartiasData[guid].IsNotFree) node.TextColor = Color.DarkRed;
                        //    if (Orders.PFPartiasData[guid].IsNotOnlyPartFree) node.TextColor = Color.DarkOrange;
                        //}

                        var allPrevs = partiaClass1.OpersHere[i][j].Opers.SelectMany(x => x.Prevs);
                        Add(
                            i - offset, node,
                            prevs
                            .Where(x => allPrevs.Contains(x.Key))
                            .Select(x => x.Value)
                            .ToArray()
                            );
                        foreach (var op2 in partiaClass1.OpersHere[i][j].Opers)
                        {
                            prevs.TryAdd(op2.Index, node);
                        }
                    }
                }
            }
            OffsetXNeeded = - Width / 2;
            OffsetYNeeded = 0;
            WillCheck = true;
            Invalidate();
            CenterIt();
        }

        internal void SetPreviousData(Node node, params Node[] previous)
        {
            if (!Previous.ContainsKey(node))
            {
                Previous.Add(node, null);
            }

            if (previous == null || previous.Length == 0)
            {
                Previous[node] = null;
            }
            else
            {
                Previous[node] = previous.ToList();
            }
        }

        internal void Add(int Level, Node node, params Node[] Previous)
        {
            if (Nodes.ContainsKey(Level))
            {
                Nodes[Level].Add(node);
            }
            else
            {
                Nodes.Add(Level, new() { node });
            }

            SetPreviousData(node, Previous);
            UpdateDictionary();
        }

        internal void Remove(Node node)
        {
            var level = NodesPositions[node].Y;
            Nodes[level].Remove(node);
            UpdateDictionary();
        }

        internal void RemoveAll()
        {
            Nodes.Clear();
            Previous.Clear();
            NodesPositions.Clear();
        }

        void UpdateDictionary()
        {
            NodesPositions = Nodes.SelectMany(
                x => x.Value.Select(
                        (node, index) => (node, x: index, y: x.Key)
                    )
                )
                .ToDictionary(x => x.node, x => (x.x, x.y));
        }


        /// <summary>
        /// Текущая графика.
        /// </summary>
        Graphics graphics;

        /// <summary>
        /// Отрисовать весь график.
        /// </summary>
        //internal void DrawTree() => Invalidate();
        int GOffX = 0, GOffY = 0;
        float OffsetXNeeded = 0, OffsetYNeeded = 0;
        bool WillCheck = true;
        float scl = 1.0f;
        bool Moving = false;
        Point last;
        Node choosenNode = null;
        public TextBox AssignedTextBox = null;

        public Color CurrentTheme;

        public void CenterIt()
        {
            GOffX = -(int)OffsetXNeeded;
            GOffY = -(int)OffsetYNeeded;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Moving = true;
            last = e.Location;
            if (e.Button == MouseButtons.Middle)
            {
                CenterIt();
            }
            if (e.Button == MouseButtons.Right)
            {
                // Очистить всё
                choosenNode = null;
                ChosenOps.Clear();
                if (AssignedTextBox != null)
                {
                    AssignedTextBox.Text = "";
                }

                Invalidate();
            }
            var ALL = SessionLink.MasterDataLinks.StagesMasterData.AllStages;
            if (e.Button == MouseButtons.Left)
            {
                // Ищем и тыкаем на операцию
                AllIterator((press, key, j, f1, f2) =>
                {
                    if (press)
                    {
                        if (AssignedTextBox != null)
                        {
                            AssignedTextBox.Text = Nodes[key][j].Data;
                        }

                        choosenNode = Nodes[key][j];
                        ChosenOps.Clear();
                        foreach (var ID in choosenNode.IDs)
                        {
                            ChosenOps.Add((ID, false));

                            if (ALL.ContainsKey(ID))
                            {
                                foreach (var prev in ALL[ID].Prevs)
                                {
                                    ChosenOps.Add((prev, false));
                                    //ChosenOps.Add((prev, true));
                                }

                                foreach (var next in ALL[ID].Nexts)
                                {
                                    ChosenOps.Add((next, false));
                                    if (ALL.ContainsKey(next))
                                    {
                                        foreach (var next2 in ALL[next].Nexts)
                                        {
                                            ChosenOps.Add((next2, true));
                                        }
                                    }
                                }
                            }

                        }
                    }
                });
                Invalidate();
            }
        }

        void AllIterator(Action<bool/*нажата*/, int /*key*/, int /*j*/, float /*X*/, float /*YNow*/> whatToDo)
        {
            PointF cp = PointToClient(Cursor.Position);
            cp.X /= scl;
            cp.Y /= scl;
            float YNow = (Height - Nodes.Count * CellHeight) / 2 + GOffY;

            for (var i = 0; i < Nodes.Count; i++) // считаем с самого нижнего, он же самый высокий
            {
                var key = Nodes.Keys[i];
                float currentXOffset = CellWidth;//(float)Width / Nodes[key].Count;
                var Xoff = Nodes[key].Count * (CellWidth + 30) / -2f; //(currentXOffset - CellWidth) / 2;
                for (var j = 0; j < Nodes[key].Count; j++)
                {
                    var X = Xoff + (currentXOffset + 30) * j + GOffX;
                    whatToDo.Invoke(cp.X >= X && cp.X < X + CellWidth && cp.Y >= YNow && cp.Y < YNow + CellHeight, key, j, X, YNow);

                }
                YNow += CellHeight + 50;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Moving = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (Control.ModifierKeys == Keys.Shift)
            {
                CellWidth += (int)(e.Delta / 50f);
                if (CellWidth < 0)
                {
                    CellWidth = 0;
                }
            }
            else
            {
                if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
                {
                    CellHeight += (int)(e.Delta / 50f);
                    if (CellHeight < 0)
                    {
                        CellHeight = 0;
                    }
                }
                else
                {
                    scl += e.Delta * 0.001f;
                    if (scl < 0.15)
                    {
                        scl = 0.15f;
                    }

                    var pos = e.Location;
                    GOffX += (int)(pos.X / (scl * 8 * (e.Delta > 0 ? -1 : 1)));
                    GOffY += (int)(pos.Y / (scl * 8 * (e.Delta > 0 ? -1 : 1)));
                }
            }


            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (Moving)
            {
                var pos = e.Location;
                if (pos.X != last.X || pos.Y != last.Y)
                {
                    GOffX -= last.X - pos.X;
                    GOffY -= last.Y - pos.Y;
                    last = pos;
                }
            }

            //if (choosenNode != null)
            //{
            //    if (!assigned || lastN != choosenNode)
            //    {
            //        assigned = true;
            //        lastN = choosenNode;
            //        InfimumMES.Form1.This.ganttTip.Hide(this);
            //        InfimumMES.Form1.This.ganttTip.Show(choosenNode.Data, this, 60000);
            //    }
            //}
            //else
            //{
            //    if (assigned)
            //    {
            //        InfimumMES.Form1.This.ganttTip.Hide(this);
            //        assigned = false;
            //    }
            //}

            Invalidate();
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            //MessageBox.Show("double");
            //Graphics graphics = CreateGraphics();
            //PaintEventArgs pe = new PaintEventArgs(graphics, new Rectangle(0, 0, 1000, 1000));
            var bm = new Bitmap(6000, 6000);
            var gr = Graphics.FromImage(bm);
            PaintIt(gr);
            //DrawToBitmap(bm, new Rectangle(0, 0, 2000, 2000));
            //OnPaint(pe);
            bm.Save($"partia_{this.Name}.png");
        }
        /// <summary>
        /// При изменении размера срабатывает форма.
        /// </summary>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Invalidate();
        }

        /// <summary>
        /// Стандартный шрифт.
        /// </summary>
        internal Font StandartFont = new Font("Montserrat", 9f, FontStyle.Regular, GraphicsUnit.Point);
        internal Font StandartFontBold = new Font("Montserrat", 7f, FontStyle.Bold, GraphicsUnit.Point);
        internal Font StandartFontBig = new Font("Montserrat", 36f, FontStyle.Bold, GraphicsUnit.Point);

        int CellHeight = 60;
        int CellWidth = 140;

        HashSet<(int, bool)> ChosenOps = new(); // int - ID, bool - последние ли они, ради отростка в конце

        void PaintIt(Graphics graphics)
        {
            try
            {
                var stringFormat = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                Dictionary<Node, (float X, float Y)> nodesPositions = new();
                var pen = new Pen(Color.Black);
                var brush = new SolidBrush(Color.Black);
                graphics.Clear(BackColor);
                graphics.ScaleTransform(scl, scl);
                graphics.DrawString($"{BarabanasCount}; {K}; {GPsNow}; {MaxLongestCount}", StandartFontBig, brush, new PointF(10, 10));
                AllIterator((hover, key, j, X, YNow) =>
                {
                    if (WillCheck)
                    {
                        if (X < OffsetXNeeded)
                        {
                            OffsetXNeeded = X;
                        }

                        if (YNow < OffsetYNeeded)
                        {
                            OffsetYNeeded = YNow;
                        }
                    }
                    var ids = Nodes[key][j].IDs; // На каком сейчас он айдишнике
                    pen.Width = 1f / scl;
                    pen.Color = Color.Black;
                    var countHere = Nodes[key][j].OpersData.Sum(x => x.Count);
                    var multiple = Nodes[key][j].OpersData.Count > 1;
                    var isVspom = Nodes[key][j].IsVspom;
                    var SelectedBold = false;
                    var Selected = false;

                    brush.Color = isVspom ? Color.DimGray/*MediumSeaGreen*/ : Nodes[key][j].OpersData[0].color;//Nodes[key][j].Multiple ? Color.MediumPurple : Color.White;
                    if (choosenNode != null)
                    {
                        foreach (var id in ids)
                        {
                            if (choosenNode.IDs.Contains(id))
                            {
                                SelectedBold = true;
                                break;
                            }
                        }
                    }
                    //if (hover)
                    //{
                    //    pen.Color = Color.RoyalBlue;
                    //}
                    foreach (var id in ids)
                    {
                        if (ChosenOps.Contains((id, true)) || ChosenOps.Contains((id, false)))
                        {
                            // Он прям выбран
                            pen.Width = 3f / scl;
                            Selected = true;
                        }
                    }

                    // Просто рисуем

                    float DrawIt(float offset, double size, double max, Color col, string label, Color labelColor, bool isVspom = false)
                    {
                        //if (size > max) size = max;
                        var h = (float)(CellHeight * (size / max));
                        var rect = new RectangleF(X, YNow + offset, CellWidth, h);

                        brush.Color = SelectedBold ? CurrentTheme : col;
                        graphics.FillRoundedRectangle(brush, rect, 0);

                        pen.Color = Selected ? CurrentTheme : (hover ? Color.RoyalBlue : 
                        (brush.Color == Color.White ? Color.Black : Color.FromArgb((int)(col.R * 0.75f), (int)(col.G * 0.75f), (int)(col.B * 0.75f)))
                        );
                        graphics.DrawRoundedRectangle(pen, rect, 0);

                        brush.Color = labelColor;
                        graphics.DrawString(label, StandartFont, brush,
                            new RectangleF(X,//+2
                            YNow + offset, Math.Max(1, CellWidth), Math.Max(1, h)), stringFormat);//+2, -2, -2

                        offset += h;
                        return offset;
                    }

                    //if (multiple)
                    {
                        if (isVspom)
                        {
                            float offset = 0;
                            for (var i = 0; i < Nodes[key][j].OpersData.Count; i++)
                            {
                                offset = DrawIt(offset, Nodes[key][j].OpersData[i].Count, countHere, Color.DimGray, Nodes[key][j].OpersData[i].Label, Nodes[key][j].TextColor, true);
                            }
                        }
                        else
                        {
                            float offset = 0;
                            var max = Nodes[key][j].MaxCount;
                            //var lengths = lengthsBoth.Item1 + lengthsBoth.Item2;
                            if (max > 0 && countHere <= max)
                            {
                                //offset = DrawIt(offset, lengths, max, Color.Black, $"{lengthsBoth.Item1};{lengthsBoth.Item2}", Color.Orange);
                                var diff = max - countHere;
                                if (diff > 0)
                                {
                                    offset = DrawIt(offset, diff, max, Color.White, Math.Round(diff, 3).ToString(), Color.Black);
                                }

                                for (var i = 0; i < Nodes[key][j].OpersData.Count; i++)
                                {
                                    offset = DrawIt(offset, Nodes[key][j].OpersData[i].Count, max, Nodes[key][j].OpersData[i].color, Nodes[key][j].OpersData[i].Label, Nodes[key][j].TextColor);
                                }
                            }
                            else
                            {
                                //offset = DrawIt(offset, lengths, countHere + lengths, Color.Black, $"{lengthsBoth.Item1};{lengthsBoth.Item2}", Color.Orange);
                                for (var i = 0; i < Nodes[key][j].OpersData.Count; i++)
                                {
                                    offset = DrawIt(offset, Nodes[key][j].OpersData[i].Count, countHere, Nodes[key][j].OpersData[i].color, Nodes[key][j].OpersData[i].Label, Nodes[key][j].TextColor);
                                }
                            }

                        }
                    }


                    //if (multiple)
                    {
                        var lengthsBoth = Nodes[key][j].NaladLengths;
                        brush.Color = Nodes[key][j].TextColor == Color.Blue ? Color.Blue : Color.Black;
                        graphics.DrawString(Nodes[key][j].IsVspom ? countHere.ToString() : Nodes[key][j].UsedTara.Label, StandartFontBold, brush,
                            new RectangleF(X - 5,
                            YNow - 12, Math.Max(1, CellWidth + 5), 12), stringFormat);

                        graphics.DrawString($"[ {Math.Round(lengthsBoth.Item1, 3)} ; {Math.Round(lengthsBoth.Item2, 3)} ({Math.Round(lengthsBoth.Item3, 3)}) ]", StandartFontBold, brush,
                            new RectangleF(X - 5,
                            YNow - 26, Math.Max(1, CellWidth + 5), 12), stringFormat);

                        if (Nodes[key][j].ZadelsColors != null)
                        {
                            pen.Color = Selected ? CurrentTheme : (hover ? Color.RoyalBlue : Color.Black);
                            var sizeX = (float)CellWidth / Nodes[key][j].ZadelsColors.Length;
                            for (int i = 0; i < Nodes[key][j].ZadelsColors.Length; i++)
                            {
                                brush.Color = Nodes[key][j].ZadelsColors[i];
                                graphics.FillRectangle(brush, new RectangleF(X + sizeX * i, YNow + CellHeight, sizeX, 10));
                                graphics.DrawRectangle(pen, X + sizeX * i, YNow + CellHeight, sizeX, 10);
                            }

                        }
                    }

                    nodesPositions.Add(Nodes[key][j], (X, YNow));
                });

                foreach (var prevs in Previous)
                {
                    if (prevs.Value != null)
                    {
                        var pos = nodesPositions[prevs.Key];
                        pos.X += CellWidth / 2;
                        //pos.Y += CellHeight / 2;
                        var Start = new PointF(pos.X, pos.Y - 15 - 12);
                        foreach (var val in prevs.Value)
                        {
                            var b = ChosenOps.Any(x => val.IDs.Contains(x.Item1) && !x.Item2);
                            pen.Color = b ? CurrentTheme : Color.Black;
                            pen.Width = (b ? 3f : 1f) / scl;
                            var pos2 = nodesPositions[val];
                            pos2.X += CellWidth / 2;
                            pos2.Y += CellHeight;
                            var end = new PointF(pos2.X, pos2.Y);
                            graphics.DrawLine(pen, Start, end);
                            graphics.FillEllipse(brush, Start.X - 2, Start.Y - 2, 4, 4);
                        }

                    }
                }
                graphics.DrawString($"{GOffX};{GOffY};{scl}", StandartFont, brush, new PointF(3, 3));
            }
            catch
            {
                ;
            }
            WillCheck = false;
        }

        /// <summary>
        /// Функция отрисовки.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            //graphics = pe.Graphics;
            PaintIt(pe.Graphics);
        }

    }
}
