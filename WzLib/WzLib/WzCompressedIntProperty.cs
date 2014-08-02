namespace WzLib
{
    using System;

    public class WzCompressedIntProperty : IWzImageProperty, IWzObject, IDisposable
    {
        internal WzImage imgParent;
        internal string name;
        internal IWzObject parent;
        internal int val;

        public WzCompressedIntProperty()
        {
        }

        public WzCompressedIntProperty(string name)
        {
            this.name = name;
        }

        public WzCompressedIntProperty(string name, int value)
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
                return WzPropertyType.CompressedInt;
            }
        }

        public int Value
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

