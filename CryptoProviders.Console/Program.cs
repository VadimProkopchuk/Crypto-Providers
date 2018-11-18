using System.Text;
using CryptoProviders.Core.Contracts;
using CryptoProviders.Des;
using CryptoProviders.Des.Contracts;
using Autofac;

namespace CryptoProviders.Console
{
    class Program
    {
        private static IContainer _container;

        static void Main(string[] args)
        {
            ConfigureDependencies();

            using (var scope = _container.BeginLifetimeScope())
            {
                var cryptoProvider = scope.Resolve<ICryptoProvider>();

                var text = Encoding.ASCII.GetBytes("It is a secured message.");
                var key = Encoding.ASCII.GetBytes("87654321");
                var encrypted = cryptoProvider.Encrypt(text, key);
                var decrypted = cryptoProvider.Decrypt(encrypted, key);

                System.Console.WriteLine(Encoding.ASCII.GetString(encrypted));
                System.Console.WriteLine(Encoding.ASCII.GetString(decrypted));
                System.Console.ReadLine();
            }
        }

        private static void ConfigureDependencies()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DesCryptoProvider>().As<ICryptoProvider>();
            builder.RegisterType<DesCryptoProvider>().As<IDesCryptoProvider>();
            builder.RegisterType<DesCryptoSettings>().As<IDesCryptoSettings>();
            builder.RegisterType<DesCryptoTransform>().As<IDesCryptoTransform>();

            _container = builder.Build();
        }
    }
}