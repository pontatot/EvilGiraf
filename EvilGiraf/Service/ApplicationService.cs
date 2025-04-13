using EvilGiraf.Dto;
using EvilGiraf.Extensions;
using EvilGiraf.Interface;
using EvilGiraf.Model;
using Microsoft.EntityFrameworkCore;

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
        if (applicationUpdateDto.Name is not null)
            application.Name = applicationUpdateDto.Name;
        if (applicationUpdateDto.Type is not null)
            application.Type = applicationUpdateDto.Type.Value;
        if (applicationUpdateDto.Link is not null)
            application.Link = applicationUpdateDto.Link;
        if (applicationUpdateDto.Version is not null)
            application.Version = applicationUpdateDto.Version;
        if (applicationUpdateDto.Port is not null)
            application.Port = applicationUpdateDto.Port;

        var updatedApp = databaseService.Applications.Update(application).Entity;
        await databaseService.SaveChangesAsync();
        return updatedApp;
    }

    public async Task<List<Application>> ListApplications()
    {
        return await databaseService.Applications.ToListAsync();
    }
}