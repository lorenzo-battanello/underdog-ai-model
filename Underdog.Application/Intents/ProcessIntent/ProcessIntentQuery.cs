using MediatR;
using Underdog.Application.Models;

namespace Underdog.Application.Intents.ProcessIntent;

public sealed record ProcessIntentQuery(string UserPrompt) : IRequest<IntentResult>;
