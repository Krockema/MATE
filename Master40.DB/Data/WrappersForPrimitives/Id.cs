namespace Master40.DB.Data.WrappersForPrimitives
{
    public class Id
    {
        private readonly int _id;

        public Id(int id)
        {
            _id = id;
        }

        public int GetValue()
        {
            return _id;
        }

        public override bool Equals(object obj)
        {
            Id that = (Id) obj;
            return _id.Equals(that.GetValue());
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public override string ToString()
        {
            return _id.ToString();
        }
    }
}