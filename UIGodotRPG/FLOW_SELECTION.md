# Flow de Sélection des Personnages

## Architecture mise à jour (26 oct 2025)

### 🎮 Scène principale
**`project.godot`** → `res://Arene/Arene.tscn` (Mode Debug Direct)

> **Note**: Pour utiliser le système complet Menu → Sélection → Combat, 
> changez vers `res://Scenes/GameController.tscn`

### 🚀 Mode Debug Direct (Actuel)

Le mode actuel lance directement l'arène avec connexion automatique :

```
1. Arene (Arene.tscn) - Lance au démarrage
   ├─ AreneController._Ready()
   │  ├─ Récupère WebSocketClient AutoLoad
   │  ├─ Récupère ProfileGrid (9 slots par défaut)
   │  ├─ Configure CombatLogParser
   │  └─ Connecte événements WebSocket
   │
   ├─ Connexion automatique au serveur
   │  └─ WebSocketClient.ConnectToServer()
   │
   ├─ OnConnectionEstablished()
   │  └─ StartDefaultBattle() si pas de sélection
   │     └─ Combat par défaut avec 4 personnages:
   │        • Ragnar (Berserker)
   │        • Shadow (Assassin)
   │        • Lumière (Pretre)
   │        • Merlin (Magicien)
   │
   └─ Messages WebSocket reçus
      ├─ OnEventParsed() → Crée UIs dynamiquement
      └─ UpdateCharacterUIs() → Met à jour affichage temps réel
```

### ✨ Fonctionnement

1. **Démarrage**: Arene.tscn charge immédiatement
2. **Connexion**: WebSocket se connecte à `ws://localhost:5000/ws`
3. **Combat auto**: Envoie demande de combat avec 4 personnages par défaut
4. **UI dynamique**: Crée les cartes de personnages au fur et à mesure des événements
5. **Mise à jour**: Les UIs se mettent à jour en temps réel selon les événements de combat

### 📋 Flow complet

```
1. Menu (Menu.tscn)
   ├─ Bouton "Play" → CharacterSelection
   └─ Bouton "Quit" → Ferme le jeu

2. CharacterSelection (CharacterSelectionScreen.tscn)
   ├─ LoadAvailableClassesAsync()
   │  ├─ Appel HTTP GET: /api/personnages/classes
   │  ├─ Si backend disponible: Charge classes réelles
   │  └─ Si backend indisponible: Fallback 12 classes par défaut
   │
   ├─ User toggle caractères (min 2, max 10)
   ├─ Bouton "Start Battle" (enabled si >= 2 sélectionnés)
   └─ Event CharactersSelected(List<CharacterConfig>)

3. BattleViewer (BattleViewer.tscn)
   ├─ StartNewBattle(characters)
   ├─ Crée display pour chaque personnage
   ├─ WebSocketClient.StartBattle(characters)
   └─ Écoute messages combat en temps réel
```

### 🔧 Composants modifiés

#### **WebSocketClient.cs**
```csharp
// Nouvelle méthode
public async Task<List<string>> GetAvailableClassesAsync()
{
    // HTTP GET vers /api/personnages/classes
    // Fallback: 12 classes par défaut
}
```

#### **CharacterSelectionUI.cs**
```csharp
// Au démarrage
private async void LoadAvailableClassesAsync()
{
    var wsClient = GetNode<WebSocketClient>("/root/WebSocketClient");
    var availableClasses = await wsClient.GetAvailableClassesAsync();
    // Crée boutons dynamiquement
}
```

#### **ProfileGrid.cs**
```csharp
// Nouvelle méthode pour initialisation dynamique
public void InitializeWithCharacters(List<CharacterConfig> characters)
{
    // Crée exactement N profils selon sélection
}
```

#### **AreneController.cs**
```csharp
// Nouvelle méthode
public void SetSelectedCharacters(List<CharacterConfig> characters)
{
    // Configure les personnages avant _Ready()
}
```

### ⚠️ Backend endpoint requis

**CRITIQUE**: Le frontend appelle `/api/personnages/classes` qui n'existe pas encore.

**À créer dans RPG-Arena-Backend**:
```csharp
// Controllers/PersonnagesController.cs
[HttpGet("api/personnages/classes")]
public IActionResult GetAvailableClasses()
{
    // Option 1: Depuis PersonnageFactory
    var classes = PersonnageFactory.GetAllClassNames();
    
    // Option 2: Depuis enum
    var classes = Enum.GetNames(typeof(PersonnageType));
    
    return Ok(classes);
}
```

### 🧪 Test sans backend

Le système fonctionne même sans backend grâce au fallback:
1. Lancer Godot uniquement
2. Menu → Sélection
3. 12 classes par défaut chargées automatiquement
4. Sélection fonctionne (2-10 personnages)
5. **Combat ne démarrera pas** (WebSocket non connecté)

### 🚀 Test avec backend

1. Créer endpoint `/api/personnages/classes`
2. Lancer Docker: `docker-compose up -d`
3. Lancer Godot
4. Menu → Sélection → Combat
5. ✅ Tout fonctionne end-to-end

### 📊 Classes par défaut (fallback)

1. Guerrier
2. Berserker
3. Magicien
4. Assassin
5. Pretre
6. Paladin
7. Necromancien
8. Alchimiste
9. Illusioniste
10. Vampire
11. Zombie
12. Robot

### 🔍 Logs de debug

```
[CharacterSelection] Chargement de X classes disponibles
[Selection] Ajouté: [Nom] ([Type])
[Selection] Retiré: [Nom]
[Selection] Démarrage du combat avec X personnages
[GameController] X personnages sélectionnés, démarrage du combat
[GameController] Envoi de la configuration de combat au serveur
[BattleViewer] Nouvelle bataille initialisée avec X personnages
```

### 📝 Points techniques

- **MIN_CHARACTERS**: 2
- **MAX_CHARACTERS**: 10
- **HTTP timeout**: Géré automatiquement
- **WebSocket AutoLoad**: `/root/WebSocketClient`
- **Backend URL**: ws://localhost:5000/ws → http://localhost:5000
- **Fallback**: Toujours fonctionnel si backend down
