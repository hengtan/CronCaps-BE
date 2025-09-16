using AutoMapper;
using CronCaps.Application.DTOs;
using CronCaps.Domain.Entities;

namespace CronCaps.Infrastructure.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Job, JobDto>()
            .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.CreatedBy.GetFullName()))
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
                Domain.ValueObjects.JobConfiguration.Create(
                    Domain.Enums.JobType.Command, // Default type
                    src.Configuration.Environment?.ToDictionary(k => k.Key, v => (object)v.Value),
                    TimeSpan.FromSeconds(src.Configuration.TimeoutSeconds),
                    src.Configuration.MaxRetries,
                    false,
                    src.Configuration.NotificationEmail
                ),
                context.Items["CreatedById"] as Guid? ?? Guid.Empty,
                src.Description,
                src.TeamId,
                src.CategoryId,
                src.Tags
            ));

        CreateMap<Domain.ValueObjects.JobConfiguration, JobConfigurationDto>()
            .ForMember(dest => dest.TimeoutSeconds, opt => opt.MapFrom(src => src.Timeout.HasValue ? (int)src.Timeout.Value.TotalSeconds : 0))
            .ForMember(dest => dest.MaxRetries, opt => opt.MapFrom(src => src.MaxRetries))
            .ForMember(dest => dest.NotifyOnFailure, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.NotificationEmail)))
            .ForMember(dest => dest.NotifyOnSuccess, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.NotificationEmail, opt => opt.MapFrom(src => src.NotificationEmail))
            .ForMember(dest => dest.Environment, opt => opt.MapFrom(src => src.Parameters.ToDictionary(p => p.Key, p => p.Value.ToString())));

        CreateMap<JobExecution, JobExecutionDto>()
            .ForMember(dest => dest.JobName, opt => opt.MapFrom(src => src.Job.Name))
            .ForMember(dest => dest.TriggeredBy, opt => opt.MapFrom(src => "Manual"));

        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.GetFullName()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.TeamName, opt => opt.MapFrom(src => src.Teams.FirstOrDefault() != null ? src.Teams.First().Name : null))
            .ForMember(dest => dest.TeamId, opt => opt.MapFrom(src => src.Teams.FirstOrDefault() != null ? src.Teams.First().Id : (Guid?)null));

        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ConstructUsing((src, context) => User.Create(
                src.Name,
                src.Email,
                src.Password,
                src.Role,
                src.Name.Split(' ').FirstOrDefault(),
                src.Name.Split(' ').Skip(1).FirstOrDefault()
            ));

        CreateMap<Team, TeamDto>()
            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Members.Count))
            .ForMember(dest => dest.JobCount, opt => opt.MapFrom(src => 0)); // Would need to query separately

        CreateMap<CreateTeamDto, Team>()
            .ConstructUsing((src, context) => Team.Create(src.Name, context.Items["CreatedById"] as Guid? ?? Guid.Empty, src.Description));

        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.JobCount, opt => opt.MapFrom(src => src.Jobs.Count));

        CreateMap<CreateCategoryDto, Category>()
            .ConstructUsing((src, context) => Category.Create(src.Name, context.Items["CreatedById"] as Guid? ?? Guid.Empty, src.Description, src.Color));
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