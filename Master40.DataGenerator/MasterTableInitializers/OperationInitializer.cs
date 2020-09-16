using System;
using System.Collections.Generic;
using System.Linq;
using Master40.DataGenerator.DataModel.ProductStructure;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.MasterTableInitializers
{
    public class OperationInitializer
    {
        public static void init(List<Dictionary<long, Node>> nodesPerLevel, MasterDBContext context)
        {
            List<List<List<M_Operation>>> operations = new List<List<List<M_Operation>>>();
            var currentList1 = new List<List<M_Operation>>();
            operations.Add(currentList1);
            var currentList2 = new List<M_Operation>();
            currentList1.Add(currentList2);
            var counter1 = 0;
            var counter2 = 0;
            foreach (var article in nodesPerLevel.SelectMany(articleSet => articleSet.Values))
            {
                foreach (var operation in article.Operations)
                {

                    currentList2.Add(operation);
                    counter2++;
                    if (counter2 == Int32.MaxValue)
                    {
                        currentList2 = new List<M_Operation>();
                        currentList1.Add(currentList2);
                        counter2 = 0;
                        counter1++;
                        if (counter1 == Int32.MaxValue)
                        {
                            currentList1 = new List<List<M_Operation>>();
                            operations.Add(currentList1);
                            counter1 = 0;
                        }
                    }
                }
            }

            foreach (var operationSet1 in operations)
            {
                foreach (var operationSet2 in operationSet1)
                {
                    context.Operations.AddRange(operationSet2);
                    context.SaveChanges();
                }
            }
        }
    }
}