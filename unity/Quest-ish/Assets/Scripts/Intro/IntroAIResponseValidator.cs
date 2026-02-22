using System;

public static class IntroAIResponseValidator
{
    public static bool TryNormalize(IntroAIResponse response, out IntroIntentType intent)
    {
        intent = IntroIntentType.DIALOGUE;
        if (response == null) return false;

        if (!Enum.TryParse(response.intentType ?? string.Empty, true, out intent))
            return false;

        response.intentType = intent.ToString();
        response.narration ??= string.Empty;
        response.clarifyQuestion ??= string.Empty;
        response.directives ??= Array.Empty<IntroDirective>();
        response.options ??= Array.Empty<IntroOption>();
        return true;
    }
}
