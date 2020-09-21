using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper.Types;
using Master40.DB.Data.Initializer.Tables;
using Master40.DB.DataModel;

namespace Master40.DataGenerator.SampleData1
{
    class OperationData
    {

        private TEnumerator<M_ResourceCapability> SawTools;
        private TEnumerator<M_ResourceCapability> DrillTools;
        private TEnumerator<M_ResourceCapability> AssemblyTools;

        internal M_Operation BODENPLATTE_BOHREN;
        internal M_Operation BODENPLATTE_ZUSAMMENBAUEN;
        internal M_Operation ABLAGE_ZUSAMMENBAUEN;
        internal M_Operation DECKPLATTE_BOHREN;
        internal M_Operation SEITENWAND_BOHREN;
        internal M_Operation RÜCKWAND_ZUSAMMENBAUEN;
        internal M_Operation SCHRANKTÜR_BOHREN;
        internal M_Operation SCHRANKTÜR_ZUSAMMENBAUEN;

        internal M_Operation RAHMEN_ZUSAMMENBAUEN;
        internal M_Operation RÜCKWAND_MONTIEREN;
        internal M_Operation REGAL_DÜBEL_ANBRINGEN;
        internal M_Operation REGAL_ABLAGEN_EINBAUEN;
        internal M_Operation SCHRANK_KLEIDERSTANGENHALTERUNGEN_ANBRINGEN;
        internal M_Operation SCHRANK_KLEIDERSTANGE_EINBAUEN;
        internal M_Operation SCHRANKTÜR_BEFESTIGEN;

        internal OperationData(ArticleData articles, MasterTableResourceCapability resourceCapability)
        {
            SawTools = new TEnumerator<M_ResourceCapability>(
                obj: resourceCapability.Capabilities.Single(x => x.Name.Equals(resourceCapability.CUTTING.Name))
                    .ChildResourceCapabilities.ToArray());

            DrillTools = new TEnumerator<M_ResourceCapability>(
                obj: resourceCapability.Capabilities.Single(x => x.Name.Equals(resourceCapability.DRILLING.Name))
                    .ChildResourceCapabilities.ToArray());

            AssemblyTools = new TEnumerator<M_ResourceCapability>(
                obj: resourceCapability.Capabilities.Single(x => x.Name.Equals(resourceCapability.ASSEMBLING.Name))
                    .ChildResourceCapabilities.ToArray());

            BODENPLATTE_BOHREN = new M_Operation
            {
                ArticleId = articles.BODENPLATTE.Id,
                Name = "Bodenplatte bohren",
                Duration = 5,
                ResourceCapabilityId = DrillTools.GetNext().Id,
                HierarchyNumber = 10
            };

            BODENPLATTE_ZUSAMMENBAUEN = new M_Operation
            {
                ArticleId = articles.BODENPLATTE.Id,
                Name = "Bodenplatte zusammenbauen",
                Duration = 5,
                ResourceCapabilityId = AssemblyTools.GetNext().Id,
                HierarchyNumber = 20
            };

            ABLAGE_ZUSAMMENBAUEN = new M_Operation
            {
                ArticleId = articles.ABLAGE.Id,
                Name = "Ablage zusammenbauen",
                Duration = 1,
                ResourceCapabilityId = AssemblyTools.GetNext().Id,
                HierarchyNumber = 10
            };

            DECKPLATTE_BOHREN = new M_Operation
            {
                ArticleId = articles.DECKPLATTE.Id,
                Name = "Deckplatte bohren",
                Duration = 5,
                ResourceCapabilityId = DrillTools.GetNext().Id,
                HierarchyNumber = 10
            };

            SEITENWAND_BOHREN = new M_Operation
            {
                ArticleId = articles.SEITENWAND.Id,
                Name = "Seitenwand bohren",
                Duration = 5,
                ResourceCapabilityId = DrillTools.GetNext().Id,
                HierarchyNumber = 10
            };

            RÜCKWAND_ZUSAMMENBAUEN = new M_Operation
            {
                ArticleId = articles.RÜCKWAND.Id,
                Name = "Rückwand zusammenbauen",
                Duration = 1,
                ResourceCapabilityId = AssemblyTools.GetNext().Id,
                HierarchyNumber = 10
            };

            SCHRANKTÜR_BOHREN = new M_Operation
            {
                ArticleId = articles.SCHRANKTÜR.Id,
                Name = "Schranktür bohren",
                Duration = 5,
                ResourceCapabilityId = DrillTools.GetNext().Id,
                HierarchyNumber = 10
            };

            SCHRANKTÜR_ZUSAMMENBAUEN = new M_Operation
            {
                ArticleId = articles.SCHRANKTÜR.Id,
                Name = "Schranktür zusammenbauen",
                Duration = 10,
                ResourceCapabilityId = AssemblyTools.GetNext().Id,
                HierarchyNumber = 20
            };

            RAHMEN_ZUSAMMENBAUEN = new M_Operation
            {
                ArticleId = articles.RAHMEN.Id,
                Name = "Rahmen zusammenbauen",
                Duration = 10,
                ResourceCapabilityId = AssemblyTools.GetNext().Id,
                HierarchyNumber = 10
            };

            RÜCKWAND_MONTIEREN = new M_Operation
            {
                ArticleId = articles.RAHMEN_MIT_RÜCKWAND.Id,
                Name = "Rückenwand montieren",
                Duration = 20,
                ResourceCapabilityId = AssemblyTools.GetNext().Id,
                HierarchyNumber = 10
            };

            REGAL_DÜBEL_ANBRINGEN = new M_Operation
            {
                ArticleId = articles.REGAL.Id,
                Name = "Regal: Dübel anbringen",
                Duration = 5,
                ResourceCapabilityId = AssemblyTools.GetNext().Id,
                HierarchyNumber = 10
            };

            REGAL_ABLAGEN_EINBAUEN = new M_Operation
            {
                ArticleId = articles.REGAL.Id,
                Name = "Regal: Ablagen einbauen",
                Duration = 5,
                ResourceCapabilityId = AssemblyTools.GetNext().Id,
                HierarchyNumber = 20
            };

            SCHRANK_KLEIDERSTANGENHALTERUNGEN_ANBRINGEN = new M_Operation
            {
                ArticleId = articles.SCHRANK.Id,
                Name = "Schrank: Kleiderstangenhalterungen anbringen",
                Duration = 5,
                ResourceCapabilityId = AssemblyTools.GetNext().Id,
                HierarchyNumber = 10
            };

            SCHRANK_KLEIDERSTANGE_EINBAUEN = new M_Operation
            {
                ArticleId = articles.SCHRANK.Id,
                Name = "Schrank: Kleiderstange einbauen",
                Duration = 2,
                ResourceCapabilityId = AssemblyTools.GetNext().Id,
                HierarchyNumber = 20
            };

            SCHRANKTÜR_BEFESTIGEN = new M_Operation
            {
                ArticleId = articles.SCHRANK.Id,
                Name = "Schrank: Tür befestigen",
                Duration = 10,
                ResourceCapabilityId = AssemblyTools.GetNext().Id,
                HierarchyNumber = 30
            };

        }

        internal M_Operation[] Init(MasterDBContext context)
        {
            var operations = new M_Operation[] {
                BODENPLATTE_BOHREN,
                BODENPLATTE_ZUSAMMENBAUEN,
                ABLAGE_ZUSAMMENBAUEN,
                DECKPLATTE_BOHREN,
                SEITENWAND_BOHREN,
                RÜCKWAND_ZUSAMMENBAUEN,
                SCHRANKTÜR_BOHREN,
                SCHRANKTÜR_ZUSAMMENBAUEN,

                RAHMEN_ZUSAMMENBAUEN,
                RÜCKWAND_MONTIEREN,
                REGAL_DÜBEL_ANBRINGEN,
                REGAL_ABLAGEN_EINBAUEN,
                SCHRANK_KLEIDERSTANGENHALTERUNGEN_ANBRINGEN,
                SCHRANK_KLEIDERSTANGE_EINBAUEN,
                SCHRANKTÜR_BEFESTIGEN
            };
            context.Operations.AddRange(entities: operations);
            context.SaveChanges();
            return operations;
        }
    }
}
