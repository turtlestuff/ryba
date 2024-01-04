using Fluent.Net;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Globalization;

namespace Ryba.Data;

public class FluentLocalizationService
{
    public const string DefaultLanguage = "en";

    readonly ILogger<FluentLocalizationService>? _logger;
    readonly CultureInfo _fallbackLang = new(DefaultLanguage);
    readonly ImmutableDictionary<CultureInfo, MessageContext> _messageContexts;

    public string this[string id, params (string Argument, object Value)[] args]
     => this[CultureInfo.CurrentCulture, id, args];

    public string this[string lang, string id, params (string Argument, object Value)[] args]
     => this[new CultureInfo(lang), id, args];

    public string this[CultureInfo? lang, string id, params (string Argument, object Value)[] args]
    {
        get
        {
            var currentLang = lang ?? _fallbackLang;
            while (true)
            {
                if (_messageContexts.TryGetValue(currentLang, out var mc) && mc.HasMessage(id))
                {
                    var msg = mc.GetMessage(id);
                    Dictionary<string, object>? argsDict = null;
                    if (args.Length != 0)
                        argsDict = args.ToDictionary(x => x.Argument, x => x.Value);

                    return mc.Format(msg, argsDict);
                }

                if (Equals(currentLang.Parent, CultureInfo.InvariantCulture))
                {
                    if (Equals(currentLang, _fallbackLang))
                    {
                        _logger?.Log(LogLevel.Debug, $"Missing key [{id}]!");
                        return $"!{id}!" + string.Join("; ", args.Select(x => $"({x.Argument}: {x.Value})")); //no message found
                    }
                    currentLang = _fallbackLang;
                }
                else
                {
                    currentLang = currentLang.Parent;
                }
            }
        }
    }

    public IEnumerable<CultureInfo> AvailableLanguages => _messageContexts.Keys.OrderBy(c => c.Name);

    public FluentLocalizationService(ILogger<FluentLocalizationService> logger) : this()
    {
        this._logger = logger;
    }

    public FluentLocalizationService()
    {   
        var assembly = GetType().Assembly;
        const string resPrefix = "Ryba.Data.Localizations.";
        var locPaths = assembly.GetManifestResourceNames()
            .Where(x => x.StartsWith(resPrefix) && x.EndsWith(".ftl"));

        var msgCtxs = new Dictionary<CultureInfo, MessageContext>();
        foreach (var path in locPaths)
        {
            var cultureName = path[resPrefix.Length..^4];
            var mc = new MessageContext(cultureName);
            using var stream = assembly.GetManifestResourceStream(path)!;
            using var reader = new StreamReader(stream);
            var errors = mc.AddMessages(reader); // no async 😭😭

            if(errors is not null && errors.Count > 0)
                throw new AggregateException("Parsing of Fluent translation files has failed.", errors);

            msgCtxs.TryAdd(new CultureInfo(cultureName), mc);
        }
        _messageContexts = msgCtxs.ToImmutableDictionary();
    }
}
