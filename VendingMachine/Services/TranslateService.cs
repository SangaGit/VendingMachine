using System.Resources;
using VendingMachine.Factories;

namespace VendingMachine.Services
{
    public class TranslateService : ITranslateService
    {
        private readonly IResourceManagerFactory _resourceManagerFactory;
        private readonly ResourceManager _resourceManager;

        public TranslateService(IResourceManagerFactory resourceManagerFactory)
        {
            _resourceManagerFactory = resourceManagerFactory;
            _resourceManager = _resourceManagerFactory.GetResourceManager();
        }
        public string Translate(string message)
        {
            return _resourceManager.GetString(message) ?? "";
        }
    }
}