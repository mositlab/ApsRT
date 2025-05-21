using UniversalPack.Attributes;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UniversalPack.Data.Entities
{
    /// <summary>
    /// Логи
    /// </summary>
    [Table("Logs")]
    public sealed class Log : BaseEntity
    {
        /// <summary>
        /// Типы событий
        /// </summary>
        /// <remarks>
        /// Коды событий разделены на группы по 100, чтобы каждой сотне соответствовала своя группа событий
        /// </remarks>
        public enum Events : int
        {
            /// <summary>
            /// Неизвестная ошибка
            /// </summary>
            Undefined = default,

            /// <summary>
            /// Ошибка загрузки операции
            /// </summary>
            OperationLoadError = 101,

            /// <summary>
            /// Не удалось спарсить GUID операции во время загрузки операций
            /// </summary>
            OperationGuidEmptyOnOperationLoad = 102,

            /// <summary>
            /// Для операции ПовивыОбъединены = да, но не распарсился номер повива количество мест будет равно 1.";
            /// </summary>
            OperationTwistMismatch = 103,

            /// <summary>
            /// При восстановлении хронологии не были найдены операции в зависимостях
            /// </summary>
            OperationHasNoDependencies = 104,

            /// <summary>
            /// Ошибка загрузки материалов
            /// </summary>
            MaterialLoadError = 201,

            /// <summary>
            /// Не удалось спарсить GUID операции
            /// </summary>
            MaterialOperationGuidEmpty = 202,

            /// <summary>
            /// Операции не были найдены при загрузке материалов
            /// </summary>
            MaterialOperationNotFound = 203,

            /// <summary>
            /// Ошибка загрузки макс. намотки
            /// </summary>
            MaxContainerLoadError = 301,

            /// <summary>
            /// Макс. намотка не найдена из-за продукции
            /// </summary>
            MaxContainerNotFoundByProduct = 302,

            /// <summary>
            /// Макс. намотка не найдена из-за номенклатуры
            /// </summary>
            MaxContainerNotFoundByNomenclature = 303,

            /// <summary>
            /// Макс. намотка не найдена из-за тех. операции
            /// </summary>
            MaxContainerNotFoundByTechOperation = 304,

            /// <summary>
            /// Макс. намотка не найдена из-за ошибки хэша
            /// </summary>
            /// <remarks>
            /// Серьёзная ошибка
            /// </remarks>
            MaxContainerNotFoundByHashError = 305,

            /// <summary>
            /// Ошибка загрузки РЦ
            /// </summary>
            WorkCenterLoadError = 401,

            /// <summary>
            /// Не удалось спарсить GUID РЦ
            /// </summary>
            WorkCenterGuidEmpty = 402,

            /// <summary>
            /// Ошибка загрузки РЦ
            /// </summary>
            AltWorkCenterLoadError = 501,

            /// <summary>
            /// Не найдены РЦ по GUID
            /// </summary>
            AltWorkCenterGuidMissing = 502,

            /// <summary>
            /// Не удалось спарсить GUID РЦ при загрузке альт. рц
            /// </summary>
            WorkCenterGuidEmptyOnAltWorkCenterLoad = 502,

            /// <summary>
            /// Дублирование GUID и имени РЦ
            /// </summary>
            WorkCenterGuidDuplicate = 503,

            /// <summary>
            /// Ошибка загрузки тары
            /// </summary>
            ContainerLoadError = 601,

            /// <summary>
            /// Не удалось спарсить GUID РЦ при загрузке тары
            /// </summary>
            ContainerWorkCenterGuidEmpty = 602,

            /// <summary>
            /// Ошибка загрузки доп.условия
            /// </summary>
            AddConditionLoadError = 701,

            /// <summary>
            /// Ошибка загрузки зависимости
            /// </summary>
            DependenciesLoadError = 801,

            /// <summary>
            /// Не удалось спарсить GUID операции зависимости
            /// </summary>
            DependencyOperationGuidEmpty = 802,

            /// <summary>
            /// Не удалось спарсить GUID следующей операции зависимости
            /// </summary>
            DependencyNextOperationGuidEmpty = 803,

            /// <summary>
            /// Не удалось найти GUID операции зависимости
            /// </summary>
            DependencyOperationGuidNotFound = 804,

            /// <summary>
            /// Не удалось спарсить GUID следующей операции зависимости
            /// </summary>
            DependencyNextOperationGuidNotFound = 805,

            /// <summary>
            /// Ошибка загрузки переналадки
            /// </summary>
            RechangeLoadError = 901,

            /// <summary>
            /// Ошибка загрузки количества тары
            /// </summary>
            ContainerCountLoadError = 1001
        }

        /// <summary>
        /// Когда
        /// </summary>
        [Column(TypeName = "TIMESTAMP"), DefaultValueSql("CURRENT_TIMESTAMP")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Тип события
        /// </summary>
        public int EventId { get; set; }

        /// <summary>
        /// Данные
        /// </summary>
        [Required, Column(TypeName = "JSONB"), DefaultValueSql("'{}'")]
        public string Data { get; set; }

        /// <summary>
        /// Данные исключения
        /// </summary>
        [Required, Column(TypeName = "JSONB"), DefaultValueSql("'{}'")]
        public string Exception { get; set; }
    }
}
