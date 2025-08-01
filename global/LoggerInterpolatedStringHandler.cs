using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace RPG.global;

#if TOOLS
[InterpolatedStringHandler]
public readonly struct LoggerInterpolatedStringHandler {
    private readonly StringBuilder _builder;

    public LoggerInterpolatedStringHandler(int pLiteralLength, int pFormattedCount, Logger _, out bool pShouldAppend) {
        _builder = new StringBuilder(pLiteralLength);
        pShouldAppend = true;
    }

    public void AppendLiteral(string pString) {
        _builder.Append(pString);
    }

    // IMPORTANT: DO NOT change the names of the parameters below!

    // ReSharper disable InconsistentNaming
    public void AppendFormatted<T>(T value) {
        switch (value) {
            case Node node: {
                Node? current = node;
                Node? owner = node.Owner;
                string sceneFile = owner?.SceneFilePath ?? string.Empty;
                StringName nodeName = new("<orphan node>");

                while (owner is null && current is not null) {
                    if (string.IsNullOrEmpty(sceneFile) && !string.IsNullOrEmpty(current.SceneFilePath)) {
                        sceneFile = current.SceneFilePath;
                        owner = current;
                        break;
                    }

                    current = current.GetParent();
                    owner = current?.Owner;

                    if (owner != null && !string.IsNullOrEmpty(owner.SceneFilePath)) {
                        sceneFile = owner.SceneFilePath;
                    }
                }

                if (node.IsInsideTree()) {
                    nodeName = node.Name;
                }

                NodePath path = owner?.GetPathTo(node) ?? new NodePath();
                _builder.Append($"[url=^{path:D}:{node.GetInstanceId()}@{sceneFile}]{nodeName:D}[/url]");

                break;
            }
            case Resource resource: {
                _builder.Append($"[url={resource.ResourcePath:D}]{resource.ResourcePath:D}[/url]");
                break;
            }
            default:
                _builder.Append(value);
                break;
        }
    }


    public void AppendFormatted<T>(T value, string? format) {
        switch (value) {
            case string str when format == "url":
                _builder.Append($"[url={str}]{str}[/url]");
                break;

            default:
                _builder.AppendFormat($"{{0:{format}}}", value);
                break;
        }
    }
    // ReSharper restore InconsistentNaming

    public override string ToString() {
        return _builder.ToString();
    }
}
#endif