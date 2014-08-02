namespace WzLib
{
    using System;

    public interface IWzObject : IDisposable
    {
        string Name { get; }

        WzObjectType ObjectType { get; }

        IWzObject Parent { get; set; }
    }
}

