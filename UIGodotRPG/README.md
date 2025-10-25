# Front-BR-RPG - Client Godot WebSocket

Interface utilisateur Godot 4.4 (C#) pour le serveur RPG-Arena.

## 🎯 Objectif

Client front-end pour visualiser et interagir avec des combats RPG en temps réel via WebSocket.

## 🏗️ Architecture

### Structure du projet

```
UIGodotRPG/
├── Scripts/
│   ├── Network/
│   │   ├── WebSocketClient.cs     # Client WebSocket (AutoLoad singleton)
│   │   └── BattleConfig.cs        # DTOs pour la communication
│   └── UI/
│       └── MainUI.cs               # Interface principale
├── Scenes/
│   └── TestWebSocket.tscn          # Scène de test WebSocket
├── Components/                     # Composants UI réutilisables
├── Arene/                          # Scène d'arène
└── icons/                          # Assets visuels

```

### Composants principaux

#### 1. WebSocketClient (AutoLoad)
- **Chemin**: `/root/WebSocketClient`
- **Fonctionnalités**:
  - Connexion/reconnexion automatique
  - Envoi de configurations de bataille
  - Réception des logs de combat en temps réel
  - Gestion des erreurs et déconnexions

#### 2. Protocol WebSocket

**Envoi au serveur** (démarrage de combat):
```json
["Guerrier", "Berserker", "Assassin"]
```

**Réception du serveur**:
- Messages texte ligne par ligne
- Logs de combat formatés
- Notification de fin de combat

## 🚀 Utilisation

### 1. Lancer le serveur RPG-Arena

```bash
cd ../RPG-Arena-Backend/RPG-Arena/RPGArena.Backend
dotnet run
```

Le serveur démarre sur `ws://localhost:5000/ws`

### 2. Lancer le client Godot

1. Ouvrir le projet dans Godot 4.4
2. Compiler les scripts C# (Build)
3. Lancer la scène `TestWebSocket.tscn` (F5)

### 3. Interface de test

- **Bouton "Se connecter"**: Établit la connexion WebSocket
- **Bouton "Démarrer Combat"**: Lance un combat de test
- **Zone de logs**: Affiche les messages du serveur en temps réel

## 📡 API WebSocket

### Signaux disponibles

```csharp
// WebSocketClient émet ces signaux:
[Signal] MessageReceived(string message)           // Message reçu du serveur
[Signal] ConnectionEstablished()                   // Connexion établie
[Signal] ConnectionClosed(string reason)           // Connexion fermée
[Signal] ConnectionError(string error)             // Erreur de connexion
```

### Méthodes publiques

```csharp
var wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");

// Connexion
wsClient.ConnectToServer();

// Démarrer une bataille
var characters = new List<string> { "Guerrier", "Berserker" };
wsClient.StartBattle(characters);

// Déconnexion
wsClient.Disconnect();

// Vérifier l'état
bool connected = wsClient.IsConnected;
```

## 🔧 Configuration

### Changer l'URL du serveur

Dans `WebSocketClient.cs`:
```csharp
private string _serverUrl = "ws://localhost:5000/ws";
```

Ou dynamiquement:
```csharp
wsClient.ServerUrl = "ws://votre-serveur:port/ws";
wsClient.ConnectToServer();
```

## 🐛 Debug

### Logs dans la console Godot

Le client affiche des logs détaillés:
- 🔄 Tentatives de connexion
- ✅ Connexion établie
- 📨 Messages envoyés/reçus
- ❌ Erreurs
- 🔌 Déconnexions

### Problèmes courants

1. **"Not connected to server"**
   - Vérifier que le serveur RPG-Arena est lancé
   - Vérifier l'URL de connexion

2. **Reconnexion en boucle**
   - Le serveur n'est pas accessible
   - Firewall bloque la connexion

3. **Pas de messages reçus**
   - Vérifier que la bataille a bien démarré
   - Vérifier les logs du serveur

## 🎮 Personnages disponibles

Selon le serveur RPG-Arena:
- Guerrier
- Berserker
- Assassin
- Alchimiste
- Illusioniste
- Prêtre
- Paladin
- Zombie
- Vampire
- Robot

## 📝 TODO

- [ ] Interface graphique complète pour l'arène
- [ ] Sélection visuelle des personnages
- [ ] Affichage des statistiques en temps réel
- [ ] Animation des actions de combat
- [ ] Historique des combats
- [ ] Mode spectateur multi-combats

## 🔗 Dépendances

- Godot 4.4+
- .NET 8.0
- Serveur RPG-Arena en cours d'exécution

## 📄 Licence

Projet éducatif - Serious Game
