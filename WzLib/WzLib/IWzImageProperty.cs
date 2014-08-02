namespace WzLib
{
    using System;

    public interface IWzImageProperty : IWzObject, IDisposable
    {
        string Name { get; set; }

        WzImage ParentImage { get; set; }

        WzPropertyType PropertyType { get; }
    }
}

