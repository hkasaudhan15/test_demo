using CleanArch.Application.Abstractions.Messaging.Queries;

namespace CleanArch.Application.Features.Settings.Queries.GetByKey;

public sealed record GetSettingByKeyQuery(string Key) : IQuery<SettingResponse>;
