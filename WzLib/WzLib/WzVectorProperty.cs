namespace WzLib
{
    using System;

    public class WzVectorProperty : IWzImageProperty, IWzObject, IDisposable
    {
        internal WzImage imgParent;
        internal string name;
        internal IWzObject parent;
        internal WzCompressedIntProperty x;
        internal WzCompressedIntProperty y;

        public WzVectorProperty()
        {
        }

        public WzVectorProperty(string name)
        {
            this.name = name;
        }

        public WzVectorProperty(string name, WzCompressedIntProperty x, WzCompressedIntProperty y)
        {
            this.name = name;
            this.x = x;
            this.y = y;
        }

        public void Dispose()
        {
            this.name = null;
            this.x.Dispose();
            this.x = null;
            this.y.Dispose();
            this.y = null;
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
                return WzPropertyType.Vector;
            }
        }

        public WzCompressedIntProperty X
        {
            get
            {
                return this.x;
            }
            set
            {
                this.x = value;
            }
        }

        public WzCompressedIntProperty Y
        {
            get
            {
                return this.y;
            }
            set
            {
                this.y = value;
            }
        }
    }
}

