using EvilGiraf.Dto;
using EvilGiraf.Interface;
using EvilGiraf.Model;

namespace EvilGiraf.Service;

public class ApplicationService(DatabaseService databaseService) : IApplicationService
{
    public async Task<Application> CreateApplication(ApplicationDto applicationDto)
    {
        var application = new Application
        {
            Name = applicationDto.Name,
            Type = applicationDto.Type,
            Link = applicationDto.Link,
            Version = applicationDto.Version
        };
        var result = databaseService.Applications.Add(application);
        await databaseService.SaveChangesAsync();
        return result.Entity;
    }

    public async Task<Application?> GetApplication(int applicationId)
    {
        return await databaseService.Applications.FindAsync(applicationId);
    }

    public async Task<Application?> DeleteApplication(int applicationId)
    {
        var application = await GetApplication(applicationId);

        if (application is null)
            return null;
        
        databaseService.Applications.Remove(application);
        await databaseService.SaveChangesAsync();
        return application;
    }
}