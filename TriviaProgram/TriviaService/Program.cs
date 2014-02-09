using System;
using System.ServiceModel;
using TriviaLibrary;

namespace TriviaService
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                // Create a service host object
                ServiceHost serviceHost = new ServiceHost(typeof(Trivia));

                // Start the service
                serviceHost.Open();

                // Keep the server running until <Enter> is pressed
                Console.WriteLine("Trivia Program is activated. Press <Enter> to quit.");
                Console.ReadKey();

                // Shut down the service
                serviceHost.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }
}