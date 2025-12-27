using Kuddle.Serialization;

namespace Kuddle.Tests.Serialization.Models;

public class AppSettings
{
    [KdlNode("themes")]
    public Dictionary<string, Theme> Themes { get; set; } = new();

    [KdlNode("layouts")]
    public Dictionary<string, LayoutDefinition> Layouts { get; set; } = new();
}

public class Theme : Dictionary<string, ElementStyle> { }

public class LayoutDefinition
{
    [KdlProperty("section")]
    public string Section { get; set; } = default!;

    [KdlProperty("size")]
    public int Ratio { get; set; } = 1;

    [KdlProperty("split")]
    public string SplitDirection { get; set; } = "columns";

    [KdlNode("child")]
    public List<LayoutDefinition> Children { get; set; } = [];
}

public class ElementStyle
{
    [KdlNode("border")]
    public BorderStyleSettings? BorderStyle { get; set; }

    [KdlNode("header")]
    public PanelHeaderSettings? PanelHeader { get; set; }

    [KdlNode("align")]
    public AlignmentSettings? Alignment { get; set; }

    [KdlIgnore]
    public bool WrapInPanel { get; internal set; } = true;
}

public class BorderStyleSettings
{
    [KdlProperty("color")]
    public string? ForegroundColor { get; set; }

    [KdlProperty("style")]
    public string? Decoration { get; set; }
}

public class PanelHeaderSettings
{
    [KdlProperty("text")]
    public string? Text { get; set; }
}

public class AlignmentSettings
{
    [KdlProperty("v")]
    public VerticalAlignment Vertical { get; set; }

    [KdlProperty("h")]
    public HorizontalAlignment Horizontal { get; set; }
}

public enum VerticalAlignment
{
    Top,
    Middle,
    Bottom,
}

public enum HorizontalAlignment
{
    Left,
    Center,
    Right,
}
