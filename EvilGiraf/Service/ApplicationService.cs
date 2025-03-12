using EvilGiraf.Dto;
using EvilGiraf.Extensions;
using EvilGiraf.Interface;
using EvilGiraf.Model;

namespace EvilGiraf.Service;

public class ApplicationService(DatabaseService databaseService) : IApplicationService
{
    public async Task<Application> CreateApplication(ApplicationCreateDto applicationDto)
    {
        var result = databaseService.Applications.Add(applicationDto.ToModel());
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
    
    public async Task<Application?> UpdateApplication(int applicationId, ApplicationUpdateDto applicationUpdateDto)
    {
        var application = await GetApplication(applicationId);

        if (application is null)
            return null;
        var updatedApplication = new Application
        {   
            Id = application.Id,
            Name = applicationUpdateDto.Name ?? application.Name,
            Type = applicationUpdateDto.Type ?? application.Type,
            Link = applicationUpdateDto.Link ?? application.Link,
            Version = applicationUpdateDto.Version ?? application.Version
        };
        
        databaseService.Applications.Update(updatedApplication);
        await databaseService.SaveChangesAsync();
        return updatedApplication;
    }
}