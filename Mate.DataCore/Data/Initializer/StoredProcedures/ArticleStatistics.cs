using Mate.DataCore.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Mate.DataCore.Data.Initializer.StoredProcedures
{
    public static class ArticleStatistics
    {
        public static void CreateProcedures(MateDb ctx)
        {

			string sql = string.Format(
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


			using (var command = ctx.Database.GetDbConnection().CreateCommand())
			{
				command.CommandText = sql;
				ctx.Database.OpenConnection();
				command.ExecuteNonQuery();
			}
		}

		public static long DeliveryDateEstimator(int articleId, double factor, MateDb dBContext)
        {
			var sql = string.Format("Execute ArticleCTE {0}", articleId);
			var estimatedProductDelivery = 2880L;
			using (var command = dBContext.Database.GetDbConnection().CreateCommand())
			{
				
				command.CommandText = sql;
				dBContext.Database.OpenConnection();
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						System.Diagnostics.Debug.WriteLine(string.Format("Summe der Dauer {0}; Summe der Operationen {1}; Summe der Prodktionsaufträge {2}", reader[0], reader[1], reader[2]));
						// TODO Catch false informations
						estimatedProductDelivery = (long)(System.Convert.ToInt64(reader[0]) * factor);
						System.Diagnostics.Debug.WriteLine("Estimated Product Delivery{0}", estimatedProductDelivery);
					}

				}
			}
			return estimatedProductDelivery;
		}

    }
}
