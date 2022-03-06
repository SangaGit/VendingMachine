using System.Resources;
namespace VendingMachine.Factories
{
    public interface IResourceManagerFactory
    {
        public ResourceManager GetResourceManager();
    }
}