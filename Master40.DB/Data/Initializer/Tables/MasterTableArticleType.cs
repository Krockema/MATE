using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    public static class MasterTableArticleType
    {
        public static M_ArticleType ASSEMBLY = new M_ArticleType { Name = "Assembly"};
        public static M_ArticleType MATERIAL = new M_ArticleType { Name = "Material"};
        public static M_ArticleType CONSUMABLE = new M_ArticleType { Name = "Consumable" };
        public static M_ArticleType PRODUCT = new M_ArticleType { Name = "Product" };

        public static M_ArticleType[] Init(MasterDBContext context)
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
