# 🎮 RPG Arena UI - Guide d'utilisation

## 📋 Vue d'ensemble

Interface Godot 4.4 (C#) pour le système de combat RPG Arena Battle Royale.

## 🏗️ Architecture

### Structure des fichiers

```
UIGodotRPG/
├── Scenes/
│   ├── GameController.tscn          # Contrôleur principal (scène de démarrage)
│   ├── Menu.tscn                    # Menu principal
│   ├── CharacterSelectionScreen.tscn # Sélection des personnages
│   ├── BattleViewer.tscn            # Visualisation du combat en temps réel
│   └── TestWebSocket.tscn           # Test WebSocket (ancien, conservé pour debug)
├── Scripts/
│   ├── GameController.cs            # Navigation entre les écrans
│   ├── Network/
│   │   ├── WebSocketClient.cs       # Client WebSocket (AutoLoad)
│   │   └── Models.cs                # DTOs (CharacterConfig, CharacterTypes, etc.)
│   ├── Combat/
│   │   ├── CombatModels.cs          # CombatEvent, CharacterState, BattleState
│   │   └── CombatLogParser.cs       # Parse les logs textuels en événements structurés
│   └── UI/
│       ├── CharacterSelectionUI.cs  # Logique de sélection (2-10 personnages)
│       ├── BattleViewer.cs          # Affichage temps réel (HP bars, logs, statuts)
│       └── MainUI.cs                # Test WebSocket (ancien)
└── project.godot                    # Configuration Godot
```

### Flux de l'application

```
Menu
  ↓ [Jouer]
CharacterSelectionScreen
  ↓ [Démarrer le Combat]
BattleViewer
  ↓ WebSocket → RPG-Arena Server
Combat en temps réel avec parsing des logs
```

## 🚀 Démarrage

### 1. Lancer le serveur RPG-Arena

Le serveur doit être lancé séparément. Cherchez le projet `RPG-Arena` à la racine de votre workspace et lancez :

```bash
cd ../RPG-Arena  # Depuis UIGodotRPG
dotnet run --project RPGArena.Backend
```

Le serveur écoute sur `ws://localhost:5018/ws`

### 2. Lancer l'interface Godot

Ouvrez le projet dans Godot 4.4 et appuyez sur F5 (ou cliquez sur Play).

## 🎯 Fonctionnalités

### 1. Menu Principal

- **Jouer** : Lance le flux de sélection → combat
- **Quitter** : Ferme l'application

### 2. Sélection de Personnages

- **12 types disponibles** :
  - 🧪 Alchimiste
  - 🗡️ Assassin
  - 🪓 Berserker
  - 🛡️ Guerrier
  - ✨ Illusioniste
  - 🔮 Magicien
  - 💀 Nécromancien
  - ⚔️ Paladin
  - 📿 Prêtre
  - 🤖 Robot
  - 🧛 Vampire
  - 🧟 Zombie

- **Contraintes** :
  - Minimum : 2 personnages
  - Maximum : 10 personnages

- **Noms par défaut** : Chaque type a des noms prédéfinis (ex: Conan, Ragnar, Merlin)

### 3. Visualisation du Combat

#### Panneau de gauche : Combattants
- Carte pour chaque personnage avec :
  - Nom et type
  - Barre de HP (vert → orange → rouge selon le %)
  - HP numérique (actuel/max)
  - Statut : ✅ Vivant / 💀 Mort
  - Effets de statut actifs

#### Panneau de droite : Journal de combat
- Logs colorés selon le type d'événement :
  - 🟢 Vert : Début de combat
  - 🛑 Rouge : Fin de combat
  - 🪓 Jaune : Attaques
  - 💥 Orange : Dégâts
  - ❤️ Vert clair : Soins
  - 💀 Rouge : Morts
  - 🏆 Or : Victoire

- Auto-scroll vers le bas
- Horodatage pour chaque message

## 🔧 Système de Parsing

### CombatLogParser

Parse les messages textuels du serveur en événements structurés grâce aux emojis :

```csharp
"🪓 Ragnar fonce sur Conan avec rage !"
  ↓
CombatEvent {
  Type = Attack,
  SourceCharacter = "Ragnar",
  TargetCharacter = "Conan"
}
```

### Agrégation d'événements

Le parser peut agréger plusieurs messages consécutifs :
- Attaque + Lancer de dé + Dégâts = Action complète avec toutes les infos

### Types d'événements détectés

| Emoji | CombatEventType | Description |
|-------|----------------|-------------|
| 🟢 | BattleStart | Début du combat |
| 🛑 | BattleEnd | Fin du combat |
| 🏆 | Winner | Annonce du gagnant |
| 💀/☠️ | Death | Mort d'un personnage |
| 🪓/🛡️ | Attack | Attaque standard |
| ✨/💨/🧟 | SpecialAbility | Capacité spéciale |
| 💥/🩸 | Damage | Dégâts infligés |
| 🎲 | DiceRoll | Lancer de dé |
| ❤️ | Heal | Soin |

## 📡 Protocole WebSocket

### Connexion

```
ws://localhost:5018/ws
```

### Message de démarrage de combat

```json
[
  {"type": "guerrier", "name": "Conan"},
  {"type": "berserker", "name": "Ragnar"}
]
```

### Messages reçus

Flux de logs textuels avec emojis :

```
🟢 Début du combat !
🎲 Conan lance les dés : 15
🪓 Conan attaque Ragnar.
💥 Ragnar subit 12 dégâts !
🎲 Ragnar lance les dés : 18
🪓 Ragnar fonce sur Conan avec rage !
💥 Conan subit 18 dégâts !
...
💀 Ragnar est mort !
🛑 Fin du combat
🏆 Conan est le dernier survivant !
```

## 🧪 Tests

### Test manuel

1. Lancer le serveur RPG-Arena
2. Lancer Godot
3. Suivre le flux : Menu → Sélection → Combat
4. Observer la mise à jour en temps réel des HP et logs

### Test Python (debug)

```bash
python test_websocket.py
```

Envoie une configuration de combat de test et affiche les logs reçus.

### TestWebSocket.tscn (debug)

Scène de test simple avec :
- Bouton "Se connecter"
- Bouton "Démarrer un combat"
- Log de débogage

Utile pour tester le WebSocket sans passer par le flux complet.

## 🎨 Améliorations futures

### Court terme
- [ ] Animations lors des attaques
- [ ] Sons/effets sonores
- [ ] Icônes personnalisées pour chaque type de personnage
- [ ] Système de replay des combats

### Moyen terme
- [ ] Intégration de la scène Arene.tscn (3D)
- [ ] Positionnement spatial des personnages
- [ ] Caméra dynamique suivant l'action
- [ ] Effets visuels (particules, éclairs, etc.)

### Long terme
- [ ] Mode multijoueur (spectateur)
- [ ] Statistiques et historique des combats
- [ ] Système de tournoi
- [ ] Éditeur de personnage personnalisé

## 🐛 Débogage

### Le combat ne démarre pas

1. Vérifier que le serveur RPG-Arena est lancé
2. Vérifier la console Godot pour les messages de connexion
3. Essayer TestWebSocket.tscn pour isoler le problème

### Les HP ne se mettent pas à jour

1. Vérifier que les logs sont reçus (panneau de droite)
2. Vérifier la console Godot pour les messages "[Parser]"
3. Les dégâts peuvent ne pas être explicités dans certains logs du serveur

### Personnages manquants dans le BattleViewer

Le parser crée automatiquement les affichages quand il détecte un nom dans les logs. Si un personnage n'apparaît jamais dans les logs, il ne sera pas affiché.

## 📝 Notes techniques

### AutoLoad WebSocketClient

Le `WebSocketClient` est configuré en AutoLoad dans `project.godot`. Il est accessible depuis n'importe quel script via :

```csharp
var wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");
```

### Events C# vs Signals Godot

Les events complexes (avec List<T> ou objets custom) utilisent des events C# au lieu de signaux Godot car les signaux ne supportent pas les types génériques.

```csharp
// ✅ Event C# (supporté)
public event Action<List<CharacterConfig>> CharactersSelected;

// ❌ Signal Godot (non supporté)
[Signal]
public delegate void CharactersSelectedEventHandler(List<CharacterConfig> characters);
```

### Reconnexion automatique

Le WebSocketClient tente de se reconnecter toutes les 3 secondes en cas de déconnexion.

## 📚 Ressources

- [Godot 4.4 Documentation](https://docs.godotengine.org/)
- [Godot C# API](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/)
- [WebSocket C# Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.net.websockets)

## 📄 Licence

Projet éducatif - Serious Game RPG Arena
