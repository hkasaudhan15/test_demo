using CleanArch.Application.Abstractions.Messaging.Queries;
using CleanArch.Application.Features.Settings.Queries.GetByKey;

namespace CleanArch.Application.Features.Settings.Queries.GetByGroup;

public sealed record GetSettingsByGroupQuery(string Group) : IQuery<IReadOnlyList<SettingResponse>>;
