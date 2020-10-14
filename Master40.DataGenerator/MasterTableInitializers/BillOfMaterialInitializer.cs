using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.MasterTableInitializers
{
    public class BillOfMaterialInitializer
    {
        public static void Init(List<Dictionary<long, Node>> nodesPerLevel, MasterDBContext context)
        {
            List<List<List<M_ArticleBom>>> boms = new List<List<List<M_ArticleBom>>>();
            var currentList1 = new List<List<M_ArticleBom>>();
            boms.Add(currentList1);
            var currentList2 = new List<M_ArticleBom>();
            currentList1.Add(currentList2);
            var counter1 = 0;
            var counter2 = 0;
            foreach (var article in nodesPerLevel.SelectMany(articleSet => articleSet.Values))
            {
                foreach (var operation in article.Operations)
                {
                    foreach (var bom in operation.Bom)
                    {

                        currentList2.Add(bom);
                        counter2++;
                        if (counter2 == Int32.MaxValue)
                        {
                            currentList2 = new List<M_ArticleBom>();
                            currentList1.Add(currentList2);
                            counter2 = 0;
                            counter1++;
                            if (counter1 == Int32.MaxValue)
                            {
                                currentList1 = new List<List<M_ArticleBom>>();
                                boms.Add(currentList1);
                                counter1 = 0;
                            }
                        }
                    }
                }
            }

            foreach (var bomSet1 in boms)
            {
                foreach (var bomSet2 in bomSet1)
                {
                    context.ArticleBoms.AddRange(bomSet2);
                    context.SaveChanges();
                }
            }
        }
    }
}