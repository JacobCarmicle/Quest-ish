# 6️⃣ AI Response Contract (Schema)

## Purpose
Define the exact **machine-readable response format** produced by the AI DM and delivered to Unity (via the DM Gateway). This contract ensures:
- Unity can safely render narration and UI
- AI cannot invent mechanics or assets
- Directives are validated and predictable
- Responses are streamable for low latency

> The AI Response Contract is the “API boundary” between creativity (AI) and correctness (engine).

---

## High-Level Rules (Non-Negotiable)

1. **Unity is authoritative**
   - AI never decides roll results, damage, inventory/stat changes.

2. **AI may only request changes via directives**
   - Directives must be from an allowlist and must reference known tags.

3. **All directives are validated**
   - DM Gateway validates schema + tags
   - Unity validates feasibility and state transitions
   - Unknown directives are ignored (never crash)

4. **Narration is streamable**
   - Text is emitted incrementally
   - Directives/options should be delivered either:
     - at the end of the stream, or
     - as separate structured packets during streaming

---

## Response Object: `AIResponse`

### Required Top-Level Fields
- `responseId`: string (unique per response; useful for debugging)
- `narration`: string (player-facing; can be streamed)
- `directives`: Directive[] (may be empty)
- `options`: Option[] (may be empty)
- `metadata`: Metadata (optional but recommended)

### AIResponse (JSON)
```json
{
  "responseId": "resp_001",
  "narration": "The innkeeper squints at you like you're a sandwich that owes him money...",
  "directives": [],
  "options": [],
  "metadata": {
    "tone": "goofy",
    "intentEcho": "DIALOGUE",
    "safety": { "needsClarification": false }
  }
}
