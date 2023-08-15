using Mate.DataCore.Data.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.CodeDom;
using System.Collections.Generic;

namespace Mate.DataCore.Data.Initializer.StoredProcedures
{
    public static class ArticleStatistics
    {
        public static void CreateProcedures(MateDb ctx)
        {
			List<string> ctes = new List<string>();

			string sqlArticleCTE = string.Format(
			@"CREATE OR ALTER PROCEDURE ArticleCTE
				@ArticleId int
			AS
			BEGIN
				SET NOCOUNT ON;
				DROP TABLE IF EXISTS dbo.#Temp;
				DROP TABLE IF EXISTS dbo.#Union;
				WITH Parts(AssemblyID, ComponentID, PerAssemblyQty, ComponentLevel) AS  
				(  
					SELECT b.ArticleParentId, b.ArticleChildId, CAST(b.Quantity AS decimal),0 AS ComponentLevel  
					FROM dbo.M_ArticleBom  AS b  
					join dbo.M_Article a on a.Id = b.ArticleParentId
					where @ArticleId = a.Id
					UNION ALL  
					SELECT bom.ArticleParentId, bom.ArticleChildId, CAST(PerAssemblyQty * bom.Quantity as DECIMAL), ComponentLevel + 1  
					 FROM dbo.M_ArticleBom  AS bom  
						INNER join dbo.M_Article ac on ac.Id = bom.ArticleParentId
						INNER JOIN Parts AS p ON bom.ArticleParentId = p.ComponentID  
				)
				select * into #Temp 
				from (
					select pr.Id,pr.Name, Sum(p.PerAssemblyQty) as qty, pr.ToBuild as ToBuild
					FROM Parts AS p INNER JOIN M_Article AS pr ON p.ComponentID = pr.Id
					Group By pr.Id, pr.Name, p.ComponentID, pr.ToBuild) as x

				select * into #Union from (
					select Sum(o.Duration * t.qty) as dur, sum(t.qty) as count ,0 as 'Po'
						from dbo.M_Operation o join #Temp t on t.Id = o.ArticleId
						where o.ArticleId in (select t.Id from #Temp t)
				UNION ALL
					SELECT SUM(ot.Duration) as dur, COUNT(*) as count , 0 as 'Po'
						from dbo.M_Operation ot where ot.ArticleId = @ArticleId ) as x
				UNION ALL 
					SELECT 0 as dur, 0 as count, sum(t.qty) + 1 as 'Po'
					from #Temp t where t.ToBuild = 1
				select Sum(u.dur) as SumDuration , sum(u.count) as SumOperations, sum(u.Po)  as ProductionOrders from #Union u
			END");

            ctes.Add(sqlArticleCTE);


            string sqlArticleCapabilityComplexityCTE = string.Format(
            @"CREATE OR ALTER PROCEDURE [dbo].[ArticleCapabilityComplexity]
				@ArticleId int
			AS
			BEGIN
				SET NOCOUNT ON;
				DROP TABLE IF EXISTS dbo.#Temp;
				DROP TABLE IF EXISTS dbo.#Union;
				WITH Parts(AssemblyID, ComponentID, PerAssemblyQty, ComponentLevel, ResourceCapabilityId) AS  
				(  
					SELECT b.ArticleParentId, b.ArticleChildId, CAST(b.Quantity AS decimal),0 AS ComponentLevel, 0 as ResourceCapabilityId
					FROM dbo.M_ArticleBom  AS b  
					join dbo.M_Article a on a.Id = b.ArticleParentId
					where @ArticleId = a.Id
					UNION ALL  
					SELECT bom.ArticleParentId, bom.ArticleChildId, CAST(PerAssemblyQty * bom.Quantity as DECIMAL), ComponentLevel + 1, 0 as ResourceCapabilityId 
					 FROM dbo.M_ArticleBom  AS bom  
						INNER join dbo.M_Article ac on ac.Id = bom.ArticleParentId
						INNER JOIN Parts AS p ON bom.ArticleParentId = p.ComponentID  
				)
				select * into #Temp 
				from (
					select pr.Id,pr.Name, Sum(p.PerAssemblyQty) as qty, pr.ToBuild as ToBuild, p.ResourceCapabilityId as ResourceCapabilityId 
					FROM Parts AS p INNER JOIN M_Article AS pr ON p.ComponentID = pr.Id
					Group By pr.Id, pr.Name, p.ComponentID, pr.ToBuild, p.ResourceCapabilityId) as x

				select * into #Union from (
					select Sum(o.Duration * t.qty) as dur, sum(t.qty) as count ,0 as 'Po', o.ResourceCapabilityId as ResourceCapabilityId
						from dbo.M_Operation o join #Temp t on t.Id = o.ArticleId
						FULL OUTER join M_ResourceCapability allCaps on allCaps.Id = o.ResourceCapabilityId
						where o.ArticleId in (select t.Id from #Temp t)
						group by o.ResourceCapabilityId
				UNION ALL
					SELECT SUM(ot.Duration) as dur, COUNT(*) as count , 0 as 'Po', ot.ResourceCapabilityId as ResourceCapabilityId
						from dbo.M_Operation ot 
						FULL OUTER join M_ResourceCapability allCaps on allCaps.Id = ot.ResourceCapabilityId
						where ot.ArticleId = @ArticleId 
						group by ot.ResourceCapabilityId
						) as x
				UNION ALL 
					SELECT 0 as dur, 0 as count, sum(t.qty) + 1 as 'Po', t.ResourceCapabilityId as ResourceCapabilityId
					from #Temp t 
					FULL OUTER join M_ResourceCapability allCaps on allCaps.Id = t.ResourceCapabilityId
					where t.ToBuild = 1 
					group by t.ResourceCapabilityId
				select Sum(u.dur) as SumDuration , sum(u.count) as SumOperations, sum(u.Po) as ProductionOrders, allCaps.Id as ResourceCapabilityId from #Union u
				FULL OUTER join M_ResourceCapability allCaps on allCaps.Id = u.ResourceCapabilityId
				group by allCaps.Id
			END");

			ctes.Add(sqlArticleCapabilityComplexityCTE);
            
			using (var command = ctx.Database.GetDbConnection().CreateCommand())
			{
				foreach(var cte in ctes)
				{ 
					command.CommandText = cte;
					ctx.Database.OpenConnection();
					command.ExecuteNonQuery();
                }
            }
		}

		public static TimeSpan DeliveryDateEstimator(int articleId, double factor, MateDb dBContext)
        {
			var sql = string.Format("Execute ArticleCTE {0}", articleId);
			var estimatedProductDelivery = TimeSpan.FromMinutes(2880L);
			using (var command = dBContext.Database.GetDbConnection().CreateCommand())
			{
				
				command.CommandText = sql;
				dBContext.Database.OpenConnection();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						System.Diagnostics.Debug.WriteLine(string.Format("Summe der Dauer {0}; Anzahl der Operationen {1}; Summe der Produktionsaufträge {2}", reader[0], reader[1], int.Parse(reader[2].ToString()) + 1));
						// TODO Catch false informations
						estimatedProductDelivery = TimeSpan.FromTicks((long)(Convert.ToInt64(reader[0]) * factor));
						System.Diagnostics.Debug.WriteLine("Estimated Product Delivery {0}", estimatedProductDelivery);
					}

				}
			}
			return estimatedProductDelivery;
		}

    }
}
