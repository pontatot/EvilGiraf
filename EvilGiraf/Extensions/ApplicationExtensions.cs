using EvilGiraf.Model;
using EvilGiraf.Dto;

namespace EvilGiraf.Extensions;

public static class ApplicationExtensions
{
    public static ApplicationResultDto ToDto(this Application application)
    {
        return new ApplicationResultDto(
            application.Id,
            application.Name,
            application.Type,
            application.Link,
            application.Version,
            application.Port,
            application.DomainName
        );
    }

    public static Application ToModel(this ApplicationCreateDto applicationDto)
    {
        return new Application
        {
            Name = applicationDto.Name,
            Type = applicationDto.Type,
            Link = applicationDto.Link,
            Version = applicationDto.Version,
            Port = applicationDto.Port,
            DomainName = applicationDto.DomainName
        };
    }
}