# État du Projet - Docker Backend WebSocket

## ✅ Infrastructure Docker

### Services Opérationnels
```bash
docker ps
```

| Service | Port | Statut | Description |
|---------|------|--------|-------------|
| **rpgarena-backend** | 5000 | ✅ Healthy | Backend WebSocket .NET 9.0 |
| **rpgarena-mongodb** | 27017 | ✅ Healthy | Base de données MongoDB 8.0 |
| **rpgarena-mongo-express** | 8081 | ✅ Running | Interface web MongoDB |

### Configuration

**Backend WebSocket:**
- URL: `ws://localhost:5000/ws`
- Health Check: `http://localhost:5000/health`
- Environment: Development
- Dockerfile: Multi-stage build (SDK → Publish → Runtime)

**MongoDB:**
- Connection String: `mongodb://admin:admin123@mongodb:27017/RPGArena`
- User credentials: `rpgarena_user:rpgarena_pass`
- Database: `RPGArena`

**Network:**
- Nom: `rpgarena-network`
- Type: Bridge

---

## 🔧 Configuration Godot

### WebSocketClient.cs - Ligne 14
```csharp
private string _serverUrl = "ws://localhost:5000/ws";  // ✅ Mis à jour (était 5018)
```

### Types de Personnages Supportés
```
alchimiste, assassin, berserker, guerrier, illusioniste, 
magicien, necromancien, paladin, pretre, robot, vampire, zombie
```

---

## 📡 Protocole WebSocket

### Format d'Entrée
Le serveur attend **directement** une liste de personnages au format JSON:

```json
[
  {"type": "guerrier", "name": "Arthas"},
  {"type": "magicien", "name": "Jaina"},
  {"type": "pretre", "name": "Uther"},
  {"type": "assassin", "name": "Valeera"}
]
```

**Important:** 
- Minimum **2 personnages** requis
- Types en **français** (voir liste ci-dessus)
- Pas de wrapper `{"type": "start_battle", "characters": [...]}`

### Format de Sortie (Logs)
Le serveur envoie des messages de log au format texte avec emojis:
```
🟢 Début du combat !
⚔️  Arthas attaque Jaina pour 15 dégâts
💀 Jaina est mort !
🏆 Victoire de l'équipe !
```

---

## 🧪 Test de Connexion

### Script Python
```bash
cd /mnt/.../UIGodotRPG
python3 test_websocket_docker.py
```

### Résultat Actuel
```
✅ Connexion établie
✅ Format personnages accepté  
⚠️  1 événement reçu puis déconnexion (code 1006)
```

**Comportement observé:**
- Le serveur accepte la connexion
- Le format JSON est validé
- Message "🟢 Début du combat !" reçu
- Connexion fermée immédiatement après (code 1006: abnormal closure)

---

## 🐛 Problèmes Identifiés

### 1. Combat Ne Continue Pas
**Symptôme:** Le serveur envoie uniquement "Début du combat" puis se déconnecte

**Logs Backend:**
```
📨 Message reçu: [{"type": "guerrier", ...}]
✅ Format nouveau protocole détecté: 4 personnages
[LOG] 🟢 Début du combat !
```

**Hypothèses:**
- Le BattleManager pourrait ne pas lancer la boucle de combat
- Problème dans FightService ou Arena
- Exception silencieuse côté backend

### 2. Ancien Protocole dans WebSocketHandler.cs
Le code tente de supporter l'ancien format (liste de noms simples) mais ce n'est plus nécessaire.

---

## 📋 Actions Recommandées

### Court Terme
1. ✅ ~~Mettre à jour `WebSocketClient.cs` avec port 5000~~ (FAIT)
2. ⏳ Investiguer pourquoi le combat ne démarre pas après l'initialisation
3. ⏳ Vérifier les logs MongoDB pour voir si les données sont enregistrées
4. ⏳ Ajouter plus de logs dans BattleManager.cs

### Moyen Terme
1. Documenter le protocole WebSocket complet
2. Ajouter des tests d'intégration automatisés
3. Configurer les certificats SSL pour HTTPS (port 5001)
4. Mettre en place des logs structurés (Serilog?)

### Long Terme
1. Implémenter un système de rooms pour gérer plusieurs combats simultanés
2. Ajouter authentification/autorisation
3. Migrer vers SignalR pour une meilleure gestion des connexions
4. Mettre en place monitoring (Prometheus + Grafana)

---

## 🔍 Commandes Utiles

### Docker
```bash
# Démarrer tous les services
docker-compose up -d

# Arrêter tous les services
docker-compose down

# Voir les logs en temps réel
docker-compose logs -f backend

# Redémarrer le backend
docker-compose restart backend

# Rebuild complet
docker-compose up -d --build
```

### Debug Backend
```bash
# Logs complets
docker logs rpgarena-backend

# Logs sans warnings
docker logs rpgarena-backend | grep -v "warning CS"

# Entrer dans le conteneur
docker exec -it rpgarena-backend bash

# Health check
curl http://localhost:5000/health
```

### MongoDB
```bash
# Shell MongoDB
docker exec -it rpgarena-mongodb mongosh

# Lister les bases de données
docker exec rpgarena-mongodb mongosh --eval "show dbs"

# Vérifier la collection des combats
docker exec rpgarena-mongodb mongosh RPGArena --eval "db.combats.find().pretty()"
```

---

## 📁 Fichiers Clés

### Frontend Godot
- `Scripts/Network/WebSocketClient.cs` - Client WebSocket singleton
- `Scripts/Combat/CombatLogParser.cs` - Parser des logs emoji
- `Scripts/UI/MainUI.cs` - Interface de monitoring
- `Scripts/PersonnageUIManager.cs` - Gestion des cartes personnages
- `Scripts/AreneController.cs` - Synchronisation WebSocket → Arena
- `Scripts/ViewManager.cs` - Navigation entre vues

### Backend Docker
- `docker-compose.yml` - Stack complète (backend + MongoDB + MongoExpress)
- `Dockerfile` - Build multi-stage .NET 9.0
- `RPGArena.Backend/Services/WebSocketHandler.cs` - Gestion des connexions
- `RPGArena.Backend/Services/BattleManager.cs` - Orchestration des combats
- `RPGArena.CombatEngine/Characters/CharacterFactory.cs` - Création des personnages

---

**Dernière mise à jour:** 26 octobre 2025 - 01:23 UTC
**Status:** ✅ Infrastructure opérationnelle, ⚠️ Combat incomplet
