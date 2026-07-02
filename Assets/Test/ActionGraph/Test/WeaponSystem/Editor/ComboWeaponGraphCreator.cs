// using System;
// using System.IO;
// using System.Reflection;
// using SAS.StateMachineCharacterController;
// using SAS.WeaponSystem;
// using SAS.WeaponSystem.Components;
// using UnityEditor;
// using UnityEngine;
//
// namespace SAS.ActionGraph.WeaponSystem
// {
// public static class ComboWeaponGraphCreator
// {
//     private const string FallbackSampleFolder = "Assets/Test/ActionGraph/Test/WeaponSystem";
//     private const float ColumnWidth = 380f;
//     private const float AttackLaneHeight = 560f;
//
//     [MenuItem("Assets/Create/Action Graph/Sword Combo Graph From Weapon Data", false, 82)]
//     public static void CreateSwordComboGraph()
//     {
//         CreateSwordComboGraphFromSelection();
//     }
//
//     [MenuItem("Tools/Action Graph/Create Sword Combo Graph From Selected Weapon Data", false, 130)]
//     public static void CreateSwordComboGraphFromToolsMenu()
//     {
//         CreateSwordComboGraphFromSelection();
//     }
//
//     [MenuItem("Assets/Action Graph/Create Sword Combo Graph From Selected Weapon Data", false, 1201)]
//     public static void CreateSwordComboGraphFromAssetMenu()
//     {
//         CreateSwordComboGraphFromSelection();
//     }
//
//     private static void CreateSwordComboGraphFromSelection()
//     {
//         var weaponData = Selection.activeObject as WeaponDataSO;
//         if (weaponData == null)
//         {
//             EditorUtility.DisplayDialog(
//                 "Action Graph",
//                 "Select a WeaponDataSO asset first. It is only used once to bake values into graph nodes.",
//                 "OK");
//             return;
//         }
//
//         int attackCount = Mathf.Max(1, weaponData.NumberOfAttacks);
//
//         var config = ScriptableObject.CreateInstance<ActionGraphAsset>();
//         config.root = CreateRoot(weaponData, attackCount);
//
//         string folder = FindSampleFolder();
//         EnsureFolder(folder);
//
//         string graphName = weaponData != null && !string.IsNullOrEmpty(weaponData.Name)
//             ? $"{weaponData.Name} Action Graph.asset"
//             : "Sword Combo Action Graph.asset";
//
//         string path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{graphName}");
//         AssetDatabase.CreateAsset(config, path);
//         AssetDatabase.SaveAssets();
//         AssetDatabase.Refresh();
//
//         Selection.activeObject = config;
//         EditorGUIUtility.PingObject(config);
//         global::ActionGraphWindow.OpenWithConfig(config);
//     }
//
//     [MenuItem("Assets/Create/Action Graph/Sword Combo Graph From Weapon Data", true)]
//     private static bool CanCreateSwordComboGraph()
//     {
//         return true;
//     }
//
//     [MenuItem("Assets/Action Graph/Create Sword Combo Graph From Selected Weapon Data", true)]
//     private static bool CanCreateSwordComboGraphFromAssetMenu()
//     {
//         return Selection.activeObject is WeaponDataSO;
//     }
//
//     private static NodeConfig CreateRoot(WeaponDataSO weaponData, int attackCount)
//     {
//         var root = new FlowNodeConfig
//         {
//             type = FlowNodeType.Sequence,
//             editorPosition = Position(0, 0)
//         };
//
//         root.children.Add(CreateSetupNode(weaponData));
//         root.children.Add(CreateComboLoop(weaponData, attackCount));
//         root.children.Add(ResetNode(13, 0));
//         return root;
//     }
//
//     private static NodeConfig CreateComboLoop(WeaponDataSO weaponData, int attackCount)
//     {
//         return new LoopNodeConfig
//         {
//             maxIterations = attackCount,
//             conditionTiming = LoopConditionTiming.AfterChild,
//             condition = new ComboInputAcceptedCondition { expected = true },
//             child = CreateAttackStep(weaponData, attackCount),
//             editorPosition = Position(2, 0)
//         };
//     }
//
//     private static NodeConfig CreateAttackStep(WeaponDataSO weaponData, int attackCount)
//     {
//         var sequence = new FlowNodeConfig
//         {
//             type = FlowNodeType.Sequence,
//             editorPosition = Position(3, 0)
//         };
//
//         sequence.children.Add(Action(
//             Provider(new ComboBeginCurrentAttackProvider(), true,
//                 new ComboBeginCurrentAttackData()),
//             4,
//             0));
//
//         sequence.children.Add(Action(
//             Provider(new WeaponIndexedAnimationProvider(), true,
//                 new WeaponIndexedAnimationData
//                 {
//                     statePrefix = "Attack",
//                     layer = 0,
//                     crossFade = false,
//                     normalizedStartTime = 0f
//                 }),
//             5,
//             0));
//
//         sequence.children.Add(CreateAttackBody(weaponData, attackCount));
//
//         sequence.children.Add(Action(
//             Provider(new ComboAdvanceIfInputAcceptedProvider(), true,
//                 new ComboAdvanceIfInputAcceptedData { comboCount = attackCount }),
//             12,
//             0));
//
//         return sequence;
//     }
//
//     private static NodeConfig CreateAttackBody(WeaponDataSO weaponData, int attackCount)
//     {
//         var parallel = new FlowNodeConfig
//         {
//             type = FlowNodeType.Parallel,
//             editorPosition = Position(6, 0)
//         };
//
//         parallel.children.Add(Action(
//             Provider(new WeaponForwardMovementProvider(), false,
//                 CreateMovementDataArray(weaponData, attackCount)),
//             7,
//             0,
//             -180f));
//
//         parallel.children.Add(CreateHitSequence(weaponData, attackCount));
//
//         parallel.children.Add(Action(
//             Provider(new ComboWaitInputProvider(), false,
//                 CreateComboInputDataArray(weaponData, attackCount)),
//             7,
//             0,
//             180f));
//
//         return parallel;
//     }
//
//     private static NodeConfig CreateHitSequence(WeaponDataSO weaponData, int attackCount)
//     {
//         var sequence = new FlowNodeConfig
//         {
//             type = FlowNodeType.Sequence,
//             editorPosition = Position(7, 0)
//         };
//
//         sequence.children.Add(Action(
//             Provider(new WeaponTimedHitBoxProvider(), false,
//                 CreateHitBoxDataArray(weaponData, attackCount)),
//             8,
//             0));
//
//         sequence.children.Add(Action(
//             Provider(new WeaponApplyDamageToHitsProvider(), false,
//                 CreateDamageDataArray(weaponData, attackCount)),
//             9,
//             0));
//
//         sequence.children.Add(Action(
//             Provider(new WeaponTriggerHitEffectProvider(), true,
//                 CreateEffectData(weaponData)),
//             10,
//             0));
//
//         sequence.children.Add(Action(
//             Provider(new WeaponApplyKnockbackToHitsProvider(), false,
//                 CreateKnockbackDataArray(weaponData, attackCount)),
//             11,
//             0));
//
//         return sequence;
//     }
//
//     private static ActionNodeConfig CreateSetupNode(WeaponDataSO weaponData)
//     {
//         var setup = weaponData.GetData<SwordWeaponSetupComponenetData>();
//         return Action(
//             Provider(new WeaponAttachModelsProvider(), true,
//                 new WeaponAttachModelsData
//                 {
//                     leftSocketPath = setup != null ? setup.LeftSocketPath : string.Empty,
//                     rightSocketPath = setup != null ? setup.RightSocketPath : string.Empty,
//                     leftWeapon = setup != null ? setup.LeftWeapon : null,
//                     rightWeapon = setup != null ? setup.RightWeapon : null
//                 }),
//             1,
//             0,
//             -260f);
//     }
//
//     private static WeaponForwardMovementData[] CreateMovementDataArray(WeaponDataSO weaponData, int attackCount)
//     {
//         return CreateAttackDataArray(attackCount, attackIndex => CreateMovementData(weaponData, attackIndex));
//     }
//
//     private static ComboWaitInputData[] CreateComboInputDataArray(WeaponDataSO weaponData, int attackCount)
//     {
//         return CreateAttackDataArray(attackCount, attackIndex =>
//         {
//             ComboWaitInputData data = CreateComboInputData(weaponData, attackIndex);
//             data.comboCount = attackCount;
//             return data;
//         });
//     }
//
//     private static WeaponTimedHitBoxData[] CreateHitBoxDataArray(WeaponDataSO weaponData, int attackCount)
//     {
//         return CreateAttackDataArray(attackCount, attackIndex => CreateHitBoxData(weaponData, attackIndex));
//     }
//
//     private static WeaponApplyDamageToHitsData[] CreateDamageDataArray(WeaponDataSO weaponData, int attackCount)
//     {
//         return CreateAttackDataArray(attackCount, attackIndex => CreateDamageData(weaponData, attackIndex));
//     }
//
//     private static WeaponApplyKnockbackToHitsData[] CreateKnockbackDataArray(WeaponDataSO weaponData, int attackCount)
//     {
//         return CreateAttackDataArray(attackCount, attackIndex => CreateKnockbackData(weaponData, attackIndex));
//     }
//
//     private static TData[] CreateAttackDataArray<TData>(int attackCount, Func<int, TData> factory)
//     {
//         int count = Mathf.Max(1, attackCount);
//         TData[] data = new TData[count];
//         for (int i = 0; i < count; i++)
//             data[i] = factory(i);
//
//         return data;
//     }
//
//     private static WeaponForwardMovementData CreateMovementData(WeaponDataSO weaponData, int attackIndex)
//     {
//         MovementAttackData data = weaponData.GetData<global::MovementData>()?.GetAttackData(attackIndex);
//         return new WeaponForwardMovementData
//         {
//             velocity = data != null ? data.Velocity : 0f,
//             duration = data != null ? data.Duration : 0f,
//             velocityContributionMode = MovementVelocityContributionMode.OverrideHorizontal,
//             velocityContributionPriority = 100
//         };
//     }
//
//     private static ComboWaitInputData CreateComboInputData(WeaponDataSO weaponData, int attackIndex)
//     {
//         ComboComponentData combo = weaponData.GetData<ComboComponentData>();
//         ComboAttackData data = combo?.GetAttackData(attackIndex);
//         return new ComboWaitInputData
//         {
//             inputDelay = combo != null ? combo.InputDelay : 0.1f,
//             requiredAnimationProgress = data != null ? data.RequiredAnimationProgress : 0.35f,
//             stateTag = data != null && !string.IsNullOrEmpty(data.StateTag) ? data.StateTag : "Attack",
//             bufferEarlyInput = true
//         };
//     }
//
//     private static WeaponTimedHitBoxData CreateHitBoxData(WeaponDataSO weaponData, int attackIndex)
//     {
//         ActionHitBoxData componentData = weaponData.GetData<ActionHitBoxData>();
//         AttackActionHitBox3D data = componentData?.GetAttackData(attackIndex);
//         return new WeaponTimedHitBoxData
//         {
//             hitBox = data != null ? data.HitBox : new Bounds(new Vector3(0f, 0.75f, 0.5f), new Vector3(1.2f, 1.5f, 1f)),
//             layers = componentData != null ? componentData.DetectableLayers : -1,
//             startTime = data != null ? data.StartTime : 0.17f,
//             endTime = data != null ? data.EndTime : 0.35f,
//             stateTag = data != null && !string.IsNullOrEmpty(data.StateTag) ? data.StateTag : "Attack",
//             maxHits = 10,
//             ignoreOwner = true,
//             groupHitsByRoot = false,
//             triggerInteraction = QueryTriggerInteraction.UseGlobal
//         };
//     }
//
//     private static WeaponApplyDamageToHitsData CreateDamageData(WeaponDataSO weaponData, int attackIndex)
//     {
//         AttackDamage data = weaponData.GetData<DamageOnHitBoxActionData>()?.GetAttackData(attackIndex);
//         return new WeaponApplyDamageToHitsData
//         {
//             amount = data != null ? data.Amount : 0f,
//             useOwnerDamageModifier = true
//         };
//     }
//
//     private static WeaponTriggerHitEffectData CreateEffectData(WeaponDataSO weaponData)
//     {
//         EffectOnHitBoxActionData data = weaponData.GetData<EffectOnHitBoxActionData>();
//         return new WeaponTriggerHitEffectData
//         {
//             eventName = data != null ? data.EventName : string.Empty,
//             onlyIfDamageable = true
//         };
//     }
//
//     private static WeaponApplyKnockbackToHitsData CreateKnockbackData(WeaponDataSO weaponData, int attackIndex)
//     {
//         AttackKnockback data = weaponData.GetData<KnockbackData>()?.GetAttackData(attackIndex);
//         return new WeaponApplyKnockbackToHitsData
//         {
//             angle = data != null ? data.Angle : Vector3.zero,
//             strength = data != null ? data.Strength : 0f
//         };
//     }
//
//     private static NodeConfig ResetNode(int column, int attackIndex, float yOffset = 0f)
//     {
//         return Action(
//             Provider(new ComboResetProvider(), true,
//                 new ComboResetData()),
//             column,
//             attackIndex,
//             yOffset);
//     }
//
//     private static ActionNodeConfig Action(ActionDataProvider provider, int column, int attackIndex, float yOffset = 0f)
//     {
//         return new ActionNodeConfig
//         {
//             dataProvider = provider,
//             editorPosition = Position(column, attackIndex, yOffset)
//         };
//     }
//
//     private static Vector2 Position(int column, int attackIndex, float yOffset = 0f)
//     {
//         return new Vector2(80f + column * ColumnWidth, 160f + attackIndex * AttackLaneHeight + yOffset);
//     }
//
//     private static TProvider Provider<TProvider, TData>(TProvider provider, bool useSingleValue, params TData[] data)
//         where TProvider : ActionDataProvider<TData>
//     {
//         Type baseType = typeof(ActionDataProvider<TData>);
//         FieldInfo useSingleValueField = baseType.GetField("useSingleValue", BindingFlags.Instance | BindingFlags.NonPublic);
//         FieldInfo dataField = baseType.GetField("data", BindingFlags.Instance | BindingFlags.NonPublic);
//
//         useSingleValueField.SetValue(provider, useSingleValue);
//         dataField.SetValue(provider, data);
//         return provider;
//     }
//
//     private static void EnsureFolder(string folder)
//     {
//         string[] parts = folder.Split('/');
//         string current = parts[0];
//
//         for (int i = 1; i < parts.Length; i++)
//         {
//             string next = current + "/" + parts[i];
//             if (!AssetDatabase.IsValidFolder(next))
//                 AssetDatabase.CreateFolder(current, parts[i]);
//
//             current = next;
//         }
//     }
//
//     private static string FindSampleFolder()
//     {
//         string[] guids = AssetDatabase.FindAssets("ComboWeapon t:MonoScript");
//
//         for (int i = 0; i < guids.Length; i++)
//         {
//             string path = AssetDatabase.GUIDToAssetPath(guids[i]);
//             if (Path.GetFileName(path) == "ComboWeapon.cs")
//                 return Path.GetDirectoryName(path).Replace('\\', '/');
//         }
//
//         return FallbackSampleFolder;
//     }
// }
// }

