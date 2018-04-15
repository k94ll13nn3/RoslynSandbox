using System;

namespace RoslynTestLibrary
{
    [Serializable]
    public class Data
    {
        [Obsolete]
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public int Id { get; set; }
    }
}