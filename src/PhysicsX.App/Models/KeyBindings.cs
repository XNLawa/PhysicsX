using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PhysicsX.App.Models;

/// <summary>
/// 快捷键配置
/// </summary>
public class KeyBindings
{
    [JsonPropertyName("playPause")]
    public string PlayPause { get; set; } = "Space";

    [JsonPropertyName("reset")]
    public string Reset { get; set; } = "R";

    [JsonPropertyName("delete")]
    public string Delete { get; set; } = "Delete";

    [JsonPropertyName("save")]
    public string Save { get; set; } = "Ctrl+S";

    [JsonPropertyName("open")]
    public string Open { get; set; } = "Ctrl+O";

    [JsonPropertyName("newProject")]
    public string NewProject { get; set; } = "Ctrl+N";

    [JsonPropertyName("undo")]
    public string Undo { get; set; } = "Ctrl+Z";

    [JsonPropertyName("redo")]
    public string Redo { get; set; } = "Ctrl+Y";

    [JsonPropertyName("duplicate")]
    public string Duplicate { get; set; } = "Ctrl+D";

    [JsonPropertyName("selectAll")]
    public string SelectAll { get; set; } = "Ctrl+A";

    public Dictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>
        {
            ["PlayPause"] = PlayPause,
            ["Reset"] = Reset,
            ["Delete"] = Delete,
            ["Save"] = Save,
            ["Open"] = Open,
            ["NewProject"] = NewProject,
            ["Undo"] = Undo,
            ["Redo"] = Redo,
            ["Duplicate"] = Duplicate,
            ["SelectAll"] = SelectAll
        };
    }
}
