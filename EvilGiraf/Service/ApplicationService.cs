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

    public async Task<Application> GetApplication(int applicationId)
    {
        var application = await databaseService.Applications.FindAsync(applicationId);

        if (application == null)
        {
            throw new KeyNotFoundException($"Application with id {applicationId} not found");
        }
        else {
            return application;
        }
    }

    public async Task<Application> DeleteApplication(int applicationId)
    {
        var application = await databaseService.Applications.FindAsync(applicationId);

        if (application == null)
        {
            throw new KeyNotFoundException($"Application with id {applicationId} not found");
        }
        else {
            databaseService.Applications.Remove(application);
            await databaseService.SaveChangesAsync();
            return application;
        }
    }
}