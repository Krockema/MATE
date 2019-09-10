using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Data.Context
{
    public class DBSqlRaw : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sqlscript = @"WITH DirectReports(Id, ArticleName, ParentId, BomLevel) AS(
                                SELECT art.Id, art.""Name"", bom.ArticleParentId, 0 AS BomLevel
                                FROM Articles as art

                                JOIN ArticleBoms as bom on bom.ArticleChildId = art.Id
                                WHERE bom.""Name"" = 'Dump - Truck'
                                UNION ALL
                                SELECT e.Id, e.""Name"", bc.ArticleParentId, BomLevel + 1
                                FROM Articles AS e
                                    JOIN ArticleBoms as bc on bc.ArticleChildId = e.Id
                                    INNER JOIN DirectReports AS d ON bc.ArticleParentId = d.Id
                                )
                                SELECT*
                                FROM DirectReports";

            migrationBuilder.Sql(sql: sqlscript);
        }
    }
}
