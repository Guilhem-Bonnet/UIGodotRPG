#!/usr/bin/env python3
"""
Script de test WebSocket pour valider la connexion au serveur RPG-Arena
Simule le comportement du client Godot
"""

import asyncio
import websockets
import json

async def test_websocket():
    uri = "ws://localhost:5018/ws"
    
    print("🔄 Connexion au serveur RPG-Arena...")
    
    try:
        async with websockets.connect(uri) as websocket:
            print("✅ Connexion établie !")
            
            # Envoyer la configuration de bataille (nouveau format)
            characters = [
                {"type": "guerrier", "name": "Conan"},
                {"type": "berserker", "name": "Ragnar"}
            ]
            config_json = json.dumps(characters)
            
            print(f"📤 Envoi de la configuration: {config_json}")
            await websocket.send(config_json)
            
            print("\n📜 Logs de combat:\n" + "="*50)
            
            # Recevoir les messages
            try:
                while True:
                    message = await websocket.recv()
                    print(message)
            except websockets.exceptions.ConnectionClosed:
                print("="*50)
                print("\n✅ Combat terminé - Connexion fermée")
                
    except ConnectionRefusedError:
        print("❌ Erreur: Le serveur n'est pas accessible")
        print("   Assurez-vous que le serveur RPG-Arena est lancé")
    except Exception as e:
        print(f"❌ Erreur: {e}")

if __name__ == "__main__":
    print("🎮 Test du client WebSocket RPG-Arena\n")
    asyncio.run(test_websocket())
