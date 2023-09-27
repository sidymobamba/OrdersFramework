using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity; 


namespace OrdersFramework
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool loggedIn = false;
            string currentUser = null;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Menu:");
                Console.WriteLine("1. Login");
                Console.WriteLine("2. Crea un nuovo utente");
                Console.WriteLine("3. Lista degli ordini (id, nome customer, data, totale)");
                Console.WriteLine("4. Dettaglio ordine (dato l'id di un ordine -> prodotto qty prezzo)");
                Console.WriteLine("5. Creazione nuovo ordine");
                Console.WriteLine("6. Esci");
                

                if (loggedIn)
                {
                    Console.WriteLine($"Utente attuale: {currentUser}");
                }

                Console.Write("Seleziona un'opzione: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Nome utente: ");
                        string username = Console.ReadLine();
                        Console.Write("Password: ");
                        string password = Console.ReadLine();

                        if (AuthenticateUser(username, password))
                        {
                            loggedIn = true;
                            currentUser = username;
                            Console.WriteLine("Accesso effettuato con successo.");
                        }
                        else
                        {
                            Console.WriteLine("Autenticazione fallita. Controlla le credenziali.");
                        }
                        break;
                    case "2":
                        Console.Write("Nuovo nome utente: ");
                        string newUsername = Console.ReadLine();
                        Console.Write("Nuova password: ");
                        string newPassword = Console.ReadLine();

                        CreateUser(newUsername, newPassword);
                        break;
                    case "3":
                        ListOrders();
                        Console.ReadKey();
                        break;
                    case "4":
                        Console.Write("Inserisci l'ID dell'ordine: ");
                        int orderId = int.Parse(Console.ReadLine());
                        OrderDetails(orderId);
                        Console.ReadKey();
                        break;
                    case "5":
                        if (loggedIn)
                        {
                            Console.Write("Nome cliente: ");
                            string customerName = Console.ReadLine();
                            Console.Write("ID dell'ordine: ");
                            int newOrderId = int.Parse(Console.ReadLine());

                            CreateNewOrder(customerName, newOrderId);
                        }
                        else
                        {
                            Console.WriteLine("Effettua l'accesso per creare un nuovo ordine.");
                        }
                        break;
                    case "6":
                        Exit();
                        break;
                    default:
                        Console.WriteLine("Scelta non valida. Riprova.");
                        break;
                }
            }
        }



        private static bool AuthenticateUser(string username, string password)
        {
            using (var context = new ordiniEntities())
            {
                var user = context.utentis.FirstOrDefault(u => u.login == username && u.password == password);
                return user != null;
            }
        }
        private static void CreateUser(string username, string password)
        {
            using (var context = new ordiniEntities())
            {
                if (context.utentis.Any(u => u.login == username))
                {
                    Console.WriteLine("Il nome utente esiste già.");
                }
                else
                {
                    var newUser = new utenti { login = username, password = password };
                    context.utentis.Add(newUser);
                    context.SaveChanges();
                    Console.WriteLine("Nuovo utente creato con successo.");
                }
            }
        }
        private static decimal CalculateOrderTotal(order order)
        {
            decimal total = 0;

            foreach (var orderItem in order.orderitems)
            {
                total += orderItem.qty * orderItem.price;
            }

            return total;
        }

        private static void ListOrders()
        {
            using (var context = new ordiniEntities())
            {
                var orders = context.orders.Include(o => o.customer).ToList();

                Console.WriteLine("Lista degli ordini:");
                foreach (var order in orders)
                {
                    var total = CalculateOrderTotal(order);
                    Console.WriteLine($"ID: {order.orderid}, Nome Customer: {order.customer.customer1}, Data: {order.orderdate}, Totale: {total}");
                }
            }
        }
        private static void OrderDetails(int orderId)
        {
            using (var context = new ordiniEntities())
            {
                var order = context.orders.Include(o => o.orderitems).FirstOrDefault(o => o.orderid == orderId);

                if (order != null)
                {
                    Console.WriteLine($"Dettagli dell'ordine (ID: {order.orderid}):");
                    foreach (var item in order.orderitems)
                    {
                        Console.WriteLine($"Prodotto: {item.item.item1}, Quantità: {item.qty}, Prezzo: {item.price}");
                    }
                }
                else
                {
                    Console.WriteLine($"Ordine con ID {orderId} non trovato.");
                }
            }
        }


        private static void CreateNewOrder(string customerName, int orderId)
        {
            using (var context = new ordiniEntities())
            {
                var customer = context.customers.FirstOrDefault(c => c.customer1 == customerName);

                if (customer != null)
                {
                    var order = new order { customerId = customer.customerId, orderdate = DateTime.Now };
                    context.orders.Add(order);

                    while (true)
                    {
                        Console.WriteLine("Inserisci il nome del prodotto (o esci per terminare): ");
                        string itemName = Console.ReadLine();

                        if (itemName.ToLower() == "esci")
                            break;

                        var item = context.items.FirstOrDefault(i => i.item1 == itemName);
                        if (item != null)
                        {
                            Console.Write("Quantità: ");
                            int quantity = int.Parse(Console.ReadLine());

                            Console.Write("Prezzo: ");
                            int price = int.Parse(Console.ReadLine());

                            var orderItem = new orderitem { orderid = order.orderid, itemId = item.itemId, qty = quantity, price = price };
                            context.orderitems.Add(orderItem);
                        }
                        else
                        {
                            Console.WriteLine($"Prodotto '{itemName}' non trovato.");
                        }
                    }

                    context.SaveChanges();
                    Console.WriteLine("Nuovo ordine creato con successo.");
                }
                else
                {
                    Console.WriteLine($"Il cliente '{customerName}' non è stato trovato.");
                }
            }
        }

        private static void Exit()
        {
            Console.WriteLine("Arrivederci!");
            Environment.Exit(0);
        }

    }
}

