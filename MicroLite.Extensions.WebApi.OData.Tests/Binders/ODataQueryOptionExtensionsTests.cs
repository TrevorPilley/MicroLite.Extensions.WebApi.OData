﻿using System.Net.Http;
using MicroLite.Builder;
using MicroLite.Extensions.WebApi.OData.Binders;
using MicroLite.Extensions.WebApi.Tests.OData.TestEntities;
using Net.Http.WebApi.OData.Model;
using Net.Http.WebApi.OData.Query;
using Xunit;

namespace MicroLite.Extensions.WebApi.Tests.OData.Binders
{
    public class ODataQueryOptionExtensionsTests
    {
        [Fact]
        public void CreateSqlQueryBindsSelectThenAddsFilterAndOrderBy()
        {
            TestHelper.EnsureEDM();

            var option = new ODataQueryOptions(
                new HttpRequestMessage(HttpMethod.Get, "http://services.microlite.org/odata/Customers?$select=Forename,Surname&$filter=Forename eq 'John'&$orderby=Surname"),
                EntityDataModel.Current.EntitySets["Customers"]);

            SqlQuery sqlQuery = option.CreateSqlQuery();

            var expected = SqlBuilder.Select("Forename", "Surname").From(typeof(Customer)).Where("(Forename = ?)", "John").OrderByAscending("Surname").ToSqlQuery();

            Assert.Equal(expected, sqlQuery);
        }

        [Fact]
        public void CreateSqlQueryBindsSelectWildcardThenAddsFilterAndOrderBy()
        {
            TestHelper.EnsureEDM();

            var option = new ODataQueryOptions(
                new HttpRequestMessage(HttpMethod.Get, "http://services.microlite.org/odata/Customers?$filter=Forename eq 'John'&$orderby=Surname"),
                EntityDataModel.Current.EntitySets["Customers"]);

            SqlQuery sqlQuery = option.CreateSqlQuery();

            var expected = SqlBuilder.Select("*").From(typeof(Customer)).Where("(Forename = ?)", "John").OrderByAscending("Surname").ToSqlQuery();

            Assert.Equal(expected, sqlQuery);
        }
    }
}
