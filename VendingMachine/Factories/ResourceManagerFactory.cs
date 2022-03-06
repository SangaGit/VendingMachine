using System.Reflection;
using System.Resources;

namespace VendingMachine.Factories
{
    public class ResourceManagerFactory : IResourceManagerFactory
    {
        private readonly ResourceManager _resourceManager;
        public ResourceManagerFactory()
        {
            _resourceManager = new ResourceManager("VendingMachine.Resources.Translate", Assembly.GetExecutingAssembly());
        }

        public ResourceManager GetResourceManager()
        {
            return _resourceManager;
        }
    }
}