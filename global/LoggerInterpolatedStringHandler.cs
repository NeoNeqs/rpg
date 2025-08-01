using System.Runtime.CompilerServices;
using System.Text;

namespace RPG.global;

[InterpolatedStringHandler]
public readonly struct LoggerInterpolatedStringHandler {
    private readonly StringBuilder _builder;

    public LoggerInterpolatedStringHandler(int pLiteralLength, int pFormattedCount, Logger _, out bool pShouldAppend) {
        _builder = new StringBuilder(pLiteralLength);
        pShouldAppend = true;
    }

    public void AppendLiteral(string pString) => _builder.Append(pString);

    // IMPORTANT: DO NOT change the names of the parameters below!
    
    // ReSharper disable InconsistentNaming
    public void AppendFormatted<T>(T value) =>
        _builder.Append(value);

    public void AppendFormatted<T>(T value, string? format) {
        if (value is string str && format == "url") {
            _builder.Append($"[url={str}]{str}[/url]");
        } else {
            _builder.AppendFormat($"{{0:{format}}}", value);
        }
    }
    // ReSharper restore InconsistentNaming

    public override string ToString() => _builder.ToString();
}