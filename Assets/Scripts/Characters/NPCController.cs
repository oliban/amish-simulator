using UnityEngine;
using System;
using System.Collections.Generic;

namespace AmishSimulator
{
    public enum NPCType { Bishop, Neighbor, Spouse, Elder, Child }
    public enum DialogueContext { Greeting, Quest, Disapproval, Approval, Shunning }

    public class NPCController : MonoBehaviour
    {
        [SerializeField] private string npcId;
        [SerializeField] private string npcName;
        [SerializeField] private NPCType npcType;

        private int _affinityScore = 50;

        public string NpcId => npcId;
        public string NpcName => npcName;
        public NPCType NpcType => npcType;

        private static readonly Dictionary<NPCType, Dictionary<DialogueContext, string[]>> DialogueLines = new()
        {
            {
                NPCType.Bishop, new()
                {
                    { DialogueContext.Greeting,    new[] { "The Lord bless your labors today.", "A fine morning for honest work, ja?" } },
                    { DialogueContext.Quest,        new[] { "The community needs your help with the barn.", "Will you lead the butter churning this week?" } },
                    { DialogueContext.Disapproval,  new[] { "I have noticed some... irregularities in your conduct.", "The Ordnung is not a suggestion, friend." } },
                    { DialogueContext.Approval,     new[] { "Your fields are well-plowed. God smiles on this farmstead.", "The butter is well-churned. Excellent work." } },
                    { DialogueContext.Shunning,     new[] { "The community has spoken. You must reflect on your ways.", "Until you repent, we cannot share your table." } },
                }
            },
            {
                NPCType.Neighbor, new()
                {
                    { DialogueContext.Greeting,    new[] { "Gut morye! Fine weather for the fields.", "Good morning, neighbor!" } },
                    { DialogueContext.Quest,        new[] { "Could you spare some butter? We are low this week.", "The quilting bee is tomorrow. Will you attend?" } },
                    { DialogueContext.Disapproval,  new[] { "I heard about the... incident. Concerning.", "We expected better from you." } },
                    { DialogueContext.Approval,     new[] { "Thank you for your help with the harvest!", "Your family is a blessing to this community." } },
                    { DialogueContext.Shunning,     new[] { "I... cannot speak with you now. I am sorry.", "Please speak with the Bishop." } },
                }
            },
            {
                NPCType.Spouse, new()
                {
                    { DialogueContext.Greeting,    new[] { "Good morning, my love.", "Did you sleep well?" } },
                    { DialogueContext.Approval,     new[] { "The children are fed and happy. A good day.", "Your beard is coming in nicely." } },
                    { DialogueContext.Disapproval,  new[] { "You have been neglecting the family.", "The Bishop spoke to me about your conduct." } },
                    { DialogueContext.Quest,        new[] { "We need more food for the winter. Can you harvest today?", "The children need new clothes. Can we trade?" } },
                    { DialogueContext.Shunning,     new[] { "I stand by you. We will get through this together.", "The community will forgive in time." } },
                }
            },
            {
                NPCType.Elder, new()
                {
                    { DialogueContext.Greeting,    new[] { "Ah, the young ones. Come, sit.", "I remember when this land was first cleared..." } },
                    { DialogueContext.Quest,        new[] { "In my day, we plowed twice the fields you youngsters manage.", "Let me tell you about the great winter of '74..." } },
                    { DialogueContext.Approval,     new[] { "You remind me of my late husband. Hardworking. Faithful.", "God has blessed this farmstead through your labors." } },
                    { DialogueContext.Disapproval,  new[] { "In my day, we did not behave so.", "You would do well to heed the Bishop." } },
                    { DialogueContext.Shunning,     new[] { "I have seen this before. It passes. Be humble.", "The community's love is strong. But so is its memory." } },
                }
            },
        };

        public int GetAffinity() => _affinityScore;

        public void ModifyAffinity(int delta)
        {
            int old = _affinityScore;
            _affinityScore = Mathf.Clamp(_affinityScore + delta, 0, 100);
            RelationshipSystem.Instance?.NotifyAffinityChanged(npcId, old, _affinityScore);
        }

        public string GetDialogue(DialogueContext context)
        {
            if (!DialogueLines.TryGetValue(npcType, out var contextMap)) return "...";
            if (!contextMap.TryGetValue(context, out var lines) || lines.Length == 0) return "...";
            return lines[UnityEngine.Random.Range(0, lines.Length)];
        }
    }
}
