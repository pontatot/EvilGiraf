using EvilGiraf.Dto;
using EvilGiraf.Model;

namespace EvilGiraf.Interface;

public interface IApplicationService
{
    public Task<Application> CreateApplication(ApplicationDto applicationDto);

    public Task<Application?> GetApplication(int applicationId);

    public Task<Application?> DeleteApplication(int applicationId);
}