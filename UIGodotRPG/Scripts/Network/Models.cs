using System;
using System.Collections.Generic;

namespace FrontBRRPG.Network
{
    /// <summary>
    /// Configuration d'un personnage pour le combat
    /// </summary>
    public class CharacterConfig
    {
        public string Type { get; set; } = "guerrier"; // Type du personnage (guerrier, berserker, etc.)
        public string Name { get; set; } = ""; // Nom du personnage
    }

    /// <summary>
    /// Configuration pour démarrer une bataille
    /// Compatible avec le format attendu par RPG-Arena Backend
    /// </summary>
    public class BattleStartRequest
    {
        public List<CharacterConfig> Characters { get; set; } = new List<CharacterConfig>();
    }

    /// <summary>
    /// Message de log reçu du serveur
    /// Format: type de log suivi du message
    /// </summary>
    public class LogMessage
    {
        public string Type { get; set; } = "info"; // info, warning, error, combat
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// État de la connexion WebSocket
    /// </summary>
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Error
    }

    /// <summary>
    /// Types de personnages disponibles sur le serveur
    /// </summary>
    public static class CharacterTypes
    {
        public const string Alchimiste = "alchimiste";
        public const string Assassin = "assassin";
        public const string Berserker = "berserker";
        public const string Guerrier = "guerrier";
        public const string Illusioniste = "illusioniste";
        public const string Magicien = "magicien";
        public const string Necromancien = "necromancien";
        public const string Paladin = "paladin";
        public const string Pretre = "pretre";
        public const string Robot = "robot";
        public const string Vampire = "vampire";
        public const string Zombie = "zombie";

        public static readonly string[] All = 
        {
            Alchimiste, Assassin, Berserker, Guerrier,
            Illusioniste, Magicien, Necromancien, Paladin,
            Pretre, Robot, Vampire, Zombie
        };
    }
}
