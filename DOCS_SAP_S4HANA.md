# Guide d'Intégration SAP S/4 HANA

## 📚 Pour un développeur Rails

Si vous venez de Rails, voici les équivalences :

| Concept SAP | Équivalent Rails |
|-------------|------------------|
| RFC (Remote Function Call) | API REST endpoint |
| BAPI (Business API) | Service object avec validations métier |
| Table SAP | ActiveRecord model |
| Function Module | Controller action |
| ABAP Code | Ruby code côté serveur |
| SAP .NET Connector | Gem HTTP client (comme `faraday`, `httparty`) |

---

## 🔧 Installation SAP .NET Connector (NCo)

### Étape 1: Télécharger NCo

1. Aller sur [SAP Support Portal](https://support.sap.com/en/product/connectors/msnet.html)
2. Se connecter avec compte SAP S-User
3. Télécharger **SAP .NET Connector 3.0** (version 64-bit pour Windows)
4. Version requise : NCo 3.0 ou supérieur pour S/4 HANA

### Étape 2: Installer NCo

```bash
# Exécuter l'installeur
sapnco30setup.exe

# Fichiers installés dans:
C:\Program Files\SAP\SAP_DotNetConnector3_Net40_x64\
  - sapnco.dll
  - sapnco_utils.dll
  - icudt50.dll
  - icuin50.dll
  - icuuc50.dll
```

### Étape 3: Référencer NCo dans le projet

Décommenter dans `PontBascule.csproj` :

```xml
<Reference Include="sapnco">
  <HintPath>C:\Program Files\SAP\SAP_DotNetConnector3_Net40_x64\sapnco.dll</HintPath>
</Reference>
```

---

## 🌐 Configuration SAP S/4 HANA

### appsettings.json

```json
{
  "SAP": {
    "Host": "sap-s4hana.votre-entreprise.com",
    "SystemNumber": "00",
    "Client": "100",
    "Username": "PONT_USER",
    "Password": "VotreMotDePasse",
    "Language": "FR",
    
    // Optionnel pour connexion via routeur SAP
    "Router": "/H/saprouter.company.com/S/3299/H/",
    
    // Pool de connexions
    "PoolSize": "5",
    "MaxPoolSize": "10",
    "IdleTimeout": "600"
  }
}
```

**⚠️ Sécurité:** Ne jamais commiter les credentials ! Utilisez des variables d'environnement ou Azure Key Vault.

---

## 📡 Création de Function Modules RFC dans SAP

### Côté SAP (Transaction SE37)

Créer une fonction RFC personnalisée : `Z_WEIGHBRIDGE_CREATE`

```abap
*"----------------------------------------------------------------------
*" FUNCTION Z_WEIGHBRIDGE_CREATE
*"----------------------------------------------------------------------
*" IMPORTING
*"   IV_TRUCK_NUMBER TYPE CHAR20
*"   IV_WEIGHT TYPE P DECIMALS 2
*"   IV_WEIGHING_TYPE TYPE CHAR10
*"   IV_TRANSPORTER TYPE CHAR40
*"   IV_PRODUCT TYPE CHAR40
*"   IV_TIMESTAMP TYPE DATUM
*" EXPORTING
*"   EV_DOCUMENT_NUMBER TYPE CHAR20
*"   EV_MESSAGE TYPE STRING
*"   EV_SUCCESS TYPE CHAR1
*"----------------------------------------------------------------------

DATA: lv_doc_number TYPE char20.

* Générer numéro de document
CALL FUNCTION 'NUMBER_GET_NEXT'
  EXPORTING
    nr_range_nr = '01'
    object      = 'ZWEIGHT'
  IMPORTING
    number      = lv_doc_number.

* Insérer dans table custom ZWEIGHBRIDGE
INSERT INTO zweighbridge VALUES (
  doc_number = lv_doc_number
  truck = iv_truck_number
  weight = iv_weight
  wtype = iv_weighing_type
  transporter = iv_transporter
  product = iv_product
  datum = iv_timestamp
  status = 'N'  " New
).

IF sy-subrc = 0.
  COMMIT WORK.
  ev_document_number = lv_doc_number.
  ev_success = 'X'.
  ev_message = 'Pesée enregistrée avec succès'.
ELSE.
  ROLLBACK WORK.
  ev_success = ''.
  ev_message = 'Erreur lors de l\''enregistrement'.
ENDIF.

ENDFUNCTION.
```

**Équivalent Rails:**

```ruby
# app/controllers/api/weighings_controller.rb
class Api::WeighingsController < ApplicationController
  def create
    @weighing = Weighing.new(weighing_params)
    
    if @weighing.save
      render json: {
        document_number: @weighing.document_number,
        success: true,
        message: "Pesée enregistrée avec succès"
      }
    else
      render json: {
        success: false,
        message: @weighing.errors.full_messages.join(", ")
      }, status: :unprocessable_entity
    end
  end
end
```

---

## 💻 Utilisation dans C#

### Exemple complet de connexion et appel

```csharp
using SAP.Middleware.Connector;

public class SapConnector
{
    private RfcDestination _destination;
    
    public void Connect()
    {
        // Configuration
        var config = new RfcConfigParameters();
        config.Add(RfcConfigParameters.AppServerHost, "sap-server.com");
        config.Add(RfcConfigParameters.SystemNumber, "00");
        config.Add(RfcConfigParameters.Client, "100");
        config.Add(RfcConfigParameters.User, "USERNAME");
        config.Add(RfcConfigParameters.Password, "PASSWORD");
        config.Add(RfcConfigParameters.Language, "FR");
        
        _destination = RfcDestinationManager.GetDestination(config);
        _destination.Ping(); // Test connexion
    }
    
    public string CreateWeighing(Weighing weighing)
    {
        // Créer l'instance de fonction
        var function = _destination.Repository.CreateFunction("Z_WEIGHBRIDGE_CREATE");
        
        // Paramètres d'entrée
        function.SetValue("IV_TRUCK_NUMBER", weighing.TruckNumber);
        function.SetValue("IV_WEIGHT", weighing.Weight);
        function.SetValue("IV_WEIGHING_TYPE", weighing.WeighingType.ToString());
        function.SetValue("IV_TRANSPORTER", weighing.Transporter);
        function.SetValue("IV_PRODUCT", weighing.Product);
        function.SetValue("IV_TIMESTAMP", weighing.Timestamp.ToString("yyyyMMdd"));
        
        // Exécuter
        function.Invoke(_destination);
        
        // Récupérer les résultats
        var docNumber = function.GetString("EV_DOCUMENT_NUMBER");
        var success = function.GetString("EV_SUCCESS");
        var message = function.GetString("EV_MESSAGE");
        
        if (success != "X")
        {
            throw new Exception($"Erreur SAP: {message}");
        }
        
        return docNumber;
    }
}
```

**Équivalent Rails:**

```ruby
class SapConnector
  def initialize
    @client = Faraday.new(url: ENV['SAP_API_URL']) do |f|
      f.request :json
      f.response :json
      f.adapter Faraday.default_adapter
    end
  end
  
  def create_weighing(weighing)
    response = @client.post('/api/weighings') do |req|
      req.headers['Authorization'] = "Bearer #{ENV['SAP_TOKEN']}"
      req.body = {
        truck_number: weighing.truck_number,
        weight: weighing.weight,
        weighing_type: weighing.weighing_type,
        transporter: weighing.transporter,
        product: weighing.product,
        timestamp: weighing.timestamp
      }
    end
    
    raise "Erreur SAP: #{response.body['message']}" unless response.body['success']
    
    response.body['document_number']
  end
end
```

---

## 🔍 Fonctions SAP Utiles

### 1. Lecture de données camion

```csharp
// RFC: Z_GET_TRUCK_DATA
var function = _destination.Repository.CreateFunction("Z_GET_TRUCK_DATA");
function.SetValue("IV_TRUCK_NUMBER", "AB-123-CD");
function.Invoke(_destination);

var transporter = function.GetString("EV_TRANSPORTER");
var maxWeight = function.GetDecimal("EV_MAX_WEIGHT");
```

### 2. Création d'ordre de fabrication (BAPI standard)

```csharp
var function = _destination.Repository.CreateFunction("BAPI_PRODORD_CREATE");

// Structure ORDERDATA
var orderData = function.GetStructure("ORDERDATA");
orderData.SetValue("MATERIAL", "MAT-001");
orderData.SetValue("PLANT", "1000");
orderData.SetValue("ORDER_TYPE", "PP01");

// Quantité
var quantityData = function.GetStructure("ORDERDATA");
quantityData.SetValue("TARGET_QUANTITY", 1000);
quantityData.SetValue("BASE_UOM", "KG");

function.Invoke(_destination);

var orderNumber = function.GetString("ORDER_NUMBER");
```

### 3. Lecture de tables SAP

```csharp
// Lire table MARA (données articles)
var function = _destination.Repository.CreateFunction("RFC_READ_TABLE");
function.SetValue("QUERY_TABLE", "MARA");
function.SetValue("DELIMITER", "|");
function.SetValue("ROWCOUNT", 100);

// Filtres
var options = function.GetTable("OPTIONS");
options.Append();
options.CurrentRow.SetValue("TEXT", "MATNR = 'MAT-001'");

function.Invoke(_destination);

var data = function.GetTable("DATA");
foreach (IRfcStructure row in data)
{
    var line = row.GetString("WA");
    // Parser le line avec le délimiteur |
}
```

---

## 🚀 Architecture Recommandée

```
┌─────────────────────────────────────────┐
│  Application .NET WPF                    │
│  (Poste de pesée Windows)                │
└──────────────┬──────────────────────────┘
               │
               │ SAP NCo RFC
               │
┌──────────────▼──────────────────────────┐
│  SAP S/4 HANA                            │
│  ┌────────────────────────────────────┐ │
│  │ Function Modules RFC               │ │
│  │ - Z_WEIGHBRIDGE_CREATE             │ │
│  │ - Z_WEIGHBRIDGE_UPDATE             │ │
│  │ - Z_GET_TRUCK_DATA                 │ │
│  └────────────────────────────────────┘ │
│  ┌────────────────────────────────────┐ │
│  │ Tables Custom                      │ │
│  │ - ZWEIGHBRIDGE (pesées)            │ │
│  │ - ZTRUCKS (camions)                │ │
│  └────────────────────────────────────┘ │
│  ┌────────────────────────────────────┐ │
│  │ BAPIs Standard                     │ │
│  │ - BAPI_PRODORD_CREATE              │ │
│  │ - BAPI_MATERIAL_GETALL             │ │
│  └────────────────────────────────────┘ │
└─────────────────────────────────────────┘
```

---

## 🔒 Sécurité & Bonnes Pratiques

### 1. Credentials sécurisés

```csharp
// ❌ JAMAIS faire ça
var password = "MonMotDePasse123";

// ✅ Utiliser variables d'environnement
var password = Environment.GetEnvironmentVariable("SAP_PASSWORD");

// ✅ Ou Azure Key Vault
var client = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential());
var password = await client.GetSecretAsync("SAP-Password");
```

### 2. Gestion des erreurs

```csharp
try
{
    function.Invoke(_destination);
}
catch (RfcCommunicationException ex)
{
    // Problème réseau
    Logger.Error($"Erreur réseau SAP: {ex.Message}");
}
catch (RfcAbapException ex)
{
    // Erreur ABAP côté SAP
    Logger.Error($"Erreur ABAP: {ex.Key} - {ex.Message}");
}
catch (RfcLogonException ex)
{
    // Problème d'authentification
    Logger.Error($"Erreur login SAP: {ex.Message}");
}
```

### 3. Pool de connexions

```csharp
// NCo gère automatiquement un pool de connexions
// Configurer dans RfcConfigParameters:
config.Add(RfcConfigParameters.PoolSize, "5");
config.Add(RfcConfigParameters.MaxPoolSize, "10");
config.Add(RfcConfigParameters.IdleTimeout, "600");
```

---

## 📊 Monitoring & Logs

### Transaction SAP pour surveiller RFC

- **SM59**: Configuration destinations RFC
- **ST22**: Dumps ABAP (erreurs)
- **SM21**: System log
- **ST05**: SQL trace (performance)

---

## 🧪 Tests

### Mock SAP pour développement local

```csharp
public class MockSapService : ISapService
{
    public Task<string> SendWeighingAsync(Weighing weighing)
    {
        // Simule un délai réseau
        await Task.Delay(500);
        
        // Génère un faux doc number
        return $"MOCK{DateTime.Now:yyyyMMddHHmmss}";
    }
}

// Dans App.xaml.cs
#if DEBUG
    services.AddSingleton<ISapService, MockSapService>();
#else
    services.AddSingleton<ISapService, SapS4HanaService>();
#endif
```

---

## 📚 Ressources

- [SAP .NET Connector Documentation](https://help.sap.com/doc/saphelp_nwpi711/7.1.1/en-US/48/d3ccb2e6ed0237e10000000a421937/content.htm)
- [BAPI Explorer (Transaction BAPI)](https://help.sap.com/docs/)
- [SAP Community](https://community.sap.com/)

---

Prochaine étape : Créer les Function Modules dans SAP avec votre consultant SAP/ABAP ! 🚀
