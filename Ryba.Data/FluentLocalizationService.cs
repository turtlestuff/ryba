using Fluent.Net;
using System.Collections.Immutable;
using System.Globalization;

namespace Ryba.Data;

public class FluentLocalizationService
{
    public static readonly string DefaultLanguage = "en";

    readonly CultureInfo fallbackLang = new(DefaultLanguage);
    readonly ImmutableDictionary<CultureInfo, MessageContext> messageContexts;

    public string this[string id, params (string Argument, object Value)[] args]
     => this[CultureInfo.CurrentCulture, id, args];

    public string this[string lang, string id, params (string Argument, object Value)[] args]
     => this[new CultureInfo(lang), id, args];

    public string this[CultureInfo? lang, string id, params (string Argument, object Value)[] args]
    {
        get
        {
            var currentLang = lang ?? fallbackLang;
            while (true)
            {
                if (messageContexts.TryGetValue(currentLang, out var mc) && mc.HasMessage(id))
                {
                    var msg = mc.GetMessage(id);
                    Dictionary<string, object>? argsDict = null;
                    if (args.Length != 0)
                        argsDict = args.ToDictionary(x => x.Argument, x => x.Value);

                    return mc.Format(msg, argsDict);
                }
                else
                {
                    if (currentLang.Parent == CultureInfo.InvariantCulture)
                    {
                        if (currentLang == fallbackLang)
                            return $"!{id}!" + string.Join("; ", args.Select(x => $"({x.Argument}: {x.Value})")); //no message found

                        currentLang = fallbackLang;
                    }
                    else
                    {
                        currentLang = currentLang.Parent;
                    }
                    continue;
                }
            }
        }
    }

    public IEnumerable<CultureInfo> AvailableLanguages => messageContexts.Keys.OrderBy(c => c.Name);

    public FluentLocalizationService()
    {   
        var assembly = GetType().Assembly;
        var resPrefix = "Ryba.Data.Localizations.";
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

            msgCtxs.TryAdd(new(cultureName), mc);
        }
        messageContexts = msgCtxs.ToImmutableDictionary();
    }
}
