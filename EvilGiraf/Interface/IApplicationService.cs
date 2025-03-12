using EvilGiraf.Dto;
using EvilGiraf.Model;

namespace EvilGiraf.Interface;

public interface IApplicationService
{
    public Task<Application> CreateApplication(ApplicationCreateDto applicationDto);

    public Task<Application?> GetApplication(int applicationId);

    public Task<Application?> DeleteApplication(int applicationId);
    
    public Task<Application?> UpdateApplication(int applicationId, ApplicationDto applicationDto);
}