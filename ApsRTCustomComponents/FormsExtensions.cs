using ApsRTCustomComponents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZedGraph;

namespace ApsRTCustomComponents
{
    public static class FormsExtensions
    {
        static IEnumerable<T> Descendant<T>(Control control) where T : class
        {
            foreach (Control child in control.Controls)
            {
                if (child is T thisControl)
                {
                    yield return (T)thisControl;
                }

                if (child.HasChildren)
                {
                    foreach (T descendant in Descendant<T>(child))
                    {
                        yield return descendant;
                    }
                }
            }
        }

        public static void SetFormStyle(this Form form, Color color, Color colorDark, Color textColor, Color backColor, Color backColor2)
        {

            form.BackColor = backColor;

            foreach (var comp in Descendant<CustomTreeView>(form))
            {
                comp.BackColor = Color.White;
                //button.ForeColor = Color.Black;
            }

            foreach (var comp in Descendant<NumericUpDown>(form))
            {
                comp.BackColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<System.Windows.Forms.Label>(form))
            {
                comp.ForeColor = textColor;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<GroupBox>(form))
            {
                comp.ForeColor = textColor;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<TabPage>(form))
            {
                comp.BackColor = backColor;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<SplitContainer>(form))
            {
                comp.BackColor = backColor;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<SplitterPanel>(form))
            {
                comp.BackColor = backColor;
                //button.ForeColor = Color.Black;
            }

            foreach (var comp in Descendant<ZedGraphControl>(form))
            {
                //comp.GraphPane.Chart.Fill.Color = backColor;
                //comp.GraphPane.Title. = backColor;
                comp.GraphPane.Chart.Fill.Brush = new System.Drawing.SolidBrush(backColor2);
                comp.GraphPane.Fill.Color = backColor;
                comp.GraphPane.Title.FontSpec.FontColor = textColor;

                comp.GraphPane.Chart.Border.Color = textColor;

                comp.GraphPane.Legend.FontSpec.FontColor = textColor;
                comp.GraphPane.Legend.Fill.Color = backColor2;
                comp.GraphPane.Legend.Fill.Brush = new System.Drawing.SolidBrush(backColor2);

                comp.GraphPane.XAxis.Color = textColor;
                comp.GraphPane.XAxis.Title.FontSpec.FontColor = textColor;
                comp.GraphPane.XAxis.MajorGrid.Color = textColor;
                comp.GraphPane.XAxis.MinorGrid.Color = textColor;
                comp.GraphPane.XAxis.MajorTic.Color = textColor;
                comp.GraphPane.XAxis.MinorTic.Color = textColor;
                comp.GraphPane.XAxis.Scale.FontSpec.FontColor = textColor;

                comp.GraphPane.YAxis.Color = textColor;
                comp.GraphPane.YAxis.Title.FontSpec.FontColor = textColor;
                comp.GraphPane.YAxis.MajorGrid.Color = textColor;
                comp.GraphPane.YAxis.MinorGrid.Color = textColor;
                comp.GraphPane.YAxis.MajorTic.Color = textColor;
                comp.GraphPane.YAxis.MinorTic.Color = textColor;
                comp.GraphPane.YAxis.Scale.FontSpec.FontColor = textColor;

                //button.ForeColor = Color.Black;
            }

            foreach (var comp in Descendant<Button>(form))
            {
                comp.BackColor = color;
                comp.FlatAppearance.MouseDownBackColor = colorDark;
                comp.FlatAppearance.MouseOverBackColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<WinFormsCustomComponents.CustomProgressBar>(form))
            {
                comp.ForeColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<WinFormsCustomComponents.MaterialCheckBox>(form))
            {
                comp.BackColor = textColor;
                comp.ForeColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<WinFormsCustomComponents.MaterialRadioButton>(form))
            {
                comp.BackColor = textColor;
                comp.ForeColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<WinFormsCustomComponents.MaterialTabSelector>(form))
            {
                comp.BackColor = color;
                //button.ForeColor = Color.Black;
            }
            foreach (var comp in Descendant<ApsRTCustomComponents.CustomTreeView>(form))
            {
                comp.CurrentTheme = color;
                //button.ForeColor = Color.Black;
            }
        }
    }
}
