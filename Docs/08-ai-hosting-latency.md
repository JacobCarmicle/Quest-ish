# 8️⃣ AI Hosting & Latency Spec

## Purpose
Define how the AI Dungeon Master is **hosted**, how responses are **delivered with minimal latency**, and how we control:
- perceived responsiveness (streaming)
- consistency (session summaries + state)
- cost (token budgets, caching, routing)
- reliability (fallbacks, retries, rate limits)

This doc is the foundation for “it feels instant” on mobile.

---

## Goals & Non-Goals

### Goals
- **Low perceived latency**: player sees output quickly after submitting an action
- **Stable gameplay**: AI can’t corrupt mechanics/state
- **Scalable backend**: can evolve from MVP to production
- **Predictable cost**: heavy users shouldn’t bankrupt the system

### Non-Goals (MVP)
- Offline AI DM mode
- Multi-region global routing
- On-device LLM

---

## Latency Targets (MVP)

### UX-Critical Metrics
- **TTFT (Time to First Token):**
  - Target: **≤ 0.8s**
  - Acceptable: **≤ 1.5s**
- **Text streaming rate (perceived):**
  - Target: **15–40 tokens/sec** equivalent feel
- **Turn completion time (varies by narration length):**
  - Target: **2–6s**

### Backend Metrics to Record
- Gateway ingress latency
- Provider request latency
- TTFT at Unity client
- Tokens out / second
- Total tokens per turn
- Error rates (timeouts, schema failures)

---

## Recommended Hosting Architecture (MVP)

### Summary
**Unity Client → DM Gateway (your service) → LLM Inference (provider or self-host) → stream back to Unity**

The DM Gateway is mandatory for:
- validation (schema + tags)
- prompt compaction (summary strategy)
- streaming protocol consistency
- caching + prefetch
- provider abstraction

---

## Component Responsibilities

### A) Unity Client
- Sends free-form `TurnRequest`
- Displays streamed narration immediately
- Executes validated directives
- Resolves mechanics (skill checks / combat)
- Sends `Resolution` summaries
- Saves `WorldState.json` locally

### B) DM Gateway (Your Backend)
**Key responsibilities**
- Build prompts from compact state
- Maintain session-level rolling summary
- Enforce AI response contract (JSON schema)
- Validate tags via taxonomy versioning
- Stream narration to Unity (SSE/WebSocket)
- Cache & prefetch next likely responses
- Rate limit and protect against abuse

### C) LLM Inference Layer
MVP can use:
- Hosted LLM API (fast to ship)
Later upgrade:
- self-hosted inference (vLLM/TGI) for cost + latency control

---

## Deployment Options

### Option 1 (Recommended MVP): Hosted Model + Single-Region Gateway
- Host DM Gateway in **one region near your primary users**
- Use streaming to Unity
- Keep prompts small

**Pros:** fastest to ship, strong narrative quality  
**Cons:** per-token cost and dependency on provider latency

### Option 2 (Scale): Hybrid Routing (Hosted + Self-Hosted)
- Use self-hosted for most turns
- Use hosted premium model for major “set-piece” scenes

**Pros:** best long-term cost/performance  
**Cons:** more ops complexity

### Option 3 (Later): Multi-Region
- Multiple DM gateways across regions
- Geo-routing for lower RTT
- Shared session store with replication strategy

**Not needed for MVP**

---

## Network Protocol: Streaming

### Why streaming is mandatory
Even if total generation takes a few seconds, streaming makes the game *feel* instant by showing text immediately.

### Recommended: Server-Sent Events (SSE)
**SSE pros**
- simple over HTTP
- works well for token streaming
- easy to implement in gateway

**Alternative:** WebSocket
- better for bidirectional real-time, but more complex

**MVP recommendation:** SSE unless Unity tooling pushes you to WebSockets.

---

## Streaming Message Types (Gateway → Unity)

### `narration_delta`
Incremental narration chunks.

```json
{
  "type": "narration_delta",
  "data": { "text": "The innkeeper squints at you..." }
}
