This document defines the strict JSON Schemas for the core contracts:
- `WorldState`
- `TurnRequest`
- `IntentResult`
- `AIResponse`

These schemas enable:
- backend validation (DM Gateway)
- client-side validation (Unity)
- contract testing
- safer iteration

> Note: These are MVP-focused. You can extend them later without breaking backward compatibility by using `schemaVersion` and optional fields.

---

## File Layout (Suggested)

schemas/
world_state.schema.json
turn_request.schema.json
intent_result.schema.json
ai_response.schema.json

bash
Copy code

---

## `schemas/world_state.schema.json`

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://example.com/schemas/world_state.schema.json",
  "title": "WorldState",
  "type": "object",
  "additionalProperties": false,
  "required": [
    "schemaVersion",
    "sessionId",
    "player",
    "party",
    "location",
    "campaign",
    "quests",
    "relationships",
    "inventory",
    "flags",
    "storySummary",
    "recentEvents"
  ],
  "properties": {
    "schemaVersion": { "type": "integer", "minimum": 1 },
    "sessionId": { "type": "string", "minLength": 1 },

    "player": { "$ref": "#/$defs/PlayerState" },
    "party": { "$ref": "#/$defs/PartyState" },
    "location": { "$ref": "#/$defs/LocationState" },
    "campaign": { "$ref": "#/$defs/CampaignState" },

    "quests": {
      "type": "array",
      "items": { "$ref": "#/$defs/QuestState" }
    },

    "relationships": { "$ref": "#/$defs/RelationshipState" },
    "inventory": { "$ref": "#/$defs/InventoryState" },

    "flags": {
      "type": "object",
      "additionalProperties": {
        "oneOf": [
          { "type": "boolean" },
          { "type": "integer" },
          { "type": "number" },
          { "type": "string" },
          { "type": "null" }
        ]
      }
    },

    "storySummary": { "type": "string", "minLength": 0, "maxLength": 6000 },

    "recentEvents": {
      "type": "array",
      "maxItems": 20,
      "items": { "type": "string", "minLength": 0, "maxLength": 500 }
    },

    "time": { "$ref": "#/$defs/TimeState" },
    "rngSeed": { "type": "integer" }
  },

  "$defs": {
    "PlayerState": {
      "type": "object",
      "additionalProperties": false,
      "required": ["name", "classTag", "level", "xp", "stats", "skills", "statusEffects"],
      "properties": {
        "name": { "type": "string", "minLength": 1, "maxLength": 50 },
        "classTag": { "type": "string", "minLength": 1, "maxLength": 50 },
        "level": { "type": "integer", "minimum": 1, "maximum": 99 },
        "xp": { "type": "integer", "minimum": 0 },
        "stats": { "$ref": "#/$defs/StatsState" },
        "skills": {
          "type": "array",
          "items": { "$ref": "#/$defs/SkillRef" }
        },
        "statusEffects": {
          "type": "array",
          "items": { "$ref": "#/$defs/StatusEffect" }
        }
      }
    },

    "StatsState": {
      "type": "object",
      "additionalProperties": false,
      "required": ["hp", "attack", "defense", "speed"],
      "properties": {
        "hp": { "$ref": "#/$defs/BoundedInt" },
        "mp": { "$ref": "#/$defs/BoundedInt" },
        "attack": { "type": "integer", "minimum": 0, "maximum": 999 },
        "defense": { "type": "integer", "minimum": 0, "maximum": 999 },
        "speed": { "type": "integer", "minimum": 0, "maximum": 999 }
      }
    },

    "BoundedInt": {
      "type": "object",
      "additionalProperties": false,
      "required": ["current", "max"],
      "properties": {
        "current": { "type": "integer", "minimum": 0, "maximum": 9999 },
        "max": { "type": "integer", "minimum": 1, "maximum": 9999 }
      }
    },

    "SkillRef": {
      "type": "object",
      "additionalProperties": false,
      "required": ["skillId"],
      "properties": {
        "skillId": { "type": "string", "minLength": 1, "maxLength": 80 }
      }
    },

    "StatusEffect": {
      "type": "object",
      "additionalProperties": false,
      "required": ["id", "stacks", "durationTurns"],
      "properties": {
        "id": { "type": "string", "minLength": 1, "maxLength": 80 },
        "stacks": { "type": "integer", "minimum": 1, "maximum": 99 },
        "durationTurns": { "type": "integer", "minimum": 0, "maximum": 999 }
      }
    },

    "PartyState": {
      "type": "object",
      "additionalProperties": false,
      "required": ["members", "activeMemberIds"],
      "properties": {
        "members": {
          "type": "array",
          "minItems": 1,
          "maxItems": 4,
          "items": { "type": "string", "minLength": 1, "maxLength": 80 }
        },
        "activeMemberIds": {
          "type": "array",
          "minItems": 1,
          "maxItems": 4,
          "items": { "type": "string", "minLength": 1, "maxLength": 80 }
        }
      }
    },

    "LocationState": {
      "type": "object",
      "additionalProperties": false,
      "required": ["regionTag", "sceneTag"],
      "properties": {
        "regionTag": { "type": "string", "minLength": 1, "maxLength": 80 },
        "sceneTag": { "type": "string", "minLength": 1, "maxLength": 80 },
        "subLocation": { "type": "string", "maxLength": 80 },
        "knownNpcsHere": {
          "type": "array",
          "maxItems": 50,
          "items": { "type": "string", "minLength": 1, "maxLength": 80 }
        }
      }
    },

    "CampaignState": {
      "type": "object",
      "additionalProperties": false,
      "required": ["chapterId", "beatId", "difficulty", "activeThreads"],
      "properties": {
        "chapterId": { "type": "string", "minLength": 1, "maxLength": 80 },
        "beatId": { "type": "string", "minLength": 1, "maxLength": 120 },
        "difficulty": { "type": "integer", "minimum": 1, "maximum": 10 },
        "toneModifiers": {
          "type": "object",
          "additionalProperties": { "type": ["integer", "number", "string", "boolean", "null"] }
        },
        "activeThreads": {
          "type": "array",
          "items": { "$ref": "#/$defs/CampaignThread" }
        }
      }
    },

    "CampaignThread": {
      "type": "object",
      "additionalProperties": false,
      "required": ["id", "title", "status", "priority", "notes"],
      "properties": {
        "id": { "type": "string", "minLength": 1, "maxLength": 80 },
        "title": { "type": "string", "minLength": 1, "maxLength": 120 },
        "status": { "type": "string", "enum": ["active", "resolved", "dormant"] },
        "priority": { "type": "integer", "minimum": 1, "maximum": 5 },
        "notes": { "type": "string", "maxLength": 500 }
      }
    },

    "QuestState": {
      "type": "object",
      "additionalProperties": false,
      "required": ["questId", "title", "status", "stage", "objectives", "rewards"],
      "properties": {
        "questId": { "type": "string", "minLength": 1, "maxLength": 80 },
        "title": { "type": "string", "minLength": 1, "maxLength": 120 },
        "status": { "type": "string", "enum": ["inactive", "active", "completed", "failed"] },
        "stage": { "type": "integer", "minimum": 0, "maximum": 999 },
        "objectives": {
          "type": "array",
          "items": { "$ref": "#/$defs/ObjectiveState" }
        },
        "rewards": {
          "type": "array",
          "items": { "$ref": "#/$defs/RewardState" }
        },
        "lastUpdated": { "type": "string" }
      }
    },

    "ObjectiveState": {
      "type": "object",
      "additionalProperties": false,
      "required": ["id", "description", "status"],
      "properties": {
        "id": { "type": "string", "minLength": 1, "maxLength": 80 },
        "description": { "type": "string", "minLength": 1, "maxLength": 200 },
        "status": { "type": "string", "enum": ["pending", "complete"] },
        "progress": {
          "type": "object",
          "additionalProperties": false,
          "required": ["current", "target"],
          "properties": {
            "current": { "type": "integer", "minimum": 0, "maximum": 9999 },
            "target": { "type": "integer", "minimum": 1, "maximum": 9999 }
          }
        }
      }
    },

    "RewardState": {
      "type": "object",
      "additionalProperties": false,
      "required": ["type"],
      "properties": {
        "type": { "type": "string", "enum": ["gold", "item", "xp"] },
        "amount": { "type": "integer", "minimum": 0, "maximum": 999999 },
        "itemId": { "type": "string", "minLength": 1, "maxLength": 80 }
      },
      "allOf": [
        {
          "if": { "properties": { "type": { "const": "item" } } },
          "then": { "required": ["itemId", "amount"] }
        },
        {
          "if": { "properties": { "type": { "const": "gold" } } },
          "then": { "required": ["amount"] }
        },
        {
          "if": { "properties": { "type": { "const": "xp" } } },
          "then": { "required": ["amount"] }
        }
      ]
    },

    "RelationshipState": {
      "type": "object",
      "additionalProperties": false,
      "required": ["factions", "npcs"],
      "properties": {
        "factions": {
          "type": "object",
          "additionalProperties": { "type": "integer", "minimum": -100, "maximum": 100 }
        },
        "npcs": {
          "type": "object",
          "additionalProperties": { "type": "integer", "minimum": -100, "maximum": 100 }
        }
      }
    },

    "InventoryState": {
      "type": "object",
      "additionalProperties": false,
      "required": ["gold", "items"],
      "properties": {
        "gold": { "type": "integer", "minimum": 0, "maximum": 999999 },
        "items": {
          "type": "array",
          "items": { "$ref": "#/$defs/ItemStack" }
        }
      }
    },

    "ItemStack": {
      "type": "object",
      "additionalProperties": false,
      "required": ["itemId", "count"],
      "properties": {
        "itemId": { "type": "string", "minLength": 1, "maxLength": 80 },
        "count": { "type": "integer", "minimum": 1, "maximum": 9999 }
      }
    },

    "TimeState": {
      "type": "object",
      "additionalProperties": false,
      "required": ["tick"],
      "properties": {
        "tick": { "type": "integer", "minimum": 0, "maximum": 9999999 }
      }
    }
  }
}
schemas/turn_request.schema.json
json
Copy code
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://example.com/schemas/turn_request.schema.json",
  "title": "TurnRequest",
  "type": "object",
  "additionalProperties": false,
  "required": ["sessionId", "requestId", "playerInput", "worldStateCompact"],
  "properties": {
    "sessionId": { "type": "string", "minLength": 1 },
    "requestId": { "type": "string", "minLength": 1 },
    "clientTimestamp": { "type": "string" },

    "playerInput": { "type": "string", "minLength": 1, "maxLength": 1000 },

    "worldStateCompact": {
      "type": "object",
      "additionalProperties": false,
      "required": ["schemaVersion", "location", "campaign", "flags", "storySummary", "recentEvents", "player"],
      "properties": {
        "schemaVersion": { "type": "integer", "minimum": 1 },

        "location": {
          "type": "object",
          "additionalProperties": false,
          "required": ["sceneTag", "regionTag"],
          "properties": {
            "sceneTag": { "type": "string", "minLength": 1, "maxLength": 80 },
            "regionTag": { "type": "string", "minLength": 1, "maxLength": 80 }
          }
        },

        "campaign": {
          "type": "object",
          "additionalProperties": false,
          "required": ["chapterId", "beatId", "difficulty"],
          "properties": {
            "chapterId": { "type": "string", "minLength": 1, "maxLength": 80 },
            "beatId": { "type": "string", "minLength": 1, "maxLength": 120 },
            "difficulty": { "type": "integer", "minimum": 1, "maximum": 10 }
          }
        },

        "flags": {
          "type": "object",
          "additionalProperties": {
            "oneOf": [
              { "type": "boolean" },
              { "type": "integer" },
              { "type": "number" },
              { "type": "string" },
              { "type": "null" }
            ]
          }
        },

        "storySummary": { "type": "string", "maxLength": 6000 },

        "recentEvents": {
          "type": "array",
          "maxItems": 12,
          "items": { "type": "string", "maxLength": 500 }
        },

        "player": {
          "type": "object",
          "additionalProperties": false,
          "required": ["classTag", "level", "stats"],
          "properties": {
            "classTag": { "type": "string", "minLength": 1, "maxLength": 50 },
            "level": { "type": "integer", "minimum": 1, "maximum": 99 },
            "stats": {
              "type": "object",
              "additionalProperties": false,
              "required": ["hp", "attack", "defense", "speed"],
              "properties": {
                "hp": {
                  "type": "object",
                  "additionalProperties": false,
                  "required": ["current", "max"],
                  "properties": {
                    "current": { "type": "integer", "minimum": 0, "maximum": 9999 },
                    "max": { "type": "integer", "minimum": 1, "maximum": 9999 }
                  }
                },
                "attack": { "type": "integer", "minimum": 0, "maximum": 999 },
                "defense": { "type": "integer", "minimum": 0, "maximum": 999 },
                "speed": { "type": "integer", "minimum": 0, "maximum": 999 }
              }
            }
          }
        }
      }
    },

    "recentMessages": {
      "type": "array",
      "maxItems": 16,
      "items": { "$ref": "#/$defs/ChatMessage" }
    },

    "taxonomyVersion": { "type": "string", "minLength": 1, "maxLength": 80 }
  },

  "$defs": {
    "ChatMessage": {
      "type": "object",
      "additionalProperties": false,
      "required": ["role", "text"],
      "properties": {
        "role": { "type": "string", "enum": ["player", "dm"] },
        "text": { "type": "string", "minLength": 0, "maxLength": 2000 }
      }
    }
  }
}
schemas/intent_result.schema.json
json
Copy code
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://example.com/schemas/intent_result.schema.json",
  "title": "IntentResult",
  "type": "object",
  "additionalProperties": false,
  "required": ["intentType", "confidence", "notes"],
  "properties": {
    "intentType": {
      "type": "string",
      "enum": ["DIALOGUE", "SKILL_CHECK", "COMBAT", "CLARIFY", "INVALID"]
    },
    "confidence": { "type": "number", "minimum": 0, "maximum": 1 },
    "notes": { "type": "string", "maxLength": 500 },
    "proposed": { "$ref": "#/$defs/ProposedPayload" },
    "clarifyQuestion": { "type": ["string", "null"], "maxLength": 300 }
  },
  "$defs": {
    "ProposedPayload": {
      "type": "object",
      "additionalProperties": false,
      "properties": {
        "skillTag": { "type": "string", "maxLength": 50 },
        "difficultyBand": { "type": "string", "enum": ["easy", "medium", "hard", "extreme"] },
        "target": { "type": "string", "maxLength": 80 },
        "approach": { "type": "string", "maxLength": 80 },

        "enemyHintTags": {
          "type": "array",
          "maxItems": 3,
          "items": { "type": "string", "maxLength": 80 }
        },
        "reason": { "type": "string", "maxLength": 80 },
        "surprise": { "type": "boolean" },

        "topic": { "type": "string", "maxLength": 120 },

        "reasonCode": { "type": "string", "enum": ["CHEAT", "IMPOSSIBLE", "OUT_OF_WORLD", "UNSUPPORTED"] },
        "alternatives": {
          "type": "array",
          "maxItems": 4,
          "items": { "type": "string", "maxLength": 120 }
        }
      }
    }
  }
}
schemas/ai_response.schema.json
json
Copy code
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "https://example.com/schemas/ai_response.schema.json",
  "title": "AIResponse",
  "type": "object",
  "additionalProperties": false,
  "required": ["responseId", "narration", "directives", "options"],
  "properties": {
    "responseId": { "type": "string", "minLength": 1, "maxLength": 80 },
    "narration": { "type": "string", "maxLength": 12000 },

    "directives": {
      "type": "array",
      "maxItems": 20,
      "items": { "$ref": "#/$defs/Directive" }
    },

    "options": {
      "type": "array",
      "maxItems": 6,
      "items": { "$ref": "#/$defs/Option" }
    },

    "metadata": {
      "type": "object",
      "additionalProperties": true,
      "properties": {
        "tone": { "type": "string", "maxLength": 50 },
        "intentEcho": {
          "type": "string",
          "enum": ["DIALOGUE", "SKILL_CHECK", "COMBAT", "CLARIFY", "INVALID"]
        },
        "safety": {
          "type": "object",
          "additionalProperties": false,
          "properties": {
            "needsClarification": { "type": "boolean" }
          }
        }
      }
    }
  },

  "$defs": {
    "Option": {
      "type": "object",
      "additionalProperties": false,
      "required": ["id", "label"],
      "properties": {
        "id": { "type": "string", "minLength": 1, "maxLength": 80 },
        "label": { "type": "string", "minLength": 1, "maxLength": 120 },
        "type": { "type": "string", "enum": ["choice", "clarify", "system"] },
        "payload": {
          "type": "object",
          "additionalProperties": true
        }
      }
    },

    "Directive": {
      "type": "object",
      "additionalProperties": false,
      "required": ["type", "data"],
      "properties": {
        "type": {
          "type": "string",
          "enum": [
            "set_background",
            "show_actor",
            "update_actor",
            "hide_actor",
            "play_music",
            "stop_music",
            "play_sfx",
            "play_fx",
            "camera_shake",
            "start_encounter",
            "request_skill_check",
            "ask_clarification"
          ]
        },
        "data": { "type": "object" }
      },
      "allOf": [
        {
          "if": { "properties": { "type": { "const": "set_background" } } },
          "then": {
            "properties": {
              "data": {
                "additionalProperties": false,
                "required": ["tag"],
                "properties": { "tag": { "type": "string", "minLength": 1, "maxLength": 80 } }
              }
            }
          }
        },

        {
          "if": { "properties": { "type": { "const": "show_actor" } } },
          "then": {
            "properties": {
              "data": {
                "additionalProperties": false,
                "required": ["slot", "tag"],
                "properties": {
                  "slot": { "type": "string", "maxLength": 20 },
                  "tag": { "type": "string", "minLength": 1, "maxLength": 80 },
                  "pose": { "type": "string", "maxLength": 40 },
                  "expression": { "type": "string", "maxLength": 40 }
                }
              }
            }
          }
        },

        {
          "if": { "properties": { "type": { "const": "update_actor" } } },
          "then": {
            "properties": {
              "data": {
                "additionalProperties": false,
                "required": ["slot"],
                "properties": {
                  "slot": { "type": "string", "maxLength": 20 },
                  "pose": { "type": "string", "maxLength": 40 },
                  "expression": { "type": "string", "maxLength": 40 }
                }
              }
            }
          }
        },

        {
          "if": { "properties": { "type": { "const": "hide_actor" } } },
          "then": {
            "properties": {
              "data": {
                "additionalProperties": false,
                "required": ["slot"],
                "properties": { "slot": { "type": "string", "maxLength": 20 } }
              }
            }
          }
        },

        {
          "if": { "properties": { "type": { "const": "play_music" } } },
          "then": {
            "properties": {
              "data": {
                "additionalProperties": false,
                "required": ["tag"],
                "properties": { "tag": { "type": "string", "minLength": 1, "maxLength": 80 } }
              }
            }
          }
        },

        {
          "if": { "properties": { "type": { "const": "stop_music" } } },
          "then": {
            "properties": {
              "data": { "type": "object", "additionalProperties": false }
            }
          }
        },

        {
          "if": { "properties": { "type": { "const": "play_sfx" } } },
          "then": {
            "properties": {
              "data": {
                "additionalProperties": false,
                "required": ["tag"],
                "properties": { "tag": { "type": "string", "minLength": 1, "maxLength": 80 } }
              }
            }
          }
        },

        {
          "if": { "properties": { "type": { "const": "play_fx" } } },
          "then": {
            "properties": {
              "data": {
                "additionalProperties": false,
                "required": ["tag"],
                "properties": {
                  "tag": { "type": "string", "minLength": 1, "maxLength": 80 },
                  "targetSlot": { "type": "string", "maxLength": 20 }
                }
              }
            }
          }
        },

        {
          "if": { "properties": { "type": { "const": "camera_shake" } } },
          "then": {
            "properties": {
              "data": {
                "additionalProperties": false,
                "required": ["preset"],
                "properties": { "preset": { "type": "string", "enum": ["tiny", "small", "medium", "big"] } }
              }
            }
          }
        },

        {
          "if": { "properties": { "type": { "const": "start_encounter" } } },
          "then": {
            "properties": {
              "data": {
                "additionalProperties": false,
                "required": ["background", "enemies"],
                "properties": {
                  "background": { "type": "string", "minLength": 1, "maxLength": 80 },
                  "enemies": {
                    "type": "array",
                    "minItems": 1,
                    "maxItems": 3,
                    "items": { "type": "string", "minLength": 1, "maxLength": 80 }
                  },
                  "music": { "type": "string", "maxLength": 80 },
                  "reason": { "type": "string", "maxLength": 80 }
                }
              }
            }
          }
        },

        {
          "if": { "properties": { "type": { "const": "request_skill_check" } } },
          "then": {
            "properties": {
              "data": {
                "additionalProperties": false,
                "required": ["skillTag", "difficultyBand"],
                "properties": {
                  "skillTag": { "type": "string", "minLength": 1, "maxLength": 50 },
                  "difficultyBand": { "type": "string", "enum": ["easy", "medium", "hard", "extreme"] },
                  "target": { "type": "string", "maxLength": 80 }
                }
              }
            }
          }
        },

        {
          "if": { "properties": { "type": { "const": "ask_clarification" } } },
          "then": {
            "properties": {
              "data": {
                "additionalProperties": false,
                "required": ["question"],
                "properties": {
                  "question": { "type": "string", "minLength": 1, "maxLength": 300 },
                  "suggestedOptions": {
                    "type": "array",
                    "maxItems": 3,
                    "items": { "$ref": "#/$defs/Option" }
                  }
                }
              }
            }
          }
        }
      ]
    }
  }
}