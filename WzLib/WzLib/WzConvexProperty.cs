namespace WzLib
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class WzConvexProperty : IWzImageProperty, IWzObject, IDisposable
    {
        internal WzImage imgParent;
        internal string name;
        internal IWzObject parent;
        internal List<WzExtendedProperty> properties;

        public WzConvexProperty()
        {
            this.properties = new List<WzExtendedProperty>();
        }

        public WzConvexProperty(string name)
        {
            this.properties = new List<WzExtendedProperty>();
            this.name = name;
        }

        public void AddProperty(WzExtendedProperty prop)
        {
            prop.extendedProperty.Parent = this;
            prop.extendedProperty.ParentImage = this.ParentImage;
            this.properties.Add(prop);
        }

        public void Dispose()
        {
            this.name = null;
            foreach (WzExtendedProperty property in this.properties)
            {
                property.Dispose();
            }
            this.properties.Clear();
            this.properties = null;
        }

        public void RemoveProperty(string name)
        {
            this.properties.Remove(GetRawProperty(name));
        }

        public WzExtendedProperty GetRawProperty(string name)
        {
            foreach (WzExtendedProperty property in this.properties)
            {
                if (property.Name == name)
                {
                    return property;
                }
            }
            throw new KeyNotFoundException("A wz property with the specified name was not found. (TL; DR: tell this to haha01haha01 and he will fix it)");
        }

        public IWzImageProperty this[string name]
        {
            get
            {
                foreach (WzExtendedProperty property in this.properties)
                {
                    if (property.Name == name)
                    {
                        return property.ExtendedProperty;
                    }
                }
                throw new KeyNotFoundException("A wz property with the specified name was not found");
            }
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
                return WzPropertyType.Convex;
            }
        }

        public WzExtendedProperty[] WzProperties
        {
            get
            {
                return this.properties.ToArray();
            }
        }
    }
}

