# 🚀 Guide de Démarrage Rapide - RPG Arena UI

## ⚡ Lancement en 3 étapes

### 1️⃣ Démarrer le serveur (Terminal 1)
```bash
cd RPG-Arena/RPGArena.Backend
dotnet run
```
✅ Attendre : `Now listening on: http://localhost:5018`

### 2️⃣ Lancer Godot
- Ouvrir le projet dans Godot 4.4
- Appuyer sur **F5** ou cliquer sur **▶ Play**

### 3️⃣ Utiliser l'interface
1. Cliquer sur **🔌 Se connecter**
2. Attendre le statut **🟢 Connecté**
3. Cliquer sur **⚔️ Démarrer Combat**
4. Observer le combat en temps réel !

## 📊 Ce que vous verrez

### En haut à gauche : Connexion
- 🟢 Statut de connexion
- ⏱️ Temps de connexion actif
- 📡 Latence réseau
- 📨 Nombre de messages reçus

### En haut à droite : Statistiques
- ⚔️ État du combat (en cours, terminé, vainqueur)
- 👥 Nombre de personnages
- ✅ Vivants vs 💀 Morts
- 💥 Actions effectuées

### En bas à gauche : Personnages
```
✅ Conan (guerrier)
  [████████░░] 80/100 HP (80%)

✅ Merlin (magicien)
  [██████████] 100/100 HP (100%)

💀 Ragnar (berserker)
  [░░░░░░░░░░] 0/100 HP (0%)
```

### En bas à droite : Journal
```
[12:34:56.123] 🟢 Début du combat !
[12:34:57.456] 🪓 Conan attaque Ragnar
[12:34:57.789] 💥 Ragnar subit 15 dégâts !
[12:35:02.012] 💀 Ragnar est mort !
[12:35:05.345] 🏆 Conan est le gagnant !
```

## 🎮 Contrôles

| Bouton | Action | Quand l'utiliser |
|--------|--------|------------------|
| 🔌 Se connecter | Connecte au serveur | Au démarrage |
| 🔌 Déconnecter | Ferme la connexion | Avant de quitter |
| ⚔️ Démarrer Combat | Lance un combat test | Après connexion |
| 🗑️ Effacer Logs | Nettoie le journal | Quand trop de logs |

## 🎨 Code couleur

### Connexion
- 🟢 Vert : Tout va bien
- 🟡 Jaune : Latence moyenne
- 🔴 Rouge : Problème de connexion

### HP des personnages
- 🟢 Vert : > 50% HP (en forme)
- 🟠 Orange : 25-50% HP (blessé)
- 🔴 Rouge : < 25% HP (critique)
- ⚫ Gris : 0% HP (mort)

### Types d'actions
- 🟢 Début/fin de combat
- 🟡 Attaques normales
- 🟠 Dégâts infligés
- 🟣 Capacités spéciales
- 💚 Soins
- 🏆 Victoire

## 🐛 Résolution de problèmes

### ❌ "Connexion refusée"
➡️ Le serveur n'est pas démarré
- Vérifier que `dotnet run` tourne dans un autre terminal
- Vérifier le port 5018

### ❌ "Aucun message reçu"
➡️ Le combat n'a pas démarré
- Cliquer sur **⚔️ Démarrer Combat** après connexion

### ❌ "Statistiques ne bougent pas"
➡️ Pas de parsing des événements
- Vérifier la console Godot pour erreurs
- Relancer le combat

### ❌ "Interface ne répond pas"
➡️ Godot figé
- Redémarrer Godot (Ctrl+Alt+R)
- Vérifier pas de boucle infinie dans le code

## 📁 Fichiers importants

```
UIGodotRPG/
├── Scenes/
│   └── TestWebSocket.tscn        ⭐ SCÈNE PRINCIPALE
├── Scripts/
│   ├── UI/
│   │   └── MainUI.cs             ⭐ LOGIQUE UI
│   ├── Network/
│   │   ├── WebSocketClient.cs    ⭐ CONNEXION
│   │   └── Models.cs             
│   └── Combat/
│       ├── CombatLogParser.cs    ⭐ PARSING
│       └── CombatModels.cs       
└── project.godot
```

## 🔧 Personnages de test

Le bouton **⚔️ Démarrer Combat** lance automatiquement :
1. **Conan** (guerrier) 🛡️
2. **Ragnar** (berserker) 🪓
3. **Merlin** (magicien) 🔮
4. **Shadow** (assassin) 🗡️

## 📈 Statistiques typiques

Pour un combat de 4 personnages :
- ⏱️ Durée : 30-60 secondes
- 📨 Messages : 100-200
- 💥 Actions : 40-80
- 🏆 Gagnant : Variable !

## 💡 Astuces

### Pour un meilleur suivi
1. **Agrandir la fenêtre** pour voir plus de logs
2. **Effacer les logs** entre deux combats
3. **Observer les HP bars** pour anticiper la fin
4. **Lire l'horodatage** pour la chronologie exacte

### Pour tester différemment
Modifier dans `MainUI.cs` ligne ~215 :
```csharp
var characters = new List<CharacterConfig>
{
    new CharacterConfig { Type = CharacterTypes.Vampire, Name = "Dracula" },
    new CharacterConfig { Type = CharacterTypes.Zombie, Name = "Walker" },
    // ... ajoutez vos personnages
};
```

## 🎯 Ce qui fonctionne

✅ Connexion WebSocket avec reconnexion auto  
✅ Parsing de 10+ types d'événements  
✅ Statistiques temps réel (60 FPS)  
✅ HP bars colorées dynamiques  
✅ Journal avec horodatage milliseconde  
✅ 4 panneaux d'information simultanés  
✅ Auto-scroll des logs  
✅ Gestion propre de la connexion  

## 📚 Documentation complète

- `INTERFACE_ENRICHIE.md` : Guide détaillé de l'interface
- `GUIDE_UTILISATION.md` : Guide utilisateur complet
- `RECAPITULATIF_DEV.md` : Résumé développement
- `README.md` : Documentation technique

## 🆘 Support

En cas de problème :
1. Consulter la console Godot (F4)
2. Vérifier les logs du serveur
3. Relancer serveur + client
4. Consulter `INTERFACE_ENRICHIE.md` pour détails

---

## 🎉 Bon combat !

Lancez votre première bataille et observez les personnages s'affronter en temps réel avec toutes les statistiques sous les yeux ! ⚔️
