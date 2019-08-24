using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQ_DeadLetter_App.Provider
{
    public static class RabbitMqServiceProvider
    {
        public static IConnection GetConnection()
        {

            ConnectionFactory connectionFactory = new ConnectionFactory()
            {
                HostName = "Your existing rabbitmq URL",//127.0.0.1
                UserName = "Your existing rabbitmq username",
                Password = "Your existing rabbitmq password",
                VirtualHost = "If exists, host info",
            };

            return connectionFactory.CreateConnection();
        }
    }
}
