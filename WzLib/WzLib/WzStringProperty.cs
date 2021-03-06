﻿namespace WzLib
{
    using System;

    public class WzStringProperty : IWzImageProperty, IWzObject, IDisposable
    {
        internal WzImage imgParent;
        internal string name;
        internal IWzObject parent;
        internal string val;

        public WzStringProperty()
        {
        }

        public WzStringProperty(string name)
        {
            this.name = name;
        }

        public WzStringProperty(string name, string value)
        {
            this.name = name;
            this.val = value;
        }

        public void Dispose()
        {
            this.name = null;
            this.val = null;
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
                return WzPropertyType.String;
            }
        }

        public string Value
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

