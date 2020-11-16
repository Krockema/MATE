using System;
using System.Threading.Tasks;
using Zeiss.PiWeb.Api.Rest.Dtos.Data;
using Zeiss.PiWeb.Api.Rest.HttpClient.Data;

namespace Master40.PiWebApi
{
    public class Configuration
    {
        public static readonly  AttributeDefinitionDto attr = new AttributeDefinitionDto(15001, "Hole", AttributeTypeDto.AlphaNumeric, 255);
        public static readonly AttributeDefinitionDto attr2 = new AttributeDefinitionDto(15002, "Tiefe", AttributeTypeDto.Float, 255);

        public static readonly CatalogDto catalog = new CatalogDto
        {
            Name = "InspectorCat"
            , Uuid = Guid.NewGuid()
            , ValidAttributes = new []{ attr.Key }
            , CatalogEntries = new []
            {
                new CatalogEntryDto
                {
                    Key = 1,
                    Attributes = new []{ new AttributeDto( attr.Key, "Zu gross.") }
                }
            }
        };

        public static readonly CatalogAttributeDefinitionDto catalogeBaseAttr = new CatalogAttributeDefinitionDto
        {
            Key = 15003
            , Description = "Cataloge based Attribute"
            , Catalog = catalog.Uuid
        };

        public static async Task CreateAttribute(DataServiceRestClient client)
        {
            await client.CreateAttributeDefinitions(EntityDto.Characteristic, new[] {attr2});
            await client.CreateAttributeDefinitions(EntityDto.Catalog, new[] {attr});
            await client.CreateCatalogs(new []{ catalog });
            await client.CreateAttributeDefinitions(EntityDto.Part, new[] { catalogeBaseAttr });
        }

        public static async Task ClearData(DataServiceRestClient client)
        {
            await client.DeleteAttributeDefinitions(EntityDto.Part, new[] {catalogeBaseAttr.Key});
            await client.DeleteCatalogs(new[] {catalog.Uuid});
            await client.DeleteAttributeDefinitions(EntityDto.Characteristic, new[] {attr2.Key});
            await client.DeleteAttributeDefinitions(EntityDto.Catalog,new[] {attr.Key});
        }

    }
}
