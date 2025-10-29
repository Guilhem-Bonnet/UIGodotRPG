# Problèmes identifiés dans le Backend RPG Arena

## ✅ RÉSOLU : WebSocketLogger bloquant
**Fichier**: `RPGArena.Backend/Loggers/WebSocketLogger.cs`

**Problème**:
```csharp
_socket.SendAsync(...).GetAwaiter().GetResult(); // DEADLOCK !
```

**Solution appliquée**:
```csharp
_ = Task.Run(async () =>
{
    try
    {
        await _socket.SendAsync(...);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ WebSocket send error: {ex.Message}");
    }
});
```

**Status**: ✅ Corrigé et déployé

---

## ⚠️ NON RÉSOLU : Les personnages n'exécutent pas leurs stratégies

**Fichier**: `RPGArena.CombatEngine/Core/BattleArena.cs`

**Problème**: Dans `StartBattle()`, les tasks des personnages sont lancées mais ne s'exécutent jamais:

```csharp
public async Task StartBattle()
{
    _endBattle = false;
    _logger.Log("🟢 Début du combat !");

    // Lance les strategies mais elles ne s'exécutent jamais !
    var tasks = _characters.Select(c => Task.Run(() => c.ExecuteStrategyAsync())).ToArray();

    // La boucle attend que des personnages meurent, mais personne n'agit...
    while (_characters.Count(p => p.Life > 0) > 1 && ...)
    {
        await Task.Delay(1000);
    }
    // ...
}
```

**Observations**:
1. "🟢 Début du combat !" est loggé
2. Aucun log d'attaque n'apparaît ("🛡️ X attaque Y")
3. La boucle `while` attend indéfiniment (timeout MongoDB de 30s)
4. Aucune exception n'est levée

**Hypothèses**:
1. Les `Task.Run(() => c.ExecuteStrategyAsync())` ne sont peut-être pas correctement awaités
2. Il manque peut-être `await Task.WhenAll(tasks)` quelque part
3. Les conditions dans `ExecuteStrategyAsync()` (`!IsDead && !_arena.Ended`) pourraient être fausses dès le début
4. `CanAct` dans les personnages pourrait toujours retourner `false`

**Solution possible**:
```csharp
public async Task StartBattle()
{
    _endBattle = false;
    _logger.Log("🟢 Début du combat !");

    // Lancer les stratégies de tous les personnages
    var tasks = _characters.Select(c => Task.Run(c.ExecuteStrategyAsync)).ToArray();

    // Attendre qu'il n'y ait plus qu'un survivant
    while (_characters.Count(p => p.Life > 0) > 1 && ...)
    {
        await Task.Delay(1000);
    }

    _endBattle = true;

    // Attendre que toutes les tasks se terminent
    await Task.WhenAll(tasks);

    // Résumé...
}
```

**Status**: ⚠️  **NON RÉSOLU** - Nécessite modification du CombatEngine

---

## 📝 Configuration Docker

**Réseau**: `rpgarena-network`

**Services**:
- **rpgarena-mongodb** (mongo:8.0)
  - Port: 27017
  - Credentials: admin/admin123
  - Database: RPGArena

- **rpgarena-backend** (rpg-arena_backend)
  - Port: 5000 (HTTP only)
  - WebSocket: ws://localhost:5000/ws
  - Environment:
    - `ASPNETCORE_ENVIRONMENT=Development`
    - `ASPNETCORE_URLS=http://+:5000`
    - `ConnectionStrings__mongodb=mongodb://admin:admin123@rpgarena-mongodb:27017/RPGArena?authSource=admin`

**Commandes de démarrage**:
```bash
# Créer le réseau
docker network create rpgarena-network

# Lancer MongoDB
docker run -d --name rpgarena-mongodb \
  --network rpgarena-network \
  -p 27017:27017 \
  -e MONGO_INITDB_ROOT_USERNAME=admin \
  -e MONGO_INITDB_ROOT_PASSWORD=admin123 \
  -e MONGO_INITDB_DATABASE=RPGArena \
  mongo:8.0

# Attendre 5 secondes

# Lancer le Backend
docker run -d --name rpgarena-backend \
  --network rpgarena-network \
  -p 5000:5000 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ASPNETCORE_URLS=http://+:5000 \
  -e "ConnectionStrings__mongodb=mongodb://admin:admin123@rpgarena-mongodb:27017/RPGArena?authSource=admin" \
  rpg-arena_backend
```

---

## 🧪 Tests

**Script de test**: `test_websocket_docker.py`

**Format de message attendu**:
```json
[
  {"type": "guerrier", "name": "Arthas"},
  {"type": "magicien", "name": "Jaina"},
  {"type": "pretre", "name": "Uther"},
  {"type": "assassin", "name": "Valeera"}
]
```

**Types de personnages supportés**:
- alchimiste, assassin, berserker, guerrier
- illusioniste, magicien, necromancien, paladin
- pretre, robot, vampire, zombie

**Résultat actuel**:
✅ Connexion WebSocket OK
✅ Format JSON validé
✅ Personnages créés
⚠️  Combat ne démarre pas (aucune action)
❌ Timeout après "🟢 Début du combat !"

---

## 📌 TODO

1. [ ] Investiguer pourquoi `ExecuteStrategyAsync()` ne s'exécute pas
2. [ ] Ajouter des logs de debug dans `Character.ExecuteStrategyAsync()`
3. [ ] Vérifier les conditions `CanAct`, `IsDead`, `_arena.Ended`
4. [ ] Tester avec un seul personnage
5. [ ] Vérifier si `Task.Run(() => lambda)` vs `Task.Run(method)` fait une différence
6. [ ] Potentiellement refactorer `BattleArena.StartBattle()` pour awaiter les tasks

