using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CSSandbox
{
    public sealed class XMLConfiguration
    {
        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
        public class XmlCommentAttribute : Attribute
        {
            public string Value { get; set; }
        }
        [XmlRoot("XMLConfiguration")]
        public class XmlConfigurationData : IXmlSerializable
        {
            public string StringParameter1 { get; set; }

            [XmlComment(Value = "Description appears as XML comment.")]
            public int IntParameter1 { get; set; }

            public void WriteXml(XmlWriter writer)
            {
                var properties = GetType().GetProperties();
                foreach (var propertyInfo in properties)
                {
                    if (propertyInfo.IsDefined(typeof(XmlCommentAttribute), false))
                    {
                        writer.WriteComment(
                            propertyInfo.GetCustomAttributes(typeof(XmlCommentAttribute), false)
                                .Cast<XmlCommentAttribute>().Single().Value);
                    }
                    writer.WriteElementString(propertyInfo.Name, propertyInfo.GetValue(this, null).ToString());
                }
            }

            public XmlSchema GetSchema()
            {
                throw new NotImplementedException();
            }

            public void ReadXml(XmlReader reader)
            {
                XmlRootAttribute att = Attribute.GetCustomAttribute(GetType(), typeof(XmlRootAttribute)) as XmlRootAttribute;

                if (reader.MoveToContent() == XmlNodeType.Element && reader.LocalName.Equals(att.ElementName))
                {
                    var properties = GetType().GetProperties();
                    while (reader.Read())
                    {
                        //if (reader.MoveToContent() == XmlNodeType.Element && properties.Any(x => x.Name.Equals(reader.LocalName)))
                        if (reader.NodeType == XmlNodeType.Element && properties.Any(x => x.Name.Equals(reader.LocalName)))
                        {
                            string propertyName = reader.LocalName;
                            reader.Read();
                            PropertyInfo pi = properties.First(x => x.Name.Equals(propertyName));
                            pi.SetValue(this, Convert.ChangeType(reader.Value, pi.PropertyType));
                        }
                    }
                }
            }
        }

        // in case the framework will be used multithreaded
        // we create the singleton instance threadsafe
        private static volatile XMLConfiguration instance;
        private static object syncRoot = new object();

        private XmlConfigurationData data;

        public string ConfigFile { get; private set; }
        public string StringParameter1 { get { return data.StringParameter1; } }
        public int IntParameter1 { get { return data.IntParameter1; } }

        private XMLConfiguration() : this(String.Empty) { }
        private XMLConfiguration(string configFile)
        {
            if (configFile == String.Empty)
            {
                data = new XmlConfigurationData();
                data.StringParameter1 = "Default String";
                data.IntParameter1 = 0;
            }
            else
            {
                ConfigFile = configFile;
                using (var stream = System.IO.File.OpenRead(configFile))
                {
                    var serializer = new XmlSerializer(typeof(XmlConfigurationData));
                    data = serializer.Deserialize(stream) as XmlConfigurationData;
                }
            }
        }

        public static XMLConfiguration Instance
        {
            get
            {
                if (instance == null) throw new InvalidOperationException("BaseConfiguration not yet initialized. Call Initialize() first");
                return instance;
            }
        }

        public static void Initialize(string configFile)
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        instance = new XMLConfiguration(configFile);
                    }
                }
            }
        }

        public static void Update()
        {
            if (instance == null) throw new InvalidOperationException("BaseConfiguration not yet initialized. Call Initialize() first");

            string file = instance.ConfigFile;
            instance = null;
            Initialize(file);
        }

        public static void CreateTemplate(string fileName)
        {
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                // creata a BaseConfiguration with default data
                var bc = new XMLConfiguration();
                var serializer = new XmlSerializer(typeof(XmlConfigurationData));
                serializer.Serialize(writer, bc.data);
                writer.Flush();
            }
        }
    }
}
