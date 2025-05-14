using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UniversalPack.Extensions
{
    public static class ValidationExtensions
    {

        public static bool TryUniversalValidation<T>(this T Object) where T : class
        {
            return Validator.TryValidateObject(Object, new ValidationContext(Object), null, true);
        }

        public static bool TryUniversalValidation<T>(this T Object, bool showMessageBox) where T : class
            => TryUniversalValidation(Object, showMessageBox, new List<ValidationResult>());

        public static bool TryUniversalValidation<T>(this T Object, bool showMessageBox, ICollection<ValidationResult> result) where T : class
        {
            if (Validator.TryValidateObject(Object, new ValidationContext(Object), result, true))
            {
                return true;
            }
            else
            {
                if (showMessageBox)
                {
                    MessageBox.Show(string.Join("\r\n", result.Select(x => $"- {x.ErrorMessage} (поля: {(string.Join(", ", x.MemberNames))})")), "Объект не прошёл валидацию!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    Console.WriteLine("Объект не прошёл валидацию!");
                    Console.WriteLine(string.Join("\r\n", result.Select(x => $"- {x.ErrorMessage} (поля: {(string.Join(", ", x.MemberNames))})")));
                    Console.WriteLine();
                }
                return false;
            }
        }
    }
}
