using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableArticleType
    {
        internal M_ArticleType ASSEMBLY;
        internal M_ArticleType MATERIAL;
        internal M_ArticleType CONSUMABLE;
        internal M_ArticleType PRODUCT;

        internal MasterTableArticleType()
        {
            ASSEMBLY = new M_ArticleType {Name = "Assembly"};
            MATERIAL = new M_ArticleType {Name = "Material"};
            CONSUMABLE = new M_ArticleType {Name = "Consumable"};
            PRODUCT = new M_ArticleType {Name = "Product"};
        }

        internal M_ArticleType[] Init(MasterDBContext context)
        {
            var articleTypes = new M_ArticleType[]
            {
                ASSEMBLY,
                MATERIAL,
                CONSUMABLE,
                PRODUCT
            };
            context.ArticleTypes.AddRange(entities: articleTypes);
            context.SaveChanges();
            return articleTypes;
        }
    }
}
