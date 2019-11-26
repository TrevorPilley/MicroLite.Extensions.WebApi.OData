﻿// -----------------------------------------------------------------------
// <copyright file="OrderByBinder.cs" company="Project Contributors">
// Copyright 2012 - 2019 Project Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// </copyright>
// -----------------------------------------------------------------------
namespace MicroLite.Extensions.WebApi.OData.Binders
{
    using System;
    using MicroLite.Builder.Syntax.Read;
    using MicroLite.Mapping;
    using Net.Http.WebApi.OData.Query;
    using Net.Http.WebApi.OData.Query.Binders;

    /// <summary>
    /// The binder class which can append the $order by query option.
    /// </summary>
    public sealed class OrderByBinder : AbstractOrderByBinder
    {
        private readonly IObjectInfo objectInfo;
        private readonly IOrderBy orderBySqlBuilder;

        private OrderByBinder(IObjectInfo objectInfo, IOrderBy orderBySqlBuilder)
        {
            this.objectInfo = objectInfo;
            this.orderBySqlBuilder = orderBySqlBuilder;
        }

        /// <summary>
        /// Binds the order by query option to the sql builder.
        /// </summary>
        /// <param name="orderByQueryOption">The order by query.</param>
        /// <param name="objectInfo">The IObjectInfo for the type to bind the order by list for.</param>
        /// <param name="orderBySqlBuilder">The order by SQL builder.</param>
        /// <returns>The SqlBuilder after the order by clause has been added.</returns>
        public static IOrderBy BindOrderBy(OrderByQueryOption orderByQueryOption, IObjectInfo objectInfo, IOrderBy orderBySqlBuilder)
        {
            if (objectInfo is null)
            {
                throw new ArgumentNullException(nameof(objectInfo));
            }

            if (orderBySqlBuilder is null)
            {
                throw new ArgumentNullException(nameof(orderBySqlBuilder));
            }

            if (orderByQueryOption != null)
            {
                var orderByBinder = new OrderByBinder(objectInfo, orderBySqlBuilder);
                orderByBinder.Bind(orderByQueryOption);
            }
            else
            {
                orderBySqlBuilder.OrderByAscending(objectInfo.TableInfo.IdentifierColumn.ColumnName);
            }

            return orderBySqlBuilder;
        }

        /// <summary>
        /// Binds the specified <see cref="Net.Http.WebApi.OData.Query.OrderByProperty" />.
        /// </summary>
        /// <param name="orderByProperty">The <see cref="Net.Http.WebApi.OData.Query.OrderByProperty" /> to bind.</param>
        protected override void Bind(OrderByProperty orderByProperty)
        {
            if (orderByProperty is null)
            {
                throw new ArgumentNullException(nameof(orderByProperty));
            }

            var column = this.objectInfo.TableInfo.GetColumnInfoForProperty(orderByProperty.Property.Name);

            if (orderByProperty.Direction == OrderByDirection.Ascending)
            {
                this.orderBySqlBuilder.OrderByAscending(column.ColumnName);
            }
            else
            {
                this.orderBySqlBuilder.OrderByDescending(column.ColumnName);
            }
        }
    }
}