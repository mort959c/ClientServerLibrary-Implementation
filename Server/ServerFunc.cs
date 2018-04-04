using EntityFrameworkExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TcpServerCommunication;

namespace Server
{
    class ServerFunc : IHandleRequest
    {
        DBMethods DBMethods = new DBMethods();

       public  ServerFunc()
        {
            List<Item> items = new List<Item>();
            if (DBMethods.getAllItems().Count == 0)
            {
                Item item = new Item()
                {
                    Name = "Magnum classic",
                    Description = "Chokoladeovertrukket vaniljeis",
                    UnitPrice = 12.2m,
                };

                items.Add(item);

                item = new Item()
                {
                    Name = "Magnum Chokolade",
                    Description = "Chokoladeovertrukket chokoladeis",
                    UnitPrice = 14.2m,
                };

                items.Add(item);
                item = new Item()
                {
                    Name = "Magnum Double",
                    Description = "Chokoladeovertrukket doubleis",
                    UnitPrice = 16.2m,
                };

                items.Add(item);
                item = new Item()
                {
                    Name = "Cornetto classic",
                    Description = "Isvaffel",
                    UnitPrice = 18.2m,
                };

                items.Add(item);


                DBMethods.AddItems(items);
            }
        }

        public string HandleRequest(string receivedData)
        {
            XDocument xdoc = XDocument.Parse(receivedData);
            XElement firstElement = xdoc.Root;

            Console.WriteLine("Handeling request: {0}", firstElement.Name.ToString());

            switch (firstElement.Name.ToString())
            {
                case "GetAllItems":
                    return getAllItemsAsXML();

                case "GetAllOrders":
                    return getAllOrdersAsXML();

                case "CreateOrders":
                    return CreateOrdersFromClient(receivedData);
            }
            return "";
        }

        //XML FORMAT
        //<CreateOrders>
        //  <OrderList>
        //    <Order>
        //      <item/>
        //    <Order>
        //    .....
        //    <Order>
        //      <item/>
        //    <Order>
        //  </OrderList>
        //</CreateOrders>
        private string CreateOrdersFromClient(string request)
        {
            List<Order> ordersToAdd = new List<Order>();

            XDocument xOrders = XDocument.Parse(request);

            foreach (XElement orderElement in xOrders.Descendants("Order"))
            {
                // quantity is the first and only attribute of the orderElement
                int quantity = Convert.ToInt32(orderElement.Attributes().First().Value);
                int itemId = Convert.ToInt32(orderElement.Descendants("Item").Attributes().First().Value);
                //string itemName = string.Empty;
                //decimal itemUnitPrice = new decimal();
                //string itemDescription = string.Empty;

                //foreach (var itemAttr in orderElement.Descendants("Item").Attributes())
                //{
                //    switch (itemAttr.Name.ToString().ToLower())
                //    {
                //        case "id":
                //            itemId = Convert.ToInt32(itemAttr.Value);
                //            break;
                //    }
                //}
                Item item = (from i in DBMethods.getAllItems() where i.Id == itemId select i).First();
                //Create order objects, put in list, and add list of orders to db
                ordersToAdd.Add(new Order() {
                    Quantity = quantity,
                    Item = item,
                });
            }

            try
            {
                DBMethods.addOrders(ordersToAdd);
                XDocument xdocResponse = new XDocument();
                XElement responseElement = new XElement("Success");
                xdocResponse.Add(responseElement);

                return xdocResponse.ToString(SaveOptions.DisableFormatting);
            }
            catch (Exception ex)
            {
                //throw new Exception($"Ordrene blev ikke gemt da der opstod en fejl, prøv igen /n fejlbesked: {ex.Message}");
                XDocument xdocResponse = new XDocument();
                XElement responseElement = new XElement("Error: " + ex.Message);
                xdocResponse.Add(responseElement);
                return xdocResponse.ToString(SaveOptions.DisableFormatting);
            }

        }

        // XML Item List Format
        //<ItemList>
        //  <Item>
        //    <OrderList>
        //      <Order/>
        //      ....
        //      <Order/>
        //    </OrderList>
        //  </Item>
        //  ...
        //  <Item>
        //    <OrderList>
        //      <Order/>
        //      ....
        //      <Order/>
        //    </OrderList>
        //  </Item>
        //</ItemList>
        private string getAllItemsAsXML()
        {
            List<Item> items = DBMethods.getAllItems();

            XDocument xdoc = new XDocument();
            XElement itemListElement = new XElement("ItemList");
            xdoc.Add(itemListElement);

            foreach (Item i in items)
            {
                XElement itemElement = new XElement("Item");
                itemListElement.Add(itemElement);

                // ID
                itemElement.SetAttributeValue("Id", i.Id.ToString().Trim());
                // Name
                itemElement.SetAttributeValue("Name", i.Name.Trim());
                // UnitPrice
                itemElement.SetAttributeValue("UnitPrice", i.UnitPrice.ToString().Trim());
                // Description
                itemElement.SetAttributeValue("Description", i.Description.Trim());
                //OrderList element
                XElement orderListElement = new XElement("OrderList");
                itemElement.Add(orderListElement);
                //Orders
                foreach (Order order in i.Order)
                {
                    // Order Element
                    XElement orderElement = new XElement("Order");
                    orderListElement.Add(orderElement);
                    // Order ID
                    orderElement.SetAttributeValue("Id", order.Id.ToString().Trim());
                    // Order Quantity
                    orderElement.SetAttributeValue("Quantity", order.Quantity.ToString().Trim());
                    // Order ItemId
                    //orderElement.SetAttributeValue("ItemId", i.Id.ToString().Trim());
                }

            }

            return xdoc.ToString(SaveOptions.DisableFormatting);
        }


        //XML Order List Format
        //<OrderList>
        //  <Order>
        //      <Item/>
        //  </Order>
        //  <Order>
        //      <Item/>
        //  </Order>
        //</OrderList>
        private string getAllOrdersAsXML()
        {
            List<Order> orders = DBMethods.getAllOrders();

            XDocument xdoc = new XDocument();
            XElement orderListElement = new XElement("OrderList");
            xdoc.Add(orderListElement);

            foreach (Order order in orders)
            {
                XElement orderElement = new XElement("Order");
                orderListElement.Add(orderElement);

                // ID
                orderElement.SetAttributeValue("Id", order.Id.ToString().Trim());
                // Quantity
                orderElement.SetAttributeValue("Quantity", order.Quantity.ToString().Trim());
                // ItemId
                //orderElement.SetAttributeValue("ItemId", order.ItemId.ToString().Trim());
                // Item Element
                XElement ItemElement = new XElement("Item");
                orderElement.Add(ItemElement);
                // Item ID
                ItemElement.SetAttributeValue("Id", order.Item.Id.ToString().Trim());
                // Item Name
                ItemElement.SetAttributeValue("Name", order.Item.Name.Trim());
                // Item UnitPrice
                ItemElement.SetAttributeValue("UnitPrice", order.Item.UnitPrice.ToString().Trim());
                // Item Description
                ItemElement.SetAttributeValue("Description", order.Item.Description.Trim());
            }

            return xdoc.ToString(SaveOptions.DisableFormatting);
        }
    }
}
