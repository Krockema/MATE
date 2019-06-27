namespace Master40.DB.Data.WrappersForPrimitives
{
    public class Id
    {
        private int _id;

        public Id(int id)
        {
            _id = id;
        }

        public int GetValue()
        {
            return _id;
        }
    }
}