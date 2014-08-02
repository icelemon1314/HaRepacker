namespace WzLib
{
    using System;
    using System.IO;

    public class WzSoundProperty : IWzImageProperty, IWzObject, IDisposable
    {
        internal WzImage imgParent;
        internal byte[] mp3bytes;
        internal string name;
        internal IWzObject parent;

        public WzSoundProperty()
        {
        }

        public WzSoundProperty(string name)
        {
            this.name = name;
        }

        public void Dispose()
        {
            this.name = null;
            this.mp3bytes = null;
        }

        internal void ParseSound(BinaryReader wzReader)
        {
            Stream baseStream = wzReader.BaseStream;
            baseStream.Position += 1L;
            int count = WzTools.ReadCompressedInt(wzReader);
            WzTools.ReadCompressedInt(wzReader);
            this.mp3bytes = wzReader.ReadBytes(count);
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
                return WzPropertyType.Sound;
            }
        }

        public byte[] SoundData
        {
            get
            {
                return this.mp3bytes;
            }
            set
            {
                this.mp3bytes = value;
            }
        }
    }
}

