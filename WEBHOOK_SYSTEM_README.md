# 🚀 Void Roleplay - Mega Webhook System

## 🎯 Übersicht

Das **komplett neue Void Roleplay Webhook-System** ersetzt das alte, veraltete Discord-Handler System und bietet moderne, professionelle Discord-Integration mit **vollständigem Logging ALLER Server-Aktivitäten**!

## ✨ Features

### 🎨 **Moderne Discord Embeds**
- Türkis als Standard-Farbe für Void Roleplay
- Vollständige Discord Embed-Unterstützung (Felder, Bilder, Footer, etc.)
- Farb-kodierte Event-Typen
- Automatische Discord-Limits-Respektierung

### 🛡️ **Enterprise-Level Features**
- **Rate Limiting** (30 Requests/Minute)
- **Retry-Logic** bei Fehlern (3 Versuche)
- **Asynchrone Verarbeitung** mit Queue-System
- **HTTP Timeouts** und Error Handling
- **Detailliertes Logging** aller Webhook-Aktivitäten

### 📊 **4 Spezialisierte Webhook-Kanäle**
1. **General** - Allgemeine Server-Events
2. **PlayerActions** - Player-spezifische Aktionen  
3. **AdminActions** - Admin & Security Events
4. **System** - Server & System-Benachrichtigungen

### 🔍 **MEGA-LOGGING - WIRKLICH ALLES wird geloggt:**

#### Player-Events:
- ✅ Player Connect/Disconnect (IP, Social Club, Ping, etc.)
- ✅ Alle Chat-Nachrichten (Global, Private, Team, etc.)
- ✅ Player Tode (Killer, Waffe, Position, verdächtige Tode)
- ✅ Level-Ups und Character-Progression
- ✅ VIP-Player Willkommensnachrichten

#### Admin & Security:
- ✅ Alle Admin-Befehle mit Parametern
- ✅ NoClip, Teleport und andere Admin-Tools
- ✅ Anti-Cheat Detections mit Confidence-Level
- ✅ Bans und Kicks mit Grund
- ✅ Verdächtige Aktivitäten

#### Interaktionen:
- ✅ Player Cuffing/Uncuffing
- ✅ Frisking und Durchsuchungen
- ✅ Geld-Transaktionen (mit Überwachung großer Beträge)
- ✅ Item-Trades zwischen Spielern
- ✅ Fahrzeug-Interaktionen (Enter, Exit, Purchase)

#### System & Server:
- ✅ Server-Statistiken (Spieleranzahl, Memory, etc.)
- ✅ Globale Nachrichten und Announcements
- ✅ Database-Aktivitäten und langsame Queries
- ✅ Fahrzeug-Events (Sirenen, etc.)
- ✅ Milestone-Events (50. Spieler online, etc.)

## 📁 Dateistruktur

```
Handler/
├── WebhookConfig.cs          # Konfiguration & URLs
├── DiscordEmbed.cs          # Moderne Embed-Klassen
├── WebhookClient.cs         # HTTP-Client mit Rate Limiting
├── WebhookLogger.cs         # Spezielles Webhook-Logging
├── VoidEventLogger.cs       # MEGA Event-Logger
├── MessageTemplates.cs      # Vorgefertigte Templates
├── VoidWebhookIntegration.cs # Integration Guide
└── DiscordHandler.cs        # Erneuerte Haupt-Klasse
```

## 🚀 Verwendung

### Quick Start - Einfach in bestehende Events einfügen:

```csharp
// Player Connect
VoidWebhookIntegration.PlayerEvents.OnPlayerConnect(player);

// Admin Command
VoidWebhookIntegration.CommandEvents.OnAdminCommand(admin, "ban", new[] { target.Name, reason }, target);

// Money Transaction  
VoidWebhookIntegration.MoneyEvents.OnMoneyGiven(player, 5000, "Payday");

// Custom Event
VoidEventLogger.LogCustomEvent("Custom Title", 
    new Dictionary<string, string> { ["Player"] = player.Name, ["Action"] = "Something" });
```

### Moderne API:

```csharp
// Einfache Nachricht
DiscordHandler.SendMessage("Test Nachricht");

// Embed-Nachricht
DiscordHandler.SendEmbedMessage("Titel", "Beschreibung", WebhookConfig.Colors.Success);

// Admin-Benachrichtigung
DiscordHandler.SendAdminNotification("Wichtiger Event", "Details hier", adminPlayer);

// Mit Message Builder
VoidMessageBuilder.Create()
    .SetTitle("Custom Event")
    .SetEventColor(EventType.Success)
    .AddPlayerInfo(player)
    .AddField("Custom", "Value", true)
    .AddTimestamp()
    .SendTo(WebhookConfig.WebhookChannel.AdminActions);
```

### Vorgefertigte Templates:

```csharp
// Server Start
var startupMessage = MessageTemplates.ServerStartup();
WebhookClient.SendMessageAsync(startupMessage, WebhookConfig.WebhookChannel.System);

// Hacker Report
var hackerReport = MessageTemplates.HackerReport(player, "Aimbot", "Evidence here", 95);
WebhookClient.SendMessageAsync(hackerReport, WebhookConfig.WebhookChannel.AdminActions);

// VIP Welcome
var vipMessage = MessageTemplates.VipWelcome(player, "Gold");
WebhookClient.SendMessageAsync(vipMessage, WebhookConfig.WebhookChannel.PlayerActions);
```

## ⚙️ Konfiguration

### Webhook-URLs (bereits konfiguriert):
- **General**: `https://discord.com/api/webhooks/1423324613844140032/...`
- **PlayerActions**: `https://discord.com/api/webhooks/1423980284495396965/...`
- **AdminActions**: `https://discord.com/api/webhooks/1423980285304897536/...`
- **System**: `https://discord.com/api/webhooks/1423980285522870326/...`

### Rate Limits:
- 30 Requests pro Minute
- 3 Retry-Versuche bei Fehlern
- 10 Sekunden HTTP-Timeout

### Farben:
- **VoidTurquoise** (Standard): `0x40E0D0`
- **Success**: `0x00FF00`
- **Error**: `0xFF0000`
- **Warning**: `0xFFAA00`
- **Admin**: `0x9932CC`

## 🔧 Installation & Setup

1. **System wurde bereits in deiner Main.cs initialisiert:**
   ```csharp
   DiscordHandler.Initialize(); // Startup-Message senden
   ```

2. **Integration ist bereits in folgenden Modulen aktiv:**
   - ✅ Main.cs (Player Connect/Disconnect)
   - ✅ PlayerEventHandler.cs (Money, Cuffing, etc.)
   - ✅ AdminModule.cs (NoClip, Death Events)
   - ✅ Chat/Chats.cs (Global Messages)
   - ✅ VehicleEventHandler.cs (Sirens, etc.)
   - ✅ AntiCheatModule.cs (Cheat Detection, Bans)

3. **Rückwärtskompatibilität:** Alter Code funktioniert weiterhin, nutzt aber das neue System!

## 📈 Monitoring & Statistics

```csharp
// Webhook-Statistiken abrufen
var stats = WebhookClient.GetStats();
Console.WriteLine($"Queue: {stats.QueueSize}, Rate Limit: {stats.AvailableRateLimit}");

// Server-Statistiken senden
DiscordHandler.SendServerStats();
```

## 🎯 Beispiel-Output

Das System generiert wunderschöne, professionelle Discord-Messages:

```
🟢 Player Connected
Spieler: MaxMustermann (SocialClub123)
IP: 127.0.0.1 | Ping: 42ms
Zeit: 12:34:56
Player ID: 123 | Void Roleplay
```

```
🚨 ANTI-CHEAT ALERT
⚠️ Verdächtige Aktivität Erkannt
Spieler: Cheater123
Cheat-Typ: Weapon Cheat
Vertrauen: 95%
Evidence: Spawned weapon: AssaultRifle
```

## 🔥 Highlights

- **100% Rückwärtskompatibilität** - Alter Code funktioniert weiterhin
- **Zero-Downtime Deployment** - Sofort einsatzbereit
- **Professional Discord Integration** - Wie bei großen Servern
- **Vollständige Transparenz** - ALLES wird geloggt
- **Performance-optimiert** - Asynchron, keine Lag-Spikes
- **Fehler-resistent** - Webhook-Probleme crashen den Server nicht

---

**🎮 Void Roleplay - Das neue Level des Server-Monitorings! 🚀**

*Alle 4 Webhooks sind konfiguriert und bereit. Das System startet automatisch mit dem Server und loggt ab sofort ALLES!*