using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace StylizedWater2
{
    [CustomEditor(typeof(UnderwaterRenderer))]
    public class UnderwaterRendererInspector : Editor
    {
        private UnderwaterRenderer renderer;

        private SerializedProperty waterLevelSource;
        private SerializedProperty waterLevel;
        private SerializedProperty waterLevelTransform;
        
        private SerializedProperty waterMaterial;
        private SerializedProperty dynamicMaterial;
        
        private SerializedProperty useVolumeBlending;
        private SerializedProperty verticalDensity;
        private SerializedProperty verticalDepth;
        private SerializedProperty horizontalDensity;
        private SerializedProperty startDistance;
        private SerializedProperty fogBrightness;
        private SerializedProperty subsurfaceStrength;

        private SerializedProperty offset;
        private SerializedProperty waterLineThickness;
        
        private SerializedProperty enableBlur;
        private SerializedProperty enableDistortion;

        private bool renderFeaturePresent;
        private bool renderFeatureEnabled;
        #if URP
        private UnderwaterRenderFeature renderFeature;
        private Editor renderFeatureEditor;
        #endif

        private void OnEnable()
        {
            renderer = (UnderwaterRenderer)target;
            
            waterLevelSource = serializedObject.FindProperty("waterLevelSource");
            waterLevel = serializedObject.FindProperty("waterLevel");
            waterLevelTransform = serializedObject.FindProperty("waterLevelTransform");
            
            waterMaterial = serializedObject.FindProperty("waterMaterial");
            dynamicMaterial = serializedObject.FindProperty("dynamicMaterial");
            
            useVolumeBlending = serializedObject.FindProperty("useVolumeBlending");
            verticalDensity = serializedObject.FindProperty("verticalDensity");
            verticalDepth = serializedObject.FindProperty("verticalDepth");
            horizontalDensity = serializedObject.FindProperty("horizontalDensity");
            startDistance = serializedObject.FindProperty("startDistance");
            fogBrightness = serializedObject.FindProperty("fogBrightness");
            subsurfaceStrength = serializedObject.FindProperty("subsurfaceStrength");
            
            offset = serializedObject.FindProperty("offset");
            waterLineThickness = serializedObject.FindProperty("waterLineThickness");
            
            enableBlur = serializedObject.FindProperty("enableBlur");
            enableDistortion = serializedObject.FindProperty("enableDistortion");

            #if URP
            renderFeaturePresent = PipelineUtilities.RenderFeatureAdded<UnderwaterRenderFeature>();
            renderFeatureEnabled = PipelineUtilities.IsRenderFeatureEnabled<UnderwaterRenderFeature>();
            renderFeature = PipelineUtilities.GetRenderFeature<UnderwaterRenderFeature>() as UnderwaterRenderFeature;
            #endif
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Version " + UnderwaterRenderer.Version, EditorStyles.centeredGreyMiniLabel);
            
            #if URP
            DrawNotifications();
            
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            EditorGUILayout.LabelField("Material", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(waterMaterial);

            UI.DrawNotification(waterMaterial.objectReferenceValue == null, "The water material used by the water plane must be assigned", MessageType.Error);
            UI.DrawNotification(renderer.waterMaterial && renderer.waterMaterial.GetInt("_Cull") != (int)CullMode.Off, "The water material is not double-sided", "Make it so", () => SetMaterialDoubleSided(), MessageType.Error);
            
            if (waterMaterial.objectReferenceValue != null)
            {
                EditorGUILayout.PropertyField(dynamicMaterial);
                
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Water level", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(waterLevelSource, new GUIContent("Source", waterLevelSource.tooltip));

                if (waterLevelSource.intValue == (int)UnderwaterRenderer.WaterLevelSource.FixedValue)
                {
                    EditorGUILayout.PropertyField(waterLevel);
                }
                else
                {
                    EditorGUILayout.PropertyField(waterLevelTransform);
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Fog", EditorStyles.boldLabel);
                
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(useVolumeBlending);
                if (EditorGUI.EndChangeCheck())
                {
                     renderer.GetVolumeSettings();
                }

                if (!useVolumeBlending.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(verticalDensity);
                    EditorGUILayout.PropertyField(verticalDepth);
                    EditorGUILayout.PropertyField(horizontalDensity);
                    EditorGUILayout.PropertyField(startDistance);
                    EditorGUILayout.PropertyField(fogBrightness);
                    EditorGUILayout.PropertyField(subsurfaceStrength);
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Lens", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(offset);
                EditorGUILayout.PropertyField(waterLineThickness);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(enableBlur);
                EditorGUILayout.PropertyField(enableDistortion);
                
                if (renderFeature)
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Render feature settings", EditorStyles.boldLabel);

                    if (!renderFeatureEditor) renderFeatureEditor = Editor.CreateEditor(renderFeature);
                    SerializedObject serializedRendererFeaturesEditor = renderFeatureEditor.serializedObject;
                    serializedRendererFeaturesEditor.Update();
                
                    EditorGUI.BeginChangeCheck();

                    renderFeatureEditor.OnInspectorGUI();

                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedRendererFeaturesEditor.ApplyModifiedProperties();
                        EditorUtility.SetDirty(renderFeature);
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                renderer.UpdateProperties();
                serializedObject.ApplyModifiedProperties();
            }
            
            #else
            EditorGUILayout.HelpBox("The Universal Render Pipeline is not installed", MessageType.Error);
            #endif
            
            UI.DrawFooter();
        }

        private void DrawNotifications()
        {
            UI.DrawNotification( !AssetInfo.MeetsMinimumVersion(UnderwaterRenderer.MinBaseVersion), "Version mismatch, requires Stylized Water 2 v" + UnderwaterRenderer.MinBaseVersion +".\n\nUpdate to avoid any issues or resolve errors", "Update", () => AssetInfo.OpenStorePage(), MessageType.Error);
            
            UI.DrawNotification(UniversalRenderPipeline.asset == null, "The Universal Render Pipeline is not active", MessageType.Error);
            UI.DrawNotification(UniversalRenderPipeline.asset && UniversalRenderPipeline.asset.msaaSampleCount > 1, "MSAA is enabled, this causes artifacts in the fog","Disable", () => DisableMSSA(), MessageType.Warning);

            using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
            {
                UI.DrawNotification(!renderFeaturePresent, "The underwater render feature hasn't be added to the default renderer", "Add", () => AddRenderFeature(), MessageType.Error);
            }
            if(Application.isPlaying && !renderFeaturePresent) EditorGUILayout.HelpBox("Exit play mode to perform this action", MessageType.Warning);
            
            UI.DrawNotification(renderFeaturePresent && !renderFeatureEnabled, "The underwater render feature is disabled", "Enable", () => EnableRenderFeature(), MessageType.Warning);

        }
        
        #if URP
        private void AddRenderFeature()
        {
            PipelineUtilities.AddRenderFeature<UnderwaterRenderFeature>();
            renderFeaturePresent = true;
            renderFeature = PipelineUtilities.GetRenderFeature<UnderwaterRenderFeature>() as UnderwaterRenderFeature;
        }

        private void EnableRenderFeature()
        {
            PipelineUtilities.ToggleRenderFeature<UnderwaterRenderFeature>(true);
            renderFeatureEnabled = true;
        }

        private void DisableMSSA()
        {
            UniversalRenderPipeline.asset.msaaSampleCount = 1;
            EditorUtility.SetDirty(UniversalRenderPipeline.asset);
        }

        private void SetMaterialDoubleSided()
        {
            renderer.SetMaterialCulling();
        }
        
        [MenuItem("Window/Stylized Water 2/Set up underwater rendering", false, 2000)]
        private static void CreateUnderwaterRenderer()
        {
            UnderwaterRenderer r = FindObjectOfType<UnderwaterRenderer>();

            if (r)
            {
                EditorUtility.DisplayDialog(AssetInfo.ASSET_NAME, "An Underwater Renderer instance already exists. Only one can be created", "OK");
                
                return;
            }
            
            GameObject obj = new GameObject("Underwater Renderer", typeof(UnderwaterRenderer));
            r = obj.GetComponent<UnderwaterRenderer>();
            
            Selection.activeObject = obj;
        }
        #endif
    }
}
