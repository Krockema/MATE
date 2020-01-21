using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeiss.IMT.PiWeb.Api.DataService.Rest;
using Attribute = Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute;

namespace Mater40.PiWebApi
{
    public class Configuration
    {
        public static readonly  AttributeDefinition attr = new AttributeDefinition(15001, "Hole", AttributeType.AlphaNumeric, 255);
        public static readonly  AttributeDefinition attr2 = new AttributeDefinition(15002, "Tiefe", AttributeType.Float, 255);

        public static readonly Catalog catalog = new Catalog
        {
            Name = "InspectorCat"
            , Uuid = Guid.NewGuid()
            , ValidAttributes = new []{ attr.Key }
            , CatalogEntries = new []
            {
                new CatalogEntry
                {
                    Key = 1,
                    Attributes = new []{ new Attribute( attr.Key, "Zu gross.") }
                }
            }
        };

        public static readonly CatalogAttributeDefinition catalogeBaseAttr = new CatalogAttributeDefinition
        {
            Key = 15003
            , Description = "Cataloge based Attribute"
            , Catalog = catalog.Uuid
        };

        public static async Task CreateAttribute(DataServiceRestClient client)
        {
            await client.CreateAttributeDefinitions(Entity.Characteristic, new[] {attr2});
            await client.CreateAttributeDefinitions(Entity.Catalog, new[] {attr});
            await client.CreateCatalogs(new []{ catalog });
            await client.CreateAttributeDefinitions(Entity.Part, new[] { catalogeBaseAttr });
        }

        public static async Task ClearData(DataServiceRestClient client)
        {
            await client.DeleteAttributeDefinitions(Entity.Part, new[] {catalogeBaseAttr.Key});
            await client.DeleteCatalogs(new[] {catalog.Uuid});
            await client.DeleteAttributeDefinitions(Entity.Characteristic, new[] {attr2.Key});
            await client.DeleteAttributeDefinitions(Entity.Catalog,new[] {attr.Key});
        }

    }
}
