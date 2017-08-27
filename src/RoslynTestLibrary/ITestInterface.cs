namespace RoslynTestLibrary
{
    public interface ITestInterface<out T> where T : Data
    {
        T Get();

        T Convert(Data data);

        T Convert<U>(U data) where U : Data;

        T Convert<U, V>(U data, string name, V id) where U : Data;
    }
}