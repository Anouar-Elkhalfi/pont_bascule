# Instructions pour pousser le projet vers GitHub

## 1. Créer le repository sur GitHub
1. Aller sur https://github.com/Anouar-Elkhalfi
2. Cliquer sur "New repository"
3. Nom: `pont_bascule`
4. Description: `Application Windows WPF pour gestion de pont de bascule avec intégration SAP`
5. **NE PAS** initialiser avec README, .gitignore ou licence
6. Créer le repository

## 2. Pousser le code vers GitHub

Exécuter ces commandes depuis votre Mac :

```bash
cd /Users/anouarelkhalfi/code/Anouar-Elkhalfi/pont_bascule

# Ajouter le remote GitHub
git remote add origin https://github.com/Anouar-Elkhalfi/pont_bascule.git

# Pousser le code
git push -u origin master
```

## 3. Si vous préférez SSH

```bash
# Ajouter le remote avec SSH
git remote add origin git@github.com:Anouar-Elkhalfi/pont_bascule.git

# Pousser le code
git push -u origin master
```

## 4. Vérification

Une fois poussé, vérifiez sur https://github.com/Anouar-Elkhalfi/pont_bascule que :
- Tous les fichiers sont présents
- Le README s'affiche correctement
- La structure du projet est bonne

## 5. Cloner sur une machine Windows pour développer

```bash
git clone https://github.com/Anouar-Elkhalfi/pont_bascule.git
cd pont_bascule
dotnet restore
dotnet build
```

---

Le projet est maintenant prêt à être partagé avec votre équipe !
