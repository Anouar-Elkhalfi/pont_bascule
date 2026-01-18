# Guide de Développement - Pont de Bascule

## 🎯 Pour un développeur Rails

Ce guide vous aide à développer l'application en comprenant les parallèles avec Rails.

---

## 📁 Structure du Projet (équivalences Rails)

```
pont_bascule/
├── Models/                    ← app/models/
│   ├── Weighing.cs           ← weighing.rb
│   ├── ScaleConfiguration.cs  
│   └── SapConfiguration.cs   
│
├── Services/                  ← app/services/
│   ├── ScaleService.cs       ← Communication hardware
│   ├── SapService.cs         ← Intégration SAP
│   ├── DatabaseService.cs    ← ActiveRecord-like
│   └── PrintService.cs       ← Impression tickets
│
├── ViewModels/                ← app/controllers/ + présentation
│   └── MainViewModel.cs      ← Logique + données UI
│
├── Views/                     ← app/views/
│   └── MainWindow.xaml       ← Interface graphique
│
├── App.xaml.cs               ← config/application.rb
├── appsettings.json          ← config/database.yml + secrets
└── PontBascule.csproj        ← Gemfile
```

---

## 🚀 Workflow de Développement

### 1. Sur Mac (Vous - Logique Métier)

```bash
# Ouvrir le projet dans VS Code
code /Users/anouarelkhalfi/code/Anouar-Elkhalfi/pont_bascule

# Éditer les Models et Services
# Ces fichiers ne dépendent pas de Windows

# Tester la compilation
dotnet build

# Commiter
git add .
git commit -m "Amélioration logique métier"
git push
```

**Vous pouvez éditer :**
- ✅ Models/*.cs
- ✅ Services/*.cs (sauf ScaleService sur Mac)
- ✅ Logique business
- ❌ Views/*.xaml (WPF Windows only)
- ❌ ViewModels/*.cs (dépend de WPF)

---

### 2. Sur Windows (Vos développeurs - UI + Tests)

```bash
# Cloner ou pull
git clone https://github.com/Anouar-Elkhalfi/pont_bascule.git
# ou
git pull

# Ouvrir dans Visual Studio 2022
# Double-cliquer sur PontBascule.csproj

# Compiler
dotnet build

# Exécuter
dotnet run
# ou F5 dans Visual Studio

# Tester avec vraie balance
# Modifier l'interface
# Commiter et push
```

---

## 💻 Commandes Essentielles

### Compilation

```bash
# Compiler le projet
dotnet build

# Compiler en mode Release
dotnet build -c Release

# Nettoyer
dotnet clean
```

### Exécution

```bash
# Lancer l'application
dotnet run

# Lancer en mode Debug
dotnet run --configuration Debug
```

### Publication (Créer l'EXE)

```bash
# Publier pour Windows 64-bit (standalone)
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# Le .exe sera dans:
# bin/Release/net8.0-windows/win-x64/publish/PontBascule.exe
```

---

## 🔧 Développement par Fonctionnalité

### Ajouter un nouveau modèle

**Parallèle Rails :** `rails generate model Transporter`

```csharp
// Models/Transporter.cs
namespace PontBascule.Models
{
    public class Transporter
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
    }
}
```

**Créer la table :**

```csharp
// Dans DatabaseService.cs -> InitializeDatabaseAsync()
createTableCommand.CommandText += @"
    CREATE TABLE IF NOT EXISTS Transporters (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Name TEXT NOT NULL,
        Address TEXT,
        Phone TEXT,
        Active INTEGER NOT NULL DEFAULT 1
    );
";
```

---

### Ajouter un nouveau service

**Parallèle Rails :** Créer `app/services/email_service.rb`

```csharp
// Services/IEmailService.cs
namespace PontBascule.Services
{
    public interface IEmailService
    {
        Task SendWeighingReportAsync(string recipient, List<Weighing> weighings);
    }
}

// Services/EmailService.cs
using System.Net.Mail;

namespace PontBascule.Services
{
    public class EmailService : IEmailService
    {
        public async Task SendWeighingReportAsync(string recipient, List<Weighing> weighings)
        {
            var client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("user", "password");
            
            var message = new MailMessage();
            message.From = new MailAddress("noreply@pontbascule.com");
            message.To.Add(recipient);
            message.Subject = "Rapport de pesées";
            message.Body = GenerateReport(weighings);
            
            await client.SendMailAsync(message);
        }
        
        private string GenerateReport(List<Weighing> weighings)
        {
            // Générer le contenu du rapport
            return $"Total pesées: {weighings.Count}";
        }
    }
}
```

**Enregistrer le service :**

```csharp
// Dans App.xaml.cs -> ConfigureServices()
services.AddSingleton<IEmailService, EmailService>();
```

---

### Ajouter une commande dans le ViewModel

**Parallèle Rails :** Ajouter une action dans le controller

```csharp
// Dans MainViewModel.cs

// 1. Ajouter une propriété observable si nécessaire
[ObservableProperty]
private bool _isExporting = false;

// 2. Créer la commande
[RelayCommand]
private async Task ExportToExcel()
{
    IsExporting = true;
    StatusMessage = "Export en cours...";
    
    try
    {
        var weighings = await _databaseService.GetRecentWeighingsAsync(100);
        
        // TODO: Implémenter export Excel
        // Utiliser EPPlus ou ClosedXML
        
        StatusMessage = "✓ Export Excel terminé";
    }
    catch (Exception ex)
    {
        StatusMessage = $"❌ Erreur export: {ex.Message}";
    }
    finally
    {
        IsExporting = false;
    }
}
```

**Lier au bouton dans la vue :**

```xml
<!-- Dans MainWindow.xaml -->
<Button Content="EXPORT EXCEL" 
        Command="{Binding ExportToExcelCommand}"
        IsEnabled="{Binding IsExporting, Converter={StaticResource InverseBoolConverter}}"
        Width="150" Height="45"/>
```

---

## 🧪 Tests (comme RSpec en Rails)

### Créer un projet de tests

```bash
# Créer un projet de tests xUnit
dotnet new xunit -n PontBascule.Tests

# Ajouter la référence au projet principal
cd PontBascule.Tests
dotnet add reference ../PontBascule.csproj

# Ajouter des packages de test
dotnet add package Moq
dotnet add package FluentAssertions
```

### Exemple de test

```csharp
// PontBascule.Tests/Services/DatabaseServiceTests.cs
using Xunit;
using FluentAssertions;
using PontBascule.Services;
using PontBascule.Models;

namespace PontBascule.Tests.Services
{
    public class DatabaseServiceTests
    {
        [Fact]
        public async Task SaveWeighing_ShouldReturnId()
        {
            // Arrange (équivalent Rails: let/before)
            var service = new DatabaseService();
            await service.InitializeDatabaseAsync();
            
            var weighing = new Weighing
            {
                TruckNumber = "AB-123-CD",
                Weight = 12500,
                WeighingType = WeighingType.Entrée,
                Timestamp = DateTime.Now
            };
            
            // Act (équivalent Rails: l'action testée)
            var id = await service.SaveWeighingAsync(weighing);
            
            // Assert (équivalent Rails: expect)
            id.Should().BeGreaterThan(0);
        }
        
        [Fact]
        public async Task GetRecentWeighings_ShouldReturnList()
        {
            // Arrange
            var service = new DatabaseService();
            await service.InitializeDatabaseAsync();
            
            // Act
            var weighings = await service.GetRecentWeighingsAsync(10);
            
            // Assert
            weighings.Should().NotBeNull();
            weighings.Should().HaveCountLessOrEqualTo(10);
        }
    }
}
```

**Lancer les tests :**

```bash
dotnet test
```

---

## 🐛 Debugging

### Dans Visual Studio Code (Mac)

```json
// .vscode/launch.json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/bin/Debug/net8.0-windows/PontBascule.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "console": "internalConsole",
            "stopAtEntry": false
        }
    ]
}
```

**Breakpoints :** Cliquez à gauche d'une ligne (comme Pry en Rails)

### Dans Visual Studio (Windows)

- **F5** : Lancer en mode debug
- **F9** : Ajouter/retirer breakpoint
- **F10** : Step over
- **F11** : Step into
- **Shift+F5** : Stop debug

---

## 📦 Packages NuGet Utiles

### Installation

```bash
# Export Excel
dotnet add package EPPlus

# Logging
dotnet add package Serilog
dotnet add package Serilog.Sinks.File

# PDF
dotnet add package QuestPDF

# HTTP/REST pour SAP
dotnet add package RestSharp

# Validation
dotnet add package FluentValidation
```

### Exemple : Export Excel

```csharp
using OfficeOpenXml;

public async Task ExportToExcelAsync(List<Weighing> weighings, string filePath)
{
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    
    using var package = new ExcelPackage();
    var worksheet = package.Workbook.Worksheets.Add("Pesées");
    
    // En-têtes
    worksheet.Cells[1, 1].Value = "Date/Heure";
    worksheet.Cells[1, 2].Value = "N° Camion";
    worksheet.Cells[1, 3].Value = "Poids (kg)";
    worksheet.Cells[1, 4].Value = "Type";
    
    // Données
    int row = 2;
    foreach (var w in weighings)
    {
        worksheet.Cells[row, 1].Value = w.Timestamp.ToString("dd/MM/yyyy HH:mm");
        worksheet.Cells[row, 2].Value = w.TruckNumber;
        worksheet.Cells[row, 3].Value = w.Weight;
        worksheet.Cells[row, 4].Value = w.WeighingType.ToString();
        row++;
    }
    
    await package.SaveAsAsync(new FileInfo(filePath));
}
```

---

## 🔐 Configuration Sécurisée

### Ne jamais commiter les secrets !

```json
// appsettings.json (commité sur Git)
{
  "SAP": {
    "Host": "sap-server.com",
    "SystemNumber": "00",
    "Client": "100",
    "Username": "",  // VIDE
    "Password": "",  // VIDE
    "Language": "FR"
  }
}
```

```json
// appsettings.Production.json (jamais commité, dans .gitignore)
{
  "SAP": {
    "Username": "PROD_USER",
    "Password": "RealPassword123!"
  }
}
```

### Utiliser variables d'environnement

```csharp
// Lire depuis l'environnement
var sapPassword = Environment.GetEnvironmentVariable("SAP_PASSWORD") 
                  ?? _config.Password;
```

---

## 🚀 Déploiement

### Créer l'installateur

```bash
# 1. Publier en un seul fichier
dotnet publish -c Release -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true

# Résultat : PontBascule.exe (standalone ~80MB)
```

### Avec Inno Setup (créer un vrai installateur)

```bash
# Installer Inno Setup: https://jrsoftware.org/isdl.php

# Créer setup.iss
[Setup]
AppName=Pont de Bascule
AppVersion=1.0
DefaultDirName={pf}\PontBascule
OutputDir=.\Setup
OutputBaseFilename=PontBasculeInstaller

[Files]
Source: "bin\Release\net8.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{commondesktop}\Pont de Bascule"; Filename: "{app}\PontBascule.exe"
```

---

## 📚 Resources Utiles

### Documentation
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [WPF Tutorial](https://wpf-tutorial.com/)
- [C# Guide](https://docs.microsoft.com/dotnet/csharp/)

### Communautés
- Stack Overflow: [c#] [wpf]
- Reddit: r/csharp, r/dotnet
- Discord: .NET Community

### Vidéos (YouTube)
- "WPF Tutorial for Beginners"
- "C# MVVM Pattern Explained"
- "Building Desktop Apps with .NET"

---

## 🎓 Prochaines Étapes

1. **Cette semaine :**
   - Comprendre la structure Models/Services/ViewModels
   - Modifier appsettings.json avec vos paramètres
   - Tester la compilation : `dotnet build`

2. **Semaine prochaine :**
   - Connecter une vraie balance série
   - Implémenter le protocole spécifique de votre balance
   - Tester l'impression de tickets

3. **Mois prochain :**
   - Installer SAP NCo
   - Créer les Function Modules SAP
   - Tester l'intégration SAP

---

Besoin d'aide ? Posez vos questions ! 🚀
