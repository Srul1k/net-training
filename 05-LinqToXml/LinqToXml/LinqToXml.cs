using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace LinqToXml
{
    public static class LinqToXml
    {
        /// <summary>
        /// Creates hierarchical data grouped by category
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation (refer to CreateHierarchySourceFile.xml in Resources)</param>
        /// <returns>Xml representation (refer to CreateHierarchyResultFile.xml in Resources)</returns>
        public static string CreateHierarchy(string xmlRepresentation)
        {
            var groups = XElement
                .Parse(xmlRepresentation)
                .Elements("Data")
                .GroupBy(e => (string)e.Element("Category"));

            return new XElement("Root",
                    groups.Select(group => 
                        new XElement("Group", 
                            new XAttribute("ID", group.Key),
                            group.Select(e =>
                                new XElement("Data",
                                    e.Element("Quantity"),
                                    e.Element("Price"))))))
                .ToString();
        }

        /// <summary>
        /// Get list of orders numbers (where shipping state is NY) from xml representation
        /// </summary>
        /// <param name="xmlRepresentation">Orders xml representation (refer to PurchaseOrdersSourceFile.xml in Resources)</param>
        /// <returns>Concatenated orders numbers</returns>
        /// <example>
        /// 99301,99189,99110
        /// </example>
        public static string GetPurchaseOrders(string xmlRepresentation)
        {
            XNamespace aw = "http://www.adventure-works.com";

            var numbers = XDocument
                .Parse(xmlRepresentation)
                .Descendants(aw + "Address")
                .Where(adress =>
                    adress.Attribute(aw + "Type").Value == "Shipping" &&
                    adress.Element(aw + "State").Value == "NY")
                .Select(adress =>
                    adress.Parent.Attribute(aw + "PurchaseOrderNumber").Value);

            return String.Join(",", numbers);
        }

        /// <summary>
        /// Reads csv representation and creates appropriate xml representation
        /// </summary>
        /// <param name="customers">Csv customers representation (refer to XmlFromCsvSourceFile.csv in Resources)</param>
        /// <returns>Xml customers representation (refer to XmlFromCsvResultFile.xml in Resources)</returns>
        public static string ReadCustomersFromCsv(string customers)
        {
            IEnumerable<string[]> customersAsIEnumerable = customers
                .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(cus => cus
                    .Split(','));

            return new XElement("Root",
                    customersAsIEnumerable.Select(fields =>
                        new XElement("Customer",
                            new XAttribute("CustomerID", fields[0]),
                            new XElement("CompanyName", fields[1]),
                            new XElement("ContactName", fields[2]),
                            new XElement("ContactTitle", fields[3]),
                            new XElement("Phone", fields[4]),
                            new XElement("FullAddress",
                                new XElement("Address", fields[5]),
                                new XElement("City", fields[6]),
                                new XElement("Region", fields[7]),
                                new XElement("PostalCode", fields[8]),
                                new XElement("Country", fields[9])))))
                .ToString();
        }

        /// <summary>
        /// Gets recursive concatenation of elements
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation of document with Sentence, Word and Punctuation elements. (refer to ConcatenationStringSource.xml in Resources)</param>
        /// <returns>Concatenation of all this element values.</returns>
        public static string GetConcatenationString(string xmlRepresentation)
        {
            return XDocument
                .Parse(xmlRepresentation)
                .Elements()
                .Aggregate("", (str, e) => String.Join("", e.Value))
                .ToString();
        }

        /// <summary>
        /// Replaces all "customer" elements with "contact" elements with the same childs
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation with customers (refer to ReplaceCustomersWithContactsSource.xml in Resources)</param>
        /// <returns>Xml representation with contacts (refer to ReplaceCustomersWithContactsResult.xml in Resources)</returns>
        public static string ReplaceAllCustomersWithContacts(string xmlRepresentation)
        {
            XElement root = XElement.Parse(xmlRepresentation);
            
            root.ReplaceAll(root
                    .Elements("customer")
                    .Select(e => new XElement("contact", e.Elements())));

            return root.ToString();
        }

        /// <summary>
        /// Finds all ids for channels with 2 or more subscribers and mark the "DELETE" comment
        /// </summary>
        /// <param name="xmlRepresentation">Xml representation with channels (refer to FindAllChannelsIdsSource.xml in Resources)</param>
        /// <returns>Sequence of channels ids</returns>
        public static IEnumerable<int> FindChannelsIds(string xmlRepresentation)
        {
            return XElement
                .Parse(xmlRepresentation)
                .Elements("channel")
                .Where(channel =>
                    channel.Elements("subscriber").Count() >= 2 &&
                    channel.Nodes().OfType<XComment>().Any(x => x.Value == "DELETE"))
                .Aggregate(new List<int>(), (ids, channel) =>
                {
                    ids.Add(Int32.Parse(channel.Attribute("id").Value));
                    return ids;
                });
        }

        /// <summary>
        /// Sort customers in docement by Country and City
        /// </summary>
        /// <param name="xmlRepresentation">Customers xml representation (refer to GeneralCustomersSourceFile.xml in Resources)</param>
        /// <returns>Sorted customers representation (refer to GeneralCustomersResultFile.xml in Resources)</returns>
        public static string SortCustomers(string xmlRepresentation)
        {
            IEnumerable<XElement> root = XElement
                .Parse(xmlRepresentation)
                .Elements()
                .OrderBy(c => c
                    .Element("FullAddress")
                        .Element("Country").Value)
                .ThenBy(c => c
                    .Element("FullAddress")
                        .Element("City").Value);

            return new XElement("Root", root).ToString();
        }

        /// <summary>
        /// Gets XElement flatten string representation to save memory
        /// </summary>
        /// <param name="xmlRepresentation">XElement object</param>
        /// <returns>Flatten string representation</returns>
        /// <example>
        ///     <root><element>something</element></root>
        /// </example>
        public static string GetFlattenString(XElement xmlRepresentation)
        {
            return xmlRepresentation.ToString();
        }

        /// <summary>
        /// Gets total value of orders by calculating products value
        /// </summary>
        /// <param name="xmlRepresentation">Orders and products xml representation (refer to GeneralOrdersFileSource.xml in Resources)</param>
        /// <returns>Total purchase value</returns>
        public static int GetOrdersValue(string xmlRepresentation)
        {
            var products = XElement
                .Parse(xmlRepresentation)
                .Elements("products")
                .Elements("product")
                .ToDictionary(key => key.Attribute("Id").Value,
                value => Int32.Parse(value.Attribute("Value").Value));

            return XElement.Parse(xmlRepresentation)
                .Elements("Orders")
                .Elements("Order")
                .Elements("product")
                .Sum(e => products[e.Value]);
        }
    }
}
