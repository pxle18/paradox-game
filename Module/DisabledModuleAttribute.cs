using System;

namespace VMP_CNR.Module
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DisabledModuleAttribute : Attribute
    {
    }
}