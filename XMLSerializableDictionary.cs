using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CSSandbox
{
    [Serializable]
    public class XMLSerializableDictionary<K, V> : IXmlSerializable
    {
        // Using dictionary serialization according to Microsoft: https://blogs.msdn.microsoft.com/adam/2010/09/10/how-to-serialize-a-dictionary-or-hashtable-in-c/

        [Serializable]
        [XmlRoot("element")]
        public class DataItem<K, V> : IXmlSerializable
        {
            public K key;
            public V value;
            public DataItem() { }
            public DataItem(K k, V v)
            {
                key = k;
                value = v;
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("key");
                var keySerializer = new XmlSerializer(typeof(K));
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement("value");
                var valueSerializer = new XmlSerializer(typeof(V));
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

            }

            public void ReadXml(XmlReader reader)
            {

            }

            public XmlSchema GetSchema()
            {
                return null;
            }
        }

        private Dictionary<K, V> contentDict = new Dictionary<K, V>();

        public XMLSerializableDictionary() { }

        public void WriteXml(XmlWriter writer)
        {
            foreach (K key in contentDict.Keys)
            {
                var serializer = new XmlSerializer(typeof(DataItem<K, V>));
                serializer.Serialize(writer, new DataItem<K, V>(key, contentDict[key]));
            }
        }

        public void ReadXml(XmlReader reader)
        {
            contentDict = new Dictionary<K, V>();
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(DataItem<K, V>));

            while (reader.Read())
            {
                var element = serializer.Deserialize(reader) as DataItem<K, V>;
                contentDict.Add(element.key, element.value);
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void Add(K key, V value)
        {
            contentDict.Add(key, value);
        }

    }
}

