# Guide Communication Balance Série

## 🔌 Pour un développeur Rails

Si vous venez de Rails, pensez à la communication série comme un **HTTP request/response très basique** via un câble physique.

```ruby
# Rails HTTP
response = HTTParty.get("http://balance.local/weight")
weight = response["weight"]

# Communication Série (concept similaire)
sEcrit(port, "GET_WEIGHT\r\n")  # Envoyer commande
weight = sLit(port, 20)          # Lire réponse
```

---

## 📡 Protocoles Balances Industrielles Courantes

### 1. **Toledo/Mettler Toledo** (Standard de facto)

**Protocole SICS (Standard Interface Command Set)**

```
Commande envoyée : SI\r\n
Réponse reçue    : S S     12345.50 kg\r\n

Légende:
- S S    = Stable (S D = instable)
- 12345.50 = Poids
- kg     = Unité
```

**Implémentation C# :**

```csharp
public async Task<decimal> ReadToledoWeight()
{
    // Envoyer commande "Send Immediately"
    await _serialPort.BaseStream.WriteAsync(Encoding.ASCII.GetBytes("SI\r\n"));
    
    // Attendre stabilisation
    await Task.Delay(500);
    
    // Lire réponse (format: "S S     12345.50 kg")
    var buffer = new byte[50];
    var bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, 0, 50);
    var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
    
    // Parser la réponse
    // Exemple: "S S     12345.50 kg\r\n"
    var parts = response.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    
    if (parts.Length >= 3 && parts[0] == "S" && parts[1] == "S")
    {
        if (decimal.TryParse(parts[2], out var weight))
        {
            return weight;
        }
    }
    
    throw new Exception($"Réponse invalide: {response}");
}
```

---

### 2. **Avery Weigh-Tronix**

**Protocole ZM**

```
Commande: W\r
Réponse : 0012345KG\r\n

00 = Code statut (00 = OK, 01 = Instable, etc.)
12345 = Poids
KG = Unité
```

**Implémentation C# :**

```csharp
public async Task<decimal> ReadAveryWeight()
{
    await _serialPort.BaseStream.WriteAsync(Encoding.ASCII.GetBytes("W\r"));
    await Task.Delay(300);
    
    var buffer = new byte[30];
    var bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, 0, 30);
    var response = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
    
    // Format: "0012345KG"
    if (response.Length >= 7)
    {
        var status = response.Substring(0, 2);
        var weightStr = response.Substring(2, response.Length - 4); // Sans "KG"
        
        if (status == "00" && decimal.TryParse(weightStr, out var weight))
        {
            return weight;
        }
    }
    
    throw new Exception($"Réponse invalide: {response}");
}
```

---

### 3. **Bizerba** (Allemand)

**Protocole K**

```
Commande: K\r\n
Réponse : +0012.450 kg\r\n

+ = Signe
0012.450 = Poids avec décimales
kg = Unité
```

**Implémentation C# :**

```csharp
public async Task<decimal> ReadBizerbaWeight()
{
    await _serialPort.BaseStream.WriteAsync(Encoding.ASCII.GetBytes("K\r\n"));
    await Task.Delay(400);
    
    var buffer = new byte[30];
    var bytesRead = await _serialPort.BaseStream.ReadAsync(buffer, 0, 30);
    var response = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
    
    // Format: "+0012.450 kg"
    var match = System.Text.RegularExpressions.Regex.Match(
        response, 
        @"([+-]?\d+\.?\d*)\s*kg"
    );
    
    if (match.Success && decimal.TryParse(match.Groups[1].Value, out var weight))
    {
        return weight;
    }
    
    throw new Exception($"Réponse invalide: {response}");
}
```

---

### 4. **Sartorius**

**Protocole Sartorius Standard**

```
Commande: P\r\n
Réponse : N       12.45 kg\r\n

N = Net weight
      = Espaces (alignement)
12.45 = Poids
kg = Unité
```

---

### 5. **Mode Continu (Streaming)**

Certaines balances envoient le poids en continu sans commande :

```
Balance envoie automatiquement toutes les 500ms:
12345.50\r\n
12346.00\r\n
12346.00\r\n
...
```

**Implémentation C# :**

```csharp
private void StartContinuousReading()
{
    _serialPort.DataReceived += (sender, e) =>
    {
        try
        {
            var data = _serialPort.ReadLine(); // Lit jusqu'à \r\n
            
            if (decimal.TryParse(data.Trim(), out var weight))
            {
                CurrentWeight = weight;
                WeightChanged?.Invoke(this, weight);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur lecture: {ex.Message}");
        }
    };
}
```

---

## ⚙️ Configuration Ports Série Typiques

### Paramètres Communs

| Paramètre | Valeur Typique | Alternatives |
|-----------|----------------|--------------|
| **Baud Rate** | 9600 | 4800, 19200 |
| **Data Bits** | 8 | 7 |
| **Parity** | None | Even, Odd |
| **Stop Bits** | 1 | 2 |
| **Flow Control** | None | Hardware (RTS/CTS) |

### Tableau par Marque

| Marque | Baud | Data | Parity | Stop |
|--------|------|------|--------|------|
| Toledo | 9600 | 8 | None | 1 |
| Avery | 9600 | 7 | Even | 1 |
| Bizerba | 9600 | 8 | None | 1 |
| Sartorius | 9600 | 8 | None | 1 |

---

## 🔧 Implémentation Complète dans ScaleService.cs

Voici comment adapter `ScaleService.cs` pour supporter plusieurs protocoles :

```csharp
public enum ScaleProtocol
{
    Toledo_SICS,
    Avery_ZM,
    Bizerba_K,
    Sartorius,
    Continuous
}

public class ScaleService : IScaleService
{
    private SerialPort? _serialPort;
    private readonly ScaleConfiguration _config;
    private readonly ScaleProtocol _protocol;

    public ScaleService(IConfiguration configuration)
    {
        _config = configuration.GetSection("Scale").Get<ScaleConfiguration>() 
                  ?? new ScaleConfiguration();
        
        // Lire le protocole depuis config
        var protocolName = configuration.GetSection("Scale:Protocol").Value ?? "Toledo_SICS";
        _protocol = Enum.Parse<ScaleProtocol>(protocolName);
    }

    public async Task<decimal> ReadWeightAsync()
    {
        if (!IsConnected)
            throw new InvalidOperationException("Balance non connectée");

        return _protocol switch
        {
            ScaleProtocol.Toledo_SICS => await ReadToledoWeight(),
            ScaleProtocol.Avery_ZM => await ReadAveryWeight(),
            ScaleProtocol.Bizerba_K => await ReadBizerbaWeight(),
            ScaleProtocol.Sartorius => await ReadSartoriusWeight(),
            ScaleProtocol.Continuous => CurrentWeight, // Mode streaming
            _ => throw new NotSupportedException($"Protocole {_protocol} non supporté")
        };
    }

    private async Task<decimal> ReadToledoWeight()
    {
        // Vider le buffer
        _serialPort!.DiscardInBuffer();
        
        // Envoyer "Send Immediately"
        await _serialPort.BaseStream.WriteAsync(Encoding.ASCII.GetBytes("SI\r\n"));
        
        // Attendre réponse
        await Task.Delay(500);
        
        // Lire
        var response = _serialPort.ReadLine();
        
        // Parser "S S     12345.50 kg"
        var match = Regex.Match(response, @"S\s+S\s+([\d.]+)\s*kg");
        
        if (match.Success && decimal.TryParse(match.Groups[1].Value, out var weight))
        {
            return weight;
        }
        
        throw new Exception($"Réponse Toledo invalide: {response}");
    }

    // Autres méthodes ReadAveryWeight(), ReadBizerbaWeight(), etc.
}
```

---

## 📝 Configuration appsettings.json

```json
{
  "Scale": {
    "PortName": "COM1",
    "BaudRate": 9600,
    "DataBits": 8,
    "Parity": "None",
    "StopBits": "One",
    "ReadTimeout": 1000,
    
    "Protocol": "Toledo_SICS",
    
    "Manufacturer": "Toledo",
    "Model": "IND780",
    "MaxCapacity": 60000,
    "Division": 20
  }
}
```

---

## 🧪 Tester Sans Balance Physique

### Option 1: Simulateur de Port Série

**Com0Com (Windows):**
```bash
# Installer com0com
# Crée des paires de ports virtuels: COM10 <-> COM11

# App lit sur COM10
# Vous envoyez depuis un terminal série sur COM11
```

### Option 2: Simulation dans le code

```csharp
#if DEBUG
public async Task<decimal> ReadWeightAsync()
{
    // En mode dev, simuler un poids aléatoire
    await Task.Delay(200);
    var random = new Random();
    return random.Next(5000, 45000); // Entre 5 et 45 tonnes
}
#else
public async Task<decimal> ReadWeightAsync()
{
    // En production, vraie lecture série
    return await ReadToledoWeight();
}
#endif
```

---

## 🔍 Débogage Communication Série

### Outils Windows

**1. Realterm (gratuit)**
- Télécharger: https://realterm.sourceforge.io/
- Permet de voir en temps réel ce qui transite sur le port
- Envoyer des commandes manuellement

**2. Termite (gratuit)**
- Plus simple que Realterm
- Interface claire

### Diagnostic Commun

| Problème | Solution |
|----------|----------|
| Port non trouvé | Vérifier Gestionnaire de périphériques |
| Timeout | Augmenter ReadTimeout dans config |
| Caractères bizarres | Mauvais Baud Rate |
| Pas de réponse | Mauvaise commande ou protocole |
| Poids erratique | Instabilité mécanique balance |

---

## 📚 Documentation Constructeurs

- **Toledo**: https://www.mt.com/fr/fr/home/supportive_content/maternity_software_documentation.html
- **Avery**: https://www.averyweigh-tronix.com/en-us/support/
- **Bizerba**: https://www.bizerba.com/en-global/support/manuals
- **Sartorius**: https://www.sartorius.com/en/products/weighing

---

## ✅ Checklist Mise en Service

- [ ] Identifier la marque/modèle de votre balance
- [ ] Trouver le manuel technique
- [ ] Noter le protocole utilisé (SICS, ZM, etc.)
- [ ] Configurer les paramètres série (baud, parity, etc.)
- [ ] Tester avec un terminal série (Realterm)
- [ ] Implémenter le protocole dans ScaleService.cs
- [ ] Tester en conditions réelles

---

Besoin d'aide pour votre balance spécifique ? Donnez-moi la marque et le modèle ! 🔧
