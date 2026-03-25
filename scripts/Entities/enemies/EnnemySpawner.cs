using Godot;
using System;
using System.Collections.Generic;

public partial class EnnemySpawner : Marker3D
{
    // Paramètres d'apparition
    [Export] public float InitialSpawnInterval = 15.0f; // Intervalle initial en secondes
    [Export] public float MinimumSpawnInterval = 3.0f; // Intervalle minimum
    [Export] public float SpawnIntervalReduction = 0.5f; // Réduction d'intervalle par ennemi
    [Export] public int MaxEnemiesPerWave = 3; // Nombre maximum d'ennemis par vague
    [Export] public int MaxTotalEnemies = 50; // Limite globale d'ennemis
    
    // Constante pour le facteur de détection (x3)
    private const float DETECTION_RANGE_FACTOR = 3.0f;
    
    // Références aux scènes d'ennemis
    [Export] public PackedScene MeleeEnemyScene;
    [Export] public PackedScene RangedEnemyScene;
    
    // Statistiques de jeu
    private int _enemiesSpawned = 0;
    private float _currentSpawnInterval;
    private Timer _spawnTimer;
    
    // Armes disponibles
    private List<PackedScene> _meleeWeapons = new List<PackedScene>();
    private List<PackedScene> _rangedWeapons = new List<PackedScene>();
    
    // Nœud parent pour les ennemis
    private Node _enemiesContainer;
    
    public override void _Ready()
    {
        // Initialisation du timer
        _spawnTimer = new Timer();
        _spawnTimer.OneShot = false;
        _currentSpawnInterval = InitialSpawnInterval;
        _spawnTimer.WaitTime = _currentSpawnInterval;
        _spawnTimer.Timeout += OnSpawnTimerTimeout;
        
        // Utiliser CallDeferred pour ajouter le timer de manière asynchrone
        CallDeferred(nameof(DeferredAddTimer));
        
        // Créer un conteneur pour les ennemis si nécessaire
        _enemiesContainer = GetTree().Root.GetNodeOrNull("EnemiesContainer");
        if (_enemiesContainer == null)
        {
            _enemiesContainer = new Node3D();
            _enemiesContainer.Name = "EnemiesContainer";
            GetTree().Root.CallDeferred(Node.MethodName.AddChild, _enemiesContainer);
        }
        
        // Charger les scènes d'armes disponibles
        LoadAvailableWeapons();
    }
    
    private void DeferredAddTimer()
    {
        AddChild(_spawnTimer);
        // Démarrer le timer après l'avoir ajouté
        _spawnTimer.Start();
    }
    
    private void LoadAvailableWeapons()
    {
        // Charger les armes de mêlée
        _meleeWeapons.Add(GD.Load<PackedScene>("res://scenes/Weapons/Swords/ExoKatana.tscn"));
        // Ajouter d'autres armes de mêlée si disponibles
        
        // Charger les armes à distance
        _rangedWeapons.Add(GD.Load<PackedScene>("res://scenes/Weapons/Guns/HandGuns/J42.tscn"));
        _rangedWeapons.Add(GD.Load<PackedScene>("res://scenes/Weapons/Guns/HandGuns/MagnumN42.tscn"));
        _rangedWeapons.Add(GD.Load<PackedScene>("res://scenes/Weapons/Guns/HandGuns/MagnumOldSchool.tscn"));
        _rangedWeapons.Add(GD.Load<PackedScene>("res://scenes/Weapons/Guns/HandGuns/TheWisperer.tscn"));
        // Ajouter d'autres armes à distance si disponibles
    }
    
    private void OnSpawnTimerTimeout()
    {
        // Vérifier si on a atteint la limite d'ennemis
        if (_enemiesSpawned >= MaxTotalEnemies)
        {
            _spawnTimer.Stop();
            return;
        }
        
        // Déterminer combien d'ennemis faire apparaître dans cette vague
        int enemiesToSpawn = Math.Min(
            (int)(_enemiesSpawned / 10) + 1, // Augmente le nombre d'ennemis par vague toutes les 10 vagues
            MaxEnemiesPerWave
        );
        
        // Faire apparaître les ennemis
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Rpc(nameof(SpawnRandomEnemy));
        }
        
        // Réduire l'intervalle de temps (augmenter la difficulté)
        _currentSpawnInterval = Math.Max(MinimumSpawnInterval, _currentSpawnInterval - SpawnIntervalReduction);
        _spawnTimer.WaitTime = _currentSpawnInterval;
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer,CallLocal = true)]
    private void SpawnRandomEnemy()
    {
        // Choisir un type d'ennemi aléatoire (corps à corps ou à distance)
        bool spawnMelee = GD.Randf() > 0.5f;
        
        // Instancier l'ennemi
        Enemy enemy;
        
        if (spawnMelee && MeleeEnemyScene != null)
        {
            enemy = MeleeEnemyScene.Instantiate<Enemy>();
            AssignRandomWeapon(enemy, true);
        }
        else if (RangedEnemyScene != null)
        {
            enemy = RangedEnemyScene.Instantiate<Enemy>();
            AssignRandomWeapon(enemy, false);
        }
        else
        {
            GD.PrintErr("Enemy scenes not configured correctly in EnnemySpawner");
            return;
        }
        
        // Appliquer le facteur de détection aux ennemis du spawner (x3)
        // Cette étape est ajoutée pour répondre à l'exigence des ennemis avec une range de détection x3
        enemy.RangeDetectionFactor = DETECTION_RANGE_FACTOR;
        
        // Configurer l'ennemi
        _enemiesContainer.AddChild(enemy);
        enemy.GlobalPosition = GlobalPosition;
        
        // Ajouter un léger décalage aléatoire pour éviter le chevauchement
        enemy.GlobalPosition += new Vector3(
            GD.Randf() * 2.0f - 1.0f,
            0,
            0
        );
        
        // Incrémenter le compteur
        _enemiesSpawned++;
        
        // Émettre un signal ou afficher un message de débogage
        GD.Print($"Spawned enemy {_enemiesSpawned}/{MaxTotalEnemies} - Next spawn in {_currentSpawnInterval} seconds");
    }
    
    private void AssignRandomWeapon(Enemy enemy, bool isMelee)
    {
        // Sélectionner une arme aléatoire du type approprié
        PackedScene weaponScene = null;
        
        if (isMelee && _meleeWeapons.Count > 0)
        {
            weaponScene = _meleeWeapons[GD.RandRange(0, _meleeWeapons.Count - 1)];
        }
        else if (!isMelee && _rangedWeapons.Count > 0)
        {
            weaponScene = _rangedWeapons[GD.RandRange(0, _rangedWeapons.Count - 1)];
        }
        
        // Assigner l'arme à l'ennemi si disponible
        if (weaponScene != null)
        {
            // Nous supposons que l'ennemi a une méthode ou une propriété pour définir son arme
            if (enemy is MeleeEnemy meleeEnemy)
            {
                meleeEnemy._weaponScene = weaponScene;
            }
            // Pour les ennemis à distance
            else if (enemy is RangedEnemy rangedEnemy)
            {
                rangedEnemy._weaponScene = weaponScene;
            }
        }
    }
    
    // Méthode pour démarrer ou arrêter le spawner
    public void SetActive(bool active)
    {
        if (active)
        {
            _spawnTimer.Start();
        }
        else
        {
            _spawnTimer.Stop();
        }
    }
    
    // Méthode pour réinitialiser le spawner
    public void Reset()
    {
        _enemiesSpawned = 0;
        _currentSpawnInterval = InitialSpawnInterval;
        _spawnTimer.WaitTime = _currentSpawnInterval;
        _spawnTimer.Start();
    }
}