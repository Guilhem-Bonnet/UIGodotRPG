#!/usr/bin/env python3
"""
Script de test pour valider la connexion WebSocket avec le backend Docker
"""
import asyncio
import websockets
import json

SERVER_URL = "ws://localhost:5000/ws"

async def test_connection():
    print(f"🔌 Connexion à {SERVER_URL}...")
    
    try:
        async with websockets.connect(SERVER_URL) as websocket:
            print("✅ Connexion établie !")
            
            # Le serveur attend directement la liste des personnages au format:
            # [{"type": "guerrier", "name": "Nom"}, ...]
            # Types supportés: alchimiste, assassin, berserker, guerrier, illusioniste,
            #                  magicien, necromancien, paladin, pretre, robot, vampire, zombie
            print("\n⚔️  Envoi de la liste des personnages pour démarrer le combat...")
            characters = [
                {"type": "guerrier", "name": "Arthas"},
                {"type": "magicien", "name": "Jaina"},
                {"type": "pretre", "name": "Uther"},
                {"type": "assassin", "name": "Valeera"}
            ]
            await websocket.send(json.dumps(characters))
            print(f"📤 Envoyé: {len(characters)} personnages")
            
            # Écouter les événements de combat
            print("\n📜 Écoute des événements de combat...\n")
            try:
                event_count = 0
                last_message_time = asyncio.get_event_loop().time()
                
                while True:
                    message = await asyncio.wait_for(websocket.recv(), timeout=5.0)
                    event_count += 1
                    current_time = asyncio.get_event_loop().time()
                    delta = current_time - last_message_time
                    last_message_time = current_time
                    
                    # Afficher avec timing
                    if len(message) > 150:
                        print(f"📨 [{delta:.2f}s] Événement #{event_count}: {message[:150]}...")
                    else:
                        print(f"📨 [{delta:.2f}s] Événement #{event_count}: {message}")
                    
            except asyncio.TimeoutError:
                print(f"\n⏱️  Timeout après {event_count} événements (pas de message pendant 5s)")
            except websockets.exceptions.ConnectionClosed as e:
                print(f"\n🏁 Combat terminé ({e.code}: {e.reason}) - {event_count} événements reçus")
            
    except Exception as e:
        print(f"❌ Erreur: {e}")

if __name__ == "__main__":
    print("=" * 60)
    print("Test de connexion WebSocket - Backend Docker")
    print("=" * 60)
    asyncio.run(test_connection())
    print("\n" + "=" * 60)
    print("Test terminé")
    print("=" * 60)
