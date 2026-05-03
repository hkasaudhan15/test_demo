using CleanArch.Application.Abstractions.Messaging.Queries;
using CleanArch.Application.Features.Settings.Queries.GetByKey;

namespace CleanArch.Application.Features.Settings.Queries.GetAll;

public sealed record GetAllSettingsQuery : IQuery<IReadOnlyList<SettingResponse>>;
