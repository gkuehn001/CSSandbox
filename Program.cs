using System;

namespace CSSandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            //RunTest("XMLConfiguration", TestXMLConfiguration);
            RunTest("XMLSerializableDictionary", TestXMLSerializableDictionary);

            Console.ReadLine();
        }

        static void RunTest(string name, Func<bool> f)
        {
            Console.WriteLine($"{name} Test: {(f() ? "passed" : "failed")}");
        }

        static bool TestXMLConfiguration()
        {
            var xmlConfigFile = @"..\..\example_config.xml";
            XMLConfiguration.CreateTemplate(xmlConfigFile);
            XMLConfiguration.Initialize(xmlConfigFile);
            if (!XMLConfiguration.Instance.StringParameter1.Equals("Default String"))
            {
                return false;
            }
            if (XMLConfiguration.Instance.IntParameter1 != 0)
            {
                return false;
            }
            return true;
        }

        static bool TestXMLSerializableDictionary()
        {

            return true;
        }
    }
}
