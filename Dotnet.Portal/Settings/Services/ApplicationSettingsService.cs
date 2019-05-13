using Dotnet.Portal.Settings.Factories;
using Dotnet.Portal.Settings.Models;
using Dotnet.Portal.Settings.Repositories;

namespace Dotnet.Portal.Settings.Services
{
    public class ApplicationSettingsService
    {
        public ApplicationSettings Get()
        {
            var repository = new ApplicationSettingsRepository();
            var dto = repository.Load();
            var factory = new ApplicationSettingsFactory();
            var model = factory.ToModel(dto);
            return model;
        }

        public void Save(ApplicationSettings settings)
        {
            var factory = new ApplicationSettingsFactory();
            var dto = factory.ToDto(settings);
            var repository = new ApplicationSettingsRepository();
            repository.Save(dto);
        }
    }
}