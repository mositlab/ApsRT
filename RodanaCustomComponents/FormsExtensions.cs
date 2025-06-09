using RodanaCustomComponents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RodanaCustomComponents
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

        public static void SetFormStyle(this Form form, Color color, Color colorDark, Color textColor, Color backColor)
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
            foreach (var comp in Descendant<Label>(form))
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
            foreach (var comp in Descendant<RodanaCustomComponents.CustomTreeView>(form))
            {
                comp.CurrentTheme = color;
                //button.ForeColor = Color.Black;
            }
        }
    }
}
