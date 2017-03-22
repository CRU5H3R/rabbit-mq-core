using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using Newtonsoft.Json;


namespace rabbit_mq_core
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.Write("Name: ");
            string queueName;
            string queueExchange;

            string userName = Console.ReadLine();
            var factory = new ConnectionFactory
                          {
                              HostName = @"vpros-laporta.bellevue.velocitypartners.net",
                              UserName = "asado",
                              Password = "asado",
                              Port = 5672
                          };

            using (var conn = factory.CreateConnection())
            {
                using (var model = conn.CreateModel())
                {
                    queueName = userName + "Queue";
                    queueExchange = userName + "Exchange";
                    model.QueueDeclare(queueName, true, false, false);
                    model.ExchangeDeclare(queueExchange, ExchangeType.Fanout, true);
                    model.QueueBind(queueName, queueExchange, string.Empty);
                }

                string cmd = "";
                bool exit = false;
                while (!exit)
                {
                    Console.Write("> ");
                    cmd = Console.ReadLine();
                    switch (cmd)
                    {
                        case "get":
                            ReadMessage(conn, queueName);
                            break;
                        case "send":
                            WriteMessage(conn, userName);
                            break;
                        case "exit":
                        case "quit":
                            exit = true;
                            break;
                        default:
                            Console.WriteLine("unknown command.");
                            break;
                    }
                }

            }
        }

        private static void WriteMessage(RabbitMQ.Client.IConnection conn, string from)
        {
            Console.Write("To: ");
            var to = Console.ReadLine();
            if (String.IsNullOrEmpty(to)) return;

            Console.Write("Message: ");
            var message = Console.ReadLine();

            try
            {
                using (var model = conn.CreateModel())
                {
                    model.BasicReturn += new EventHandler<RabbitMQ.Client.Events.BasicReturnEventArgs>(blabla);

                    var rabbitMessage = new RabbitMessage() { From = from, Message = message };
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(rabbitMessage));
                    model.BasicPublish(to + "Exchange", string.Empty, new BasicProperties() { DeliveryMode = 2 }, body);
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
        }

        private static void blabla(object sender, RabbitMQ.Client.Events.BasicReturnEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void ReadMessage(IConnection conn, string queueName)
        {
            string queueMessages = string.Empty;

            using (var client = conn.CreateModel())
            {
                BasicGetResult result;

                do
                {
                    if ((result = client.BasicGet(queueName, false)) != null)
                    {
                        var body = result.Body;
                        queueMessages = Encoding.UTF8.GetString(body);
                        var value = (RabbitMessage)JsonConvert.DeserializeObject(queueMessages, typeof(RabbitMessage));
                        Console.WriteLine(value.From + " said " + value.Message);
                        client.BasicAck(result.DeliveryTag, false);
                    }

                } while (result != null);
            }
        }
    }
}
