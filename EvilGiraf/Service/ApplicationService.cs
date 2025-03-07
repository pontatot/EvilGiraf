using EvilGiraf.Dto;
using EvilGiraf.Interface;
using EvilGiraf.Model;

namespace EvilGiraf.Service;

public class ApplicationService(DatabaseService databaseService) : IApplicationService
{
    private readonly DatabaseService _databaseService = databaseService;

    public async Task<Application> CreateApplication(ApplicationDto applicationDto)
    {   
        var application = new Application
        {
            Name = applicationDto.Name,
            Type = applicationDto.Type,
            Link = applicationDto.Link,
            Version = applicationDto.Version
        };
        _databaseService.Applications.Add(application);
        await _databaseService.SaveChangesAsync();
        return application;
    }
}