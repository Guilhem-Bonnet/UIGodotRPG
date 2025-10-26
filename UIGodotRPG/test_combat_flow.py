#!/usr/bin/env python3
"""
Script de test pour valider le flux complet du système de combat
"""

import asyncio
import websockets
import json
import time

async def test_combat_flow():
    uri = "ws://localhost:5018/ws"
    
    print("🔗 Connexion au serveur RPG-Arena...")
    
    try:
        async with websockets.connect(uri) as websocket:
            print("✅ Connecté au serveur")
            
            # Configuration de combat
            characters = [
                {"type": "guerrier", "name": "Conan"},
                {"type": "berserker", "name": "Ragnar"},
                {"type": "magicien", "name": "Merlin"},
                {"type": "assassin", "name": "Shadow"}
            ]
            
            # Envoyer la configuration
            message = json.dumps(characters)
            print(f"\n📤 Envoi de la configuration : {len(characters)} personnages")
            await websocket.send(message)
            
            print("\n📜 Logs de combat reçus:\n")
            print("-" * 80)
            
            # Recevoir les logs
            log_count = 0
            combat_started = False
            combat_ended = False
            
            try:
                while True:
                    response = await asyncio.wait_for(websocket.recv(), timeout=30.0)
                    log_count += 1
                    
                    # Détecter les événements importants
                    if "🟢" in response and "Début" in response:
                        combat_started = True
                        print(f"\n🟢 COMBAT COMMENCÉ\n")
                    elif "🛑" in response or "Fin du combat" in response:
                        combat_ended = True
                        print(f"\n{response}")
                        print(f"\n🛑 COMBAT TERMINÉ\n")
                    elif "🏆" in response:
                        print(f"👑 {response}")
                    elif "💀" in response:
                        print(f"☠️  {response}")
                    elif "🪓" in response or "🛡️" in response:
                        print(f"⚔️  {response}")
                    elif "💥" in response or "🩸" in response:
                        print(f"💢 {response}")
                    else:
                        print(f"   {response}")
                    
                    # Arrêter après la fin du combat
                    if combat_ended:
                        break
                        
            except asyncio.TimeoutError:
                print("\n⏱️ Timeout - Fin de la réception des logs")
            
            print("-" * 80)
            print(f"\n📊 Résumé:")
            print(f"   - Logs reçus: {log_count}")
            print(f"   - Combat démarré: {'✅' if combat_started else '❌'}")
            print(f"   - Combat terminé: {'✅' if combat_ended else '❌'}")
            
    except ConnectionRefusedError:
        print("❌ Impossible de se connecter au serveur")
        print("   Assurez-vous que le serveur RPG-Arena est lancé sur le port 5018")
    except Exception as e:
        print(f"❌ Erreur: {e}")

if __name__ == "__main__":
    print("=" * 80)
    print("🎮 RPG ARENA - Test du flux de combat")
    print("=" * 80)
    print()
    
    asyncio.run(test_combat_flow())
    
    print("\n✅ Test terminé")
