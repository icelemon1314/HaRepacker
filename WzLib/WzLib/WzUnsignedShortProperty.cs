namespace WzLib
{
    using System;

    public class WzUnsignedShortProperty : IWzImageProperty, IWzObject, IDisposable
    {
        internal WzImage imgParent;
        internal string name;
        internal IWzObject parent;
        internal ushort val;

        public WzUnsignedShortProperty()
        {
        }

        public WzUnsignedShortProperty(string name)
        {
            this.name = name;
        }

        public WzUnsignedShortProperty(string name, ushort value)
        {
            this.name = name;
            this.val = value;
        }

        public void Dispose()
        {
            this.name = null;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public WzObjectType ObjectType
        {
            get
            {
                return WzObjectType.Property;
            }
        }

        public IWzObject Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.parent = value;
            }
        }

        public WzImage ParentImage
        {
            get
            {
                return this.imgParent;
            }
            set
            {
                this.imgParent = value;
            }
        }

        public WzPropertyType PropertyType
        {
            get
            {
                return WzPropertyType.UnsignedShort;
            }
        }

        public ushort Value
        {
            get
            {
                return this.val;
            }
            set
            {
                this.val = value;
            }
        }
    }
}

