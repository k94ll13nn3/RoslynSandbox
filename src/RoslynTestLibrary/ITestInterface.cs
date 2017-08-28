namespace RoslynTestLibrary
{
    public interface ITestInterface<out T, T2> where T : Data
    {
        T Get();

        T Convert(Data data);

        T Convert<U>(U data, T2 t) where U : Data, IData;

        T Convert<U, V>(U data, string name, V id) where U : Data, new() where V : class;
    }
}