using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    public class MasterTableArticleType
    {
        public M_ArticleType ASSEMBLY;
        public M_ArticleType MATERIAL;
        public M_ArticleType CONSUMABLE;
        public M_ArticleType PRODUCT;

        public MasterTableArticleType()
        {
            ASSEMBLY = new M_ArticleType {Name = "Assembly"};
            MATERIAL = new M_ArticleType {Name = "Material"};
            CONSUMABLE = new M_ArticleType {Name = "Consumable"};
            PRODUCT = new M_ArticleType {Name = "Product"};
        }

        public M_ArticleType[] Init(MasterDBContext context)
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
