// unity/Assets/Scripts/AI/Contracts/TurnRequest.cs
using System;
using System.Collections.Generic;

namespace Game.AI.Contracts
{
    [Serializable]
    public class TurnRequest
    {
        public string sessionId;
        public string requestId;
        public string clientTimestamp; // ISO string (optional)

        public string playerInput;

        public WorldStateCompact worldStateCompact;
        public List<ChatMessage> recentMessages; // optional
        public string taxonomyVersion; // optional
    }

    [Serializable]
    public class ChatMessage
    {
        public string role; // "player" | "dm"
        public string text;
    }

    [Serializable]
    public class WorldStateCompact
    {
        public int schemaVersion;
        public LocationCompact location;
        public CampaignCompact campaign;

        // Flags as a raw dictionary; values limited to primitives in schema.
        // JsonUtility won't serialize Dictionary well; use a wrapper list OR switch to Newtonsoft JSON.
        public List<FlagKV> flags;

        public string storySummary;
        public List<string> recentEvents;

        public PlayerCompact player;
    }

    [Serializable]
    public class FlagKV
    {
        public string key;
        public string value; // store as string for JsonUtility; parse as needed
        public string valueType; // "bool"|"int"|"float"|"string"|"null"
    }

    [Serializable]
    public class LocationCompact
    {
        public string sceneTag;
        public string regionTag;
    }

    [Serializable]
    public class CampaignCompact
    {
        public string chapterId;
        public string beatId;
        public int difficulty;
    }

    [Serializable]
    public class PlayerCompact
    {
        public string classTag;
        public int level;
        public StatsCompact stats;
    }

    [Serializable]
    public class StatsCompact
    {
        public BoundedInt hp;
        public int attack;
        public int defense;
        public int speed;
    }

    [Serializable]
    public class BoundedInt
    {
        public int current;
        public int max;
    }
}
csharp
Copy code
// unity/Assets/Scripts/AI/Contracts/IntentResult.cs
using System;
using System.Collections.Generic;

namespace Game.AI.Contracts
{
    public enum IntentType
    {
        DIALOGUE,
        SKILL_CHECK,
        COMBAT,
        CLARIFY,
        INVALID
    }

    [Serializable]
    public class IntentResult
    {
        public IntentType intentType;
        public float confidence; // 0..1
        public string notes;

        public ProposedPayload proposed; // optional
        public string clarifyQuestion;    // used when CLARIFY
    }

    [Serializable]
    public class ProposedPayload
    {
        // Skill check fields
        public string skillTag;
        public string difficultyBand; // easy|medium|hard|extreme
        public string target;
        public string approach;

        // Combat fields
        public List<string> enemyHintTags;
        public string reason;
        public bool surprise;

        // Dialogue fields
        public string topic;

        // Invalid fields
        public string reasonCode; // CHEAT|IMPOSSIBLE|OUT_OF_WORLD|UNSUPPORTED
        public List<string> alternatives;
    }
}
csharp
Copy code
// unity/Assets/Scripts/AI/Contracts/AIResponse.cs
using System;
using System.Collections.Generic;

namespace Game.AI.Contracts
{
    [Serializable]
    public class AIResponse
    {
        public string responseId;
        public string narration;

        public List<Directive> directives;
        public List<Option> options;

        public Metadata metadata; // optional
    }

    [Serializable]
    public class Option
    {
        public string id;
        public string label;
        public string type; // choice|clarify|system (optional)

        // Keep payload minimal for JsonUtility; extend later with a custom serializer
        public string payloadJson;
    }

    [Serializable]
    public class Metadata
    {
        public string tone;
        public IntentType intentEcho;
        public Safety safety;
    }

    [Serializable]
    public class Safety
    {
        public bool needsClarification;
    }
}
csharp
Copy code
// unity/Assets/Scripts/AI/Contracts/Directive.cs
using System;
using System.Collections.Generic;

namespace Game.AI.Contracts
{
    public enum DirectiveType
    {
        set_background,
        show_actor,
        update_actor,
        hide_actor,
        play_music,
        stop_music,
        play_sfx,
        play_fx,
        camera_shake,
        start_encounter,
        request_skill_check,
        ask_clarification
    }

    /// <summary>
    /// MVP-friendly directive model:
    /// - We store 'dataJson' as a string because Unity JsonUtility cannot handle arbitrary objects/dictionaries.
    /// - The gateway validates schemas; Unity parses only what it needs based on directive type.
    ///
    /// If you use Newtonsoft.Json in Unity, you can replace dataJson with a strongly-typed 'object data'.
    /// </summary>
    [Serializable]
    public class Directive
    {
        public DirectiveType type;
        public string dataJson;
    }

    // Optional strongly typed "data" payloads for convenience.
    // You can parse dataJson into these types if using Newtonsoft.Json or a custom parser.

    [Serializable]
    public class SetBackgroundData { public string tag; }

    [Serializable]
    public class ShowActorData
    {
        public string slot;       // player|ally1|npc|enemy1...
        public string tag;        // characterTag
        public string pose;       // idle|talk|attack...
        public string expression; // optional
    }

    [Serializable]
    public class UpdateActorData
    {
        public string slot;
        public string pose;
        public string expression;
    }

    [Serializable]
    public class HideActorData { public string slot; }

    [Serializable]
    public class PlayMusicData { public string tag; }
    [Serializable]
    public class PlaySfxData { public string tag; }

    [Serializable]
    public class PlayFxData
    {
        public string tag;
        public string targetSlot;
    }

    [Serializable]
    public class CameraShakeData { public string preset; } // tiny|small|medium|big

    [Serializable]
    public class StartEncounterData
    {
        public string background;
        public List<string> enemies;
        public string music;
        public string reason;
    }

    [Serializable]
    public class RequestSkillCheckData
    {
        public string skillTag;
        public string difficultyBand;
        public string target;
    }

    [Serializable]
    public class AskClarificationData
    {
        public string question;
        public List<Option> suggestedOptions;
    }
}
One important Unity note (so this doesn’t bite you)
Unity’s built-in JsonUtility does not serialize:

Dictionaries

Polymorphism well

Arbitrary nested object graphs

That’s why I used dataJson / payloadJson and a List<FlagKV> wrapper.

If you’re okay adding a dependency, Newtonsoft.Json for Unity makes this way cleaner (true dictionaries + typed directive payloads). If not, this contract is still workable.