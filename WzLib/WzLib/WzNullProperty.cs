namespace WzLib
{
    using System;

    public class WzNullProperty : IWzImageProperty, IWzObject, IDisposable
    {
        internal WzImage imgParent;
        internal string name;
        internal IWzObject parent;

        public WzNullProperty()
        {
        }

        public WzNullProperty(string propName)
        {
            this.name = propName;
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
                return WzPropertyType.Null;
            }
        }
    }
}

