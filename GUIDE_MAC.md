# 🍎 Guide Développement sur Mac

## ⚠️ Limitation Importante

**WPF (Windows Presentation Foundation) ne fonctionne QUE sur Windows.**

Sur Mac, vous ne pouvez PAS :
- ❌ Exécuter l'application complète (`dotnet run`)
- ❌ Compiler les fichiers Views/ (XAML)
- ❌ Tester l'interface graphique

Sur Mac, vous POUVEZ :
- ✅ Éditer la logique métier (Models, Services)
- ✅ Créer et lancer des tests unitaires
- ✅ Versionner et pousser sur Git
- ✅ Faire du code review

---

## 🎯 Votre Rôle sur Mac

En tant que développeur Rails sur Mac, vous êtes **le chef d'orchestre** :

1. **Architecture & Design** - Définir la structure
2. **Logique Métier** - Coder Services et Models
3. **Tests** - Écrire les tests unitaires
4. **Git** - Gérer les versions
5. **Code Review** - Valider le code des devs Windows

Vos développeurs Windows :
- Testent l'interface
- Connectent le matériel (balance)
- Compilent et déploient

---

## 💻 Workflow Recommandé

### Option 1: Tests Unitaires (Recommandé)

C'est ce que vous devriez faire sur Mac :

```bash
cd /Users/anouarelkhalfi/code/Anouar-Elkhalfi/pont_bascule

# Créer un projet de tests
dotnet new xunit -n PontBascule.Tests

# Structure :
# pont_bascule/
# ├── PontBascule.csproj          ← Projet principal (WPF, ne compile pas sur Mac)
# └── PontBascule.Tests/          ← Tests (compile sur Mac !)
#     ├── PontBascule.Tests.csproj
#     ├── Models/
#     │   └── WeighingTests.cs
#     └── Services/
#         ├── DatabaseServiceTests.cs
#         └── ExportServiceTests.cs

# Ajouter la référence
cd PontBascule.Tests
dotnet add reference ../PontBascule.csproj

# Ajouter packages
dotnet add package FluentAssertions
dotnet add package Moq

# Lancer les tests (ça marche sur Mac !)
dotnet test
```

---

### Option 2: Créer une Library séparée

Extraire la logique métier dans une library .NET Standard qui compile sur Mac :

```bash
# Créer une library cross-platform
dotnet new classlib -n PontBascule.Core

# Structure :
# pont_bascule/
# ├── PontBascule.csproj          ← WPF (Windows only)
# ├── PontBascule.Core/           ← Logic (Mac OK !)
# │   ├── Models/
# │   ├── Services/
# │   └── PontBascule.Core.csproj
# └── PontBascule.Tests/          ← Tests (Mac OK !)

# Le projet WPF référence la library :
cd ..
dotnet add PontBascule.csproj reference PontBascule.Core/PontBascule.Core.csproj

# Sur Mac, vous travaillez dans PontBascule.Core/
# Sur Windows, ils travaillent dans PontBascule/
```

---

### Option 3: Utiliser VS Code avec Devcontainer (Avancé)

Créer un environnement Windows virtuel dans Docker :

```bash
# .devcontainer/devcontainer.json
{
  "name": "Pont Bascule Dev",
  "image": "mcr.microsoft.com/devcontainers/dotnet:8.0-windowsservercore",
  "features": {
    "ghcr.io/devcontainers/features/git:1": {}
  }
}
```

---

## 🚀 Actions Concrètes MAINTENANT

### 1. Créer les tests (5 min)

```bash
cd /Users/anouarelkhalfi/code/Anouar-Elkhalfi/pont_bascule

# Créer projet de tests
dotnet new xunit -n PontBascule.Tests
cd PontBascule.Tests

# Configurer pour ignorer WPF
cat > PontBascule.Tests.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
    <PackageReference Include="xunit" Version="2.6.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.4" />
    <PackageReference Include="FluentAssertions" Version="6.12.0" />
    <PackageReference Include="Moq" Version="4.20.70" />
  </ItemGroup>

  <!-- Référencer UNIQUEMENT les fichiers non-WPF du projet principal -->
  <ItemGroup>
    <Compile Include="../Models/**/*.cs" LinkBase="Models" />
    <Compile Include="../Services/**/*.cs" LinkBase="Services" />
  </ItemGroup>

</Project>
EOF

# Créer un test simple
mkdir -p Models
cat > Models/WeighingTests.cs << 'EOF'
using Xunit;
using FluentAssertions;
using PontBascule.Models;

namespace PontBascule.Tests.Models
{
    public class WeighingTests
    {
        [Fact]
        public void Weighing_ShouldHaveDefaultValues()
        {
            var weighing = new Weighing();
            
            weighing.TruckNumber.Should().NotBeNull();
            weighing.Weight.Should().Be(0);
        }
        
        [Fact]
        public void Weighing_CanSetProperties()
        {
            var weighing = new Weighing
            {
                TruckNumber = "AB-123-CD",
                Weight = 12500
            };
            
            weighing.TruckNumber.Should().Be("AB-123-CD");
            weighing.Weight.Should().Be(12500);
        }
    }
}
EOF

# Lancer les tests
dotnet restore
dotnet test

# Résultat attendu : Tests passed! ✅
```

---

### 2. Pousser sur GitHub

```bash
cd /Users/anouarelkhalfi/code/Anouar-Elkhalfi/pont_bascule

git add .
git commit -m "Ajout tests unitaires + guide Mac"
git push
```

---

### 3. Documenter pour votre équipe

Créer un README pour vos devs :

```markdown
# 👥 Organisation de l'Équipe

## Anouar (Mac) - Chef de Projet / Architecte
- Architecture globale
- Logique métier (Models, Services)
- Tests unitaires
- Code review
- Git management

## Dev Windows 1 - Interface & Tests
- Interface WPF
- Tests d'intégration
- Compilation finale
- Déploiement

## Dev Windows 2 - Hardware & SAP
- Intégration balance
- Connexion SAP
- Tests matériels
```

---

## 📊 Statut Actuel

| Composant | Peut Compiler sur Mac | Peut Exécuter sur Mac |
|-----------|----------------------|----------------------|
| Models/ | ✅ OUI* | ✅ OUI* |
| Services/ | ✅ OUI* | ⚠️ Partiel** |
| ViewModels/ | ❌ NON | ❌ NON |
| Views/ | ❌ NON | ❌ NON |
| Tests | ✅ OUI | ✅ OUI |

\* Via tests unitaires ou library séparée  
** ScaleService ne peut pas accéder aux ports série COM sur Mac

---

## 🎯 Prochaines Étapes

1. **Aujourd'hui** : Créer les tests unitaires
2. **Cette semaine** : Éditer Models et Services
3. **Semaine prochaine** : Partager avec devs Windows pour tests complets

---

## 💡 Alternative : GitHub Codespaces

Si vous voulez VRAIMENT compiler sur Mac, utilisez GitHub Codespaces avec Windows :

1. Aller sur github.com/Anouar-Elkhalfi/pont_bascule
2. Code → Codespaces → Create codespace
3. Environnement cloud avec .NET

Mais honnêtement, **les tests unitaires suffisent** pour votre rôle ! 

---

**Bottom line :** Vous êtes l'architecte, pas le testeur Windows. Laissez les devs Windows compiler l'app complète. Vous, concentrez-vous sur la qualité de la logique métier via les tests ! 🎯
