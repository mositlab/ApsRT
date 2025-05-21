using UniversalPack.Attributes;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalPack.Data.Entities
{
    /// <summary>
    /// Base entity class with timestamps
    /// </summary>
    public abstract class BaseDateTimedEntity : BaseEntity
    {
        /// <summary>
        /// Date of creation
        /// </summary>
        [Column(TypeName = "DATETIME"), DefaultValueSql("GETDATE()")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Date of last update
        /// </summary>
        [Column(TypeName = "DATETIME")]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Date of deletion
        /// </summary>
        [Column(TypeName = "DATETIME")]
        public DateTime? DeletedAt { get; set; }
    }
}
