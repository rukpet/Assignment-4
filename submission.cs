using System;
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
                result = "No Error";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            //return "No Error" if XML is valid. Otherwise, return the desired exception message.
            return result;
        }

        public static string Xml2Json(string xmlUrl)
        {


            // The returned jsonText needs to be de-serializable by Newtonsoft.Json package. (JsonConvert.DeserializeXmlNode(jsonText))
            return "{}";

        }
    }

}
