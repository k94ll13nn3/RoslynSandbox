using System;

namespace RoslynTestLibrary
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class MyAttribute : Attribute
    {
    }
}