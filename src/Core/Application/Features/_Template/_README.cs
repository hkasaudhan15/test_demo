// ════════════════════════════════════════════════════════════
// CQRS FEATURE TEMPLATE — Copy this entire folder structure
// to create a new feature. Replace "_Template" with your
// feature name (e.g., Products, Users, Orders).
// ════════════════════════════════════════════════════════════
//
// Features/
// └── YourFeature/
//     ├── Commands/
//     │   ├── Create/
//     │   │   ├── CreateYourFeatureCommand.cs      ← ICommand<Guid>
//     │   │   ├── CreateYourFeatureCommandHandler.cs ← ICommandHandler
//     │   │   └── CreateYourFeatureCommandValidator.cs ← AbstractValidator
//     │   ├── Update/
//     │   │   ├── UpdateYourFeatureCommand.cs
//     │   │   ├── UpdateYourFeatureCommandHandler.cs
//     │   │   └── UpdateYourFeatureCommandValidator.cs
//     │   └── Delete/
//     │       ├── DeleteYourFeatureCommand.cs
//     │       └── DeleteYourFeatureCommandHandler.cs
//     └── Queries/
//         ├── GetById/
//         │   ├── GetYourFeatureByIdQuery.cs        ← IQuery<YourFeatureResponse>
//         │   └── GetYourFeatureByIdQueryHandler.cs  ← IQueryHandler
//         └── GetPaged/
//             ├── GetYourFeaturePagedQuery.cs
//             └── GetYourFeaturePagedQueryHandler.cs
//
// RULES:
// 1. Each Command/Query is a RECORD (immutable)
// 2. Each Handler returns Result<T> (never throws)
// 3. Each Validator uses FluentValidation
// 4. Handlers depend on IRepository<T> and IUnitOfWork
// 5. One file per class — no multi-class files
