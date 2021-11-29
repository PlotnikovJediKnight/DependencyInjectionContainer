namespace TestDemo.ClassesUnderTest
{
    public interface IService<T> : IBaseService where T: IRepository
    {

    }
}