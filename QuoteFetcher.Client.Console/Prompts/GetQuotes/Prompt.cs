using System;

namespace QuoteFetcher.Prompts.GetQuotes;

internal enum PromptStyle
{
    SingleCharacter,
    MultiCharacter
}

internal interface IPrompt
{
    string Id { get; }
    string Text { get; set; }
    PromptStyle Style { get; set; }
    string Result { get; set; }
    Func<bool> Validate { get; set; }
#nullable enable
    Func<IPrompt?> Next { get; set; }
#nullable disable
    Func<bool> Init { get; set; }
    string GetUserPrompt();
}

internal abstract class Prompt : IPrompt
{
    public string Id { get; protected init; }
    public string Text { get; set; }
    public PromptStyle Style { get; set; } = PromptStyle.SingleCharacter;
    public string Result { get; set; }
    public Func<bool> Validate { get; set; }
#nullable enable
    public Func<IPrompt?> Next { get; set; } = null!;
#nullable disable
    public Func<bool> Init { get; set; } = () => true;

    public string GetUserPrompt()
    {
        Init();
        return Text;
    }
}