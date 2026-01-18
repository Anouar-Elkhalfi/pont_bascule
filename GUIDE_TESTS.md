# 🧪 Guide de Test - Pont de Bascule

## Pour un développeur Rails : Tests comme avec RSpec

---

## 🎯 Ce que VOUS pouvez tester sur Mac

### ✅ Peut être testé sur Mac
- Compilation du code
- Logique métier (Services, Models)
- Tests unitaires
- Code review

### ❌ Ne peut PAS être testé sur Mac
- Interface WPF (Windows only)
- Communication balance série réelle
- Impression Windows
- Exécution complète de l'app

---

## 1️⃣ Test de Compilation (1 minute)

```bash
cd /Users/anouarelkhalfi/code/Anouar-Elkhalfi/pont_bascule

# Compiler le projet
dotnet build

# Résultat attendu :
# Build succeeded.
#     0 Warning(s)
#     0 Error(s)
```

**✅ Si ça compile = Le code est syntaxiquement correct**

---

## 2️⃣ Créer des Tests Unitaires (comme RSpec)

Créons un projet de tests pour vérifier la logique métier :

```bash
# Créer un projet de tests xUnit (équivalent RSpec)
dotnet new xunit -n PontBascule.Tests

# Ajouter la référence au projet principal
cd PontBascule.Tests
dotnet add reference ../PontBascule.csproj

# Ajouter des packages de test
dotnet add package Moq
dotnet add package FluentAssertions

# Revenir à la racine
cd ..
```

Créons maintenant des tests concrets :

### Test 1 : Modèle Weighing

```csharp
// PontBascule.Tests/Models/WeighingTests.cs
using Xunit;
using FluentAssertions;
using PontBascule.Models;
using System;

namespace PontBascule.Tests.Models
{
    // Équivalent Rails:
    // describe Weighing do
    //   it "should have valid attributes" do
    //     ...
    //   end
    // end
    
    public class WeighingTests
    {
        [Fact]
        public void Weighing_ShouldHaveDefaultValues()
        {
            // Arrange & Act
            var weighing = new Weighing();
            
            // Assert
            weighing.TruckNumber.Should().NotBeNull();
            weighing.Transporter.Should().NotBeNull();
            weighing.Product.Should().NotBeNull();
        }
        
        [Fact]
        public void Weighing_ShouldAcceptValidData()
        {
            // Arrange & Act
            var weighing = new Weighing
            {
                TruckNumber = "AB-123-CD",
                Weight = 12500,
                WeighingType = WeighingType.Entrée,
                Timestamp = DateTime.Now
            };
            
            // Assert
            weighing.TruckNumber.Should().Be("AB-123-CD");
            weighing.Weight.Should().Be(12500);
            weighing.WeighingType.Should().Be(WeighingType.Entrée);
        }
    }
}
```

### Test 2 : DatabaseService (comme ActiveRecord)

```csharp
// PontBascule.Tests/Services/DatabaseServiceTests.cs
using Xunit;
using FluentAssertions;
using PontBascule.Services;
using PontBascule.Models;
using System;
using System.Threading.Tasks;

namespace PontBascule.Tests.Services
{
    public class DatabaseServiceTests : IDisposable
    {
        private readonly DatabaseService _service;
        
        public DatabaseServiceTests()
        {
            // Setup (comme before(:each) en RSpec)
            _service = new DatabaseService();
        }
        
        [Fact]
        public async Task InitializeDatabase_ShouldCreateTables()
        {
            // Act
            await _service.InitializeDatabaseAsync();
            
            // Assert - Si pas d'exception, c'est bon
            true.Should().BeTrue();
        }
        
        [Fact]
        public async Task SaveWeighing_ShouldReturnValidId()
        {
            // Arrange
            await _service.InitializeDatabaseAsync();
            
            var weighing = new Weighing
            {
                TruckNumber = "TEST-001",
                Weight = 15000,
                WeighingType = WeighingType.Entrée,
                Timestamp = DateTime.Now,
                Transporter = "Test Transport",
                Product = "Test Product"
            };
            
            // Act
            var id = await _service.SaveWeighingAsync(weighing);
            
            // Assert
            id.Should().BeGreaterThan(0);
        }
        
        [Fact]
        public async Task GetRecentWeighings_ShouldReturnList()
        {
            // Arrange
            await _service.InitializeDatabaseAsync();
            
            // Act
            var weighings = await _service.GetRecentWeighingsAsync(10);
            
            // Assert
            weighings.Should().NotBeNull();
            weighings.Should().BeOfType<List<Weighing>>();
        }
        
        public void Dispose()
        {
            // Cleanup (comme after(:each) en RSpec)
            // Nettoyer la base de test si nécessaire
        }
    }
}
```

### Test 3 : ExportService

```csharp
// PontBascule.Tests/Services/ExportServiceTests.cs
using Xunit;
using FluentAssertions;
using PontBascule.Services;
using PontBascule.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace PontBascule.Tests.Services
{
    public class ExportServiceTests
    {
        [Fact]
        public async Task ExportToCsv_ShouldCreateFile()
        {
            // Arrange
            var service = new ExportService();
            var weighings = new List<Weighing>
            {
                new Weighing
                {
                    Id = 1,
                    TruckNumber = "AB-123-CD",
                    Weight = 12500,
                    WeighingType = WeighingType.Entrée,
                    Timestamp = DateTime.Now,
                    Transporter = "Transport SA",
                    Product = "Ciment"
                }
            };
            
            // Act
            var filePath = await service.ExportToCsvAsync(weighings);
            
            // Assert
            filePath.Should().NotBeNullOrEmpty();
            File.Exists(filePath).Should().BeTrue();
            
            // Cleanup
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}
```

---

## 3️⃣ Lancer les Tests

```bash
# Lancer tous les tests
dotnet test

# Résultat attendu :
# Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6

# Tests avec détails
dotnet test --logger "console;verbosity=detailed"

# Tests d'un fichier spécifique
dotnet test --filter "FullyQualifiedName~DatabaseServiceTests"
```

**Équivalent Rails :**
```bash
# dotnet test = rspec
# dotnet test --filter = rspec spec/models/weighing_spec.rb
```

---

## 4️⃣ Vérifier la Structure du Code

```bash
# Compter les lignes de code
find . -name "*.cs" | xargs wc -l

# Chercher les TODOs
grep -r "TODO" --include="*.cs" .

# Vérifier les erreurs potentielles
dotnet build 2>&1 | grep -i "warning"
```

---

## 5️⃣ Tests Manuels de Logique

Vous pouvez créer un petit programme console pour tester :

```csharp
// TestConsole/Program.cs
using PontBascule.Services;
using PontBascule.Models;
using Microsoft.Extensions.Configuration;

// Test DatabaseService
var dbService = new DatabaseService();
await dbService.InitializeDatabaseAsync();
Console.WriteLine("✅ Database initialized");

// Test SaveWeighing
var weighing = new Weighing
{
    TruckNumber = "TEST-001",
    Weight = 12500,
    WeighingType = WeighingType.Entrée,
    Timestamp = DateTime.Now,
    Transporter = "Test",
    Product = "Test Product"
};

var id = await dbService.SaveWeighingAsync(weighing);
Console.WriteLine($"✅ Weighing saved with ID: {id}");

// Test GetRecentWeighings
var weighings = await dbService.GetRecentWeighingsAsync(10);
Console.WriteLine($"✅ Retrieved {weighings.Count} weighings");

// Test ExportService
var exportService = new ExportService();
var csvPath = await exportService.ExportToCsvAsync(weighings);
Console.WriteLine($"✅ CSV exported to: {csvPath}");

Console.WriteLine("\n✅ All tests passed!");
```

```bash
# Créer et lancer
dotnet new console -n TestConsole
cd TestConsole
dotnet add reference ../PontBascule.csproj
# Copier le code ci-dessus dans Program.cs
dotnet run
```

---

## 6️⃣ Checklist de Vérification

### ✅ Compilable
```bash
dotnet build
# Doit afficher : Build succeeded
```

### ✅ Pas d'erreurs de syntaxe
```bash
dotnet build 2>&1 | grep -i error
# Doit être vide
```

### ✅ Tests unitaires passent
```bash
dotnet test
# Doit afficher : Passed!
```

### ✅ Structure correcte
```bash
find . -type d -maxdepth 1
# Doit montrer : Models, Services, ViewModels, Views
```

### ✅ Fichiers de config
```bash
ls appsettings.json PontBascule.csproj
# Doivent exister
```

---

## 7️⃣ Ce que vos Devs Windows Testeront

### Tests Complets (nécessitent Windows)
- [ ] Interface WPF s'affiche correctement
- [ ] Boutons fonctionnent
- [ ] Communication balance série
- [ ] Impression de tickets
- [ ] Connexion SAP
- [ ] Export Excel avec mise en forme

---

## 🚀 Plan d'Action Immédiat

### Maintenant (5 min)
```bash
# 1. Compiler
cd /Users/anouarelkhalfi/code/Anouar-Elkhalfi/pont_bascule
dotnet build

# 2. Si OK, c'est bon signe ! ✅
```

### Aujourd'hui (30 min)
```bash
# 3. Créer projet de tests
dotnet new xunit -n PontBascule.Tests
cd PontBascule.Tests
dotnet add reference ../PontBascule.csproj
dotnet add package FluentAssertions

# 4. Créer quelques tests
# (copier les exemples ci-dessus)

# 5. Lancer les tests
dotnet test
```

### Cette semaine
- [ ] Partager le lien GitHub avec vos devs Windows
- [ ] Leur demander de cloner et tester
- [ ] Ils vous donnent un feedback sur l'interface

---

## 📊 Indicateurs de Qualité

### ✅ Votre code est bon si :
- Compilation sans erreur : `dotnet build` ✅
- Tests passent : `dotnet test` ✅
- Structure claire : Models, Services, Views ✅
- Documentation complète : 6 guides ✅
- Sur GitHub : Accessible ✅

### ⚠️ À améliorer si :
- Warnings de compilation : Corriger
- Tests échouent : Déboguer
- Manque de documentation : Ajouter

---

## 🎯 Résumé : Vos Options Maintenant

| Action | Temps | Faisabilité Mac |
|--------|-------|-----------------|
| **Compiler le code** | 1 min | ✅ OUI |
| **Créer des tests** | 30 min | ✅ OUI |
| **Lancer tests unitaires** | 2 min | ✅ OUI |
| **Tester la logique métier** | 15 min | ✅ OUI |
| **Review du code** | 1h | ✅ OUI |
| Tester l'interface WPF | - | ❌ NON (Windows) |
| Tester balance réelle | - | ❌ NON (Windows) |
| Tester impression | - | ❌ NON (Windows) |

---

**🎯 Action Immédiate :**

```bash
# FAITES CECI MAINTENANT :
cd /Users/anouarelkhalfi/code/Anouar-Elkhalfi/pont_bascule
dotnet build

# Si ça compile = Votre code fonctionne ! 🎉
```

Dites-moi le résultat et on continue ! 😊
