using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Xml;
using System.Xml.Schema;



/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 * **/


namespace ConsoleApp1
{


    public class Program
    {
        // These URLs will be read by the autograder, please keep the variable name un-changed and link to the correct xml/xsd files.
        public static string xmlURL = "https://rukpet.github.io/Assignment-4/ConsoleApp1/Hotels.xml"; //Q1.2
        public static string xmlErrorURL = "https://rukpet.github.io/Assignment-4/ConsoleApp1/HotelsErrors.xml"; //Q1.3
        public static string xsdURL = "https://rukpet.github.io/Assignment-4/ConsoleApp1/Hotels.xsd"; //Q1.1;

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);


            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);


            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            string result = null;
            // Create the XmlSchemaSet class .
            XmlSchemaSet sc = new XmlSchemaSet();
            // Add the schema to the collection before performing validation
            sc.Add(null, xsdUrl);
            // set the validation settings.
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = sc;
            settings.ValidationEventHandler += (sender, e) =>
            {
                result += e.Exception.ToString();
            };
            // Create the XmlReader object.
            XmlReader reader =
            XmlReader.Create(xmlUrl, settings);
            try
            {
                // Parse the file 
                while (reader.Read()) ; // will call event handler if invalid
            }
            catch (Exception) { }

            //return "No Error" if XML is valid. Otherwise, return the desired exception message.
            return result ?? "No Error";
        }

        private const string AttributePrefix = "_";

        public static string Xml2Json(string xmlUrl)
        {
            XmlTextReader xmlReader = null;

            try
            {
                xmlReader = new XmlTextReader(xmlUrl);
                xmlReader.WhitespaceHandling = WhitespaceHandling.None;

                // 'currentToken' always points at the node we're mutating while walking.
                JToken currentToken = new JObject();

                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element)
                    {
                        currentToken = HandleStartElement(xmlReader, currentToken);
                    }
                    else if (xmlReader.NodeType == XmlNodeType.Text)
                    {
                        currentToken = ReplaceWithTextValue(xmlReader, currentToken);
                    }
                    else if (xmlReader.NodeType == XmlNodeType.EndElement)
                    {
                        currentToken = HandleEndElement(xmlReader, currentToken);
                    }
                }

                // emit final JSON
                return currentToken.ToString();
            }
            finally
            {
                xmlReader?.Close();
            }
        }

        private static JToken HandleStartElement(XmlTextReader xmlReader, JToken currentToken)
        {
            if (!(currentToken is JObject))
            {
                return currentToken;
            }

            var currentObject = (JObject)currentToken;

            // look for an existing property with the same element name
            if (currentObject.Property(xmlReader.Name) is JProperty existingProperty)
            {
                if (existingProperty.Value is JArray siblingsArray)
                {
                    // push a new object into the array and set it as current
                    siblingsArray.Add(currentToken = new JObject());
                }
                else
                {
                    // convert to array when have duplicate in first time
                    existingProperty.Value = new JArray(existingProperty.Value, currentToken = new JObject());
                }
            }
            else
            {
                // create property with a new object, and descend into it
                var newElementProperty = new JProperty(xmlReader.Name, new JObject());
                currentObject.Add(newElementProperty);
                currentToken = newElementProperty.Value;
            }

            AppendAttributesToCurrent(xmlReader, currentToken);

            return currentToken;
        }

        private static void AppendAttributesToCurrent(XmlTextReader xmlReader, JToken currentToken)
        {
            if (currentToken is JContainer currentContainer && xmlReader.AttributeCount > 0)
            {
                // iterate attributes and append them directly to the current token
                while (xmlReader.MoveToNextAttribute())
                {
                    currentContainer.Add(new JProperty(AttributePrefix + xmlReader.Name, xmlReader.Value));
                }
            }
        }

        private static JToken ReplaceWithTextValue(XmlTextReader xmlReader, JToken currentToken)
        {
            var previousToken = currentToken;                          // keep reference to the old token
            previousToken.Replace(currentToken = new JValue(xmlReader.Value)); // swap it with a value node
            return currentToken;
        }

        private static JToken HandleEndElement(XmlTextReader xmlReader, JToken currentToken)
        {
            // move attribute properties that appear first to the end (preserve original order logic)
            while (currentToken is JContainer currentContainer
                   && currentToken.First is JProperty leadingAttributeProperty
                   && leadingAttributeProperty.Name.StartsWith(AttributePrefix))
            {
                leadingAttributeProperty.Remove();
                currentContainer.Add(leadingAttributeProperty);
            }

            // if this element ended with an empty object, replace it with a value
            if (currentToken.Type == JTokenType.Object && !currentToken.HasValues)
            {
                var previousToken = currentToken;
                var elementText = xmlReader.Value;
                previousToken.Replace(currentToken = new JValue(elementText));
            }

            // ascend to the closest ancestor JObject
            return currentToken.Ancestors().OfType<JObject>().FirstOrDefault();
        }
    }
}
