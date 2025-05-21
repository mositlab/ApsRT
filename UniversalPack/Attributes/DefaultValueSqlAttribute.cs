using System;

namespace UniversalPack.Attributes
{
    /// <summary>
    /// Set SQL expression as default field value
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DefaultValueSqlAttribute : Attribute
    {
        /// <summary>
        /// SQL expression
        /// </summary>
        public string Expression { get; }

        /// <summary>
        /// <inheritdoc cref="DefaultValueSqlAttribute"/>
        /// </summary>
        /// <param name="expression">SQL expression</param>
        public DefaultValueSqlAttribute(string expression)
        {
            Expression = expression;
        }
    }
}
