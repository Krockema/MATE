namespace Master40.DB.Data.Helper
{
    public class IdGeneratorHolder
    {
        private  static readonly IdGenerator _idGenerator = new IdGenerator();

        public static IdGenerator GetIdGenerator()
        {
            return _idGenerator;
        }
    }
}