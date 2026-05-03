using AutoMapper;

namespace CleanArch.Application.Common.Mappings;

/// <summary>
/// Convention-based AutoMapper mapping interface.
/// DTOs implement IMapFrom<Entity> for auto-registration.
/// </summary>
public interface IMapFrom<T>
{
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
}
