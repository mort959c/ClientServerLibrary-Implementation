
using EntityFrameworkExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TcpClientCommunication;

namespace MyClient
{
    class ClientFunc
    {
        ClientCommunication ClientCom = new ClientCommunication();
        

        public async Task<List<Item>> GetAllItemsFromServer()
        {
            List<Item> items = new List<Item>();
            string request = createXMLRequestAsString("GetAllItems");
            string response = await ClientCom.ConnectToServer(request);

            if (response != null)
            {
                XDocument xitems = XDocument.Parse(response);

                //XML Item List format
                // <Item List>
                //  <Item/>
                //  ...
                //  <Item/>
                // </Item List>
                foreach (XElement element in xitems.Descendants("Item"))
                {
                    int id = new int();
                    string name = string.Empty;
                    decimal unitPrice = new decimal();
                    string description = string.Empty;

                    foreach (var attr in element.Attributes())
                    {
                        switch (attr.Name.ToString().ToLower())
                        {
                            case "id":
                                id = Convert.ToInt32(attr.Value);
                                break;

                            case "name":
                                name = attr.Value;
                                break;

                            case "unitprice":
                                unitPrice = Convert.ToDecimal(attr.Value);
                                break;

                            case "description":
                                description = attr.Value;
                                break;
                        }
                    }
                    items.Add(new Item(id, name, unitPrice, description));
                }
                return items;
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Order>> getAllOrdersFromServer()
        {
            List<Order> orders = new List<Order>();
            string request = createXMLRequestAsString("GetAllOrders");
            string response = await ClientCom.ConnectToServer(request);

            XDocument xOrders = XDocument.Parse(response);
            //XML Order List Format
            //<OrderList>
            //  <Order>
            //      <Item/>
            //  </Order>
            //  <Order>
            //      <Item/>
            //  </Order>
            //</OrderList>
            foreach (XElement element in xOrders.Descendants("Order"))
            {
                int id = new int();
                int quantity = new int();
                //int orderItemId = new int();
                // item
                int itemId = new int();
                string itemName = string.Empty;
                decimal itemUnitPrice = new decimal();
                string itemDescription = string.Empty;

                foreach (var attr in element.Attributes())
                {
                    switch (attr.Name.ToString().ToLower())
                    {
                        case "id":
                            id = Convert.ToInt32(attr.Value);
                            break;

                        case "quantity":
                            quantity = Convert.ToInt32(attr.Value);
                            break;

                        //case "itemid":
                        //    orderItemId = Convert.ToInt32(attr.Value);
                        //    break;
                    }
                }
                foreach (var itemAttr in element.Descendants("Item").Attributes())
                {
                    switch (itemAttr.Name.ToString().ToLower())
                    {
                        case "id":
                            itemId = Convert.ToInt32(itemAttr.Value);
                            break;

                        case "name":
                            itemName = itemAttr.Value;
                            break;

                        case "unitprice":
                            itemUnitPrice = Convert.ToDecimal(itemAttr.Value);
                            break;

                        case "description":
                            itemDescription = itemAttr.Value;
                            break;
                    }
                }
                Item item = new Item()
                {
                    Id = itemId,
                    Name = itemName,
                    UnitPrice = itemUnitPrice,
                    Description = itemDescription,
                };
                Order order = new Order()
                {
                    Id = id,
                    Quantity = quantity,
                    Item = item,
                    ItemId = item.Id,
                };
                orders.Add(order);

            }

            return orders;
        }

        public async Task<List<IOrderOverview>> GetOrdersOverviewFromServer()
        {
            List<Item> items = new List<Item>();

            string request = createXMLRequestAsString("GetAllItems");
            string response = await ClientCom.ConnectToServer(request);

            XDocument xitems = XDocument.Parse(response);

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
            foreach (XElement element in xitems.Descendants("Item"))
            {
                int id = new int();
                string name = string.Empty;
                decimal unitPrice = new decimal();
                string description = string.Empty;

                foreach (var attr in element.Attributes())
                {
                    switch (attr.Name.ToString().ToLower())
                    {
                        case "id":
                            id = Convert.ToInt32(attr.Value);
                            break;

                        case "name":
                            name = attr.Value;
                            break;

                        case "unitprice":
                            unitPrice = Convert.ToDecimal(attr.Value);
                            break;

                        case "description":
                            description = attr.Value;
                            break;
                    }
                }
                Item orderItem = new Item()
                {
                    Id = id,
                    Name = name,
                    UnitPrice = unitPrice,
                    Description = description,
                };

                List<Order> orders = new List<Order>();
                foreach (XElement orderElement in element.Descendants("Order"))
                {
                    int orderId = new int();
                    int orderQuantity = new int();

                    foreach (var attr in orderElement.Attributes())
                    {
                        switch (attr.Name.ToString().ToLower())
                        {
                            case "id":
                                orderId = Convert.ToInt32(attr.Value);
                                break;

                            case "quantity":
                                orderQuantity = Convert.ToInt32(attr.Value);
                                break;
                        }
                    }
                    orders.Add(new Order()
                    {
                        Quantity = orderQuantity,
                        Item = orderItem,
                        Id = orderId,
                    });
                }
                orderItem.Order = orders;
                items.Add(orderItem);
            }

            return CreateOrderOverviewList(items);
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
        public async Task<string> SendNewOrdersToServer(List<Order> orders)
        {
            XDocument xdoc = new XDocument();
            XElement addOrdersElement = new XElement("CreateOrders");
            xdoc.Add(addOrdersElement);

            XElement orderListElement = new XElement("OrderList");
            addOrdersElement.Add(orderListElement);

            foreach (Order order in orders)
            {
                XElement orderElement = new XElement("Order");
                orderListElement.Add(orderElement);

                //Quantity
                orderElement.SetAttributeValue("Quantity", order.Quantity.ToString().Trim());
                //ItemElement
                XElement itemElement = new XElement("Item");
                orderElement.Add(itemElement);
                //ItemId
                itemElement.SetAttributeValue("Id", order.Item.Id.ToString().Trim());
                ////ItemName
                //itemElement.SetAttributeValue("Name", order.Item.Name.Trim());
                ////ItemUnitPrice
                //itemElement.SetAttributeValue("UnitPrice", order.Item.UnitPrice.ToString().Trim());
                ////ItemDescription
                //itemElement.SetAttributeValue("Description", order.Item.Description.Trim());
            }

            return await ClientCom.ConnectToServer(xdoc.ToString(SaveOptions.DisableFormatting));
        }

        private List<IOrderOverview> CreateOrderOverviewList(List<Item> items)
        {
            List<IOrderOverview> orderOverview = new List<IOrderOverview>();

            foreach (Item item in items)
            {
                orderOverview.Add(item);
                foreach (Order order in item.Order)
                {
                    orderOverview.Add(order);
                }
            }

            return orderOverview;
        }

        private string createXMLRequestAsString(string request)
        {
            XDocument xdoc = new XDocument();
            XElement element = new XElement(request);
            xdoc.Add(element);
            return xdoc.ToString(SaveOptions.DisableFormatting);
        }
    }
}
