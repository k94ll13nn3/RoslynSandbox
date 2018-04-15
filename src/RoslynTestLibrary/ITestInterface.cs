using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FileType = System.IO.FileInfo;

namespace RoslynTestLibrary
{
    public interface ITestInterface<out T, T2> where T : Data
    {
        void VoidMethod();

        void VoidMethodWithParam(int number);

        T Get();

        T Convert(Data data);

        T Convert<U>(U data, T2 t) where U : Data, IData;

        T Convert<U, V>(U data, string name, V id) where U : Data, new() where V : class;

        IEnumerable<Data> GetData(Data[] datas, int[] numbers);

        IDictionary<int, Tuple<string, double>> GetDictionary();

        (int i, string s) ToTuple(int i, string s, List<DateTime> dates);

        (int, Data s) FromTuple((int, string) param);

        ref int GetRef(int i);

        int FromRef(ref int i);

        int GetOut(string s, out ulong u);

        string WithParams(params string[] s);

        FileType GetFile();

        [Obsolete]
        [My]
        void Test([CallerMemberName]string param = "test");

        void GetDefault(MyEnum myEnum = MyEnum.D);

        int? Nullable();
    }
}