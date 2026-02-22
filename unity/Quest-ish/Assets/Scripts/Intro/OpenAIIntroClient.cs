using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OpenAIIntroClient : MonoBehaviour
{
    public bool HasConfiguredApiKey => !string.IsNullOrWhiteSpace(apiKey) || !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
    [Header("OpenAI")]
    public string apiKey;
    public string model = "gpt-4o-mini";
    public string endpoint = "https://api.openai.com/v1/chat/completions";
    public float timeoutSeconds = 20f;
    [Range(0, 3)] public int maxRetries = 1;

    [Serializable]
    private class ChatMessage { public string role; public string content; }
    [Serializable]
    private class ChatCompletionRequest { public string model; public ChatMessage[] messages; public float temperature; }
    [Serializable]
    private class ChoiceMessage { public ChatMessage message; }
    [Serializable]
    private class ChatCompletionResponse { public ChoiceMessage[] choices; }

    public IEnumerator RequestTurn(IntroTurnRequest turn, Action<IntroAIResponse, string> onDone)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            onDone?.Invoke(null, "missing_api_key");
            yield break;
        }

        var systemPrompt =
            "You are a DM guiding a player through a wizard tower tutorial (Everweave-style: state-first, fail-forward, pressure each turn). " +
            "Return ONLY strict JSON with keys: intentType,narration,clarifyQuestion,directives,options. " +
            "intentType must be one of DIALOGUE,SKILL_CHECK,COMBAT,CLARIFY,INVALID. " +
            "Narration must include: (1) what changed this turn, (2) rising pressure or consequence, (3) one guided next objective in prose. " +
            "Do not force menu choices. When a roll is needed, set intentType=SKILL_CHECK and let the engine resolve mechanics.";

        var userPrompt = JsonUtility.ToJson(turn);
        var reqObj = new ChatCompletionRequest
        {
            model = model,
            temperature = 0.5f,
            messages = new[]
            {
                new ChatMessage{ role = "system", content = systemPrompt },
                new ChatMessage{ role = "user", content = userPrompt }
            }
        };

        var payload = JsonUtility.ToJson(reqObj);
        var attempts = Mathf.Max(1, maxRetries + 1);
        string lastError = null;

        for (int attempt = 1; attempt <= attempts; attempt++)
        {
            using var req = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST);
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            req.timeout = Mathf.CeilToInt(timeoutSeconds);

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                lastError = req.error ?? "request_failed";
                if (attempt < attempts)
                {
                    yield return new WaitForSeconds(0.2f * attempt);
                    continue;
                }

                onDone?.Invoke(null, lastError);
                yield break;
            }

            var body = req.downloadHandler.text;
            ChatCompletionResponse completion;
            try
            {
                completion = JsonUtility.FromJson<ChatCompletionResponse>(body);
            }
            catch
            {
                onDone?.Invoke(null, "parse_completion_failed");
                yield break;
            }

            var content = completion?.choices != null && completion.choices.Length > 0
                ? completion.choices[0].message?.content
                : null;

            if (string.IsNullOrWhiteSpace(content))
            {
                onDone?.Invoke(null, "empty_model_response");
                yield break;
            }

            var json = ExtractJsonObject(content);
            IntroAIResponse ai;
            try
            {
                ai = JsonUtility.FromJson<IntroAIResponse>(json);
            }
            catch
            {
                onDone?.Invoke(null, "parse_ai_json_failed");
                yield break;
            }

            onDone?.Invoke(ai, null);
            yield break;
        }

        onDone?.Invoke(null, lastError ?? "request_failed");
    }

    private static string ExtractJsonObject(string text)
    {
        var start = text.IndexOf('{');
        var end = text.LastIndexOf('}');
        if (start >= 0 && end > start)
            return text.Substring(start, end - start + 1);
        return text;
    }
}
