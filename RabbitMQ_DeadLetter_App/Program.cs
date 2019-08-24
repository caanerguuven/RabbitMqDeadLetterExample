using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ_DeadLetter_App.Provider;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ_DeadLetter_App
{
    class Program
    {
        private static IConnection connection;
        private static IModel channel;
        
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome To My Little Dead Letter Project.");

            var connection = RabbitMqServiceProvider.GetConnection(); //creating rabbitmq connection
            var channel = connection.CreateModel(); //creating model from connection

            var deadLetterArguments = new Dictionary<string, object> {
                {"x-dead-letter-exchange","exc_ErrorOccured" },
                {"x-dead-letter-routing-key","rk_ErrorOccuredOnProcessing" }
            }; // defining dead letter exchange and routing key whom listens errors of my application. below, I will set it as deadLetterArgument for que_ProcessListener 

            channel.ExchangeDeclare(exchange: "exc_process", type: ExchangeType.Topic);//declaring exchange whom listens main process
            channel.QueueDeclare(queue: "que_processListener", durable: true, exclusive: false, autoDelete: false, arguments: deadLetterArguments); //declaring queue whom is looking at exc_process
            channel.QueueBind(queue: "que_processListener", exchange: "exc_process", routingKey: "rk_process");//then binding main queue and exchange which I declared before, with routing key 

            var sourceLetterArguments = new Dictionary<string, object> {
                { "x-dead-letter-exchange", "exc_process" },
                { "x-dead-letter-routing-key","rk_process" },
                { "x-message-ttl", 20000 }
            };// adding dead letter whom belongs main exchange and routing key. below, I will set it as deadLetterArgument for que_errorOccured 
            //When error occured, it will add queue to exc_process exchange after 20 seconds or we can say that it will wait for 20 seconds to add

            channel.ExchangeDeclare(exchange: "exc_ErrorOccured", type: ExchangeType.Topic);//declaring exchange whom listens for all errors.
            channel.QueueDeclare(queue: "que_errorOccured", durable: true, exclusive: false, autoDelete: false, arguments: sourceLetterArguments);//declaring queue whom is looking at exc_process
            channel.QueueBind(queue: "que_errorOccured", exchange: "exc_ErrorOccured", routingKey: "rk_ErrorOccuredOnProcessing");//then binding error queue and exchange which I declared before, with routing key 

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            channel.BasicConsume("que_processListener", false, consumer);// consuming queues who drops in que_processListener
            Console.ReadLine(); // it provides application to stand up
        }

        private static void Consumer_Received(object sender, BasicDeliverEventArgs ea)
        {
            try
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine(message);

               /*
                * Process Steps...
                    Add Whatever you want
               */
                channel.BasicAck(ea.DeliveryTag, false); //if succeed, you can remove queue by giving ack. 
            }
            catch (Exception ex)
            {
                channel.BasicNack(ea.DeliveryTag, false, false); //if failed, you can keep it by giving nack. 
                //After this step, error Dead Letter will take initiative, after 20 seconds, Consumer_Received will run again.
            }
        }
    }
}
