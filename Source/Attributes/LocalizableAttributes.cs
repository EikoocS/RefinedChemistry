using System;

namespace RefinedChemistry.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    internal sealed class LocalizableAttributes : Attribute
    {
        public string Category { get; }
        public string Identifier { get; }

        public LocalizableAttributes(string category, string identifier)
        {
            Category = category;
            Identifier = identifier;
        }
    }
}
