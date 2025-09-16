using AutoMapper;
using CronCaps.Application.DTOs;
using CronCaps.Domain.Entities;

namespace CronCaps.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Job, JobDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.Name))
            .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team != null ? src.Team.Name : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.CronExpression, opt => opt.MapFrom(src => src.CronExpression.Value))
            .ForMember(dest => dest.SuccessRate, opt => opt.MapFrom(src => src.GetSuccessRate()))
            .ForMember(dest => dest.FailureRate, opt => opt.MapFrom(src => src.GetFailureRate()))
            .ForMember(dest => dest.IsHealthy, opt => opt.MapFrom(src => src.IsHealthy()))
            .ForMember(dest => dest.EstimatedDuration, opt => opt.MapFrom(src => src.GetEstimatedDuration()));

        CreateMap<CreateJobDto, Job>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ConstructUsing((src, context) => Job.Create(
                src.Name,
                src.CronExpression,
                new Domain.ValueObjects.JobConfiguration(
                    src.Configuration.TimeoutSeconds,
                    src.Configuration.MaxRetries,
                    src.Configuration.NotifyOnFailure,
                    src.Configuration.NotifyOnSuccess,
                    src.Configuration.NotificationEmail,
                    src.Configuration.Environment ?? new Dictionary<string, string>()
                ),
                context.Items["CreatedById"] as Guid? ?? Guid.Empty,
                src.Description,
                src.TeamId,
                src.CategoryId,
                src.Tags
            ));

        CreateMap<Domain.ValueObjects.JobConfiguration, JobConfigurationDto>().ReverseMap();

        CreateMap<JobExecution, JobExecutionDto>()
            .ForMember(dest => dest.JobName, opt => opt.MapFrom(src => src.Job.Name));

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Team != null ? src.Team.Name : null));

        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ConstructUsing((src, context) => User.Create(
                src.Name,
                src.Email,
                src.Password,
                src.Role,
                src.TeamId
            ));

        CreateMap<Team, TeamDto>();
        CreateMap<CreateTeamDto, Team>()
            .ConstructUsing(src => Team.Create(src.Name, src.Description));

        CreateMap<Category, CategoryDto>();
        CreateMap<CreateCategoryDto, Category>()
            .ConstructUsing(src => Category.Create(src.Name, src.Description, src.Color));
    }
}

public record TeamDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    int MemberCount,
    int JobCount
);

public record CreateTeamDto(
    string Name,
    string? Description
);

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    string? Color,
    DateTime CreatedAt,
    int JobCount
);

public record CreateCategoryDto(
    string Name,
    string? Description,
    string? Color
);